using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Panel storage: memindahkan kentang, es, dan bibit antara tas pemain dan storage dengan klik mouse
/// (Simpan 1, Simpan Semua, Ambil 1, Ambil Semua). Tutup dengan tombol Tutup atau Esc.
/// Selama terbuka, gerakan Player dikunci (nilai sebelumnya dipulihkan saat ditutup); waktu hari TETAP berjalan.
/// Seluruh tampilan dibuat lewat kode saat Awake di bawah Canvas (UnityEngine.UI.Text legacy), jadi hanya perlu
/// dipasang pada objek Canvas (menu Mars > Setup Inventory and Storage).
/// Hanya tampilan dan input panel: aturan pindah item ada di InventorySystem.
/// </summary>
public class StorageUI : MonoBehaviour
{
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;

    [Header("Referensi (otomatis dicari jika kosong)")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private PlayerMovement player;

    private static readonly ItemType[] Items = { ItemType.Potato, ItemType.Ice, ItemType.Seed };

    private readonly Text[] rowTexts = new Text[3];
    private GameObject root;
    private Font font;
    private InventorySystem inventory;
    private Action onClosed;
    private bool previousMovementEnabled;
    private int openedFrame = -1;

    /// <summary>True selama panel storage terbuka.</summary>
    public static bool IsOpen { get; private set; }

    private void Awake()
    {
        IsOpen = false;

        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("StorageUI: Canvas tidak ditemukan. Pasang komponen ini pada Canvas.", this);
            enabled = false;
            return;
        }

        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildUI();
        root.SetActive(false);
    }

    private void Start()
    {
        if (player == null) player = FindFirstObjectByType<PlayerMovement>();
    }

    private void OnDestroy()
    {
        if (inventory != null) inventory.OnChanged -= Refresh;
        IsOpen = false;
    }

    /// <summary>Membuka panel. Mengembalikan false bila sudah terbuka atau belum siap. onClosedCallback boleh null.</summary>
    public bool Open(Action onClosedCallback)
    {
        if (!enabled || IsOpen || root == null) return false;

        inventory = InventorySystem.Instance;
        if (inventory == null)
        {
            Debug.LogError("StorageUI: InventorySystem tidak ada di scene. Jalankan menu Mars > Setup Inventory and Storage.", this);
            return false;
        }

        onClosed = onClosedCallback;
        inventory.OnChanged += Refresh;

        if (player != null)
        {
            previousMovementEnabled = player.MovementEnabled;
            player.MovementEnabled = false;
        }

        openedFrame = Time.frameCount;
        IsOpen = true;
        root.SetActive(true);
        Refresh();
        return true;
    }

    private void Update()
    {
        if (!IsOpen || Time.frameCount == openedFrame) return;

        if (Input.GetKeyDown(closeKey)) Close();
    }

    public void Close()
    {
        if (!IsOpen) return;

        if (inventory != null) inventory.OnChanged -= Refresh;
        root.SetActive(false);
        IsOpen = false;

        if (player != null) player.MovementEnabled = previousMovementEnabled;

        Action callback = onClosed;
        onClosed = null;
        callback?.Invoke();
    }

    private void Refresh()
    {
        if (inventory == null) return;

        for (int i = 0; i < Items.Length; i++)
        {
            ItemType item = Items[i];
            rowTexts[i].text = (i + 1) + ". " + InventorySystem.NameOf(item) + "\n" +
                "Tas " + inventory.GetBag(item) + " / " + inventory.GetBagMax(item) +
                "    Storage " + inventory.GetStorage(item) + " / " + inventory.GetStorageMax(item);
        }
    }

    // ---------- Pembuatan tampilan ----------

    private void BuildUI()
    {
        root = new GameObject("StorageRoot", typeof(RectTransform), typeof(Image));
        root.transform.SetParent(canvas.transform, false);
        RectTransform rootRect = (RectTransform)root.transform;
        Place(rootRect, Vector2.zero, new Vector2(960f, 440f));
        root.GetComponent<Image>().color = new Color(0.05f, 0.07f, 0.1f, 0.95f);

        Text title = MakeText(root.transform, "Title", "Storage (Esc untuk menutup)", 30, TextAnchor.MiddleCenter);
        Place((RectTransform)title.transform, new Vector2(0f, 175f), new Vector2(900f, 50f));

        for (int i = 0; i < Items.Length; i++)
        {
            ItemType item = Items[i];
            float y = 80f - i * 85f;

            Text label = MakeText(root.transform, "Row" + i, string.Empty, 22, TextAnchor.MiddleLeft);
            Place((RectTransform)label.transform, new Vector2(-290f, y), new Vector2(340f, 70f));
            rowTexts[i] = label;

            MakeButton("Simpan 1", new Vector2(-45f, y), () => inventory.Store(item, 1));
            MakeButton("Simpan Semua", new Vector2(85f, y), () => inventory.Store(item, int.MaxValue));
            MakeButton("Ambil 1", new Vector2(215f, y), () => inventory.Take(item, 1));
            MakeButton("Ambil Semua", new Vector2(345f, y), () => inventory.Take(item, int.MaxValue));
        }

        GameObject closeButton = MakeButton("Tutup", new Vector2(0f, -185f), Close);
        ((RectTransform)closeButton.transform).sizeDelta = new Vector2(200f, 50f);
    }

    private GameObject MakeButton(string label, Vector2 position, UnityAction onClick)
    {
        GameObject go = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(root.transform, false);
        Place((RectTransform)go.transform, position, new Vector2(120f, 50f));

        Image image = go.GetComponent<Image>();
        image.color = new Color(0.2f, 0.3f, 0.45f, 1f);
        go.GetComponent<Button>().targetGraphic = image;
        go.GetComponent<Button>().onClick.AddListener(onClick);

        Text text = MakeText(go.transform, "Label", label, 18, TextAnchor.MiddleCenter);
        RectTransform r = text.rectTransform;
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
        return go;
    }

    private Text MakeText(Transform parent, string name, string content, int size, TextAnchor alignment)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);

        Text text = go.GetComponent<Text>();
        text.font = font;
        text.text = content;
        text.fontSize = size;
        text.color = Color.white;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private static void Place(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }
}
