using System;
using UnityEngine;

/// <summary>
/// Water Purifier di Farming Area: Es + Energy -> Water (stok Water bersama).
/// Pilih Es di tas (tombol 2), dekati mesin, lalu tekan E untuk memulai satu proses: Es (dari tas) dan Energy
/// langsung dikurangi, Water masuk setelah beberapa detik.
/// Tidak berjalan bila Ice atau Energy kurang, atau tangki Water tidak muat; pemain diberi alasannya.
/// Tekan E tanpa es terpilih membuka MachinePanelUI (tombol PROSES, bar proses, Energy, Water); jalan pintas es
/// terpilih + E tetap ada. Proses ikut jeda TimeManager.IsPaused. Hanya logika: tampilan lewat event
/// OnProcessingChanged, MachinePanelUI, atau objek indikator opsional. Semua angka adalah USULAN.
/// Pasang pada objek ber-layer Interactable dengan Collider2D.
/// </summary>
public class WaterPurifier : MonoBehaviour, IInteractable, IItemReceiver
{
    [Header("Referensi (otomatis dicari jika kosong)")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private MachinePanelUI panelUI;

    [Header("Aturan mesin (USULAN, ubah lewat Inspector setelah playtest)")]
    [SerializeField, Min(1)] private int iceCost = 1;
    [SerializeField, Min(0f)] private float energyCost = 5f;
    [SerializeField, Min(0f)] private float waterOutput = 4f;
    [SerializeField, Min(0.1f)] private float processSeconds = 4f;

    [Header("Tampilan opsional")]
    [Tooltip("Objek yang dinyalakan selama mesin bekerja (misalnya lampu atau animasi).")]
    [SerializeField] private GameObject activeIndicator;
    [SerializeField, Min(0f)] private float reinteractCooldown = 0.3f;

    private float timer;
    private float nextInteractTime;

    /// <summary>True selama satu proses sedang berjalan.</summary>
    public bool IsProcessing { get; private set; }

    /// <summary>Sisa detik proses saat ini (0 bila tidak bekerja).</summary>
    public float RemainingSeconds => IsProcessing ? Mathf.Max(0f, processSeconds - timer) : 0f;

    /// <summary>Kemajuan proses 0 sampai 1 (0 bila tidak bekerja).</summary>
    public float Progress01 => IsProcessing ? Mathf.Clamp01(timer / processSeconds) : 0f;

    public int IceCost => iceCost;
    public float EnergyCost => energyCost;
    public float WaterOutput => waterOutput;
    public float ProcessSeconds => processSeconds;

    /// <summary>Ringkasan status (dipakai dialog; disiapkan juga untuk TARSSystem).</summary>
    public string StatusText => IsProcessing ? "bekerja, sisa " + Mathf.CeilToInt(RemainingSeconds) + " dtk" : "siap";

    /// <summary>Dipanggil saat mesin mulai/berhenti bekerja (parameter: true bila bekerja).</summary>
    public event Action<bool> OnProcessingChanged;

    public string InteractionPrompt =>
        IsProcessing ? "Water Purifier bekerja... " + Mathf.CeilToInt(RemainingSeconds) + " dtk"
            : (panelUI != null ? "Buka panel Water Purifier" : "Water Purifier (pilih es, tombol 2)");

    public bool CanUseItem(ItemType item) => item == ItemType.Ice;

    public string GetItemPrompt(ItemType item) =>
        IsProcessing ? "Water Purifier bekerja... " + Mathf.CeilToInt(RemainingSeconds) + " dtk" : "Masukkan es ke Water Purifier";

    public void UseItem(GameObject user, ItemType item)
    {
        if (item == ItemType.Ice) StartProcess();
    }

    private void Start()
    {
        if (dialogueUI == null) dialogueUI = FindFirstObjectByType<DialogueUI>();
        if (panelUI == null) panelUI = FindFirstObjectByType<MachinePanelUI>();
        if (activeIndicator != null) activeIndicator.SetActive(false);
    }

    private void Update()
    {
        if (!IsProcessing) return;

        TimeManager time = TimeManager.Instance;
        if (time != null && time.IsPaused) return;

        timer += Time.deltaTime;
        if (timer >= processSeconds) FinishProcessing();
    }

    public bool CanInteract(GameObject interactor)
    {
        // Saat bekerja tetap true agar prompt menampilkan sisa waktu; Interact() memberi pesan status.
        if (Time.time < nextInteractTime || MachinePanelUI.IsOpen) return false;
        return dialogueUI == null || !dialogueUI.IsOpen;
    }

    public void Interact(GameObject interactor)
    {
        if (panelUI != null && panelUI.Open(this, () => nextInteractTime = Time.time + reinteractCooldown)) return;

        if (IsProcessing)
        {
            ShowMessage("Water Purifier sedang bekerja. Sisa " + Mathf.CeilToInt(RemainingSeconds) + " detik.");
            return;
        }

        ShowMessage("Pilih es di tas (tombol 2), lalu tekan E untuk memasukkannya ke Water Purifier.");
    }

    /// <summary>
    /// Alasan proses tidak bisa dimulai sekarang (teks untuk pemain), atau null bila semua syarat terpenuhi.
    /// Tidak mengubah apa pun; aman dipanggil tiap frame oleh MachinePanelUI.
    /// </summary>
    public string GetStartBlocker()
    {
        ResourceManager resources = ResourceManager.Instance;
        InventorySystem inventory = InventorySystem.Instance;
        if (resources == null || inventory == null) return "ResourceManager atau InventorySystem tidak ada di scene.";

        if (IsProcessing) return "Water Purifier sedang bekerja. Sisa " + Mathf.CeilToInt(RemainingSeconds) + " detik.";

        if (inventory.GetBag(ItemType.Ice) < iceCost)
            return "Tidak ada es di tas (butuh " + iceCost + "). Kumpulkan es di Outdoor atau ambil dari storage.";

        if (!resources.Has(ResourceType.Energy, energyCost))
            return "Energy tidak cukup (butuh " + Mathf.CeilToInt(energyCost) + ", tersedia " +
                Mathf.FloorToInt(resources.Get(ResourceType.Energy)) + "). Periksa Solar Panel.";

        float freeWater = resources.GetMax(ResourceType.Water) - resources.Get(ResourceType.Water);
        if (freeWater < waterOutput - 0.001f)
            return "Tangki Water hampir penuh (muat " + Mathf.FloorToInt(freeWater) + ", hasil " +
                Mathf.CeilToInt(waterOutput) + "). Pakai Water dulu.";

        return null;
    }

    /// <summary>
    /// Memulai satu proses bila syarat terpenuhi (Es dari tas dan Energy langsung dikurangi). Mengembalikan false
    /// dan alasannya di message bila tidak bisa. Tidak membuka dialog; pemanggil yang menampilkan message.
    /// </summary>
    public bool TryStartProcess(out string message)
    {
        string blocker = GetStartBlocker();
        if (blocker != null)
        {
            message = blocker;
            return false;
        }

        ResourceManager resources = ResourceManager.Instance;
        InventorySystem inventory = InventorySystem.Instance;

        // Semua syarat sudah dicek di atas, jadi kedua pengurangan pasti berhasil.
        inventory.RemoveFromBag(ItemType.Ice, iceCost);
        resources.TryConsume(ResourceType.Energy, energyCost);

        timer = 0f;
        SetProcessing(true);

        Vector3 popupAnchor = FloatingPopupManager.AnchorAbove(this);
        FloatingPopupManager.ShowAmount(popupAnchor, PopupIcon.Ice, "Es", -iceCost);
        FloatingPopupManager.ShowAmount(popupAnchor, PopupIcon.Energy, "Energy", -energyCost);

        message = "Memproses " + iceCost + " es dan " + Mathf.CeilToInt(energyCost) +
            " Energy menjadi " + Mathf.CeilToInt(waterOutput) + " Water. Selesai dalam " +
            Mathf.CeilToInt(processSeconds) + " detik.";
        return true;
    }

    // Dipanggil lewat UseItem (pemain memilih Es di tas lalu menekan E di dekat mesin).
    private void StartProcess()
    {
        if (ResourceManager.Instance == null || InventorySystem.Instance == null)
        {
            Debug.LogError("WaterPurifier: ResourceManager atau InventorySystem tidak ada di scene.", this);
            return;
        }

        string message;
        TryStartProcess(out message);
        ShowMessage(message);
    }

    private void FinishProcessing()
    {
        ResourceManager resources = ResourceManager.Instance;
        float added = resources != null ? resources.Add(ResourceType.Water, waterOutput) : 0f;

        if (added < waterOutput - 0.001f)
            Debug.LogWarning("WaterPurifier: hanya " + added + " dari " + waterOutput + " Water yang masuk (tangki penuh).", this);

        timer = 0f;
        SetProcessing(false);
        FloatingPopupManager.ShowAmount(FloatingPopupManager.AnchorAbove(this), PopupIcon.Water, "Water", added);
    }

    private void SetProcessing(bool processing)
    {
        if (IsProcessing == processing) return;

        IsProcessing = processing;
        if (activeIndicator != null) activeIndicator.SetActive(processing);
        OnProcessingChanged?.Invoke(processing);
    }

    private void ShowMessage(string message)
    {
        if (dialogueUI == null)
        {
            Debug.Log("WaterPurifier: " + message);
            nextInteractTime = Time.time + reinteractCooldown;
            return;
        }

        dialogueUI.Show("Water Purifier", new[] { message }, () => nextInteractTime = Time.time + reinteractCooldown);
    }
}
