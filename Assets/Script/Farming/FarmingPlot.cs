using UnityEngine;

/// <summary>
/// Satu petak tanam kentang di Farming Area (GDD bagian 10). Siklus lewat tombol E:
/// kosong -> tanam (pilih Bibit di tas, tombol 3, lalu E); belum disiram -> siram SEKALI (memakai Water bersama);
/// matang -> panen (Kentang dan Bibit masuk ke tas; bila tas kentang tidak muat, panen ditunda sampai pemain menyimpan di storage).
/// Pertumbuhan (revisi desain 2026-11-17, permintaan developer; menggantikan GDD 5.5 "tumbuh saat hari berganti"):
/// begitu disiram, tanaman tumbuh dalam WAKTU NYATA selama durasi acak growSecondsMin..growSecondsMax (default 60 sampai 120
/// detik), lalu matang. Timer ikut jeda TimeManager.IsPaused (tidur, minigame). Pergantian hari tidak mempengaruhi tumbuh.
/// Tanaman yang belum disiram tidak tumbuh dan tidak mati. Hasil panen acak (default 2 sampai 4 kentang dan 1 sampai 2
/// bibit), diundi saat disiram agar tidak bisa diulang-ulang dengan mencoba panen berkali-kali.
/// Tidak ada pupuk, hama, atau jenis tanaman lain. Bibit adalah item tas. Semua angka adalah USULAN.
/// Pasang pada objek ber-layer Interactable dengan Collider2D. Satu objek = satu petak; duplikat untuk petak lain.
/// Status (HasPlant, Stage, IsWatered) disediakan sebagai properti untuk SaveSystem nanti.
/// </summary>
public class FarmingPlot : MonoBehaviour, IInteractable, IItemReceiver
{
    [Header("Referensi (otomatis dicari jika kosong)")]
    [SerializeField] private DialogueUI dialogueUI;

    [Header("Aturan tanaman (USULAN, ubah lewat Inspector setelah playtest)")]
    [Tooltip("Jumlah tahap visual (GDD: 2-3 tahap). Tahap terakhir = matang.")]
    [SerializeField, Range(2, 3)] private int stageCount = 3;
    [Tooltip("Water yang dipakai untuk satu kali siram. Tanaman hanya perlu disiram sekali.")]
    [SerializeField, Min(0f)] private float waterPerWatering = 2f;

    [Header("Pertumbuhan waktu nyata (USULAN, permintaan developer 2026-11-17)")]
    [Tooltip("Durasi tumbuh minimum setelah disiram (detik). Durasi sebenarnya diundi acak antara minimum dan maksimum.")]
    [SerializeField, Min(1f)] private float growSecondsMin = 60f;
    [Tooltip("Durasi tumbuh maksimum setelah disiram (detik).")]
    [SerializeField, Min(1f)] private float growSecondsMax = 120f;

    [Header("Hasil panen acak (USULAN, permintaan developer 2026-11-17)")]
    [Tooltip("Kentang per panen, acak antara minimum dan maksimum (inklusif). Masuk ke tas; kapasitas tas 10.")]
    [SerializeField, Min(1)] private int potatoMin = 2;
    [SerializeField, Min(1)] private int potatoMax = 4;
    [Tooltip("Bibit per panen, acak antara minimum dan maksimum (inklusif). Masuk tas; bila penuh, masuk storage.")]
    [SerializeField, Min(0)] private int seedMin = 1;
    [SerializeField, Min(0)] private int seedMax = 2;

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

    [Header("Sprite tanaman (diisi lewat menu Mars > Setup Farming Sprites)")]
    [Tooltip("Sprite tanaman selama belum bisa dipanen (sejak ditanam sampai tahap terakhir). Dipasang pada objek anak, jadi petak tetap terlihat sebagai tanah.")]
    [SerializeField] private Sprite growingSprite;
    [Tooltip("Sprite tanaman saat siap panen.")]
    [SerializeField] private Sprite matureSprite;
    [Tooltip("Pembesaran sprite tanaman (sprite aslinya kecil, 100 pixel per unit). Ubah bila terlalu kecil atau besar.")]
    [SerializeField, Min(0.1f)] private float plantScale = 2.5f;

    [Header("Label status di atas petak (hitung mundur panen)")]
    [Tooltip("Menampilkan sisa waktu panen langsung di atas petak sejak ditanam, agar pemain tidak perlu menekan E berulang.")]
    [SerializeField] private bool showStatusLabel = true;
    [SerializeField, Min(0.1f)] private float statusLabelSize = 0.8f;
    [SerializeField] private Color waitingColor = new Color(1f, 0.9f, 0.4f);
    [SerializeField] private Color growingColor = Color.white;
    [SerializeField] private Color readyColor = new Color(0.55f, 1f, 0.55f);

