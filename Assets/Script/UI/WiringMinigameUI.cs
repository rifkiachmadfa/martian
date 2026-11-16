using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Minigame perbaikan kabel Solar Panel (GDD 11.4): 3-4 kabel berwarna di sisi kiri, konektor di sisi kanan
/// (urutannya diacak tiap dibuka). Pemain menyeret kabel dengan mouse (tahan klik kiri) ke konektor berwarna sama.
/// Salah sambung: kabel kembali dan boleh dicoba lagi. Semua kabel benar: onSuccess dipanggil.
/// Esc membatalkan (panel tetap rusak). Selama terbuka: TimeManager.ExternalPause aktif dan gerakan Player dikunci
/// (nilai sebelumnya dipulihkan saat ditutup). Tidak ada batas waktu; "15-30 detik" adalah target ukuran teka-teki.
/// Seluruh tampilan dibuat lewat kode saat Awake di bawah Canvas (UnityEngine.UI.Text legacy, sama seperti UI lain),
/// jadi hanya perlu dipasang pada objek Canvas (menu Mars > Setup Panel Damage and Wiring Minigame).
/// Hanya tampilan dan input minigame: pemanggil (RepairPoint) yang memperbaiki SolarPanel.
/// </summary>
public class WiringMinigameUI : MonoBehaviour
{
    private const int MaxWires = 4;

    [Header("Aturan (USULAN)")]
    [SerializeField, Range(3, MaxWires)] private int wireCount = 4;
    [Tooltip("Jeda singkat setelah semua kabel tersambung sebelum minigame menutup.")]
    [SerializeField, Min(0f)] private float successHoldSeconds = 0.8f;
    [SerializeField] private KeyCode cancelKey = KeyCode.Escape;

    [Header("Referensi (otomatis dicari jika kosong)")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private PlayerMovement player;

    private static readonly Color[] WireColors =
    {
        new Color(0.90f, 0.20f, 0.20f), // merah
        new Color(0.25f, 0.80f, 0.35f), // hijau
        new Color(0.25f, 0.50f, 0.95f), // biru
        new Color(0.95f, 0.80f, 0.20f), // kuning
    };
    private static readonly string[] WireNames = { "Merah", "Hijau", "Biru", "Kuning" };

    private const float SlotSpacing = 110f;
    private const float TerminalX = 330f;
    private const float LineThickness = 14f;

    private GameObject root;
    private Text titleText;
    private Text statusText;
    private RectTransform linesRect;
    private RectTransform dragLine;
    private readonly RectTransform[] leftTerminals = new RectTransform[MaxWires];
    private readonly RectTransform[] rightTerminals = new RectTransform[MaxWires];
    private readonly Image[] rightImages = new Image[MaxWires];
    private readonly Text[] rightLabels = new Text[MaxWires];
    private readonly RectTransform[] connectedLines = new RectTransform[MaxWires];

    private readonly int[] colorAtRightSlot = new int[MaxWires];
    private readonly bool[] connected = new bool[MaxWires];
    private int connectedCount;
    private int dragFrom = -1;
    private float successTimer = -1f;

    private Action onSuccess;
    private Action onCancel;
    private bool previousExternalPause;
    private bool previousMovementEnabled;
    private Font font;

    /// <summary>True selama minigame terbuka.</summary>
    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("WiringMinigameUI: Canvas tidak ditemukan. Pasang komponen ini pada Canvas.", this);
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

    /// <summary>
    /// Membuka minigame untuk satu panel. Mengembalikan false bila sudah terbuka atau belum siap.
    /// onSuccess dipanggil setelah semua kabel tersambung; onCancel bila pemain menekan Esc (boleh null).
    /// </summary>
    public bool Open(string panelName, Action successCallback, Action cancelCallback)
    {
        if (!enabled || IsOpen || root == null) return false;

        onSuccess = successCallback;
        onCancel = cancelCallback;

        ShuffleRightSlots();
        ResetState();
        titleText.text = "Perbaiki " + panelName;
        statusText.text = "Seret setiap kabel ke konektor berwarna sama. Esc untuk batal.";

        TimeManager time = TimeManager.Instance;
        if (time != null)
        {
            previousExternalPause = time.ExternalPause;
            time.ExternalPause = true;
        }

        if (player != null)
        {
            previousMovementEnabled = player.MovementEnabled;
            player.MovementEnabled = false;
        }

        IsOpen = true;
        root.SetActive(true);
        return true;
    }

    private void Update()
    {
        if (!IsOpen) return;

        if (successTimer >= 0f)
        {
            successTimer -= Time.deltaTime;
            if (successTimer <= 0f) Close(true);
            return;
        }

        if (Input.GetKeyDown(cancelKey))
        {
            Close(false);
            return;
        }

        Camera uiCamera = GetUiCamera();

        if (dragFrom < 0 && Input.GetMouseButtonDown(0))
        {
            for (int i = 0; i < wireCount; i++)
            {
                if (connected[i]) continue;
                if (RectTransformUtility.RectangleContainsScreenPoint(leftTerminals[i], Input.mousePosition, uiCamera))
                {
                    dragFrom = i;
                    dragLine.gameObject.SetActive(true);
                    dragLine.GetComponent<Image>().color = WireColors[i];
                    break;
                }
            }
        }

        if (dragFrom < 0) return;

        Vector2 start = CenterInLinesSpace(leftTerminals[dragFrom]);
        Vector2 pointer;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(linesRect, Input.mousePosition, uiCamera, out pointer);
        PlaceLine(dragLine, start, pointer);

        if (Input.GetMouseButtonUp(0)) ReleaseDrag(uiCamera);
    }

    private void ReleaseDrag(Camera uiCamera)
    {
        int from = dragFrom;
        dragFrom = -1;
        dragLine.gameObject.SetActive(false);

        int slot = -1;
        for (int j = 0; j < wireCount; j++)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(rightTerminals[j], Input.mousePosition, uiCamera))
            {
                slot = j;
                break;
            }
        }

