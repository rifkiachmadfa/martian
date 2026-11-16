using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menampilkan prompt "[E] ..." saat ada objek interaktif yang bisa dipakai.
/// Hanya tampilan; tidak berisi logika game.
/// </summary>
public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private InteractionDetector detector;
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private Text promptText;
    [SerializeField] private string keyLabel = "E";

    private void Start()
    {
        if (detector == null || promptRoot == null || promptText == null)
        {
            Debug.LogError("InteractionPromptUI: referensi Inspector belum lengkap.", this);
            enabled = false;
            return;
        }

        promptRoot.SetActive(false);
    }

    private void Update()
    {
        string prompt = detector.CurrentPrompt;
        bool canShow = !string.IsNullOrEmpty(prompt);

        if (promptRoot.activeSelf != canShow)
            promptRoot.SetActive(canShow);

        if (canShow)
            promptText.text = "[" + keyLabel + "] " + prompt;
    }
}
