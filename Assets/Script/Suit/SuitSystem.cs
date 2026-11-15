using System;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Status Astronaut Suit: sudah dikenakan atau belum, pengisian dari oksigen habitat, melepas suit,
/// dan pengurangan oksigen suit.
/// ATURAN (revisi GDD 2026-11-15): oksigen suit berkurang selama suit DIKENAKAN, di dalam maupun di luar
/// habitat. Saat suit dilepas, oksigen suit tidak berkurang. Pemain melepas suit di Suit Station.
/// Angka oksigen suit disimpan di ResourceManager (ResourceType.SuitOxygen), terpisah dari HabitatOxygen.
/// Hanya logika; tampilan ada di HudUI, interaksi ada di SuitStation, pembatas pintu ada di SuitGate.
/// Pemain dianggap "di luar" bila tidak berada di dalam IndoorZone mana pun; status ini dipakai
/// SuitGate dan larangan melepas suit di luar, bukan untuk menentukan konsumsi oksigen.
/// Game Over karena oksigen habis belum dibuat; yang ada baru event OnSuitOxygenEmpty (hanya saat di luar).
/// </summary>
public class SuitSystem : MonoBehaviour
{
    public static SuitSystem Instance { get; private set; }

    [Header("Aturan suit (USULAN, ubah lewat Inspector setelah playtest)")]
    [Tooltip("Oksigen suit minimum agar pemain boleh melewati gerbang keluar.")]
    [SerializeField, Min(0f)] private float minimumOxygenToExit = 20f;
    [Tooltip("Oksigen suit yang berkurang per detik selama suit dikenakan (di dalam maupun di luar habitat).")]
    [FormerlySerializedAs("outdoorDrainPerSecond")]
    [SerializeField, Min(0f)] private float drainPerSecond = 1f;

    private ResourceManager resources;
    private int indoorZoneCount;
    private bool emptyReported;

    /// <summary>True setelah pemain mengenakan suit di Suit Station.</summary>
    public bool IsEquipped { get; private set; }

    /// <summary>True bila pemain tidak berada di dalam IndoorZone mana pun. Awalnya dianggap di dalam habitat.</summary>
    public bool IsOutdoors { get; private set; }

    public float MinimumOxygenToExit => minimumOxygenToExit;

    /// <summary>Suit dikenakan dan oksigennya cukup untuk keluar.</summary>
    public bool CanExit =>
        IsEquipped && resources != null && resources.Get(ResourceType.SuitOxygen) >= minimumOxygenToExit;

    /// <summary>True bila tabung suit belum penuh dan stok oksigen habitat masih ada.</summary>
    public bool CanRefill =>
        resources != null &&
        resources.Get(ResourceType.SuitOxygen) < resources.GetMax(ResourceType.SuitOxygen) - 0.001f &&
        resources.Get(ResourceType.HabitatOxygen) > 0f;

    /// <summary>Dipanggil saat suit dikenakan atau dilepas (parameter: status terbaru).</summary>
    public event Action<bool> OnEquippedChanged;

    /// <summary>Dipanggil saat pemain keluar/masuk habitat (parameter: true bila di luar).</summary>
    public event Action<bool> OnOutdoorChanged;

    /// <summary>Dipanggil sekali ketika oksigen suit habis saat pemain di luar.</summary>
    public event Action OnSuitOxygenEmpty;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("SuitSystem: sudah ada instance lain, objek ini dihapus.", this);
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        resources = ResourceManager.Instance;
        if (resources == null)
            Debug.LogError("SuitSystem: ResourceManager tidak ada di scene.", this);

        if (FindFirstObjectByType<IndoorZone>() == null)
            Debug.LogWarning("SuitSystem: tidak ada IndoorZone di scene; pemain selalu dianggap di dalam habitat sehingga gerbang keluar tidak mati saat pemain di luar. Jalankan menu Mars > Setup Suit and Gate.", this);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (resources == null || !IsEquipped) return;

        // Ikut jeda seperti timer hari (TimeManager.IsPaused): transisi tidur, dan dialog hanya bila pauseDuringDialogue aktif.
        TimeManager time = TimeManager.Instance;
        if (time != null && time.IsPaused) return;

        float current = resources.Get(ResourceType.SuitOxygen);
        if (current > 0f)
        {
            float amount = Mathf.Min(drainPerSecond * Time.deltaTime, current);
            resources.TryConsume(ResourceType.SuitOxygen, amount);
        }

        // Oksigen kosong hanya berbahaya di luar habitat. Di dalam, pemain cukup melepas suit atau mengisinya.
        if (IsOutdoors && resources.Get(ResourceType.SuitOxygen) <= 0f) ReportEmptyOnce();
    }

    /// <summary>Mengenakan suit. Tidak melakukan apa pun bila sudah dikenakan.</summary>
    public void Equip()
    {
        if (IsEquipped) return;

        IsEquipped = true;
        OnEquippedChanged?.Invoke(true);
    }

    /// <summary>
    /// Melepas suit. Hanya bisa di dalam habitat. Oksigen yang tersisa di tabung tetap ada dan tidak berkurang
    /// selama suit dilepas. Mengembalikan true bila suit berhasil dilepas.
    /// </summary>
    public bool Unequip()
    {
        if (!IsEquipped || IsOutdoors) return false;

        IsEquipped = false;
        emptyReported = false;
        OnEquippedChanged?.Invoke(false);
        return true;
    }

    /// <summary>
    /// Memindahkan oksigen dari stok habitat ke tabung suit sampai penuh atau stok habitat habis.
    /// Mengembalikan jumlah yang benar-benar masuk ke suit.
    /// </summary>
    public float RefillFromHabitat()
    {
        if (resources == null) return 0f;

        float missing = resources.GetMax(ResourceType.SuitOxygen) - resources.Get(ResourceType.SuitOxygen);
        float available = resources.Get(ResourceType.HabitatOxygen);
        float amount = Mathf.Min(missing, available);
        if (amount <= 0f) return 0f;

        if (!resources.TryConsume(ResourceType.HabitatOxygen, amount)) return 0f;

        float added = resources.Add(ResourceType.SuitOxygen, amount);
        if (added > 0f) emptyReported = false;
        return added;
    }

    /// <summary>Dipanggil IndoorZone saat pemain masuk ke sebuah zona dalam habitat.</summary>
    public void NotifyEnteredIndoor()
    {
        indoorZoneCount++;
        SetOutdoors(false);
    }

    /// <summary>Dipanggil IndoorZone saat pemain keluar dari sebuah zona dalam habitat.</summary>
    public void NotifyExitedIndoor()
    {
        indoorZoneCount = Mathf.Max(0, indoorZoneCount - 1);
        if (indoorZoneCount == 0) SetOutdoors(true);
    }

    private void SetOutdoors(bool outdoors)
    {
        if (IsOutdoors == outdoors) return;

        IsOutdoors = outdoors;
        OnOutdoorChanged?.Invoke(outdoors);
    }

    private void ReportEmptyOnce()
    {
        if (emptyReported) return;

        emptyReported = true;
        Debug.LogWarning("SuitSystem: oksigen suit habis saat di luar. (Game Over/countdown darurat belum dibuat.)");
        OnSuitOxygenEmpty?.Invoke();
    }
}
