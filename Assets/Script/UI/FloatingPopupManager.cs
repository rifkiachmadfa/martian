using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>Jenis ikon popup. Ikon pixel bawaan dibuat lewat kode; isi sprite di Inspector untuk menggantinya.</summary>
public enum PopupIcon
{
    None,
    Potato,
    Ice,
    Water,
    Oxygen,
    Energy,
    Seed,
    Hunger,
    Repair
}

/// <summary>
/// Popup mengambang di dunia game (ikon + teks, misalnya "Kentang +2", "Es +1", "Water -2") di dekat objek
/// yang berinteraksi: deposit es, petak tanam, Water Purifier, Oxygen Generator, panel surya, dan pemain saat makan.
/// Hanya tampilan: sistem lain memanggil FloatingPopupManager.ShowAmount / Show setelah perubahan benar-benar terjadi;
/// manager tidak mengubah resource dan tidak membaca event resource. Tanpa manager di scene, semua panggilan diabaikan.
/// Popup memakai SpriteRenderer (ikon) dan TextMesh (teks, font bawaan Unity atau Font piksel pilihan Anda), jadi
/// tidak butuh Canvas atau TextMeshPro. Objek dipakai ulang (pool). Animasi memakai waktu tak-berskala,
/// sehingga tetap berjalan saat timer game berhenti (transisi tidur, minigame).
/// Pasang satu kali di scene lewat menu Mars > Setup Floating Popups. Semua angka tampilan adalah USULAN.
/// </summary>
public class FloatingPopupManager : MonoBehaviour
{
    public static FloatingPopupManager Instance { get; private set; }

    [Header("Ikon (opsional). Kosong = ikon pixel 8x8 bawaan")]
    [SerializeField] private Sprite potatoIcon;
    [SerializeField] private Sprite iceIcon;
    [SerializeField] private Sprite waterIcon;
    [SerializeField] private Sprite oxygenIcon;
    [SerializeField] private Sprite energyIcon;
    [SerializeField] private Sprite seedIcon;
    [SerializeField] private Sprite hungerIcon;
    [SerializeField] private Sprite repairIcon;

    [Header("Teks")]
    [Tooltip("Font teks. Kosong = font bawaan Unity. Untuk gaya pixel art, isi dengan font piksel (TTF) dari itch.io.")]
    [SerializeField] private Font font;
    [SerializeField, Min(8)] private int fontSize = 64;
    [Tooltip("Ukuran huruf di dunia. Naikkan bila teks terlalu kecil di kamera Anda.")]
    [SerializeField, Min(0.001f)] private float characterSize = 0.045f;
    [SerializeField] private Color gainColor = new Color(0.55f, 1f, 0.55f);
    [SerializeField] private Color lossColor = new Color(1f, 0.55f, 0.45f);
    [SerializeField] private Color shadowColor = new Color(0f, 0f, 0f, 0.85f);

    [Header("Ikon dan tata letak")]
    [Tooltip("Tinggi/lebar ikon di dunia (satuan Unity).")]
    [SerializeField, Min(0.05f)] private float iconWorldSize = 0.45f;
    [SerializeField, Min(0f)] private float iconTextGap = 0.08f;

    [Header("Animasi (USULAN)")]
    [SerializeField, Min(0.2f)] private float lifetime = 1.4f;
    [SerializeField, Min(0f)] private float riseDistance = 0.9f;
    [Tooltip("Jarak vertikal antar popup yang muncul hampir bersamaan di tempat yang sama.")]
    [SerializeField, Min(0.05f)] private float stackSpacing = 0.45f;
    [SerializeField, Min(1)] private int maxActive = 24;

    [Header("Urutan gambar")]
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 1000;

    private sealed class Popup
    {
        public GameObject root;
        public SpriteRenderer icon;
        public TextMesh text;
        public TextMesh shadow;
        public Vector3 start;
        public float age;
        public Color color;
    }

    private struct StackSlot
    {
        public Vector3 position;
        public float time;
        public int count;
    }

