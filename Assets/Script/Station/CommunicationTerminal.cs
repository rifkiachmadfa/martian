using System;
using UnityEngine;

/// <summary>
/// Terminal komunikasi di Control Room (GDD 3.1): menampilkan pesan singkat dari Bumi sesuai hari ini.
/// HANYA elemen cerita: bukan quest, tidak memberi tugas atau urutan aksi, dan tidak mengubah resource.
/// Teks bawaan di bawah adalah PLACEHOLDER untuk diganti developer (Inspector). Pesan konfirmasi penjemputan
/// di akhir Day 7 milik ending Victory (belum dibuat), bukan terminal ini.
/// Pasang pada objek ber-layer Interactable dengan Collider2D.
/// </summary>
public class CommunicationTerminal : MonoBehaviour, IInteractable
{
    /// <summary>Pesan untuk satu hari: beberapa baris, tampil satu per satu di dialog.</summary>
    [Serializable]
    public class DayMessage
    {
        [TextArea(2, 5)] public string[] lines;

        public DayMessage(params string[] lines)
        {
            this.lines = lines;
        }
    }

    [Header("Referensi (otomatis dicari jika kosong)")]
    [SerializeField] private DialogueUI dialogueUI;

    [Header("Pesan dari Bumi (PLACEHOLDER; indeks 0 = Day 1)")]
    [SerializeField] private string sender = "Pusat Kendali Bumi";
    [SerializeField] private DayMessage[] messages =
    {
        new DayMessage(
            "Habitat, di sini Pusat Kendali Bumi. Sinyal Anda diterima, tetapi komunikasi masih terbatas.",
            "Persediaan awal tidak akan cukup tanpa pengelolaan yang baik. Gunakan sumber daya dengan bijak."),
        new DayMessage(
            "Kami masih memantau habitat Anda dari sini.",
            "Persiapan tim penjemputan sedang berjalan."),
        new DayMessage(
            "Perjalanan tim penjemputan membutuhkan waktu. Kami akan terus menghubungi Anda."),
        new DayMessage(
            "Sinyal hari ini cukup stabil. Kami menerima data habitat Anda."),
        new DayMessage(
            "Tim penjemputan melanjutkan perjalanan sesuai rencana."),
        new DayMessage(
            "Tim penjemputan semakin dekat ke Mars."),
        new DayMessage(
            "Tim penjemputan hampir tiba. Kami menunggu kabar dari Anda di akhir hari ini."),
    };

    [SerializeField, Min(0f)] private float reinteractCooldown = 0.3f;

    private float nextInteractTime;

    public string InteractionPrompt => "Buka terminal komunikasi";

    private void Start()
    {
        if (dialogueUI == null) dialogueUI = FindFirstObjectByType<DialogueUI>();
    }

    public bool CanInteract(GameObject interactor)
    {
        if (Time.time < nextInteractTime) return false;
        return dialogueUI == null || !dialogueUI.IsOpen;
    }

    public void Interact(GameObject interactor)
    {
        TimeManager time = TimeManager.Instance;
        int day = time != null ? time.CurrentDay : 1;

        string[] lines = GetLines(day);
        if (lines == null || lines.Length == 0)
            lines = new[] { "Tidak ada pesan baru dari Bumi." };

        if (dialogueUI == null)
        {
            Debug.Log("CommunicationTerminal: " + string.Join(" ", lines));
            nextInteractTime = Time.time + reinteractCooldown;
            return;
        }

        dialogueUI.Show(sender, lines, () => nextInteractTime = Time.time + reinteractCooldown);
    }

    // Hari di luar daftar memakai pesan terakhir (atau tidak ada pesan bila daftar kosong).
    private string[] GetLines(int day)
    {
        if (messages == null || messages.Length == 0) return null;

        int index = Mathf.Clamp(day - 1, 0, messages.Length - 1);
        return messages[index] != null ? messages[index].lines : null;
    }
}
