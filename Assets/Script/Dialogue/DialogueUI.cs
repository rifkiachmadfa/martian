using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Kotak dialog sederhana. Menampilkan baris teks satu per satu; tekan E untuk lanjut.
/// Hanya tampilan: isi dialog dikirim oleh pemanggil (misalnya TARS).
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject dialogueRoot;
    [SerializeField] private Text speakerText;   // opsional
    [SerializeField] private Text lineText;
    [SerializeField] private KeyCode advanceKey = KeyCode.E;

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
    }

    private void Update()
    {
        if (!IsOpen) return;

        // Abaikan tombol yang sama yang baru membuka dialog.
        if (Time.frameCount == openedFrame) return;

        if (Input.GetKeyDown(advanceKey)) ShowNextLine();
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
