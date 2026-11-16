using UnityEngine;

/// <summary>
/// Storage (gudang) di Control Room (tombol E: "Buka storage"). Membuka StorageUI untuk memindahkan kentang, es,
/// dan bibit antara tas pemain dan storage. Kentang di storage = stok Food di HUD.
/// Nama class tetap FoodStorage agar objek yang sudah ada di scene tidak kehilangan script-nya.
/// Pasang pada objek ber-layer Interactable dengan Collider2D (Is Trigger).
/// </summary>
public class FoodStorage : MonoBehaviour, IInteractable
{
    [Header("Referensi (otomatis dicari jika kosong)")]
    [SerializeField] private StorageUI storageUI;
    [SerializeField] private DialogueUI dialogueUI;

    [SerializeField, Min(0f)] private float reinteractCooldown = 0.3f;

    private float nextInteractTime;

    public string InteractionPrompt => "Buka storage";

    private void Start()
    {
        if (storageUI == null) storageUI = FindFirstObjectByType<StorageUI>();
        if (dialogueUI == null) dialogueUI = FindFirstObjectByType<DialogueUI>();

        if (storageUI == null)
            Debug.LogError("FoodStorage: StorageUI tidak ditemukan. Jalankan menu Mars > Setup Inventory and Storage.", this);
    }

    public bool CanInteract(GameObject interactor)
    {
        if (storageUI == null || StorageUI.IsOpen || Time.time < nextInteractTime) return false;
        return dialogueUI == null || !dialogueUI.IsOpen;
    }

    public void Interact(GameObject interactor)
    {
        if (storageUI == null) return;

        storageUI.Open(() => nextInteractTime = Time.time + reinteractCooldown);
    }
}
