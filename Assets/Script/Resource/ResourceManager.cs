using System;
using UnityEngine;

/// <summary>
/// Satu-satunya tempat menyimpan stok Ice, Water, Oxygen (habitat dan suit), Energy, dan Food.
/// Sistem lain (mesin, suit, farming, TARS, HUD) membaca dan mengubah stok lewat class ini.
/// Hanya menyimpan angka dan memvalidasi perubahan; tidak ada logika mesin atau tampilan di sini.
/// </summary>
public class ResourceManager : MonoBehaviour
{
    /// <summary>Pengaturan awal satu resource. Semua angka default adalah USULAN, belum final.</summary>
    [Serializable]
    public class ResourceSetting
    {
        public ResourceType type;
        [Min(0f)] public float startAmount;
        [Min(0f)] public float maxAmount;

        public ResourceSetting(ResourceType type, float startAmount, float maxAmount)
        {
            this.type = type;
            this.startAmount = startAmount;
            this.maxAmount = maxAmount;
        }
    }

    public static ResourceManager Instance { get; private set; }

    [Header("Pengaturan resource (usulan, ubah lewat Inspector setelah playtest)")]
    [SerializeField]
    private ResourceSetting[] settings =
    {
        new ResourceSetting(ResourceType.Ice,           0f,   5f),
        new ResourceSetting(ResourceType.Water,         10f,  30f),
        new ResourceSetting(ResourceType.HabitatOxygen, 50f,  100f),
        new ResourceSetting(ResourceType.SuitOxygen,    0f,   100f),
        new ResourceSetting(ResourceType.Energy,        50f,  100f),
        new ResourceSetting(ResourceType.Food,          50f,  100f),
    };

    private float[] amounts;
    private float[] maxAmounts;

    /// <summary>Dipanggil setiap nilai berubah: (jenis, nilai baru, nilai maksimum).</summary>
    public event Action<ResourceType, float, float> OnResourceChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("ResourceManager: sudah ada instance lain, objek ini dihapus.", this);
            Destroy(this);
            return;
        }

        Instance = this;
        Initialize();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Initialize()
    {
        int count = Enum.GetValues(typeof(ResourceType)).Length;
        amounts = new float[count];
        maxAmounts = new float[count];

        foreach (ResourceSetting setting in settings)
        {
            int index = (int)setting.type;
            maxAmounts[index] = setting.maxAmount;
            amounts[index] = Mathf.Clamp(setting.startAmount, 0f, setting.maxAmount);
        }
    }

    /// <summary>Jumlah stok saat ini.</summary>
    public float Get(ResourceType type) => amounts[(int)type];

    /// <summary>Kapasitas maksimum stok.</summary>
    public float GetMax(ResourceType type) => maxAmounts[(int)type];

    /// <summary>True jika stok cukup untuk dikurangi sejumlah amount.</summary>
    public bool Has(ResourceType type, float amount) => amount >= 0f && Get(type) >= amount;

    /// <summary>
    /// Menambah stok, dibatasi kapasitas maksimum.
    /// Mengembalikan jumlah yang benar-benar masuk (bisa lebih kecil dari amount jika hampir penuh).
    /// </summary>
    public float Add(ResourceType type, float amount)
    {
        if (amount <= 0f) return 0f;

        int index = (int)type;
        float before = amounts[index];
        amounts[index] = Mathf.Min(before + amount, maxAmounts[index]);

        float added = amounts[index] - before;
        if (added > 0f) RaiseChanged(type);
        return added;
    }

    /// <summary>
    /// Mengurangi stok hanya jika cukup. Mengembalikan false (dan tidak mengubah apa pun) jika stok kurang.
    /// </summary>
    public bool TryConsume(ResourceType type, float amount)
    {
        if (amount < 0f || !Has(type, amount)) return false;
        if (amount == 0f) return true;

        amounts[(int)type] -= amount;
        RaiseChanged(type);
        return true;
    }

    private void RaiseChanged(ResourceType type)
    {
        OnResourceChanged?.Invoke(type, Get(type), GetMax(type));
    }
}
