using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Garis arah tutorial (USULAN, hanya Day 1): garis UI dari tubuh pemain ke objek tujuan langkah tutorial yang
/// sedang berjalan, dengan penanda di ujung tujuan. Tujuan ditentukan TutorialManager.GetGuideTarget.
/// Hanya tampilan: tidak mengunci gerak, tidak mengubah resource, dan hilang sendiri saat tutorial tidak aktif.
/// Dipasang pada Canvas (menu Mars > Setup Tutorial); UI dibuat lewat kode saat Awake sebagai anak PERTAMA Canvas
/// sehingga berada di bawah HUD, dialog, dan fade. Garis ditarik di ruang layar (kamera dunia -> Canvas).
/// Garis lurus: tidak mengikuti jalur, jadi dapat menembus dinding (cukup sebagai arah).
/// </summary>
public class TutorialGuideUI : MonoBehaviour
{
    [Header("Referensi (otomatis dicari jika kosong)")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private Camera worldCamera;

    [Header("Tampilan (USULAN)")]
    [SerializeField] private Color lineColor = new Color(0.3f, 0.9f, 1f, 0.9f);
    [SerializeField, Min(1f)] private float thickness = 4f;
    [SerializeField, Min(1f)] private float markerSize = 22f;
    [Tooltip("Geser titik awal garis dari pivot Player ke tubuhnya (satuan dunia).")]
    [SerializeField] private Vector2 bodyOffset = Vector2.zero;
    [Tooltip("Garis disembunyikan bila pemain sudah sedekat ini dengan tujuan (satuan dunia).")]
    [SerializeField, Min(0f)] private float hideWithinDistance = 1.5f;
    [SerializeField, Min(0f)] private float pulseSpeed = 3f;
    [SerializeField, Range(0f, 1f)] private float pulseDepth = 0.35f;
    [Tooltip("Sembunyikan garis saat panel Storage terbuka.")]
    [SerializeField] private bool hideWhileStorageOpen = true;

    private RectTransform root;
    private RectTransform lineRect;
    private RectTransform markerRect;
    private Image lineImage;
    private Image markerImage;

    private void Awake()
    {
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("TutorialGuideUI: Canvas tidak ditemukan. Pasang komponen ini pada Canvas (menu Mars > Setup Tutorial).", this);
            enabled = false;
            return;
        }

        BuildUi();
    }

    private void Start()
    {
        if (player == null) player = FindFirstObjectByType<PlayerMovement>();
        if (worldCamera == null) worldCamera = Camera.main;

        if (player == null) Debug.LogWarning("TutorialGuideUI: PlayerMovement tidak ditemukan; garis arah tidak tampil.", this);
    }

    private void OnDestroy()
    {
        if (root != null) Destroy(root.gameObject);
    }

    // LateUpdate: setelah pemain dan kamera bergerak di frame ini.
    private void LateUpdate()
    {
        TutorialManager tutorial = TutorialManager.Instance;
        if (tutorial == null || !tutorial.IsActive || player == null || (hideWhileStorageOpen && StorageUI.IsOpen))
        {
            SetVisible(false);
            return;
        }

        if (worldCamera == null) worldCamera = Camera.main;
        Transform target = tutorial.GetGuideTarget(player.transform.position);
        if (target == null || worldCamera == null)
        {
            SetVisible(false);
            return;
        }

        Vector3 startWorld = player.transform.position + (Vector3)bodyOffset;
        Vector3 endWorld = target.position;
        if (Vector2.Distance(startWorld, endWorld) <= hideWithinDistance)
        {
            SetVisible(false);
            return;
        }

        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        bool okStart = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            root, worldCamera.WorldToScreenPoint(startWorld), uiCamera, out Vector2 startLocal);
        bool okEnd = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            root, worldCamera.WorldToScreenPoint(endWorld), uiCamera, out Vector2 endLocal);
        if (!okStart || !okEnd)
        {
            SetVisible(false);
            return;
        }

        Vector2 delta = endLocal - startLocal;
        float length = delta.magnitude;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

        lineRect.anchoredPosition = startLocal;
        lineRect.sizeDelta = new Vector2(Mathf.Max(0f, length - markerSize * 0.5f), thickness);
        lineRect.localRotation = Quaternion.Euler(0f, 0f, angle);

        markerRect.anchoredPosition = endLocal;
        markerRect.sizeDelta = new Vector2(markerSize, markerSize);
        markerRect.localRotation = Quaternion.Euler(0f, 0f, angle + 45f); // kotak diputar = bentuk berlian

        Color color = lineColor;
        float wave = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * pulseSpeed);
        color.a *= 1f - pulseDepth + pulseDepth * wave;
        lineImage.color = color;
        markerImage.color = color;

        SetVisible(true);
    }

    private void SetVisible(bool visible)
    {
        if (root != null && root.gameObject.activeSelf != visible) root.gameObject.SetActive(visible);
    }

    private void BuildUi()
    {
        GameObject rootGo = new GameObject("TutorialGuideRoot", typeof(RectTransform));
        root = rootGo.GetComponent<RectTransform>();
        root.SetParent(canvas.transform, false);
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;
        root.pivot = new Vector2(0.5f, 0.5f);
        root.SetAsFirstSibling();

        lineRect = CreateImage("GuideLine", out lineImage, new Vector2(0f, 0.5f));
        markerRect = CreateImage("GuideMarker", out markerImage, new Vector2(0.5f, 0.5f));

        rootGo.SetActive(false);
    }

    private RectTransform CreateImage(string name, out Image image, Vector2 pivot)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(root, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = pivot;

        image = go.GetComponent<Image>();
        image.color = lineColor;
        image.raycastTarget = false;
        return rect;
    }
}
