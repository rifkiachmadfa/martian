using System.Collections;
using UnityEngine;

/// <summary>
/// Layar hitam penuh untuk transisi (fade to black / fade in). Hanya tampilan:
/// pemanggil (misalnya Bed) menjalankan lewat "yield return fadeUI.FadeToBlack()" di dalam coroutine.
/// Dipasang pada objek "FadeOverlay" (Image hitam + CanvasGroup) di Canvas.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class FadeUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField, Min(0.05f)] private float fadeDuration = 1f;

    private void Awake()
    {
        if (group == null) group = GetComponent<CanvasGroup>();

        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        // Selalu digambar paling atas, di atas HUD dan dialog.
        transform.SetAsLastSibling();
    }

    public IEnumerator FadeToBlack()
    {
        return Fade(1f);
    }

    public IEnumerator FadeFromBlack()
    {
        return Fade(0f);
    }

    private IEnumerator Fade(float target)
    {
        float start = group.alpha;
        float elapsed = 0f;

        group.blocksRaycasts = true;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / fadeDuration));
            yield return null;
        }

        group.alpha = target;
        group.blocksRaycasts = target > 0f;
    }
}
