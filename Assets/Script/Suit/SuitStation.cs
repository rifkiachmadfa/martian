using UnityEngine;

/// <summary>
/// Suit Station di Control Room. Interaksi (tombol E, berubah sesuai keadaan):
/// 1. Suit belum dikenakan: kenakan suit. Tabung oksigen diisi otomatis oleh sistem dari stok habitat.
/// 2. Suit dikenakan: lepas suit. Tabung oksigen diisi otomatis oleh sistem dari stok habitat saat suit dilepas.
/// Tidak ada tombol "isi oksigen" terpisah; pengisian terjadi otomatis di balik layar.
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
            return suit == null || !suit.IsEquipped ? "Kenakan suit" : "Lepas suit";
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

        // Suit dikenakan: tombol berfungsi sebagai "Lepas suit". Pengisian tabung otomatis oleh sistem setelah suit dilepas.
        if (suit.IsEquipped)
        {
            if (!suit.Unequip())
            {
                ShowMessage("Suit tidak bisa dilepas di luar habitat.");
                return;
            }

            float refilled = suit.RefillFromHabitat();
            ShowMessage("Suit dilepas. Oksigen suit tidak berkurang selama suit dilepas. " +
                BuildRefillText(resources, refilled, "diisi otomatis"));
            return;
        }

        // Suit belum dikenakan: kenakan, lalu isi tabung otomatis agar pemain tidak berangkat dengan tabung kosong.
        suit.Equip();
        float added = suit.RefillFromHabitat();

        string message = "Suit dikenakan. Oksigen suit berkurang selama suit dikenakan, termasuk di dalam habitat. " +
            BuildRefillText(resources, added, "terisi otomatis");

        if (!suit.CanExit)
            message += " Belum cukup untuk keluar (minimum " + Mathf.CeilToInt(suit.MinimumOxygenToExit) + ").";

        ShowMessage(message);
    }

    private static string BuildRefillText(ResourceManager resources, float added, string addedVerb)
    {
        float current = resources.Get(ResourceType.SuitOxygen);
        float max = resources.GetMax(ResourceType.SuitOxygen);

        string text;
        if (added > 0f)
            text = "Tabung oksigen suit " + addedVerb + " " + Mathf.RoundToInt(added) + " unit.";
        else if (current >= max - 0.001f)
            text = "Tabung oksigen suit sudah penuh.";
        else
            text = "Oksigen habitat tidak cukup untuk mengisi suit.";

        return text + " Oksigen suit: " + Mathf.RoundToInt(current) + " / " + Mathf.RoundToInt(max) + ".";
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
