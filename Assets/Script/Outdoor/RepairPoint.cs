using UnityEngine;

/// <summary>
/// Titik perbaikan sebuah Solar Panel di Outdoor. Tekan E: bila panel rusak, panel diperbaiki.
/// SEMENTARA perbaikan langsung berhasil. Milestone minigame kabel (3-4 kabel, drag-and-drop mouse,
/// sekitar 15-30 detik, timer dijeda lewat TimeManager.ExternalPause) akan memanggil SolarPanel.Repair()
/// setelah semua kabel benar, menggantikan baris perbaikan langsung di Interact().
/// Pasang pada objek ber-layer Interactable dengan Collider2D; isi Panel dengan SolarPanel yang dilayani.
/// </summary>
public class RepairPoint : MonoBehaviour, IInteractable
{
    [SerializeField] private SolarPanel panel;

    [Header("Referensi (otomatis dicari jika kosong)")]
    [SerializeField] private DialogueUI dialogueUI;

    [SerializeField, Min(0f)] private float reinteractCooldown = 0.3f;

    private float nextInteractTime;

    public SolarPanel Panel => panel;

    public string InteractionPrompt =>
        panel != null && panel.IsDamaged ? "Perbaiki Solar Panel" : "Periksa Solar Panel";

    private void Start()
    {
        if (dialogueUI == null) dialogueUI = FindFirstObjectByType<DialogueUI>();

        if (panel == null)
            Debug.LogError("RepairPoint: Solar Panel belum diisi di Inspector.", this);
    }

    public bool CanInteract(GameObject interactor)
    {
        if (panel == null || Time.time < nextInteractTime) return false;
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

        panel.Repair(); // SEMENTARA: nanti dipanggil setelah minigame kabel selesai.
        ShowMessage("Solar Panel diperbaiki. Produksi Energy kembali normal (" +
            panel.CurrentEnergyPerSecond.ToString("0.0") + " per detik).");
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
