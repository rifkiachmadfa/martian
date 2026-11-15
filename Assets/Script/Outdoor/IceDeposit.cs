using UnityEngine;

/// <summary>
/// Deposit es di Outdoor. Tekan E: pemain berhenti beberapa detik mengumpulkan es, lalu Ice masuk ke
/// inventory (stok ResourceType.Ice di ResourceManager; batasnya = kapasitas Ice di sana).
/// Deposit tidak habis (USULAN, keputusan terbuka). Perlu suit dikenakan (bisa dimatikan lewat Inspector).
/// Hanya logika interaksi; tampilan Ice ada di HudUI. Waktu pengumpulan ikut TimeManager.IsPaused.
/// Pasang pada objek ber-layer Interactable dengan Collider2D.
/// </summary>
public class IceDeposit : MonoBehaviour, IInteractable
{
    [Header("Referensi (otomatis dicari jika kosong)")]
    [SerializeField] private SuitSystem suit;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private DialogueUI dialogueUI;

    [Header("Aturan pengumpulan (USULAN, ubah lewat Inspector setelah playtest)")]
    [Tooltip("Lama pengumpulan per interaksi (detik). Pemain tidak bisa bergerak selama itu.")]
    [SerializeField, Min(0.1f)] private float gatherSeconds = 3f;
    [Tooltip("Jumlah Ice yang didapat per interaksi.")]
    [SerializeField, Min(0.1f)] private float icePerGather = 1f;
    [Tooltip("Jika dicentang, pemain harus mengenakan suit untuk mengumpulkan es.")]
    [SerializeField] private bool requireSuit = true;
    [SerializeField, Min(0f)] private float reinteractCooldown = 0.3f;

    private bool isGathering;
    private float gatherTimer;
    private float nextInteractTime;

    /// <summary>True selama pemain sedang mengumpulkan es.</summary>
    public bool IsGathering => isGathering;

    /// <summary>Kemajuan pengumpulan 0..1 (0 bila tidak sedang mengumpulkan).</summary>
    public float GatherProgress => isGathering ? Mathf.Clamp01(gatherTimer / gatherSeconds) : 0f;

    public string InteractionPrompt
    {
        get
        {
            if (!isGathering) return "Kumpulkan es";
            return "Mengumpulkan es... " + Mathf.CeilToInt(Mathf.Max(0f, gatherSeconds - gatherTimer)) + " dtk";
        }
    }

    private void Start()
    {
        if (suit == null) suit = FindFirstObjectByType<SuitSystem>();
        if (player == null) player = FindFirstObjectByType<PlayerMovement>();
        if (dialogueUI == null) dialogueUI = FindFirstObjectByType<DialogueUI>();

        if (player == null)
            Debug.LogWarning("IceDeposit: PlayerMovement tidak ditemukan; pemain tidak akan berhenti saat mengumpulkan.", this);
    }

    private void OnDisable()
    {
        // Jangan tinggalkan pemain terkunci bila objek dimatikan di tengah pengumpulan.
        if (isGathering) CancelGathering();
    }

    private void Update()
    {
        if (!isGathering) return;

        TimeManager time = TimeManager.Instance;
        if (time != null && time.IsPaused) return;

        gatherTimer += Time.deltaTime;
        if (gatherTimer >= gatherSeconds) FinishGathering();
    }

    public bool CanInteract(GameObject interactor)
    {
        // Saat mengumpulkan, tetap true agar prompt menampilkan sisa waktu; Interact() mengabaikan tekanan E.
        if (isGathering) return true;
        if (Time.time < nextInteractTime) return false;
        return dialogueUI == null || !dialogueUI.IsOpen;
    }

    public void Interact(GameObject interactor)
    {
        if (isGathering) return;

        ResourceManager resources = ResourceManager.Instance;
        if (resources == null)
        {
            Debug.LogError("IceDeposit: ResourceManager tidak ada di scene.", this);
            return;
        }

        if (requireSuit && (suit == null || !suit.IsEquipped))
        {
            ShowMessage("Anda perlu mengenakan suit untuk bekerja di permukaan Mars. Kenakan suit di Suit Station.");
            return;
        }

        float current = resources.Get(ResourceType.Ice);
        float max = resources.GetMax(ResourceType.Ice);
        if (current >= max - 0.001f)
        {
            ShowMessage("Inventory Ice penuh (" + Mathf.RoundToInt(current) + " / " + Mathf.RoundToInt(max) +
                "). Proses es di Water Purifier dulu.");
            return;
        }

        isGathering = true;
        gatherTimer = 0f;
        if (player != null) player.MovementEnabled = false;
    }

    private void FinishGathering()
    {
        ResourceManager resources = ResourceManager.Instance;
        float added = resources != null ? resources.Add(ResourceType.Ice, icePerGather) : 0f;

        if (added <= 0f)
            Debug.LogWarning("IceDeposit: tidak ada Ice yang masuk (inventory penuh atau ResourceManager hilang).", this);

        CancelGathering();
    }

    private void CancelGathering()
    {
        isGathering = false;
        gatherTimer = 0f;
        nextInteractTime = Time.time + reinteractCooldown;
        if (player != null) player.MovementEnabled = true;
    }

    private void ShowMessage(string message)
    {
        if (dialogueUI == null)
        {
            Debug.Log("IceDeposit: " + message);
            nextInteractTime = Time.time + reinteractCooldown;
            return;
        }

        dialogueUI.Show("Ice Deposit", new[] { message }, () => nextInteractTime = Time.time + reinteractCooldown);
    }
}
