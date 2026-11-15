using System;
using UnityEngine;

/// <summary>
/// Water Purifier di Farming Area: Ice + Energy -> Water (stok Water bersama).
/// Tekan E memulai satu proses: Ice dan Energy langsung dikurangi, Water masuk setelah beberapa detik.
/// Tidak berjalan bila Ice atau Energy kurang, atau tangki Water tidak muat; pemain diberi alasannya.
/// Proses ikut jeda TimeManager.IsPaused. Hanya logika: tampilan lewat event OnProcessingChanged
/// atau objek indikator opsional. Semua angka adalah USULAN.
/// Pasang pada objek ber-layer Interactable dengan Collider2D.
/// </summary>
public class WaterPurifier : MonoBehaviour, IInteractable
{
    [Header("Referensi (otomatis dicari jika kosong)")]
    [SerializeField] private DialogueUI dialogueUI;

    [Header("Aturan mesin (USULAN, ubah lewat Inspector setelah playtest)")]
    [SerializeField, Min(0f)] private float iceCost = 1f;
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

    /// <summary>Ringkasan status untuk Resource Monitor.</summary>
    public string StatusText => IsProcessing ? "bekerja, sisa " + Mathf.CeilToInt(RemainingSeconds) + " dtk" : "siap";

    /// <summary>Dipanggil saat mesin mulai/berhenti bekerja (parameter: true bila bekerja).</summary>
    public event Action<bool> OnProcessingChanged;

    public string InteractionPrompt =>
        IsProcessing ? "Water Purifier bekerja... " + Mathf.CeilToInt(RemainingSeconds) + " dtk" : "Proses Ice menjadi Water";

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
        ResourceManager resources = ResourceManager.Instance;
        if (resources == null)
        {
            Debug.LogError("WaterPurifier: ResourceManager tidak ada di scene.", this);
            return;
        }

        if (IsProcessing)
        {
            ShowMessage("Water Purifier sedang bekerja. Sisa " + Mathf.CeilToInt(RemainingSeconds) + " detik.");
            return;
        }

        if (!resources.Has(ResourceType.Ice, iceCost))
        {
            ShowMessage("Tidak ada Ice untuk diproses (butuh " + Mathf.CeilToInt(iceCost) +
                "). Kumpulkan es di Outdoor.");
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

        // Semua syarat sudah dicek di atas, jadi kedua TryConsume pasti berhasil.
        resources.TryConsume(ResourceType.Ice, iceCost);
        resources.TryConsume(ResourceType.Energy, energyCost);

        timer = 0f;
        SetProcessing(true);

        ShowMessage("Memproses " + Mathf.CeilToInt(iceCost) + " Ice dan " + Mathf.CeilToInt(energyCost) +
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