    private TimeManager time;
    private SpriteRenderer plantRenderer;
    private TextMesh statusLabel;
    private TextMesh statusShadow;
    private string lastStatus;
    private Color baseColor = Color.white;
    private float nextInteractTime;
    private float growDuration;
    private float growElapsed;
    private int rolledPotatoes;
    private int rolledSeeds;

    /// <summary>True bila ada tanaman di petak.</summary>
    public bool HasPlant { get; private set; }

    /// <summary>Tahap pertumbuhan, mulai 0. Hanya berarti bila HasPlant.</summary>
    public int Stage { get; private set; }

    /// <summary>True bila tanaman sudah disiram (tetap true sampai dipanen; cukup disiram sekali).</summary>
    public bool IsWatered { get; private set; }

    /// <summary>True bila tanaman sudah di tahap terakhir dan siap panen.</summary>
    public bool IsMature => HasPlant && Stage >= stageCount - 1;

    /// <summary>Kemajuan tumbuh 0 sampai 1 (0 bila belum disiram, 1 bila matang). Untuk prompt dan SaveSystem nanti.</summary>
    public float GrowthProgress
    {
        get
        {
            if (!HasPlant || !IsWatered) return 0f;
            if (IsMature || growDuration <= 0f) return 1f;
            return Mathf.Clamp01(growElapsed / growDuration);
        }
    }

    /// <summary>Sisa detik sampai matang (durasi penuh bila belum disiram, 0 bila matang atau tanpa tanaman).</summary>
    public float GrowSecondsRemaining
    {
        get
        {
            if (!HasPlant || IsMature) return 0f;
            return Mathf.Max(0f, growDuration - growElapsed);
        }
    }

    // Popup petak naik sedikit lebih tinggi agar tidak menimpa label status.
    private Vector3 PopupAnchor => FloatingPopupManager.AnchorAbove(this) + Vector3.up * 0.45f;

