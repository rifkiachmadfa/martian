using UnityEngine;

/// <summary>
/// Titik perbaikan sebuah Solar Panel di Outdoor. Tekan E: MachinePanelUI Solar Panel terbuka (status, output,
/// Energy); tombol PERBAIKI pada panel (aktif bila panel rusak) membuka minigame kabel (WiringMinigameUI), dan panel
/// diperbaiki (SolarPanel.Repair()) hanya setelah semua kabel tersambung. Esc membatalkan.
/// Tanpa MachinePanelUI di scene, E langsung membuka minigame bila panel rusak (perilaku lama, dialog bila normal).
/// Bila WiringMinigameUI tidak ada di scene, perbaikan langsung berhasil (cadangan, dengan peringatan di Console).
/// Pasang pada objek ber-layer Interactable dengan Collider2D; isi Panel dengan SolarPanel yang dilayani.
/// </summary>
public class RepairPoint : MonoBehaviour, IInteractable
{
    [SerializeField] private SolarPanel panel;

    [Header("Referensi (otomatis dicari jika kosong)")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private WiringMinigameUI minigame;
    [SerializeField] private MachinePanelUI panelUI;

    [SerializeField, Min(0f)] private float reinteractCooldown = 0.3f;

    private float nextInteractTime;

    public SolarPanel Panel => panel;

    public string InteractionPrompt
    {
        get
        {
            bool damaged = panel != null && panel.IsDamaged;
            if (panelUI != null) return damaged ? "Periksa Solar Panel (RUSAK)" : "Periksa Solar Panel";
            return damaged ? "Perbaiki Solar Panel" : "Periksa Solar Panel";
        }
    }

    private void Start()
    {
        if (dialogueUI == null) dialogueUI = FindFirstObjectByType<DialogueUI>();
        if (minigame == null) minigame = FindFirstObjectByType<WiringMinigameUI>();
        if (panelUI == null) panelUI = FindFirstObjectByType<MachinePanelUI>();

        if (minigame == null)
            Debug.LogWarning("RepairPoint: WiringMinigameUI tidak ditemukan; perbaikan akan langsung berhasil. Jalankan menu Mars > Setup Panel Damage and Wiring Minigame.", this);

        if (panel == null)
            Debug.LogError("RepairPoint: Solar Panel belum diisi di Inspector.", this);
    }

    public bool CanInteract(GameObject interactor)
    {
        if (panel == null || Time.time < nextInteractTime || MachinePanelUI.IsOpen) return false;
        if (minigame != null && minigame.IsOpen) return false;
        return dialogueUI == null || !dialogueUI.IsOpen;
    }

    public void Interact(GameObject interactor)
    {
        if (panel == null) return;

        if (panelUI != null && panelUI.Open(panel, BeginRepair, OnPanelClosed, null)) return;

        if (!panel.IsDamaged)
        {
            ShowMessage("Solar Panel berfungsi normal. Produksi: " +
                panel.CurrentEnergyPerSecond.ToString("0.0") + " Energy per detik.");
            return;
        }

        BeginRepair();
    }

    // Membuka minigame kabel (dipanggil langsung atau lewat tombol PERBAIKI di MachinePanelUI).
    private void BeginRepair()
    {
        if (panel == null || !panel.IsDamaged) return;

        if (minigame == null)
        {
            OnRepairSucceeded(); // cadangan: tanpa minigame di scene, perbaikan langsung berhasil.
            return;
        }

        minigame.Open(panel.name, OnRepairSucceeded, OnRepairCancelled);
    }

    private void OnPanelClosed()
    {
        nextInteractTime = Time.time + reinteractCooldown;
    }

    private void OnRepairSucceeded()
    {
        panel.Repair();
        FloatingPopupManager.Show(FloatingPopupManager.AnchorAbove(panel), PopupIcon.Repair, "Panel diperbaiki");

        // Dengan panel mesin: kembali ke panel agar hasilnya (status NORMAL, output) terlihat.
        if (panelUI != null && panelUI.Open(panel, BeginRepair, OnPanelClosed,
                "Panel diperbaiki. Produksi Energy kembali normal.")) return;

        ShowMessage("Solar Panel diperbaiki. Produksi Energy kembali normal (" +
            panel.CurrentEnergyPerSecond.ToString("0.0") + " per detik).");
    }

    private void OnRepairCancelled()
    {
        nextInteractTime = Time.time + reinteractCooldown;
    }

    private void ShowMessage(string message)
    {
        if (dialogueUI == null)
        {
            Debug.Log("RepairPoint: " + message);
            nextInteractTime = Time.time + reinteractCooldown;
            return;
        }

        dialogueUI.Show("Repair Point", new[] { message }, () => nextInteractTime = Time.time + reinteractCooldown);
    }
}
