using UnityEngine;

/// <summary>
/// Suit Station di Control Room. Interaksi (tombol E, berubah sesuai keadaan):
/// 1. Suit belum dikenakan: kenakan suit dan isi tabung dari stok oksigen habitat.
/// 2. Suit dikenakan dan tabung bisa diisi: isi ulang.
/// 3. Suit dikenakan dan tidak ada yang bisa diisi (tabung penuh atau stok habitat habis): lepas suit.
/// Oksigen suit berkurang selama suit dikenakan, jadi pemain perlu melepasnya di sini bila tidak ke luar.
/// Pasang pada objek ber-layer Interactable dengan Collider2D (Is Trigger).
/// </summary>
public class SuitStation : MonoBehaviour, IInteractable
{
    [Header("Referensi (otomatis dicari jika kosong)")]
    [SerializeField] private SuitSystem suit;
    [SerializeField] private DialogueUI dialogueUI;

    [SerializeField, Min(0f)] private float reinteractCooldown = 0.3f;

    private float nextInteractTime;

    public string InteractionPrompt
    {
        get
        {
            if (suit == null || !suit.IsEquipped) return "Kenakan suit";
            return suit.CanRefill ? "Isi oksigen suit" : "Lepas suit";
        }
    }

    private void Start()
    {
        if (suit == null) suit = FindFirstObjectByType<SuitSystem>();
        if (dialogueUI == null) dialogueUI = FindFirstObjectByType<DialogueUI>();

        if (suit == null)
            Debug.LogError("SuitStation: SuitSystem tidak ditemukan. Jalankan menu Mars > Setup Suit and Gate.", this);
    }

    public bool CanInteract(GameObject interactor)
    {
        if (suit == null || Time.time < nextInteractTime) return false;
        return dialogueUI == null || !dialogueUI.IsOpen;
    }

    public void Interact(GameObject interactor)
    {
        if (suit == null) return;

        ResourceManager resources = ResourceManager.Instance;
        if (resources == null)
        {
            Debug.LogError("SuitStation: ResourceManager tidak ada di scene.", this);
            return;
        }

        float current = resources.Get(ResourceType.SuitOxygen);
        float max = resources.GetMax(ResourceType.SuitOxygen);

        // Suit dikenakan dan tidak ada yang bisa diisi: tombol berfungsi sebagai "Lepas suit".
        if (suit.IsEquipped && !suit.CanRefill)
        {
            if (suit.Unequip())
                ShowMessage("Suit dilepas. Oksigen suit tidak berkurang selama suit dilepas. Oksigen suit: " +
                    Mathf.RoundToInt(current) + " / " + Mathf.RoundToInt(max) + ".");
            else
                ShowMessage("Suit tidak bisa dilepas di luar habitat.");
            return;
        }

        bool justEquipped = !suit.IsEquipped;
        suit.Equip();
        float added = suit.RefillFromHabitat();

        current = resources.Get(ResourceType.SuitOxygen);

        string message = justEquipped ? "Suit dikenakan. Oksigen suit berkurang selama suit dikenakan, termasuk di dalam habitat. " : "";

        if (added > 0f)
            message += "Oksigen suit terisi " + Mathf.RoundToInt(added) + " unit.";
        else if (current >= max - 0.001f)
            message += "Tabung oksigen suit sudah penuh.";
        else
            message += "Oksigen habitat tidak cukup untuk mengisi suit.";

        message += " Oksigen suit: " + Mathf.RoundToInt(current) + " / " + Mathf.RoundToInt(max) + ".";

        if (!suit.CanExit)
            message += " Belum cukup untuk keluar (minimum " + Mathf.CeilToInt(suit.MinimumOxygenToExit) + ").";

        ShowMessage(message);
    }

    private void ShowMessage(string message)
    {
        if (dialogueUI == null)
        {
            Debug.Log("SuitStation: " + message);
            nextInteractTime = Time.time + reinteractCooldown;
            return;
        }

        dialogueUI.Show("Suit Station", new[] { message }, () => nextInteractTime = Time.time + reinteractCooldown);
    }
}
