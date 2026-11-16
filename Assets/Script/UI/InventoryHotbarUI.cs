using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hotbar tas di bawah tengah layar: tiga slot (1 Kentang, 2 Es, 3 Bibit) dengan jumlah di tas. Slot yang
/// dipilih (tombol 1 sampai 3) menyala. Seluruh tampilan dibuat lewat kode saat Awake di bawah Canvas, jadi hanya
/// perlu dipasang pada objek Canvas (menu Mars > Setup Inventory and Storage). Hanya tampilan: membaca InventorySystem.
/// </summary>
public class InventoryHotbarUI : MonoBehaviour
{
    [SerializeField] private Canvas canvas;

    private static readonly ItemType[] Items = { ItemType.Potato, ItemType.Ice, ItemType.Seed };
    private static readonly Color NormalColor = new Color(0f, 0f, 0f, 0.8f);
    private static readonly Color SelectedColor = new Color(0.95f, 0.75f, 0.2f, 0.95f);

    private readonly Image[] slotImages = new Image[3];
    private readonly Text[] slotTexts = new Text[3];
    private InventorySystem inventory;

    private void Awake()
    {
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("InventoryHotbarUI: Canvas tidak ditemukan. Pasang komponen ini pada Canvas.", this);
            enabled = false;
            return;
        }

        BuildUI();
    }

    private void Start()
    {
        inventory = InventorySystem.Instance;
        if (inventory == null)
        {
            Debug.LogError("InventoryHotbarUI: InventorySystem tidak ada di scene. Jalankan menu Mars > Setup Inventory and Storage.", this);
            enabled = false;
            return;
        }

        inventory.OnChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (inventory != null) inventory.OnChanged -= Refresh;
    }

    private void Refresh()
    {
        for (int i = 0; i < Items.Length; i++)
        {
            ItemType item = Items[i];
            bool selected = inventory.HasSelection && inventory.Selected == item;

            slotImages[i].color = selected ? SelectedColor : NormalColor;
            slotTexts[i].color = selected ? Color.black : Color.white;
            slotTexts[i].text = (i + 1) + "  " + InventorySystem.NameOf(item) + "\n" +
                inventory.GetBag(item) + " / " + inventory.GetBagMax(item);
        }
    }

    private void BuildUI()
    {
        GameObject root = new GameObject("InventoryHotbar", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        root.transform.SetAsFirstSibling(); // di belakang dialog dan fade
        RectTransform rootRect = (RectTransform)root.transform;
        rootRect.anchorMin = new Vector2(0.5f, 0f);
        rootRect.anchorMax = new Vector2(0.5f, 0f);
        rootRect.pivot = new Vector2(0.5f, 0f);
        rootRect.anchoredPosition = new Vector2(0f, 20f);
        rootRect.sizeDelta = new Vector2(480f, 80f);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        for (int i = 0; i < Items.Length; i++)
        {
            GameObject slot = new GameObject("Slot" + (i + 1), typeof(RectTransform), typeof(Image));
            slot.transform.SetParent(root.transform, false);
            RectTransform r = (RectTransform)slot.transform;
            r.anchorMin = new Vector2(0.5f, 0.5f);
            r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = new Vector2((i - 1) * 160f, 0f);
            r.sizeDelta = new Vector2(150f, 80f);

            Image image = slot.GetComponent<Image>();
            image.raycastTarget = false;
            slotImages[i] = image;

            GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(slot.transform, false);
            Text text = textGO.GetComponent<Text>();
            text.font = font;
            text.fontSize = 24;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            RectTransform tr = text.rectTransform;
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(5f, 5f);
            tr.offsetMax = new Vector2(-5f, -5f);
            slotTexts[i] = text;
        }
    }
}
