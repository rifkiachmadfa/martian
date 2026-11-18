using UnityEngine;

/// <summary>
/// Bar isi untuk HUD (hanya tampilan). Isi bar adalah anak (fill) dari area isi; lebarnya diatur lewat anchor
/// sehingga sprite 9-slice yang di-tile tetap berbentuk segmen. Dibuat oleh menu Mars > Apply HUD Skin (Pixel UI Sci-Fi).
/// </summary>
public class HudBar : MonoBehaviour
{
    [SerializeField] private RectTransform fill;

    private float shown = -1f;

    public void SetFraction(float fraction)
    {
        if (fill == null) return;

        float f = Mathf.Clamp01(fraction);
        if (Mathf.Approximately(f, shown)) return;
        shown = f;

        fill.anchorMin = Vector2.zero;
        fill.anchorMax = new Vector2(f, 1f);
        fill.offsetMin = Vector2.zero;
        fill.offsetMax = Vector2.zero;
        fill.gameObject.SetActive(f > 0.001f);
    }
}
