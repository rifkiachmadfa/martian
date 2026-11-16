using UnityEngine;

/// <summary>
/// Pengingat TARS: "lepas suit" bila pemain mengenakan suit di dalam habitat terlalu lama, karena
/// oksigen suit terus berkurang selama suit dikenakan (revisi GDD 2026-11-15).
/// Pesan memakai dialog TARS yang sudah ada (teks tetap, tanpa AI generatif). Hitungan hanya berjalan saat
/// suit dikenakan, pemain di dalam habitat, game tidak dijeda, dan tidak ada dialog terbuka; hitungan diulang
/// dari awal begitu pemain melepas suit atau keluar. Pesan berikutnya muncul setelah jeda ulang (cooldown).
/// DIPINDAHKAN ke TARSSystem (Milestone 11, 11.3.1): bila TARSSystem ada di scene, komponen ini menonaktifkan diri
/// agar pengingat tidak ganda. Komponen ini boleh dihapus dari objek SuitSystem.
/// </summary>
public class SuitReminder : MonoBehaviour
{
    [Header("Referensi (otomatis dicari jika kosong)")]
    [SerializeField] private SuitSystem suit;
    [SerializeField] private DialogueUI dialogueUI;

    [Header("Waktu pengingat (USULAN, ubah lewat Inspector setelah playtest)")]
    [Tooltip("Berapa detik pemain mengenakan suit di dalam habitat sebelum pengingat pertama.")]
    [SerializeField, Min(1f)] private float firstReminderSeconds = 30f;
    [Tooltip("Jeda antar pengingat berikutnya bila suit masih dikenakan di dalam habitat (cooldown).")]
    [SerializeField, Min(1f)] private float repeatReminderSeconds = 90f;

    private float indoorSeconds;
    private float currentThreshold;

    private void Start()
    {
        if (FindFirstObjectByType<TARSSystem>() != null)
        {
            enabled = false;
            return;
        }

        if (suit == null) suit = FindFirstObjectByType<SuitSystem>();
        if (dialogueUI == null) dialogueUI = FindFirstObjectByType<DialogueUI>();

        if (suit == null)
        {
            Debug.LogError("SuitReminder: SuitSystem tidak ditemukan. Jalankan menu Mars > Setup Suit and Gate.", this);
            enabled = false;
            return;
        }

        currentThreshold = firstReminderSeconds;
    }

    private void Update()
    {
        // Hanya relevan saat suit dikenakan di dalam habitat; selain itu hitungan dimulai ulang.
        if (!suit.IsEquipped || suit.IsOutdoors)
        {
            indoorSeconds = 0f;
            currentThreshold = firstReminderSeconds;
            return;
        }

        // Jangan menghitung saat dialog terbuka atau transisi tidur, dan jangan menumpuk dialog.
        TimeManager time = TimeManager.Instance;
        if (time != null && time.IsPaused) return;
        if (dialogueUI != null && dialogueUI.IsOpen) return;

        indoorSeconds += Time.deltaTime;
        if (indoorSeconds < currentThreshold) return;

        indoorSeconds = 0f;
        currentThreshold = repeatReminderSeconds;
        ShowReminder();
    }

    private void ShowReminder()
    {
        string line = "Anda masih mengenakan suit di dalam habitat. Oksigen suit terus berkurang. " +
                      "Lepas suit di Suit Station jika Anda tidak akan keluar.";

        ResourceManager resources = ResourceManager.Instance;
        if (resources != null)
            line += " Oksigen suit: " + Mathf.RoundToInt(resources.Get(ResourceType.SuitOxygen)) + " / " +
                    Mathf.RoundToInt(resources.GetMax(ResourceType.SuitOxygen)) + ".";

        if (dialogueUI == null)
        {
            Debug.Log("TARS: " + line);
            return;
        }

        dialogueUI.Show("TARS", new[] { line }, null);
    }
}
