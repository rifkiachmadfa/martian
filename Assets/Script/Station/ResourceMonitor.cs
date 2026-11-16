using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Resource Monitor di Control Room (GDD 3.1). Tekan E: dialog ringkasan hari, stok semua resource,
/// status suit, dan kondisi fasilitas (Solar Panel, Water Purifier, Oxygen Generator).
/// Hanya MEMBACA status; tidak mengubah apa pun dan tidak memberi tugas (bukan quest).
/// Fasilitas yang belum ada di scene dilewati. Ringkasan diambil saat E ditekan (tidak diperbarui otomatis).
/// Pasang pada objek ber-layer Interactable dengan Collider2D.
/// </summary>
public class ResourceMonitor : MonoBehaviour, IInteractable
{
    [Header("Referensi (otomatis dicari jika kosong)")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField, Min(0f)] private float reinteractCooldown = 0.3f;

    private float nextInteractTime;

    public string InteractionPrompt => "Periksa Resource Monitor";

    private void Start()
    {
        if (dialogueUI == null) dialogueUI = FindFirstObjectByType<DialogueUI>();
    }

    public bool CanInteract(GameObject interactor)
    {
        if (Time.time < nextInteractTime) return false;
        return dialogueUI == null || !dialogueUI.IsOpen;
    }

    public void Interact(GameObject interactor)
    {
        ResourceManager resources = ResourceManager.Instance;
        if (resources == null)
        {
            Debug.LogError("ResourceMonitor: ResourceManager tidak ada di scene.", this);
            return;
        }

        List<string> lines = new List<string>();

        TimeManager time = TimeManager.Instance;
        if (time != null)
        {
            int seconds = Mathf.CeilToInt(time.RemainingSeconds);
            lines.Add("Hari " + time.CurrentDay + " dari " + time.TotalDays + ". Sisa waktu hari ini " +
                (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00") + ".");
        }

        lines.Add("Food (kentang di storage) " + Format(resources, ResourceType.Food) +
            "  |  Water " + Format(resources, ResourceType.Water) +
            "  |  Oxygen habitat " + Format(resources, ResourceType.HabitatOxygen) +
            "  |  Energy " + Format(resources, ResourceType.Energy));

        InventorySystem inventory = InventorySystem.Instance;
        if (inventory != null)
        {
            lines.Add("Tas: " + FormatItem(inventory, ItemType.Potato, false) + ", " + FormatItem(inventory, ItemType.Ice, false) +
                ", " + FormatItem(inventory, ItemType.Seed, false) + ".");
            lines.Add("Storage: " + FormatItem(inventory, ItemType.Potato, true) + ", " + FormatItem(inventory, ItemType.Ice, true) +
                ", " + FormatItem(inventory, ItemType.Seed, true) + ".");
        }

        HungerSystem hunger = HungerSystem.Instance;
        if (hunger != null)
            lines.Add("Kenyang " + Mathf.RoundToInt(hunger.Satiety) + "/" + Mathf.RoundToInt(hunger.MaxSatiety) +
                (hunger.IsStarving ? ". Anda kelaparan, segera makan." : "."));

        SuitSystem suit = SuitSystem.Instance;
        bool equipped = suit != null && suit.IsEquipped;
        lines.Add((equipped ? "Suit dikenakan." : "Suit tidak dikenakan.") +
            " Oksigen suit " + Format(resources, ResourceType.SuitOxygen) + ".");

        string facilities = BuildFacilityLine();
        if (facilities.Length > 0) lines.Add(facilities);

        ShowLines(lines.ToArray());
    }

    private static string Format(ResourceManager resources, ResourceType type)
    {
        return Mathf.RoundToInt(resources.Get(type)) + "/" + Mathf.RoundToInt(resources.GetMax(type));
    }

    private static string FormatItem(InventorySystem inventory, ItemType item, bool storage)
    {
        int count = storage ? inventory.GetStorage(item) : inventory.GetBag(item);
        int max = storage ? inventory.GetStorageMax(item) : inventory.GetBagMax(item);
        return InventorySystem.NameOf(item) + " " + count + "/" + max;
    }

    private static string BuildFacilityLine()
    {
        List<string> parts = new List<string>();

        SolarPanel[] panels = FindObjectsByType<SolarPanel>(FindObjectsSortMode.None);
        if (panels.Length > 0)
        {
            int damaged = 0;
            float production = 0f;
            foreach (SolarPanel panel in panels)
            {
                if (panel.IsDamaged) damaged++;
                production += panel.CurrentEnergyPerSecond;
            }

            parts.Add("Solar Panel: " + (panels.Length - damaged) + " normal, " + damaged + " rusak (" +
                production.ToString("0.0") + " Energy per detik)");
        }

        WaterPurifier purifier = FindFirstObjectByType<WaterPurifier>();
        if (purifier != null) parts.Add("Water Purifier: " + purifier.StatusText);

        OxygenGenerator generator = FindFirstObjectByType<OxygenGenerator>();
        if (generator != null) parts.Add("Oxygen Generator: " + generator.StatusText);

        return string.Join(". ", parts) + (parts.Count > 0 ? "." : "");
    }

    private void ShowLines(string[] lines)
    {
        if (dialogueUI == null)
        {
            Debug.Log("ResourceMonitor:\n" + string.Join("\n", lines));
            nextInteractTime = Time.time + reinteractCooldown;
            return;
        }

        dialogueUI.Show("Resource Monitor", lines, () => nextInteractTime = Time.time + reinteractCooldown);
    }
}
