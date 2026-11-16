using UnityEngine;

/// <summary>
/// Titik perbaikan sebuah Solar Panel di Outdoor. Tekan E: bila panel rusak, minigame kabel (WiringMinigameUI)
/// dibuka; panel diperbaiki (SolarPanel.Repair()) hanya setelah semua kabel tersambung. Esc membatalkan.
/// Bila WiringMinigameUI tidak ada di scene, perbaikan langsung berhasil (cadangan, dengan peringatan di Console).
/// Pasang pada objek ber-layer Interactable dengan Collider2D; isi Panel dengan SolarPanel yang dilayani.
/// </summary>
public class RepairPoint : MonoBehaviour, IInteractable
{
    [SerializeField] private SolarPanel panel;

    [Header("Referensi (otomatis dicari jika kosong)")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private WiringMinigameUI minigame;

    [SerializeField, Min(0f)] private float reinteractCooldown = 0.3f;

    private float nextInteractTime;

    public SolarPanel Panel => panel;

    public string InteractionPrompt =>
        panel != null && panel.IsDamaged ? "Perbaiki Solar Panel" : "Periksa Solar Panel";

    private void Start()
    {
        if (dialogueUI == null) dialogueUI = FindFirstObjectByType<DialogueUI>();
        if (minigame == null) minigame = FindFirstObjectByType<WiringMinigameUI>();

        if (minigame == null)
            Debug.LogWarning("RepairPoint: WiringMinigameUI tidak ditemukan; perbaikan akan langsung berhasil. Jalankan menu Mars > Setup Panel Damage and Wiring Minigame.", this);

        if (panel == null)
            Debug.LogError("RepairPoint: Solar Panel belum diisi di Inspector.", this);
    }

    public bool CanInteract(GameObject interactor)
    {
        if (panel == null || Time.time < nextInteractTime) return false;
        if (minigame != null && minigame.IsOpen) return false;
        return dialogueUI == null || !dialogueUI.IsOpen;
    }

    public void Interact(GameObject interactor)
    {
        if (panel == null) return;

        if (!panel.IsDamaged)
        {
            ShowMessage("Solar Panel berfungsi normal. Produksi: " +
                panel.CurrentEnergyPerSecond.ToString("0.0") + " Energy per detik.");
            return;
        }

        if (minigame == null)
        {
            OnRepairSucceeded(); // cadangan: tanpa minigame di scene, perbaikan langsung berhasil.
            return;
        }

        minigame.Open(panel.name, OnRepairSucceeded, OnRepairCancelled);
    }

    private void OnRepairSucceeded()
    {
        panel.Repair();
        FloatingPopupManager.Show(FloatingPopupManager.AnchorAbove(panel), PopupIcon.Repair, "Panel diperbaiki");
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
