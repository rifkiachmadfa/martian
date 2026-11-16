using System;
using UnityEngine;

/// <summary>
/// Water Purifier di Farming Area: Es + Energy -> Water (stok Water bersama).
/// Pilih Es di tas (tombol 2), dekati mesin, lalu tekan E untuk memulai satu proses: Es (dari tas) dan Energy
/// langsung dikurangi, Water masuk setelah beberapa detik.
/// Tidak berjalan bila Ice atau Energy kurang, atau tangki Water tidak muat; pemain diberi alasannya.
/// Proses ikut jeda TimeManager.IsPaused. Hanya logika: tampilan lewat event OnProcessingChanged
/// atau objek indikator opsional. Semua angka adalah USULAN.
/// Pasang pada objek ber-layer Interactable dengan Collider2D.
/// </summary>
public class WaterPurifier : MonoBehaviour, IInteractable, IItemReceiver
{
    [Header("Referensi (otomatis dicari jika kosong)")]
    [SerializeField] private DialogueUI dialogueUI;

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

    /// <summary>Ringkasan status (dipakai dialog; disiapkan juga untuk TARSSystem).</summary>
    public string StatusText => IsProcessing ? "bekerja, sisa " + Mathf.CeilToInt(RemainingSeconds) + " dtk" : "siap";

    /// <summary>Dipanggil saat mesin mulai/berhenti bekerja (parameter: true bila bekerja).</summary>
    public event Action<bool> OnProcessingChanged;

    public string InteractionPrompt =>
        IsProcessing ? "Water Purifier bekerja... " + Mathf.CeilToInt(RemainingSeconds) + " dtk" : "Water Purifier (pilih es, tombol 2)";

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
        if (Time.time < nextInteractTime) return false;
        return dialogueUI == null || !dialogueUI.IsOpen;
    }

    public void Interact(GameObject interactor)
    {
        if (IsProcessing)
        {
            ShowMessage("Water Purifier sedang bekerja. Sisa " + Mathf.CeilToInt(RemainingSeconds) + " detik.");
            return;
        }

        ShowMessage("Pilih es di tas (tombol 2), lalu tekan E untuk memasukkannya ke Water Purifier.");
    }

    // Dipanggil lewat UseItem (pemain memilih Es di tas lalu menekan E di dekat mesin).
    private void StartProcess()
    {
        ResourceManager resources = ResourceManager.Instance;
        InventorySystem inventory = InventorySystem.Instance;
        if (resources == null || inventory == null)
        {
            Debug.LogError("WaterPurifier: ResourceManager atau InventorySystem tidak ada di scene.", this);
            return;
        }

        if (IsProcessing)
        {
            ShowMessage("Water Purifier sedang bekerja. Sisa " + Mathf.CeilToInt(RemainingSeconds) + " detik.");
            return;
        }

        if (inventory.GetBag(ItemType.Ice) < iceCost)
        {
            ShowMessage("Tidak ada es di tas (butuh " + iceCost + "). Kumpulkan es di Outdoor atau ambil dari storage.");
            return;
        }

        if (!resources.Has(ResourceType.Energy, energyCost))
        {
            ShowMessage("Energy tidak cukup (butuh " + Mathf.CeilToInt(energyCost) + ", tersedia " +
                Mathf.FloorToInt(resources.Get(ResourceType.Energy)) + "). Periksa Solar Panel.");
            return;
        }

        float freeWater = resources.GetMax(ResourceType.Water) - resources.Get(ResourceType.Water);
        if (freeWater < waterOutput - 0.001f)
        {
            ShowMessage("Tangki Water hampir penuh (muat " + Mathf.FloorToInt(freeWater) + ", hasil " +
                Mathf.CeilToInt(waterOutput) + "). Pakai Water dulu.");
            return;
        }

        // Semua syarat sudah dicek di atas, jadi kedua pengurangan pasti berhasil.
        inventory.RemoveFromBag(ItemType.Ice, iceCost);
        resources.TryConsume(ResourceType.Energy, energyCost);

        timer = 0f;
        SetProcessing(true);

        Vector3 popupAnchor = FloatingPopupManager.AnchorAbove(this);
        FloatingPopupManager.ShowAmount(popupAnchor, PopupIcon.Ice, "Es", -iceCost);
        FloatingPopupManager.ShowAmount(popupAnchor, PopupIcon.Energy, "Energy", -energyCost);

        ShowMessage("Memproses " + iceCost + " es dan " + Mathf.CeilToInt(energyCost) +
            " Energy menjadi " + Mathf.CeilToInt(waterOutput) + " Water. Selesai dalam " +
            Mathf.CeilToInt(processSeconds) + " detik.");
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
