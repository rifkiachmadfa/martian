using UnityEngine;

/// <summary>
/// Tembok tak terlihat di pintu keluar habitat. Menghalangi pemain yang masih di dalam jika suit
/// belum dikenakan atau oksigen suit di bawah minimum. TARS memberi tahu alasannya saat pemain menabrak tembok.
/// Tembok mati otomatis saat pemain sudah di luar, agar pemain tidak terjebak di luar bila oksigen menipis.
/// Pasang pada objek di pintu keluar dengan BoxCollider2D solid (bukan Trigger).
/// </summary>
public class SuitGate : MonoBehaviour
{
    [Header("Referensi (otomatis dicari jika kosong)")]
    [SerializeField] private BoxCollider2D wallCollider;
    [SerializeField] private SuitSystem suit;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private DialogueUI dialogueUI;

    [Header("Pesan TARS")]
    [Tooltip("Jeda minimum antar pesan saat pemain terus menabrak tembok (detik).")]
    [SerializeField, Min(0f)] private float messageCooldown = 8f;

    private Collider2D playerCollider;
    private float nextMessageTime;

    private void Start()
    {
        if (wallCollider == null) wallCollider = GetComponent<BoxCollider2D>();
        if (suit == null) suit = FindFirstObjectByType<SuitSystem>();
        if (player == null) player = FindFirstObjectByType<PlayerMovement>();
        if (dialogueUI == null) dialogueUI = FindFirstObjectByType<DialogueUI>();

        if (wallCollider == null || suit == null || player == null)
        {
            Debug.LogError("SuitGate: BoxCollider2D, SuitSystem, atau PlayerMovement tidak ditemukan. Jalankan menu Mars > Setup Suit and Gate.", this);
            enabled = false;
            return;
        }

        playerCollider = player.GetComponent<Collider2D>();
    }

    private void Update()
    {
        // Blokir hanya jika pemain di dalam dan belum boleh keluar. Di luar, tembok mati agar pemain bisa masuk lagi.
        bool shouldBlock = !suit.CanExit && !suit.IsOutdoors;
        if (shouldBlock == wallCollider.enabled) return;

        if (shouldBlock)
        {
            // Jangan menyalakan tembok saat pemain sedang menumpuk dengannya (hindari terdorong atau terjebak).
            if (PlayerOverlapsWall()) return;
            wallCollider.enabled = true;
        }
        else
        {
            wallCollider.enabled = false;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (Time.time < nextMessageTime) return;
        if (collision.collider.GetComponentInParent<PlayerMovement>() == null) return;
        if (dialogueUI != null && dialogueUI.IsOpen) return;

        nextMessageTime = Time.time + messageCooldown;
        ShowBlockedMessage();
    }

    private bool PlayerOverlapsWall()
    {
        if (playerCollider == null) return false;

        // Collider yang nonaktif tidak punya bounds, jadi area tembok dihitung manual (tanpa rotasi).
        Vector3 scale = wallCollider.transform.lossyScale;
        Vector3 size = new Vector3(
            Mathf.Abs(wallCollider.size.x * scale.x),
            Mathf.Abs(wallCollider.size.y * scale.y),
            1f);
        Vector3 center = wallCollider.transform.TransformPoint(wallCollider.offset);

        Bounds playerBounds = playerCollider.bounds;
        center.z = playerBounds.center.z;

        return new Bounds(center, size).Intersects(playerBounds);
    }

    private void ShowBlockedMessage()
    {
        string line = !suit.IsEquipped
            ? "Anda perlu mengenakan suit sebelum keluar. Suit tersedia di Suit Station."
            : "Oksigen suit belum cukup untuk keluar (minimum " + Mathf.CeilToInt(suit.MinimumOxygenToExit) +
              "). Isi ulang di Suit Station.";

        if (dialogueUI == null)
        {
            Debug.Log("SuitGate: " + line);
            return;
        }

        dialogueUI.Show("TARS", new[] { line }, null);
    }

    private void OnDrawGizmos()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null) return;

        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.9f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(box.offset, box.size);
    }
}