    public string InteractionPrompt
    {
        get
        {
            if (!HasPlant) return "Petak kosong (pilih bibit, tombol 3)";
            if (IsMature) return "Panen kentang";
            return IsWatered ? "Periksa tanaman (" + Mathf.RoundToInt(GrowthProgress * 100f) + "%)" : "Siram tanaman";
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
        growElapsed = 0f;
        growDuration = Random.Range(growSecondsMin, growSecondsMax); // diundi saat tanam agar durasi langsung tampil; tumbuh baru jalan setelah disiram
        RefreshVisual();
        FloatingPopupManager.ShowAmount(PopupAnchor, PopupIcon.Seed, "Bibit", -1f);
        ShowMessage("Bibit kentang ditanam. Siram tanaman sekali agar ia mulai tumbuh.");
    }

    private void Awake()
    {
        if (spriteRenderer != null) baseColor = spriteRenderer.color;
        if (growingSprite != null || matureSprite != null) CreatePlantRenderer();
    }

    // Tanaman digambar pada objek anak di atas petak agar petak tetap tampil sebagai tanah dan sprite tanaman tidak ikut meregang.
    private void CreatePlantRenderer()
    {
        GameObject go = new GameObject("PlantSprite");
        go.transform.SetParent(transform, false);
        go.transform.localScale = new Vector3(plantScale, plantScale, 1f);

        plantRenderer = go.AddComponent<SpriteRenderer>();
        plantRenderer.enabled = false;
        if (spriteRenderer != null)
        {
            plantRenderer.sharedMaterial = spriteRenderer.sharedMaterial;
            plantRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            plantRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
        }
    }

    private void Start()
    {
        if (dialogueUI == null) dialogueUI = FindFirstObjectByType<DialogueUI>();

        time = TimeManager.Instance; // hanya untuk IsPaused; tanpa TimeManager tanaman tetap tumbuh

        if (potatoMax < potatoMin) potatoMax = potatoMin;
        if (seedMax < seedMin) seedMax = seedMin;
        if (growSecondsMax < growSecondsMin) growSecondsMax = growSecondsMin;

        RefreshVisual();
        CreateStatusLabel();
    }

    private void CreateStatusLabel()
    {
        if (!showStatusLabel) return;

        statusLabel = FloatingPopupManager.CreateWorldLabel("StatusLabel", transform, statusLabelSize, 1);
        statusShadow = FloatingPopupManager.CreateWorldLabel("StatusShadow", transform, statusLabelSize, 0);
        if (statusLabel == null || statusShadow == null) return;

        Vector3 anchor = FloatingPopupManager.AnchorAbove(this);
        statusLabel.transform.position = anchor;
        statusShadow.transform.position = anchor + new Vector3(0.025f, -0.025f, 0f);
        statusShadow.color = new Color(0f, 0f, 0f, 0.85f);
        RefreshStatusLabel();
    }

    // Teks status di atas petak: kosong (tersembunyi), "Belum disiram (M:SS)", "Matang M:SS", "Siap panen!".
    private void RefreshStatusLabel()
    {
        if (statusLabel == null) return;

        string text = string.Empty;
        Color color = growingColor;

        if (HasPlant)
        {
            if (IsMature)
            {
                text = "Siap panen!";
                color = readyColor;
            }
            else
            {
                int seconds = Mathf.CeilToInt(GrowSecondsRemaining);
                string clock = (seconds / 60) + ":" + (seconds % 60).ToString("00");
                if (IsWatered)
                {
                    text = "Matang " + clock;
                }
                else
                {
                    text = "Belum disiram (" + clock + ")";
                    color = waitingColor;
                }
            }
        }

        if (text == lastStatus) return;
        lastStatus = text;

        statusLabel.text = text;
        statusLabel.color = color;
        statusShadow.text = text;
    }

    private void Update()
    {
        RefreshStatusLabel();

        if (!HasPlant || !IsWatered || IsMature) return;
        if (time != null && time.IsPaused) return; // tidur atau minigame: waktu tumbuh ikut berhenti

        growElapsed += Time.deltaTime;

        if (growElapsed >= growDuration)
        {
            Stage = stageCount - 1;
            RefreshVisual();
            return;
        }

        // Tahap visual antara: bagi durasi rata ke (stageCount - 1) tahap sebelum matang.
        int stage = Mathf.Min(stageCount - 2, Mathf.FloorToInt(growElapsed / growDuration * (stageCount - 1)));
        if (stage != Stage)
        {
            Stage = stage;
            RefreshVisual();
        }
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
            ShowMessage("Tanaman sudah disiram dan sedang tumbuh (" + Mathf.RoundToInt(GrowthProgress * 100f) +
                " persen, tahap " + (Stage + 1) + " dari " + stageCount + "). Cukup disiram sekali; tunggu sampai matang.");
            return;
        }

        if (!resources.TryConsume(ResourceType.Water, waterPerWatering))
        {
            ShowMessage("Water tidak cukup untuk menyiram (butuh " + Mathf.CeilToInt(waterPerWatering) +
                ", tersedia " + Mathf.FloorToInt(resources.Get(ResourceType.Water)) + ").");
            return;
        }

        IsWatered = true;
        growElapsed = 0f;
        rolledPotatoes = Random.Range(potatoMin, potatoMax + 1); // batas atas int bersifat eksklusif
        rolledSeeds = Random.Range(seedMin, seedMax + 1);
        RefreshVisual();
        FloatingPopupManager.ShowAmount(PopupAnchor, PopupIcon.Water, "Water", -waterPerWatering);
        ShowMessage("Tanaman disiram memakai " + Mathf.CeilToInt(waterPerWatering) +
            " Water. Ia mulai tumbuh dan akan matang dalam sekitar 1 sampai 2 menit.");
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
        if (space < rolledPotatoes)
        {
            ShowMessage("Tas tidak muat " + rolledPotatoes + " kentang (ruang " + space +
                "). Simpan kentang di storage dulu, lalu panen lagi.");
            return;
        }

        int potatoes = inventory.AddToBag(ItemType.Potato, rolledPotatoes);
        int seedsInBag = inventory.AddToBag(ItemType.Seed, rolledSeeds);
        int seedsStored = seedsInBag < rolledSeeds ? inventory.AddToStorage(ItemType.Seed, rolledSeeds - seedsInBag) : 0;

        HasPlant = false;
        Stage = 0;
        IsWatered = false;
        growElapsed = 0f;
        RefreshVisual();

        Vector3 popupAnchor = PopupAnchor;
        FloatingPopupManager.ShowAmount(popupAnchor, PopupIcon.Potato, "Kentang", potatoes);
        FloatingPopupManager.ShowAmount(popupAnchor, PopupIcon.Seed, "Bibit", seedsInBag + seedsStored);

        string message = "Kentang dipanen: " + potatoes + " kentang masuk ke tas.";
        if (seedsInBag > 0) message += " Bibit +" + seedsInBag + " masuk ke tas.";
        if (seedsStored > 0) message += " Bibit +" + seedsStored + " masuk ke storage (tas penuh).";
        ShowMessage(message + " Petak kosong lagi dan siap ditanami.");
    }

    private void RefreshVisual()
    {
        // Mode sprite tanaman: sebelum siap panen memakai growingSprite, siap panen memakai matureSprite. Tanah tetap berwarna kosong.
        if (plantRenderer != null)
        {
            Sprite plant = !HasPlant ? null : (IsMature ? matureSprite : growingSprite);
            plantRenderer.sprite = plant;
            plantRenderer.enabled = plant != null;
            if (spriteRenderer != null) spriteRenderer.color = baseColor * emptyTint;
            return;
        }

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
