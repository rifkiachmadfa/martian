using UnityEngine;

/// <summary>
/// Satu petak tanam kentang di Farming Area (GDD bagian 10). Siklus lewat tombol E:
/// kosong -> tanam (pilih Bibit di tas, tombol 3, lalu E); tumbuh dan belum disiram -> siram (memakai Water bersama);
/// matang -> panen (Kentang masuk ke tas; bila tas tidak muat, panen ditunda sampai pemain menyimpan di storage).
/// Tanaman yang SUDAH disiram maju satu tahap saat hari berganti (TimeManager.OnDayStarted, jadi lewat Bed);
/// yang belum disiram tidak maju dan tidak mati. Tahap terakhir = matang.
/// Tidak ada pupuk, hama, atau jenis tanaman lain. Bibit adalah item tas. Semua angka adalah USULAN.
/// Pasang pada objek ber-layer Interactable dengan Collider2D. Satu objek = satu petak; duplikat untuk petak lain.
/// Status (HasPlant, Stage, IsWatered) disediakan sebagai properti untuk SaveSystem nanti.
/// </summary>
public class FarmingPlot : MonoBehaviour, IInteractable, IItemReceiver
{
    [Header("Referensi (otomatis dicari jika kosong)")]
    [SerializeField] private DialogueUI dialogueUI;

    [Header("Aturan tanaman (USULAN, ubah lewat Inspector setelah playtest)")]
    [Tooltip("Jumlah tahap pertumbuhan (GDD: 2-3 tahap). Tahap terakhir = matang.")]
    [SerializeField, Range(2, 3)] private int stageCount = 3;
    [SerializeField, Min(0f)] private float waterPerWatering = 2f;
    [Tooltip("Kentang yang didapat per panen (masuk ke tas; kapasitas tas 10).")]
    [SerializeField, Min(1)] private int potatoPerHarvest = 4;
    [Tooltip("Bibit yang dikembalikan per panen (masuk tas; bila penuh, masuk storage).")]
    [SerializeField, Min(0)] private int seedPerHarvest = 1;

    [Header("Tampilan opsional")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("Sprite petak kosong. Bila kosong, warna dipakai sebagai penanda.")]
    [SerializeField] private Sprite emptySprite;
    [Tooltip("Sprite per tahap (indeks 0 = tahap pertama). Tahap tanpa sprite memakai warna penanda.")]
    [SerializeField] private Sprite[] stageSprites;
    [SerializeField] private Color emptyTint = new Color(0.55f, 0.4f, 0.3f);
    [SerializeField] private Color youngTint = new Color(0.7f, 0.9f, 0.4f);
    [SerializeField] private Color matureTint = new Color(0.3f, 1f, 0.3f);
    [SerializeField, Min(0f)] private float reinteractCooldown = 0.3f;

    private TimeManager time;
    private Color baseColor = Color.white;
    private float nextInteractTime;

    /// <summary>True bila ada tanaman di petak.</summary>
    public bool HasPlant { get; private set; }

    /// <summary>Tahap pertumbuhan, mulai 0. Hanya berarti bila HasPlant.</summary>
    public int Stage { get; private set; }

    /// <summary>True bila tanaman sudah disiram sejak hari terakhir berganti.</summary>
    public bool IsWatered { get; private set; }

    /// <summary>True bila tanaman sudah di tahap terakhir dan siap panen.</summary>
    public bool IsMature => HasPlant && Stage >= stageCount - 1;

    public string InteractionPrompt
    {
        get
        {
            if (!HasPlant) return "Petak kosong (pilih bibit, tombol 3)";
            if (IsMature) return "Panen kentang";
            return IsWatered ? "Periksa tanaman" : "Siram tanaman";
        }
    }

    public bool CanUseItem(ItemType item) => item == ItemType.Seed && !HasPlant;

    public string GetItemPrompt(ItemType item) => "Tanam bibit kentang";

    public void UseItem(GameObject user, ItemType item)
    {
        if (item != ItemType.Seed || HasPlant) return;

        InventorySystem inventory = InventorySystem.Instance;
        if (inventory == null)
        {
            Debug.LogError("FarmingPlot: InventorySystem tidak ada di scene. Jalankan menu Mars > Setup Inventory and Storage.", this);
            return;
        }

        if (!inventory.RemoveFromBag(ItemType.Seed, 1))
        {
            ShowMessage("Tidak ada bibit di tas. Ambil bibit dari storage.");
            return;
        }

        HasPlant = true;
        Stage = 0;
        IsWatered = false;
        RefreshVisual();
        ShowMessage("Bibit kentang ditanam. Siram tanaman agar maju satu tahap saat hari berganti.");
    }

    private void Awake()
    {
        if (spriteRenderer != null) baseColor = spriteRenderer.color;
    }

