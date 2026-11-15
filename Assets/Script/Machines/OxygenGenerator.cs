using System;
using UnityEngine;

/// <summary>
/// Oxygen Generator di Farming Area: Water + Energy -> Oxygen habitat (stok HabitatOxygen, terpisah dari
/// oksigen suit). Bekerja OTOMATIS selama menyala dan resource cukup (GDD 11.2): tiap siklus mengurangi
/// Water dan Energy lalu menambah HabitatOxygen. Berhenti sendiri bila Water/Energy kurang atau oksigen penuh.
/// Tombol E menyalakan/mematikan mesin, supaya pemain bisa menghemat Water untuk tanaman (GDD 7.1 membahas
/// pembagian Water). Tombol ini USULAN, belum ada di GDD; matikan dengan Allow Manual Toggle di Inspector.
/// Siklus ikut jeda TimeManager.IsPaused. Hanya logika: tampilan lewat OnRunningChanged / indikator opsional.
/// Semua angka adalah USULAN. Pasang pada objek ber-layer Interactable dengan Collider2D.
/// </summary>
public class OxygenGenerator : MonoBehaviour, IInteractable
{
    [Header("Referensi (otomatis dicari jika kosong)")]
    [SerializeField] private DialogueUI dialogueUI;

    [Header("Aturan mesin (USULAN, ubah lewat Inspector setelah playtest)")]
    [SerializeField, Min(0f)] private float waterCostPerCycle = 1f;
    [SerializeField, Min(0f)] private float energyCostPerCycle = 2f;
    [SerializeField, Min(0f)] private float oxygenPerCycle = 5f;
    [SerializeField, Min(0.1f)] private float cycleSeconds = 5f;
    [SerializeField] private bool startOn = true;
    [Tooltip("Jika dimatikan, mesin selalu menyala dan tombol E hanya menampilkan status.")]
    [SerializeField] private bool allowManualToggle = true;

    [Header("Tampilan opsional")]
    [Tooltip("Objek yang dinyalakan selama mesin benar-benar menghasilkan oksigen.")]
    [SerializeField] private GameObject activeIndicator;
    [SerializeField, Min(0f)] private float reinteractCooldown = 0.3f;

    private float cycleTimer;
    private float nextInteractTime;

    /// <summary>Saklar mesin (menyala/mati oleh pemain).</summary>
    public bool IsOn { get; private set; }

    /// <summary>True bila mesin sedang benar-benar bekerja (menyala dan semua syarat terpenuhi).</summary>
    public bool IsRunning { get; private set; }

    /// <summary>Dipanggil saat IsRunning berubah.</summary>
    public event Action<bool> OnRunningChanged;

    /// <summary>Ringkasan status untuk dialog dan Resource Monitor.</summary>
    public string StatusText
    {
        get
        {
            if (!IsOn) return "dimatikan";
            string blocker = GetBlocker(ResourceManager.Instance);
            return blocker ?? "bekerja";
        }
    }

    public string InteractionPrompt
    {
        get
        {
            if (!allowManualToggle) return "Periksa Oxygen Generator";
            return IsOn ? "Matikan Oxygen Generator" : "Nyalakan Oxygen Generator";
        }
    }

    private void Awake()
    {
        IsOn = startOn || !allowManualToggle;
    }

    private void Start()
    {
        if (dialogueUI == null) dialogueUI = FindFirstObjectByType<DialogueUI>();
        if (activeIndicator != null) activeIndicator.SetActive(false);
    }

    private void Update()
    {
        ResourceManager resources = ResourceManager.Instance;
        if (resources == null) return;

        TimeManager time = TimeManager.Instance;
        if (time != null && time.IsPaused) return;

        bool canRun = IsOn && GetBlocker(resources) == null;
        SetRunning(canRun);

        if (!canRun)
        {
            cycleTimer = 0f;
            return;
        }

        cycleTimer += Time.deltaTime;
        if (cycleTimer < cycleSeconds) return;

        cycleTimer -= cycleSeconds;

        // Syarat sudah dicek lewat GetBlocker di frame yang sama, jadi kedua TryConsume pasti berhasil.
        resources.TryConsume(ResourceType.Water, waterCostPerCycle);
        resources.TryConsume(ResourceType.Energy, energyCostPerCycle);
        resources.Add(ResourceType.HabitatOxygen, oxygenPerCycle);
    }

    public bool CanInteract(GameObject interactor)
    {
        if (Time.time < nextInteractTime) return false;
        return dialogueUI == null || !dialogueUI.IsOpen;
    }

    public void Interact(GameObject interactor)
    {
        if (allowManualToggle) IsOn = !IsOn;

        string message = allowManualToggle
            ? (IsOn ? "Oxygen Generator dinyalakan. " : "Oxygen Generator dimatikan. Water dan Energy tidak dipakai mesin ini. ")
            : "";

        message += "Status: " + StatusText + ". Setiap " + Mathf.CeilToInt(cycleSeconds) + " detik mesin memakai " +
            Mathf.CeilToInt(waterCostPerCycle) + " Water dan " + Mathf.CeilToInt(energyCostPerCycle) +
            " Energy untuk menghasilkan " + Mathf.CeilToInt(oxygenPerCycle) + " Oxygen habitat.";

        ShowMessage(message);
    }

    // Mengembalikan alasan mesin tidak bisa bekerja, atau null bila semua syarat terpenuhi.
    private string GetBlocker(ResourceManager resources)
    {
        if (resources == null) return "ResourceManager tidak ada";
        if (!resources.Has(ResourceType.Water, waterCostPerCycle)) return "berhenti, Water tidak cukup";
        if (!resources.Has(ResourceType.Energy, energyCostPerCycle)) return "berhenti, Energy tidak cukup";
        if (resources.Get(ResourceType.HabitatOxygen) >= resources.GetMax(ResourceType.HabitatOxygen) - 0.001f)
            return "siaga, oksigen habitat sudah penuh";
        return null;
    }

    private void SetRunning(bool running)
    {
        if (IsRunning == running) return;

        IsRunning = running;
        if (activeIndicator != null) activeIndicator.SetActive(running);
        OnRunningChanged?.Invoke(running);
    }

    private void ShowMessage(string message)
    {
        if (dialogueUI == null)
        {
            Debug.Log("OxygenGenerator: " + message);
            nextInteractTime = Time.time + reinteractCooldown;
            return;
        }

        dialogueUI.Show("Oxygen Generator", new[] { message }, () => nextInteractTime = Time.time + reinteractCooldown);
    }
}
