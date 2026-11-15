using System.Collections;
using UnityEngine;

/// <summary>
/// Tempat tidur di Control Room. Saat dipakai: kunci gerakan, fade to black, hari bertambah,
/// Player dipindah ke titik spawn, timer hari baru mulai, lalu fade in.
/// Saat tidur, suit yang masih dikenakan dilepas otomatis (oksigen yang tersisa di tabung tetap ada).
/// Pada hari terakhir, ending Victory belum dibuat (milestone berikutnya).
/// Penyimpanan data (SaveSystem) belum ada; stok tetap di memori karena game satu scene.
/// </summary>
public class Bed : MonoBehaviour, IInteractable
{
    [Header("Referensi (otomatis dicari jika kosong)")]
    [SerializeField] private FadeUI fadeUI;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private SuitSystem suit; // opsional: bila ada, suit dilepas saat tidur
    [SerializeField] private Transform spawnPoint;

    [Header("Aturan tidur (usulan, belum diputuskan)")]
    [Tooltip("Boleh tidur jika sisa waktu hari ini sama dengan atau kurang dari angka ini (detik). Isi dengan durasi hari penuh agar boleh tidur kapan saja.")]
    [SerializeField, Min(0f)] private float earliestSleepRemainingSeconds = 120f;
    [SerializeField, Min(0f)] private float blackHoldSeconds = 0.5f;
    [SerializeField, Min(0f)] private float reinteractCooldown = 0.3f;

    private bool isSleeping;
    private float nextInteractTime;

    public string InteractionPrompt => "Tidur";

    private void Start()
    {
        if (fadeUI == null) fadeUI = FindFirstObjectByType<FadeUI>();
        if (player == null) player = FindFirstObjectByType<PlayerMovement>();
        if (timeManager == null) timeManager = FindFirstObjectByType<TimeManager>();
        if (dialogueUI == null) dialogueUI = FindFirstObjectByType<DialogueUI>();
        if (suit == null) suit = FindFirstObjectByType<SuitSystem>();

        if (fadeUI == null || player == null || timeManager == null)
            Debug.LogError("Bed: FadeUI, PlayerMovement, atau TimeManager tidak ditemukan. Jalankan menu Mars > Setup Bed and Fade.", this);
        if (spawnPoint == null)
            Debug.LogWarning("Bed: Spawn Point belum diisi; Player tidak akan dipindahkan setelah tidur.", this);
    }

    public bool CanInteract(GameObject interactor)
    {
        if (isSleeping || Time.time < nextInteractTime) return false;
        return dialogueUI == null || !dialogueUI.IsOpen;
    }

    public void Interact(GameObject interactor)
    {
        if (fadeUI == null || player == null || timeManager == null) return;

        bool canSleepNow = timeManager.IsDayOver || timeManager.RemainingSeconds <= earliestSleepRemainingSeconds;
        if (!canSleepNow)
        {
            int seconds = Mathf.CeilToInt(timeManager.RemainingSeconds);
            ShowMessage("Belum waktunya tidur. Sisa waktu hari ini: " +
                (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00") + ".");
            return;
        }

        if (timeManager.CurrentDay >= timeManager.TotalDays)
        {
            ShowMessage("Hari terakhir telah selesai. (Ending Victory belum dibuat.)");
            return;
        }

        StartCoroutine(SleepRoutine());
    }

    private IEnumerator SleepRoutine()
    {
        isSleeping = true;
        player.MovementEnabled = false;
        timeManager.ExternalPause = true;

        yield return fadeUI.FadeToBlack();

        // Layar sudah hitam: lepas suit (hanya berhasil jika Bed berada di dalam IndoorZone).
        if (suit != null) suit.Unequip();

        timeManager.StartDay(timeManager.CurrentDay + 1);
        MovePlayerToSpawn();

        if (blackHoldSeconds > 0f) yield return new WaitForSeconds(blackHoldSeconds);

        yield return fadeUI.FadeFromBlack();

        timeManager.ExternalPause = false;
        player.MovementEnabled = true;
        nextInteractTime = Time.time + reinteractCooldown;
        isSleeping = false;
    }

    private void MovePlayerToSpawn()
    {
        if (spawnPoint == null) return;

        Vector3 target = spawnPoint.position;
        target.z = player.transform.position.z;

        player.transform.position = target;

        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        if (body != null) body.position = target;
    }

    private void ShowMessage(string message)
    {
        if (dialogueUI == null)
        {
            Debug.Log("Bed: " + message);
            return;
        }

        dialogueUI.Show("Sistem", new[] { message }, () => nextInteractTime = Time.time + reinteractCooldown);
    }
}