    private void Start()
    {
        if (dialogueUI == null) dialogueUI = FindFirstObjectByType<DialogueUI>();

        // Berlangganan di Start (bukan OnEnable): urutan Awake/OnEnable antar objek tidak terjamin.
        time = TimeManager.Instance;
        if (time != null) time.OnDayStarted += HandleDayStarted;
        else Debug.LogWarning("FarmingPlot: TimeManager tidak ada di scene; tanaman tidak akan tumbuh.", this);

        RefreshVisual();
    }

    private void OnDestroy()
    {
        if (time != null) time.OnDayStarted -= HandleDayStarted;
    }

    public bool CanInteract(GameObject interactor)
    {
        if (Time.time < nextInteractTime) return false;
        return dialogueUI == null || !dialogueUI.IsOpen;
    }

    public void Interact(GameObject interactor)
    {
        ResourceManager resources = ResourceManager.Instance;
        if (resources == null)
        {
            Debug.LogError("FarmingPlot: ResourceManager tidak ada di scene.", this);
            return;
        }

        if (!HasPlant)
        {
            ShowMessage("Petak kosong. Pilih bibit di tas (tombol 3), lalu tekan E untuk menanam. Ambil bibit di storage bila tas kosong.");
            return;
        }

        if (IsMature)
        {
            Harvest();
            return;
        }

        if (IsWatered)
        {
            ShowMessage("Tanaman sudah disiram (tahap " + (Stage + 1) + " dari " + stageCount +
                "). Ia tumbuh saat hari berganti.");
            return;
        }

        if (!resources.TryConsume(ResourceType.Water, waterPerWatering))
        {
            ShowMessage("Water tidak cukup untuk menyiram (butuh " + Mathf.CeilToInt(waterPerWatering) +
                ", tersedia " + Mathf.FloorToInt(resources.Get(ResourceType.Water)) + ").");
            return;
        }

        IsWatered = true;
        ShowMessage("Tanaman disiram memakai " + Mathf.CeilToInt(waterPerWatering) +
            " Water. Ia akan maju satu tahap saat hari berganti.");
    }

    private void Harvest()
    {
        InventorySystem inventory = InventorySystem.Instance;
        if (inventory == null)
        {
            Debug.LogError("FarmingPlot: InventorySystem tidak ada di scene. Jalankan menu Mars > Setup Inventory and Storage.", this);
            return;
        }

        int space = inventory.BagSpace(ItemType.Potato);
        if (space < potatoPerHarvest)
        {
            ShowMessage("Tas tidak muat " + potatoPerHarvest + " kentang (ruang " + space +
                "). Simpan kentang di storage dulu, lalu panen lagi.");
            return;
        }

        int potatoes = inventory.AddToBag(ItemType.Potato, potatoPerHarvest);
        int seedsInBag = inventory.AddToBag(ItemType.Seed, seedPerHarvest);
        int seedsStored = seedsInBag < seedPerHarvest ? inventory.AddToStorage(ItemType.Seed, seedPerHarvest - seedsInBag) : 0;

        HasPlant = false;
        Stage = 0;
        IsWatered = false;
        RefreshVisual();

        string message = "Kentang dipanen: " + potatoes + " kentang masuk ke tas.";
        if (seedsInBag > 0) message += " Bibit +" + seedsInBag + " masuk ke tas.";
        if (seedsStored > 0) message += " Bibit +" + seedsStored + " masuk ke storage (tas penuh).";
        ShowMessage(message + " Petak kosong lagi dan siap ditanami.");
    }

    // Pertumbuhan: hanya tanaman yang disiram dan belum matang yang maju. Status siram direset tiap hari.
    private void HandleDayStarted(int day)
    {
        if (!HasPlant) return;

        if (IsWatered && !IsMature) Stage++;
        IsWatered = false;
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (spriteRenderer == null) return;

        Sprite sprite = null;
        if (!HasPlant) sprite = emptySprite;
        else if (stageSprites != null && Stage < stageSprites.Length) sprite = stageSprites[Stage];

        if (sprite != null)
        {
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = baseColor;
            return;
        }

        // Tanpa sprite untuk keadaan ini: warna menjadi penanda (coklat = kosong, hijau muda -> hijau = tumbuh).
        if (!HasPlant)
        {
            spriteRenderer.color = baseColor * emptyTint;
        }
        else
        {
            float t = stageCount > 1 ? Stage / (float)(stageCount - 1) : 1f;
            spriteRenderer.color = baseColor * Color.Lerp(youngTint, matureTint, t);
        }
    }

    private void ShowMessage(string message)
    {
        if (dialogueUI == null)
        {
            Debug.Log("FarmingPlot: " + message);
            nextInteractTime = Time.time + reinteractCooldown;
            return;
        }

        dialogueUI.Show("Farming Plot", new[] { message }, () => nextInteractTime = Time.time + reinteractCooldown);
    }
}
