using UnityEngine;

/// <summary>
/// Dipasang di Player. Mencari objek IInteractable terdekat dan memanggilnya saat tombol E ditekan.
/// Bila pemain memilih slot tas (tombol 1 sampai 3, InventorySystem), E memakai item: lebih dulu pada objek
/// penerima di dekatnya (IItemReceiver, misalnya tanam bibit), lalu aksi bawaan item (makan kentang saat lapar);
/// bila tidak ada yang cocok, E tetap berinteraksi biasa. Panel storage terbuka = tidak ada aksi.
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

    /// <summary>Teks prompt aksi yang akan terjadi bila E ditekan sekarang (null jika tidak ada aksi).</summary>
    public string CurrentPrompt { get; private set; }

    private enum ActionKind { None, ItemOnTarget, DefaultItem, Interact }

    private void Awake()
    {
        filter = new ContactFilter2D();
        filter.SetLayerMask(interactableLayer);
        filter.useTriggers = true;
    }

    private void Update()
    {
        CurrentTarget = FindClosestInteractable();
        CurrentPrompt = null;

        if (StorageUI.IsOpen) return;

        InventorySystem inventory = InventorySystem.Instance;
        ActionKind kind = ActionKind.None;
        ItemType item = default;
        IItemReceiver receiver = null;

        if (inventory != null && inventory.HasSelection && inventory.CanAcceptInput)
        {
            item = inventory.Selected;
            receiver = CurrentTarget as IItemReceiver;

            if (receiver != null && CurrentTarget.CanInteract(gameObject) && receiver.CanUseItem(item))
            {
                kind = ActionKind.ItemOnTarget;
                CurrentPrompt = receiver.GetItemPrompt(item);
            }
            else if (inventory.CanUseDefault(item))
            {
                kind = ActionKind.DefaultItem;
                CurrentPrompt = inventory.GetDefaultPrompt(item);
            }
        }

        if (kind == ActionKind.None && CurrentTarget != null && CurrentTarget.CanInteract(gameObject))
        {
            kind = ActionKind.Interact;
            CurrentPrompt = CurrentTarget.InteractionPrompt;
        }

        if (kind == ActionKind.None || !Input.GetKeyDown(interactKey)) return;

        switch (kind)
        {
            case ActionKind.ItemOnTarget: receiver.UseItem(gameObject, item); break;
            case ActionKind.DefaultItem: inventory.UseDefault(item); break;
            default: CurrentTarget.Interact(gameObject); break;
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
