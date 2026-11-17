using UnityEngine;

/// <summary>
/// Mengganti tampilan pemain saat Astronaut Suit dikenakan/dilepas.
/// - Suit dikenakan: Animator Controller diganti ke controller astronaut (depan, belakang, samping).
/// - Suit dilepas: Animator Controller semula dikembalikan.
/// Sprite samping (pemain biasa maupun astronaut) hanya ada satu sisi (menghadap kanan), jadi sisi sebaliknya dibuat dengan flipX.
/// Hanya tampilan; logika suit tetap di SuitSystem dan gerakan tetap di PlayerMovement.
/// Pasang pada objek Player (yang punya Animator dan SpriteRenderer). Isi lewat menu Mars > Setup Suit Player Animation.
/// </summary>
public class PlayerSuitVisual : MonoBehaviour
{
    [Header("Referensi (otomatis dicari jika kosong)")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Animator Controller")]
    [Tooltip("Controller astronaut (SquareSuit.controller). Dibuat oleh menu Mars > Setup Suit Player Animation.")]
    [SerializeField] private RuntimeAnimatorController suitController;
    [Tooltip("Controller saat suit dilepas. Kosong = diambil otomatis dari Animator ketika game mulai.")]
    [SerializeField] private RuntimeAnimatorController normalController;

    [Header("Sprite samping")]
    [Tooltip("Centang bila sprite samping asli menghadap KANAN. Sprite akan di-flip saat pemain berjalan ke kiri.")]
    [SerializeField] private bool sideSpriteFacesRight = true;

    private static readonly int DirectionXHash = Animator.StringToHash("DirectionX");

    private SuitSystem suit;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (normalController == null && animator != null)
            normalController = animator.runtimeAnimatorController;
    }

    private void Start()
    {
        suit = SuitSystem.Instance != null ? SuitSystem.Instance : FindFirstObjectByType<SuitSystem>();
        if (suit == null)
        {
            Debug.LogWarning("PlayerSuitVisual: SuitSystem tidak ditemukan. Tampilan suit tidak akan berubah.", this);
            return;
        }

        suit.OnEquippedChanged += HandleEquippedChanged;
        HandleEquippedChanged(suit.IsEquipped);
    }

    private void OnDestroy()
    {
        if (suit != null) suit.OnEquippedChanged -= HandleEquippedChanged;
    }

    private void HandleEquippedChanged(bool isEquipped)
    {
        if (animator == null) return;

        if (isEquipped && suitController == null)
        {
            Debug.LogWarning("PlayerSuitVisual: suitController belum diisi. Jalankan menu Mars > Setup Suit Player Animation.", this);
            return;
        }

        RuntimeAnimatorController target = isEquipped ? suitController : normalController;
        if (target != null && animator.runtimeAnimatorController != target)
            animator.runtimeAnimatorController = target;

        if (!isEquipped && spriteRenderer != null) spriteRenderer.flipX = false;
    }

    private void LateUpdate()
    {
        if (spriteRenderer == null || animator == null || animator.runtimeAnimatorController == null) return;

        // Berlaku untuk kedua controller (pemain biasa dan astronaut): sprite samping keduanya menghadap kanan.
        // Hanya arah kiri/kanan yang memakai sprite samping; arah atas/bawah tidak di-flip.
        float dx = animator.GetFloat(DirectionXHash);
        if (Mathf.Abs(dx) > 0.5f)
            spriteRenderer.flipX = sideSpriteFacesRight ? dx < 0f : dx > 0f;
        else
            spriteRenderer.flipX = false;
    }
}
