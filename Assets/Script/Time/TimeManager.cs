using System;
using UnityEngine;

/// <summary>
/// Mengatur hari ke-berapa dan timer harian (usulan: 10 menit per hari, 7 hari).
/// Secara default timer TETAP berjalan saat dialog terbuka (revisi 2026-11-15); aktifkan pauseDuringDialogue
/// untuk menghentikannya. Saat waktu habis, timer berhenti di 0 dan memicu OnDayTimeEnded;
/// proses tidur/pergantian hari dan grace period outdoor dibuat di milestone berikutnya.
/// Hanya mengurus waktu; tidak menampilkan apa pun.
/// </summary>
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Durasi (usulan, ubah lewat Inspector setelah playtest)")]
    [SerializeField, Min(1f)] private float dayDurationSeconds = 600f;
    [SerializeField, Min(1)] private int totalDays = 7;

    [Header("Sumber pause")]
    [SerializeField] private DialogueUI dialogueUI;
    [Tooltip("Jika dicentang, timer (dan sistem yang mengikuti IsPaused, misalnya oksigen suit) berhenti selama dialog terbuka. Default: mati, waktu dan resource tetap berjalan.")]
    [SerializeField] private bool pauseDuringDialogue = false;

    public int CurrentDay { get; private set; } = 1;
    public int TotalDays => totalDays;
    public float DayDurationSeconds => dayDurationSeconds;
    public float RemainingSeconds { get; private set; }
    public bool IsDayOver { get; private set; }

    /// <summary>Pause dari sistem lain (sekarang: transisi tidur). Minigame nanti bisa memakai ini juga.</summary>
    public bool ExternalPause { get; set; }

    /// <summary>True saat timer ditahan (ExternalPause aktif, atau dialog terbuka bila pauseDuringDialogue dicentang).</summary>
    public bool IsPaused => ExternalPause || (pauseDuringDialogue && dialogueUI != null && dialogueUI.IsOpen);

    /// <summary>Dipanggil saat hari baru dimulai (parameter: nomor hari).</summary>
    public event Action<int> OnDayStarted;

    /// <summary>Dipanggil sekali ketika waktu hari habis.</summary>
    public event Action OnDayTimeEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("TimeManager: sudah ada instance lain, objek ini dihapus.", this);
            Destroy(this);
            return;
        }

        Instance = this;
        RemainingSeconds = dayDurationSeconds;
    }

    private void Start()
    {
        if (dialogueUI == null) dialogueUI = FindFirstObjectByType<DialogueUI>();
        if (pauseDuringDialogue && dialogueUI == null)
            Debug.LogWarning("TimeManager: DialogueUI tidak ditemukan, timer tidak akan berhenti saat dialog.", this);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (IsDayOver || IsPaused) return;

        RemainingSeconds -= Time.deltaTime;

        if (RemainingSeconds <= 0f)
        {
            RemainingSeconds = 0f;
            IsDayOver = true;
            Debug.Log("TimeManager: waktu hari " + CurrentDay + " habis.");
            OnDayTimeEnded?.Invoke();
        }
    }

    /// <summary>Memulai hari tertentu dengan timer penuh. Dipakai bed/pergantian hari nanti.</summary>
    public void StartDay(int day)
    {
        CurrentDay = Mathf.Clamp(day, 1, totalDays);
        RemainingSeconds = dayDurationSeconds;
        IsDayOver = false;
        OnDayStarted?.Invoke(CurrentDay);
    }

    [ContextMenu("Debug: Mulai hari berikutnya")]
    private void DebugNextDay() => StartDay(CurrentDay + 1);

    [ContextMenu("Debug: Sisakan 5 detik")]
    private void DebugSetFiveSeconds()
    {
        if (!IsDayOver) RemainingSeconds = Mathf.Min(5f, RemainingSeconds);
    }
}
