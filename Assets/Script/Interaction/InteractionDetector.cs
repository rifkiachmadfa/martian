using UnityEngine;

/// <summary>
/// Dipasang di Player. Mencari objek IInteractable terdekat dan memanggilnya saat tombol E ditekan.
/// </summary>
public class InteractionDetector : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float interactionRadius = 1f;
    [SerializeField] private Vector2 detectionOffset = Vector2.zero;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private readonly Collider2D[] hitBuffer = new Collider2D[8];
    private ContactFilter2D filter;

    /// <summary>Objek interaktif terdekat saat ini (null jika tidak ada).</summary>
    public IInteractable CurrentTarget { get; private set; }

    private void Awake()
    {
        filter = new ContactFilter2D();
        filter.SetLayerMask(interactableLayer);
        filter.useTriggers = true;
    }

    private void Update()
    {
        CurrentTarget = FindClosestInteractable();

        if (CurrentTarget != null &&
            Input.GetKeyDown(interactKey) &&
            CurrentTarget.CanInteract(gameObject))
        {
            CurrentTarget.Interact(gameObject);
        }
    }

    private IInteractable FindClosestInteractable()
    {
        Vector2 center = (Vector2)transform.position + detectionOffset;
        int count = Physics2D.OverlapCircle(center, interactionRadius, filter, hitBuffer);

        IInteractable closest = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            IInteractable candidate = hitBuffer[i].GetComponentInParent<IInteractable>();
            if (candidate == null) continue;

            float distance = ((Vector2)hitBuffer[i].transform.position - center).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = candidate;
            }
        }

        return closest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere((Vector2)transform.position + detectionOffset, interactionRadius);
    }
}