    private readonly List<Popup> active = new List<Popup>();
    private readonly Stack<Popup> pool = new Stack<Popup>();
    private readonly List<StackSlot> slots = new List<StackSlot>();
    private readonly Dictionary<PopupIcon, Sprite> defaultIcons = new Dictionary<PopupIcon, Sprite>();
    private Font resolvedFont;
    private static bool warnedMissing;

    private const float StackWindowSeconds = 0.6f;
    private const float StackRadius = 0.75f;

    // ---------- API statis (aman dipanggil walau manager tidak ada) ----------

    /// <summary>
    /// Menampilkan perubahan jumlah: "label +N" (hijau) atau "label -N" (merah). delta 0 diabaikan.
    /// Contoh: ShowAmount(AnchorAbove(this), PopupIcon.Ice, "Es", 1) tampil "Es +1".
    /// </summary>
    public static void ShowAmount(Vector3 worldPosition, PopupIcon icon, string label, float delta)
    {
        if (Mathf.Abs(delta) < 0.05f) return;
        if (!TryGetInstance(out FloatingPopupManager manager)) return;

        string number = Mathf.Abs(delta).ToString("0.#", CultureInfo.InvariantCulture);
        string sign = delta > 0f ? "+" : "-";
        string text = string.IsNullOrEmpty(label) ? sign + number : label + " " + sign + number;
        manager.Spawn(worldPosition, icon, text, delta > 0f ? manager.gainColor : manager.lossColor);
    }

    /// <summary>Menampilkan teks bebas (misalnya "Panel diperbaiki") dengan ikon dan warna pilihan.</summary>
    public static void Show(Vector3 worldPosition, PopupIcon icon, string text, Color color)
    {
        if (string.IsNullOrEmpty(text)) return;
        if (!TryGetInstance(out FloatingPopupManager manager)) return;

        manager.Spawn(worldPosition, icon, text, color);
    }

    /// <summary>Menampilkan teks bebas berwarna hijau (hasil positif).</summary>
    public static void Show(Vector3 worldPosition, PopupIcon icon, string text)
    {
        Color color = Instance != null ? Instance.gainColor : Color.white;
        Show(worldPosition, icon, text, color);
    }

    /// <summary>
    /// Titik sedikit di atas objek (dari Collider2D, lalu SpriteRenderer, lalu posisi + 0,8) sebagai tempat popup muncul.
    /// </summary>
    public static Vector3 AnchorAbove(Component target)
    {
        if (target == null) return Vector3.zero;

        Collider2D col = target.GetComponentInChildren<Collider2D>();
        if (col != null)
        {
            Bounds b = col.bounds;
            return new Vector3(b.center.x, b.max.y + 0.15f, target.transform.position.z);
        }

        SpriteRenderer sprite = target.GetComponentInChildren<SpriteRenderer>();
        if (sprite != null)
        {
            Bounds b = sprite.bounds;
            return new Vector3(b.center.x, b.max.y + 0.15f, target.transform.position.z);
        }

        return target.transform.position + Vector3.up * 0.8f;
    }

    /// <summary>
    /// Membuat label teks tetap (bukan popup sementara) sebagai anak sebuah objek, memakai font dan urutan gambar
    /// yang sama dengan popup. Dipakai FarmingPlot untuk hitung mundur panen. Mengembalikan null bila manager tidak ada.
    /// Ukuran huruf dikompensasi terhadap skala parent. sortingOffset menaikkan/menurunkan urutan gambar (bayangan = -1).
    /// </summary>
    public static TextMesh CreateWorldLabel(string objectName, Transform parent, float sizeMultiplier, int sortingOffset)
    {
        if (Instance == null) return null;

        TextMesh mesh = Instance.CreateText(objectName, parent, Instance.sortingOrder + sortingOffset);
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.alignment = TextAlignment.Center;
        mesh.text = string.Empty;

        Vector3 ls = parent != null ? parent.lossyScale : Vector3.one;
        mesh.transform.localScale = new Vector3(
            sizeMultiplier / Mathf.Max(Mathf.Abs(ls.x), 0.0001f),
            sizeMultiplier / Mathf.Max(Mathf.Abs(ls.y), 0.0001f), 1f);
        return mesh;
    }

