using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Kotak dialog sederhana. Menampilkan baris teks satu per satu.
/// Lanjut ke baris berikutnya / menutup dialog di baris terakhir: tombol E, Spasi, atau klik mouse di mana saja.
/// Tombol Tutup (opsional, closeButton) menutup dialog langsung, melewati sisa baris.
/// Dialog TIDAK menghentikan waktu atau resource (lihat TimeManager.pauseDuringDialogue).
/// Hanya tampilan: isi dialog dikirim oleh pemanggil (misalnya TARS).
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject dialogueRoot;
    [SerializeField] private Text speakerText;   // opsional
    [SerializeField] private Text lineText;
    [Tooltip("Opsional. Tombol Tutup: menutup dialog langsung. Dibuat oleh menu Mars > Setup Dialogue and Prompt UI.")]
    [SerializeField] private Button closeButton;

    [Header("Input lanjut / tutup dialog")]
    [SerializeField] private KeyCode advanceKey = KeyCode.E;
    [SerializeField] private KeyCode alternateAdvanceKey = KeyCode.Space;
    [Tooltip("Klik mouse kiri di mana saja = lanjut / tutup (di luar tombol Tutup).")]
    [SerializeField] private bool clickAnywhereToAdvance = true;

    private string[] lines;
    private int lineIndex;
    private Action onFinished;
    private int openedFrame = -1;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (dialogueRoot == null || lineText == null)
        {
            Debug.LogError("DialogueUI: Dialogue Root / Line Text belum diisi di Inspector.", this);
            enabled = false;
            return;
        }

        dialogueRoot.SetActive(false);

        if (closeButton != null) closeButton.onClick.AddListener(CloseNow);
    }

    private void OnDestroy()
    {
        if (closeButton != null) closeButton.onClick.RemoveListener(CloseNow);
    }

    private void Update()
    {
        if (!IsOpen) return;

        // Abaikan tombol yang sama yang baru membuka dialog.
        if (Time.frameCount == openedFrame) return;

        bool advance =
            Input.GetKeyDown(advanceKey) ||
            Input.GetKeyDown(alternateAdvanceKey) ||
            (clickAnywhereToAdvance && Input.GetMouseButtonDown(0) && !IsPointerOverCloseButton());

        if (advance) ShowNextLine();
    }

    /// <summary>Menutup dialog sekarang juga (melewati sisa baris). Dipakai tombol Tutup.</summary>
    private void CloseNow()
    {
        if (IsOpen) Close();
    }

    // Klik pada tombol Tutup ditangani tombolnya sendiri; jangan dihitung juga sebagai "klik di mana saja".
    private bool IsPointerOverCloseButton()
    {
        if (closeButton == null || !closeButton.gameObject.activeInHierarchy) return false;

        Canvas canvas = closeButton.GetComponentInParent<Canvas>();
        Camera uiCamera = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;
        return RectTransformUtility.RectangleContainsScreenPoint((RectTransform)closeButton.transform, Input.mousePosition, uiCamera);
    }

    public void Show(string speaker, string[] dialogueLines, Action finishedCallback)
    {
        if (!enabled || dialogueLines == null || dialogueLines.Length == 0)
        {
            finishedCallback?.Invoke();
            return;
        }

        lines = dialogueLines;
        lineIndex = 0;
        onFinished = finishedCallback;
        openedFrame = Time.frameCount;

        if (speakerText != null) speakerText.text = speaker;

        IsOpen = true;
        dialogueRoot.SetActive(true);
        lineText.text = lines[lineIndex];
    }

    private void ShowNextLine()
    {
        lineIndex++;

        if (lineIndex >= lines.Length)
        {
            Close();
            return;
        }

        lineText.text = lines[lineIndex];
    }

    private void Close()
    {
        IsOpen = false;
        dialogueRoot.SetActive(false);

        Action callback = onFinished;
        onFinished = null;
        callback?.Invoke();
    }
}
