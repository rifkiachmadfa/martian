using System;
using UnityEngine;

/// <summary>
/// Konsumsi oksigen habitat (pemain "bernapas"). Stok HabitatOxygen berkurang pelan selama waktu gameplay
/// berjalan, sehingga Oxygen Generator benar-benar dibutuhkan untuk menjaganya. Berhenti saat
/// TimeManager.IsPaused (misalnya transisi tidur), sama seperti HungerSystem dan SuitSystem.
/// REVISI GDD (disetujui developer 2026-11-16): GDD v1.2 hanya melarang pengurangan pasif untuk Water dan Energy,
/// dan tidak mendefinisikan penyebab oksigen habitat berkurang. Angka laju di bawah adalah USULAN.
/// Berkurang per tick (bukan tiap frame) agar HUD tidak menulis ulang teks tiap frame.
/// Bila stok mencapai 0, event OnHabitatOxygenDepleted dipicu sekali (hanya log). Game Over (13.2.3) belum dibuat.
/// Hanya logika; tampilan lewat HudUI yang membaca ResourceManager.
/// Komponen ini dibuat otomatis saat game mulai bila belum ada di scene; boleh juga dipasang manual pada satu GameObject.
/// </summary>
public class HabitatOxygenDrain : MonoBehaviour
{
    public static HabitatOxygenDrain Instance { get; private set; }

    [Header("Konsumsi (USULAN, ubah lewat Inspector setelah playtest)")]
    [Tooltip("Oksigen habitat yang berkurang per detik gameplay. 0,05 = 30 unit per hari (600 detik); stok penuh (100) habis sekitar 33 menit tanpa generator.")]
    [SerializeField, Min(0f)] private float drainPerSecond = 0.05f;
    [Tooltip("Selang detik antar pengurangan. Pengurangan tiap tick = drainPerSecond x tickSeconds.")]
    [SerializeField, Min(0.1f)] private float tickSeconds = 1f;

    private ResourceManager resources;
    private float tickTimer;
    private bool depletedReported;

    /// <summary>Laju konsumsi saat ini (unit per detik).</summary>
    public float DrainPerSecond => drainPerSecond;

    /// <summary>Dipanggil sekali ketika stok oksigen habitat mencapai 0 (Game Over belum dibuat).</summary>
    public event Action OnHabitatOxygenDepleted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateIfMissing()
    {
        if (ResourceManager.Instance == null) return;
        if (FindFirstObjectByType<HabitatOxygenDrain>() != null) return;

        new GameObject("HabitatOxygenDrain").AddComponent<HabitatOxygenDrain>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("HabitatOxygenDrain: sudah ada instance lain, objek ini dihapus.", this);
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        resources = ResourceManager.Instance;
        if (resources == null)
            Debug.LogError("HabitatOxygenDrain: ResourceManager tidak ada di scene.", this);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (resources == null) return;

        TimeManager time = TimeManager.Instance;
        if (time != null && time.IsPaused) return;

        tickTimer += Time.deltaTime;
        if (tickTimer < tickSeconds) return;

        tickTimer -= tickSeconds;

        float current = resources.Get(ResourceType.HabitatOxygen);
        if (current > 0f)
        {
            float amount = Mathf.Min(drainPerSecond * tickSeconds, current);
            if (amount > 0f) resources.TryConsume(ResourceType.HabitatOxygen, amount);
        }

        if (resources.Get(ResourceType.HabitatOxygen) > 0f)
        {
            depletedReported = false;
            return;
        }

        if (depletedReported) return;

        depletedReported = true;
        Debug.LogWarning("HabitatOxygenDrain: oksigen habitat habis. (Game Over belum dibuat.)");
        OnHabitatOxygenDepleted?.Invoke();
    }
}