        if (slot < 0) return; // dilepas di tempat kosong: kabel kembali, tanpa pesan.

        if (colorAtRightSlot[slot] != from)
        {
            statusText.text = "Salah sambung. Coba lagi.";
            return;
        }

        connected[from] = true;
        connectedCount++;

        RectTransform line = connectedLines[from];
        line.gameObject.SetActive(true);
        line.GetComponent<Image>().color = WireColors[from];
        PlaceLine(line, CenterInLinesSpace(leftTerminals[from]), CenterInLinesSpace(rightTerminals[slot]));

        if (connectedCount >= wireCount)
        {
            statusText.text = "Semua kabel tersambung!";
            if (successHoldSeconds <= 0f) Close(true);
            else successTimer = successHoldSeconds;
        }
        else
        {
            statusText.text = "Tersambung (" + connectedCount + "/" + wireCount + ").";
        }
    }

    private void Close(bool success)
    {
        IsOpen = false;
        dragFrom = -1;
        successTimer = -1f;
        root.SetActive(false);

        TimeManager time = TimeManager.Instance;
        if (time != null) time.ExternalPause = previousExternalPause;
        if (player != null) player.MovementEnabled = previousMovementEnabled;

        Action callback = success ? onSuccess : onCancel;
        onSuccess = null;
        onCancel = null;
        callback?.Invoke();
    }

    // Susunan konektor kanan diacak agar ada teka-tekinya; diulang bila hasilnya sama dengan urutan kiri.
    private void ShuffleRightSlots()
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            for (int i = 0; i < wireCount; i++) colorAtRightSlot[i] = i;
            for (int i = wireCount - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                int temp = colorAtRightSlot[i];
                colorAtRightSlot[i] = colorAtRightSlot[j];
                colorAtRightSlot[j] = temp;
            }

            bool identical = true;
            for (int i = 0; i < wireCount; i++)
            {
                if (colorAtRightSlot[i] != i) { identical = false; break; }
            }
            if (!identical) return;
        }
    }

    private void ResetState()
    {
        connectedCount = 0;
        dragFrom = -1;
        successTimer = -1f;
        dragLine.gameObject.SetActive(false);

        for (int i = 0; i < MaxWires; i++)
        {
            bool active = i < wireCount;
            connected[i] = false;
            connectedLines[i].gameObject.SetActive(false);
            leftTerminals[i].gameObject.SetActive(active);
            rightTerminals[i].gameObject.SetActive(active);
            if (!active) continue;

            float y = SlotY(i, wireCount);
            leftTerminals[i].anchoredPosition = new Vector2(-TerminalX, y);
            rightTerminals[i].anchoredPosition = new Vector2(TerminalX, y);

            int colorIndex = colorAtRightSlot[i];
            rightImages[i].color = WireColors[colorIndex];
            rightLabels[i].text = WireNames[colorIndex];
        }
    }

    private static float SlotY(int index, int count) => ((count - 1) * 0.5f - index) * SlotSpacing;

    private Camera GetUiCamera() =>
        canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

    private Vector2 CenterInLinesSpace(RectTransform terminal)
    {
        Vector3 local = linesRect.InverseTransformPoint(terminal.TransformPoint(terminal.rect.center));
        return new Vector2(local.x, local.y);
    }

    // Garis = Image lurus yang diregangkan dan diputar dari titik a ke titik b (koordinat lokal linesRect).
    private static void PlaceLine(RectTransform line, Vector2 a, Vector2 b)
    {
        Vector2 delta = b - a;
        line.anchoredPosition = (a + b) * 0.5f;
        line.sizeDelta = new Vector2(delta.magnitude, LineThickness);
        line.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
    }

    // ---------- Pembuatan UI lewat kode ----------

    private void BuildUI()
    {
        root = NewRect("WiringMinigameRoot", canvas.transform, Vector2.zero, Vector2.zero, typeof(Image));
        RectTransform rootRect = (RectTransform)root.transform;
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f); // meredupkan layar dan menahan klik ke UI di belakang

        GameObject panel = NewRect("Panel", root.transform, Vector2.zero, new Vector2(900f, 560f), typeof(Image));
        panel.GetComponent<Image>().color = new Color(0.10f, 0.12f, 0.16f, 0.97f);

        titleText = NewText("TitleText", panel.transform, "Perbaiki Solar Panel", 38, TextAnchor.MiddleCenter,
            new Vector2(0f, 235f), new Vector2(860f, 60f), new Color(1f, 0.85f, 0.3f));
        statusText = NewText("StatusText", panel.transform, string.Empty, 26, TextAnchor.MiddleCenter,
            new Vector2(0f, -240f), new Vector2(860f, 50f), Color.white);

        // Lapisan garis dibuat sebelum terminal agar garis tergambar di bawah terminal.
        GameObject lines = NewRect("Lines", panel.transform, Vector2.zero, new Vector2(900f, 560f));
        linesRect = (RectTransform)lines.transform;

        for (int i = 0; i < MaxWires; i++)
            connectedLines[i] = NewLine("ConnectedLine" + i, linesRect);
        dragLine = NewLine("DragLine", linesRect);

        for (int i = 0; i < MaxWires; i++)
        {
            GameObject left = NewRect("LeftTerminal" + i, panel.transform, Vector2.zero, new Vector2(150f, 70f), typeof(Image));
            left.GetComponent<Image>().color = WireColors[i];
            leftTerminals[i] = (RectTransform)left.transform;
            NewText("Label", left.transform, WireNames[i], 26, TextAnchor.MiddleCenter,
                Vector2.zero, new Vector2(150f, 70f), Color.black);

            GameObject right = NewRect("RightTerminal" + i, panel.transform, Vector2.zero, new Vector2(150f, 70f), typeof(Image));
            rightImages[i] = right.GetComponent<Image>();
            rightTerminals[i] = (RectTransform)right.transform;
            rightLabels[i] = NewText("Label", right.transform, string.Empty, 26, TextAnchor.MiddleCenter,
                Vector2.zero, new Vector2(150f, 70f), Color.black);
        }
    }

    private RectTransform NewLine(string objectName, Transform parent)
    {
        GameObject go = NewRect(objectName, parent, Vector2.zero, new Vector2(10f, LineThickness), typeof(Image));
        go.GetComponent<Image>().raycastTarget = false;
        go.SetActive(false);
        return (RectTransform)go.transform;
    }

    // Anchor dan pivot di tengah, sehingga anchoredPosition = posisi relatif ke tengah induk.
    private static GameObject NewRect(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 size,
        params Type[] extraComponents)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rect = (RectTransform)go.transform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        foreach (Type component in extraComponents) go.AddComponent(component);
        return go;
    }

    private Text NewText(string objectName, Transform parent, string content, int size, TextAnchor alignment,
        Vector2 anchoredPosition, Vector2 sizeDelta, Color color)
    {
        GameObject go = NewRect(objectName, parent, anchoredPosition, sizeDelta, typeof(Text));
        Text text = go.GetComponent<Text>();
        text.font = font;
        text.text = content;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }
}
