using System.Collections;
using UnityEngine;

/// <summary>
/// TARS berjalan acak di dalam area tertentu. Saat pemain berinteraksi, TARS berhenti,
/// menghadap pemain, lalu lanjut berjalan setelah jeda (atau saat Resume() dipanggil).
/// TARS hanya memberi informasi; bukan pemberi quest.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class TarsWanderer : MonoBehaviour, IInteractable
{
    private enum State { Idle, Moving, Interacting }

    private enum Facing { Down, Up, Left, Right }

    [Header("Wander Area")]
    [Tooltip("BoxCollider2D (Is Trigger) yang menandai area jelajah TARS.")]
    [SerializeField] private BoxCollider2D wanderArea;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float arriveDistance = 0.1f;
    [SerializeField] private float minIdleTime = 1f;
    [SerializeField] private float maxIdleTime = 3f;
    [SerializeField] private float stuckCheckInterval = 0.75f;

    [Header("Interaction")]
    [SerializeField] private string interactionPrompt = "Bicara dengan TARS";
    [Tooltip("Dipakai hanya jika Dialogue UI kosong: lama TARS diam setelah diajak bicara (detik).")]
    [SerializeField] private float interactionPauseDuration = 3f;
    [Tooltip("Jeda singkat setelah dialog selesai agar tombol E yang sama tidak memulai dialog lagi.")]
    [SerializeField] private float reinteractCooldown = 0.3f;

    [Header("Dialogue")]    
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private string speakerName = "TARS";
    [TextArea(2, 4)]
    [SerializeField] private string[] dialogueLines =
    {
        "Selamat datang kembali. Saya TARS, asisten habitat ini.",
        "Saya memantau kondisi habitat dan akan memberi pengingat bila perlu."
    };

    [Header("Animation (sprite per arah)")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float framesPerSecond = 6f;
    [SerializeField] private Sprite[] downFrames;
    [SerializeField] private Sprite[] upFrames;
    [SerializeField] private Sprite[] leftFrames;
    [SerializeField] private Sprite[] rightFrames;

    private Rigidbody2D rb;
    private State state = State.Idle;
    private Facing facing = Facing.Down;
    private Vector2 targetPosition;
    private float idleTimer;
    private float animTimer;
    private Vector2 lastCheckPosition;
    private float stuckTimer;
    private Coroutine interactionRoutine;
    private float nextInteractTime;

    public string InteractionPrompt => interactionPrompt;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (wanderArea == null)
        {
            Debug.LogError("TarsWanderer: wanderArea belum diisi di Inspector.", this);
            enabled = false;
        }
    }

    private void Start()
    {
        StartIdle();
        ApplyFrame(0);
    }

    private void Update()
    {
        switch (state)
        {
            case State.Idle:
                idleTimer -= Time.deltaTime;
                if (idleTimer <= 0f) StartMoving();
                ApplyFrame(0);
                break;

            case State.Moving:
                CheckStuck();
                AnimateWalk();
                break;

            case State.Interacting:
                ApplyFrame(0);
                break;
        }
    }

    private void FixedUpdate()
    {
        if (state != State.Moving) return;

        Vector2 current = rb.position;
        Vector2 toTarget = targetPosition - current;

        if (toTarget.magnitude <= arriveDistance)
        {
            StartIdle();
            return;
        }

        UpdateFacing(toTarget);
        rb.MovePosition(Vector2.MoveTowards(current, targetPosition, moveSpeed * Time.fixedDeltaTime));
    }

    // ---------- Wander ----------

    private void StartIdle()
    {
        state = State.Idle;
        idleTimer = Random.Range(minIdleTime, maxIdleTime);
    }

    private void StartMoving()
    {
        Bounds b = wanderArea.bounds;
        targetPosition = new Vector2(
            Random.Range(b.min.x, b.max.x),
            Random.Range(b.min.y, b.max.y));

        lastCheckPosition = rb.position;
        stuckTimer = 0f;
        state = State.Moving;
    }

    // Jika TARS tertahan dinding/objek, berhenti sebentar lalu pilih tujuan baru.
    private void CheckStuck()
    {
        stuckTimer += Time.deltaTime;
        if (stuckTimer < stuckCheckInterval) return;

        float movedDistance = Vector2.Distance(rb.position, lastCheckPosition);
        float expectedDistance = moveSpeed * stuckCheckInterval;

        if (movedDistance < expectedDistance * 0.2f)
        {
            StartIdle();
            return;
        }

        lastCheckPosition = rb.position;
        stuckTimer = 0f;
    }

    // ---------- Interaction ----------

    public bool CanInteract(GameObject interactor)
    {
        return state != State.Interacting && Time.time >= nextInteractTime;
    }

    public void Interact(GameObject interactor)
    {
        if (state == State.Interacting) return;

        state = State.Interacting;

        // Menghadap pemain
        Vector2 toInteractor = (Vector2)interactor.transform.position - rb.position;
        if (toInteractor != Vector2.zero) UpdateFacing(toInteractor);

        if (dialogueUI != null && dialogueLines != null && dialogueLines.Length > 0)
        {
            // TARS diam sampai dialog selesai, lalu Resume() dipanggil otomatis.
            dialogueUI.Show(speakerName, dialogueLines, Resume);
            return;
        }

        // Cadangan jika Dialogue UI belum dipasang.
        Debug.LogWarning("TarsWanderer: Dialogue UI belum diisi, memakai jeda waktu saja.", this);
        if (interactionRoutine != null) StopCoroutine(interactionRoutine);
        interactionRoutine = StartCoroutine(ResumeAfterDelay());
    }

    /// <summary>Melanjutkan TARS berjalan (dipanggil otomatis, atau oleh sistem dialog nanti).</summary>
    public void Resume()
    {
        if (interactionRoutine != null)
        {
            StopCoroutine(interactionRoutine);
            interactionRoutine = null;
        }

        if (state == State.Interacting)
        {
            nextInteractTime = Time.time + reinteractCooldown;
            StartIdle();
        }
    }

    private IEnumerator ResumeAfterDelay()
    {
        yield return new WaitForSeconds(interactionPauseDuration);
        interactionRoutine = null;
        Resume();
    }

    // ---------- Animation ----------

    private void UpdateFacing(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            facing = direction.x > 0f ? Facing.Right : Facing.Left;
        else
            facing = direction.y > 0f ? Facing.Up : Facing.Down;
    }

    private void AnimateWalk()
    {
        Sprite[] frames = GetFrames(facing);
        if (frames == null || frames.Length == 0) return;

        animTimer += Time.deltaTime * framesPerSecond;
        ApplyFrame(Mathf.FloorToInt(animTimer) % frames.Length);
    }

    private void ApplyFrame(int index)
    {
        if (spriteRenderer == null) return;

        Sprite[] frames = GetFrames(facing);
        if (frames == null || frames.Length == 0) return;

        spriteRenderer.sprite = frames[Mathf.Clamp(index, 0, frames.Length - 1)];
    }

    private Sprite[] GetFrames(Facing dir)
    {
        switch (dir)
        {
            case Facing.Up: return upFrames;
            case Facing.Left: return leftFrames;
            case Facing.Right: return rightFrames;
            default: return downFrames;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (wanderArea == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(wanderArea.bounds.center, wanderArea.bounds.size);
    }
}
