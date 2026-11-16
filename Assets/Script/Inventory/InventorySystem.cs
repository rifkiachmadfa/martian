using System;
using UnityEngine;

/// <summary>
/// Tas pemain (bukan storage): kentang maks 10, es maks 10, bibit maks 5 (permintaan developer 2026-11-16).
/// Tombol 1, 2, 3 memilih slot Kentang, Es, Bibit (tekan lagi untuk membatalkan pilihan). Setelah slot dipilih,
/// tombol E memakai item: makan kentang (bila lapar), memasukkan es ke Water Purifier, menanam bibit di petak
/// kosong (lewat IItemReceiver). Kalau tas penuh, item tidak bisa diambil; simpan dulu di storage (StorageUI).
/// Storage menyimpan tiga item yang sama. Kentang di storage = stok Food di ResourceManager (angka Food di HUD);
/// es dan bibit di storage disimpan di sini. Hanya logika; tampilan ada di InventoryHotbarUI dan StorageUI.
/// Semua angka adalah USULAN kecuali kapasitas tas yang diminta developer.
/// </summary>
public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    [Header("Kapasitas tas (permintaan developer)")]
    [SerializeField, Min(1)] private int potatoBagMax = 10;
    [SerializeField, Min(1)] private int iceBagMax = 10;
    [SerializeField, Min(1)] private int seedBagMax = 5;

    [Header("Storage (USULAN). Kentang di storage = stok Food di ResourceManager")]
    [SerializeField, Min(1)] private int iceStorageMax = 30;
    [SerializeField, Min(0)] private int iceStorageStart = 0;
    [SerializeField, Min(1)] private int seedStorageMax = 20;
    [SerializeField, Min(0)] private int seedStorageStart = 5;

    [Header("Referensi (otomatis dicari jika kosong)")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private PlayerMovement player;

    private const int ItemCount = 3;
    private readonly int[] bag = new int[ItemCount];
    private readonly int[] storage = new int[ItemCount]; // indeks Potato tidak dipakai (lihat ResourceManager Food)
    private ResourceManager resources;

    /// <summary>True bila ada slot yang sedang dipilih.</summary>
    public bool HasSelection { get; private set; }

    /// <summary>Slot terpilih. Hanya berarti bila HasSelection.</summary>
    public ItemType Selected { get; private set; }

    /// <summary>Dipanggil setiap isi tas, storage, atau pilihan slot berubah.</summary>
    public event Action OnChanged;

    /// <summary>True bila tombol slot dan E boleh dipakai untuk item (tidak ada dialog, panel storage, atau kunci gerak).</summary>
    public bool CanAcceptInput =>
        (dialogueUI == null || !dialogueUI.IsOpen) &&
        (player == null || player.MovementEnabled) &&
        !StorageUI.IsOpen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("InventorySystem: sudah ada instance lain, objek ini dihapus.", this);
            Destroy(this);
            return;
        }

        Instance = this;
        storage[(int)ItemType.Ice] = Mathf.Clamp(iceStorageStart, 0, iceStorageMax);
        storage[(int)ItemType.Seed] = Mathf.Clamp(seedStorageStart, 0, seedStorageMax);
    }

    private void Start()
    {
        if (dialogueUI == null) dialogueUI = FindFirstObjectByType<DialogueUI>();
        if (player == null) player = FindFirstObjectByType<PlayerMovement>();

        resources = ResourceManager.Instance;
        if (resources != null) resources.OnResourceChanged += HandleResourceChanged;
        else Debug.LogError("InventorySystem: ResourceManager tidak ada di scene (kentang di storage memakai stok Food).", this);
    }

    private void OnDestroy()
    {
        if (resources != null) resources.OnResourceChanged -= HandleResourceChanged;
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (!CanAcceptInput) return;

        for (int i = 0; i < ItemCount; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                ToggleSelection((ItemType)i);
        }
    }

    public static string NameOf(ItemType item)
    {
        switch (item)
        {
            case ItemType.Potato: return "Kentang";
            case ItemType.Ice: return "Es";
            default: return "Bibit";
        }
    }

    // ---------- Pilihan slot ----------

    public void ToggleSelection(ItemType item)
    {
        if (HasSelection && Selected == item) HasSelection = false;
        else
        {
            HasSelection = true;
            Selected = item;
        }

        OnChanged?.Invoke();
    }

    public void ClearSelection()
    {
        if (!HasSelection) return;

        HasSelection = false;
        OnChanged?.Invoke();
    }

    // ---------- Tas ----------

    public int GetBag(ItemType item) => bag[(int)item];

    public int GetBagMax(ItemType item)
    {
        switch (item)
        {
            case ItemType.Potato: return potatoBagMax;
            case ItemType.Ice: return iceBagMax;
            default: return seedBagMax;
        }
    }

    public int BagSpace(ItemType item) => Mathf.Max(0, GetBagMax(item) - GetBag(item));

    /// <summary>Menambah item ke tas, dibatasi kapasitas. Mengembalikan jumlah yang benar-benar masuk.</summary>
    public int AddToBag(ItemType item, int amount)
    {
        int added = Mathf.Min(Mathf.Max(0, amount), BagSpace(item));
        if (added <= 0) return 0;

        bag[(int)item] += added;
        OnChanged?.Invoke();
        return added;
    }

    /// <summary>Mengurangi item dari tas hanya bila cukup.</summary>
    public bool RemoveFromBag(ItemType item, int amount)
    {
        if (amount < 0 || bag[(int)item] < amount) return false;
        if (amount == 0) return true;

        bag[(int)item] -= amount;
        OnChanged?.Invoke();
        return true;
    }

    // ---------- Storage ----------

    public int GetStorage(ItemType item)
    {
        if (item == ItemType.Potato)
            return resources != null ? Mathf.FloorToInt(resources.Get(ResourceType.Food) + 0.001f) : 0;
        return storage[(int)item];
    }

    public int GetStorageMax(ItemType item)
    {
        switch (item)
        {
            case ItemType.Potato: return resources != null ? Mathf.FloorToInt(resources.GetMax(ResourceType.Food) + 0.001f) : 0;
            case ItemType.Ice: return iceStorageMax;
            default: return seedStorageMax;
        }
    }

    public int StorageSpace(ItemType item) => Mathf.Max(0, GetStorageMax(item) - GetStorage(item));

    /// <summary>Menambah item langsung ke storage (misalnya bibit hasil panen saat tas penuh). Mengembalikan jumlah yang masuk.</summary>
    public int AddToStorage(ItemType item, int amount)
    {
        int added = Mathf.Min(Mathf.Max(0, amount), StorageSpace(item));
        if (added <= 0) return 0;

        if (item == ItemType.Potato) resources.Add(ResourceType.Food, added); // memicu OnChanged lewat HandleResourceChanged
        else
        {
            storage[(int)item] += added;
            OnChanged?.Invoke();
        }
        return added;
    }

    private bool RemoveFromStorage(ItemType item, int amount)
    {
        if (amount <= 0 || GetStorage(item) < amount) return false;

        if (item == ItemType.Potato) return resources.TryConsume(ResourceType.Food, amount);

        storage[(int)item] -= amount;
        OnChanged?.Invoke();
        return true;
    }

    /// <summary>Memindahkan item dari tas ke storage (sebanyak muat). Mengembalikan jumlah yang dipindahkan.</summary>
    public int Store(ItemType item, int amount)
    {
        int moved = Mathf.Min(Mathf.Max(0, amount), Mathf.Min(GetBag(item), StorageSpace(item)));
        if (moved <= 0) return 0;

        RemoveFromBag(item, moved);
        AddToStorage(item, moved);
        return moved;
    }

    /// <summary>Memindahkan item dari storage ke tas (sebanyak muat di tas). Mengembalikan jumlah yang dipindahkan.</summary>
    public int Take(ItemType item, int amount)
    {
        int moved = Mathf.Min(Mathf.Max(0, amount), Mathf.Min(GetStorage(item), BagSpace(item)));
        if (moved <= 0) return 0;

        if (!RemoveFromStorage(item, moved)) return 0;
        AddToBag(item, moved);
        return moved;
    }

    // ---------- Aksi bawaan item (tanpa objek penerima) ----------

    /// <summary>True bila item terpilih punya aksi bawaan yang bisa dilakukan sekarang (kentang: makan bila lapar).</summary>
    public bool CanUseDefault(ItemType item)
    {
        if (item != ItemType.Potato || GetBag(item) <= 0) return false;

        HungerSystem hunger = HungerSystem.Instance;
        return hunger != null && hunger.CanEat;
    }

    public string GetDefaultPrompt(ItemType item) => item == ItemType.Potato ? "Makan kentang" : string.Empty;

    /// <summary>Menjalankan aksi bawaan. Mengembalikan true bila berhasil.</summary>
    public bool UseDefault(ItemType item)
    {
        if (!CanUseDefault(item)) return false;

        HungerSystem hunger = HungerSystem.Instance;
        if (!RemoveFromBag(ItemType.Potato, 1)) return false;

        hunger.Feed(hunger.SatietyPerPotato);
        return true;
    }

    private void HandleResourceChanged(ResourceType type, float amount, float max)
    {
        if (type == ResourceType.Food) OnChanged?.Invoke();
    }
}