    private static bool TryGetInstance(out FloatingPopupManager manager)
    {
        manager = Instance;
        if (manager != null) return true;

        if (!warnedMissing)
        {
            warnedMissing = true;
            Debug.LogWarning("FloatingPopupManager tidak ada di scene; popup tidak ditampilkan. Jalankan menu Mars > Setup Floating Popups.");
        }
        return false;
    }

    // ---------- Lifecycle ----------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("FloatingPopupManager: sudah ada instance lain, objek ini dihapus.", this);
            Destroy(this);
            return;
        }

        Instance = this;
        warnedMissing = false;
        resolvedFont = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;

        for (int i = active.Count - 1; i >= 0; i--)
        {
            Popup p = active[i];
            p.age += dt;

            float t = Mathf.Clamp01(p.age / lifetime);
            float eased = 1f - (1f - t) * (1f - t); // naik cepat lalu melambat
            p.root.transform.position = p.start + Vector3.up * (riseDistance * eased);

            // Muncul membesar sedikit (pop), tetap penuh, lalu memudar di 35 persen terakhir.
            float pop = Mathf.Clamp01(p.age / 0.15f);
            float scale = Mathf.Lerp(0.6f, 1f, pop) + Mathf.Sin(pop * Mathf.PI) * 0.15f;
            p.root.transform.localScale = Vector3.one * scale;

            float alpha = t < 0.65f ? 1f : 1f - (t - 0.65f) / 0.35f;
            ApplyAlpha(p, alpha);

            if (t >= 1f) Release(i);
        }

        for (int i = slots.Count - 1; i >= 0; i--)
            if (Time.unscaledTime - slots[i].time > StackWindowSeconds) slots.RemoveAt(i);
    }

    // ---------- Internal ----------

    private void Spawn(Vector3 position, PopupIcon icon, string text, Color color)
    {
        if (active.Count >= maxActive) Release(0); // buang yang tertua

        position += Vector3.up * (NextStackOffset(position) * stackSpacing);

        Popup p = pool.Count > 0 ? pool.Pop() : CreatePopup();
        p.root.SetActive(true);
        p.start = position;
        p.age = 0f;
        p.color = color;
        p.root.transform.position = position;
        p.root.transform.localScale = Vector3.one * 0.6f;

        p.text.text = text;
        p.shadow.text = text;

        Sprite sprite = GetIcon(icon);
        p.icon.sprite = sprite;
        p.icon.enabled = sprite != null;
        Layout(p, sprite);

        ApplyAlpha(p, 1f);
        active.Add(p);
    }

    // Popup yang muncul hampir bersamaan di tempat yang sama ditumpuk ke atas agar tidak saling menimpa.
    private int NextStackOffset(Vector3 position)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            StackSlot s = slots[i];
            if (Time.unscaledTime - s.time > StackWindowSeconds) continue;
            if (((Vector2)(s.position - position)).sqrMagnitude > StackRadius * StackRadius) continue;

            int offset = s.count;
            s.count++;
            s.time = Time.unscaledTime;
            slots[i] = s;
            return offset;
        }

        slots.Add(new StackSlot { position = position, time = Time.unscaledTime, count = 1 });
        return 0;
    }

    private Popup CreatePopup()
    {
        Popup p = new Popup();
        p.root = new GameObject("FloatingPopup");
        p.root.transform.SetParent(transform, false);

        GameObject iconGo = new GameObject("Icon");
        iconGo.transform.SetParent(p.root.transform, false);
        p.icon = iconGo.AddComponent<SpriteRenderer>();
        ConfigureSorting(p.icon, sortingOrder);

        p.shadow = CreateText("Shadow", p.root.transform, sortingOrder);
        p.text = CreateText("Text", p.root.transform, sortingOrder + 1);
        return p;
    }

    private TextMesh CreateText(string objectName, Transform parent, int order)
    {
        GameObject go = new GameObject(objectName);
        go.transform.SetParent(parent, false);

        TextMesh mesh = go.AddComponent<TextMesh>();
        mesh.font = resolvedFont;
        mesh.fontSize = fontSize;
        mesh.characterSize = characterSize;
        mesh.anchor = TextAnchor.MiddleLeft;
        mesh.alignment = TextAlignment.Left;

        MeshRenderer renderer = go.GetComponent<MeshRenderer>();
        if (resolvedFont != null) renderer.sharedMaterial = resolvedFont.material;
        ConfigureSorting(renderer, order);
        return mesh;
    }

    private void ConfigureSorting(Renderer renderer, int order)
    {
        renderer.sortingLayerName = sortingLayerName;
        renderer.sortingOrder = order;
    }

    // Ikon di kiri, teks di kanan, seluruhnya berpusat di titik popup.
    private void Layout(Popup p, Sprite sprite)
    {
        float iconWidth = 0f;
        if (sprite != null)
        {
            Vector2 size = sprite.bounds.size;
            float largest = Mathf.Max(size.x, size.y, 0.0001f);
            float scale = iconWorldSize / largest;
            p.icon.transform.localScale = new Vector3(scale, scale, 1f);
            iconWidth = size.x * scale;
        }

        MeshRenderer textRenderer = p.text.GetComponent<MeshRenderer>();
        // bounds ada di ruang dunia, sedangkan tata letak di ruang lokal: bagi dengan skala root (popup sedang membesar).
        float rootScale = Mathf.Max(p.root.transform.lossyScale.x, 0.0001f);
        float textWidth = textRenderer != null ? textRenderer.bounds.size.x / rootScale : 0f;
        float gap = sprite != null ? iconTextGap : 0f;
        float total = iconWidth + gap + textWidth;
        float left = -total * 0.5f;

        p.icon.transform.localPosition = new Vector3(left + iconWidth * 0.5f, 0f, 0f);

        float textX = left + iconWidth + gap;
        p.text.transform.localPosition = new Vector3(textX, 0f, 0f);
        p.shadow.transform.localPosition = new Vector3(textX + 0.025f, -0.025f, 0f);
    }

    private void ApplyAlpha(Popup p, float alpha)
    {
        Color c = p.color;
        c.a = alpha;
        p.text.color = c;

        Color s = shadowColor;
        s.a *= alpha;
        p.shadow.color = s;

        Color i = Color.white;
        i.a = alpha;
        p.icon.color = i;
    }

    private void Release(int activeIndex)
    {
        Popup p = active[activeIndex];
        active.RemoveAt(activeIndex);
        p.root.SetActive(false);
        pool.Push(p);
    }

    // ---------- Ikon ----------

    private Sprite GetIcon(PopupIcon icon)
    {
        Sprite custom = null;
        switch (icon)
        {
            case PopupIcon.Potato: custom = potatoIcon; break;
            case PopupIcon.Ice: custom = iceIcon; break;
            case PopupIcon.Water: custom = waterIcon; break;
            case PopupIcon.Oxygen: custom = oxygenIcon; break;
            case PopupIcon.Energy: custom = energyIcon; break;
            case PopupIcon.Seed: custom = seedIcon; break;
            case PopupIcon.Hunger: custom = hungerIcon; break;
            case PopupIcon.Repair: custom = repairIcon; break;
        }

        if (custom != null) return custom;
        if (icon == PopupIcon.None) return null;

        if (!defaultIcons.TryGetValue(icon, out Sprite generated))
        {
            generated = BuildDefaultIcon(icon);
            defaultIcons[icon] = generated;
        }
        return generated;
    }

    // Ikon 8x8 sederhana (placeholder sampai sprite final dari itch.io diimpor). '.' = transparan,
    // 'o' = garis tepi, 'a' = warna utama, 'b' = sorotan.
    private static readonly Dictionary<PopupIcon, string[]> Bitmaps = new Dictionary<PopupIcon, string[]>
    {
        { PopupIcon.Potato, new[] { "........", "..oooo..", ".oaaaao.", "oaabaaao", "oaaaaaao", ".oaaaao.", "..oooo..", "........" } },
        { PopupIcon.Ice,    new[] { "...oo...", "..obbo..", ".obaabo.", "obaaaabo", ".oaaaao.", "..oaao..", "...oo...", "........" } },
        { PopupIcon.Water,  new[] { "...oo...", "...oo...", "..oaao..", ".oabaao.", ".oaaaao.", ".oaaaao.", "..oooo..", "........" } },
        { PopupIcon.Oxygen, new[] { "..oooo..", ".oaaaao.", "oabaaaao", "oabaaaao", "oaaaaaao", "oaaaaaao", ".oaaaao.", "..oooo.." } },
        { PopupIcon.Energy, new[] { "....oo..", "...oao..", "..oaao..", ".oaaaoo.", "..ooaao.", "...oao..", "...oao..", "....oo.." } },
        { PopupIcon.Seed,   new[] { "........", "...oo...", "..oaao..", "...oo...", "...o....", ".oooooo.", "oaaaaaao", ".oooooo." } },
        { PopupIcon.Hunger, new[] { "........", ".oo..oo.", "oaaoaaao", "oabaaaao", "oaaaaaao", ".oaaaao.", "..oaao..", "...oo..." } },
        { PopupIcon.Repair, new[] { "...oo...", ".o.aa.o.", "..oaao..", "ooaabaoo", "ooaaaaoo", "..oaao..", ".o.aa.o.", "...oo..." } },
    };

    private static void GetPalette(PopupIcon icon, out Color main, out Color light)
    {
        switch (icon)
        {
            case PopupIcon.Potato: main = new Color(0.78f, 0.58f, 0.32f); light = new Color(0.92f, 0.78f, 0.5f); break;
            case PopupIcon.Ice: main = new Color(0.55f, 0.85f, 1f); light = new Color(0.9f, 1f, 1f); break;
            case PopupIcon.Water: main = new Color(0.25f, 0.55f, 1f); light = new Color(0.6f, 0.8f, 1f); break;
            case PopupIcon.Oxygen: main = new Color(0.7f, 0.95f, 0.95f); light = Color.white; break;
            case PopupIcon.Energy: main = new Color(1f, 0.85f, 0.2f); light = new Color(1f, 1f, 0.6f); break;
            case PopupIcon.Seed: main = new Color(0.45f, 0.75f, 0.3f); light = new Color(0.7f, 0.95f, 0.5f); break;
            case PopupIcon.Hunger: main = new Color(0.95f, 0.35f, 0.4f); light = new Color(1f, 0.7f, 0.7f); break;
            default: main = new Color(0.7f, 0.7f, 0.75f); light = new Color(0.95f, 0.95f, 1f); break;
        }
    }

    private static Sprite BuildDefaultIcon(PopupIcon icon)
    {
        const int size = 8;
        if (!Bitmaps.TryGetValue(icon, out string[] rows)) return null;

        GetPalette(icon, out Color main, out Color light);
        Color outline = new Color(0.12f, 0.1f, 0.15f);

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        for (int row = 0; row < size; row++)
        {
            for (int x = 0; x < size; x++)
            {
                Color pixel;
                switch (rows[row][x])
                {
                    case 'o': pixel = outline; break;
                    case 'a': pixel = main; break;
                    case 'b': pixel = light; break;
                    default: pixel = Color.clear; break;
                }
                tex.SetPixel(x, size - 1 - row, pixel); // baris 0 di bitmap = bagian atas
            }
        }

        tex.Apply();
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        sprite.name = "PopupIcon_" + icon;
        return sprite;
    }
}
