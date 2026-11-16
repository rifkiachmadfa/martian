using UnityEngine;

/// <summary>
/// Pengingat TARS soal lapar (GDD 6.3: peringatan harus muncul sebelum game over). Dialog dengan teks tetap
/// (tanpa AI generatif) saat kenyang turun melewati batas peringatan, batas bahaya, dan saat kelaparan (0).
/// Setiap batas hanya diumumkan sekali per penurunan; setelah pemain makan dan kenyang naik di atas batas itu,
/// batas tersebut bisa diumumkan lagi. Bila dialog lain sedang terbuka, pesan menunggu sampai dialog tertutup.
/// Sementara pengganti TARSSystem: saat TARSSystem dibuat, logika ini boleh dipindahkan ke sana.
/// </summary>
public class HungerReminder : MonoBehaviour
{
    [Header("Referensi (otomatis dicari jika kosong)")]
    [SerializeField] private HungerSystem hunger;
    [SerializeField] private DialogueUI dialogueUI;

    private int announcedLevel;
    private int pendingLevel;

    private void Start()
    {
        if (hunger == null) hunger = FindFirstObjectByType<HungerSystem>();
        if (dialogueUI == null) dialogueUI = FindFirstObjectByType<DialogueUI>();

        if (hunger == null)
        {
            Debug.LogError("HungerReminder: HungerSystem tidak ditemukan. Jalankan menu Mars > Setup Hunger and Food Storage.", this);
            enabled = false;
            return;
        }

        hunger.OnSatietyChanged += HandleSatietyChanged;
    }

    private void OnDestroy()
    {
        if (hunger != null) hunger.OnSatietyChanged -= HandleSatietyChanged;
    }

    private void HandleSatietyChanged(float satiety, float max)
    {
        int level = GetLevel();

        if (level < announcedLevel) announcedLevel = level; // pemain sudah makan; batas boleh diumumkan lagi
        pendingLevel = level > announcedLevel ? level : 0;
    }

    private void Update()
    {
        if (pendingLevel <= announcedLevel) return;
        if (dialogueUI != null && dialogueUI.IsOpen) return;

        announcedLevel = pendingLevel;
        pendingLevel = 0;
        Show(BuildLine(announcedLevel));
    }

    /// <summary>0 = aman, 1 = peringatan, 2 = bahaya, 3 = kelaparan.</summary>
    private int GetLevel()
    {
        if (hunger.IsStarving) return 3;
        float fraction = hunger.Fraction;
        if (fraction <= hunger.DangerFraction) return 2;
        if (fraction <= hunger.WarnFraction) return 1;
        return 0;
    }

    private string BuildLine(int level)
    {
        switch (level)
        {
            case 1:
                return "Anda mulai lapar. Makan kentang dari tas (tombol 1, lalu E) sebelum kondisi Anda memburuk. Bila tas kosong, ambil di storage.";
            case 2:
                return "Peringatan. Anda sangat lapar. Saya menyarankan segera makan kentang dari tas.";
            default:
                return "Anda kelaparan. Waktu tersisa untuk makan: sekitar " +
                       Mathf.CeilToInt(hunger.StarvationSecondsLeft) + " detik.";
        }
    }

    private void Show(string line)
    {
        if (dialogueUI == null)
        {
            Debug.Log("TARS: " + line);
            return;
        }

        dialogueUI.Show("TARS", new[] { line }, null);
    }
}
