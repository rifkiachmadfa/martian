using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD gameplay: hari, timer, serta Food, Water, Oxygen (habitat), dan Energy.
/// Hanya tampilan: membaca ResourceManager (lewat event) dan TimeManager.
/// Suit Oxygen tampil di panel opsional (suitPanel) hanya saat suit dikenakan. Ice tampil di iceText (opsional).
/// </summary>
public class HudUI : MonoBehaviour
{
    [SerializeField] private Text dayText;
    [SerializeField] private Text timerText;
    [SerializeField] private Text foodText;
    [SerializeField] private Text waterText;
    [SerializeField] private Text oxygenText;
    [SerializeField] private Text energyText;

    [Header("Ice (opsional; diisi menu Mars > Setup Interactive Objects)")]
    [SerializeField] private Text iceText;

    [Header("Suit (opsional; diisi menu Mars > Setup Suit and Gate)")]
    [SerializeField] private GameObject suitPanel;
    [SerializeField] private Text suitOxygenText;
    [Tooltip("Teks berubah kuning/merah di bawah proporsi ini dari kapasitas tabung (usulan, sekitar 30% dan 10% sesuai GDD).")]
    [SerializeField, Range(0f, 1f)] private float suitWarnFraction = 0.3f;
    [SerializeField, Range(0f, 1f)] private float suitDangerFraction = 0.1f;

    private ResourceManager resources;
    private TimeManager time;
    private SuitSystem suit;
    private int shownSeconds = -1;
    private int shownSuitValue = -1;

    private void Start()
    {
        if (dayText == null || timerText == null || foodText == null ||
            waterText == null || oxygenText == null || energyText == null)
        {
            Debug.LogError("HudUI: referensi Text belum lengkap. Jalankan menu Mars > Setup HUD and Time.", this);
            enabled = false;
            return;
        }

        resources = ResourceManager.Instance;
        if (resources != null)
        {
            resources.OnResourceChanged += HandleResourceChanged;
            RefreshResource(ResourceType.Food);
            RefreshResource(ResourceType.Water);
            RefreshResource(ResourceType.HabitatOxygen);
            RefreshResource(ResourceType.Energy);
            RefreshResource(ResourceType.Ice);
        }
        else
        {
            Debug.LogWarning("HudUI: ResourceManager tidak ada di scene.", this);
        }

        time = TimeManager.Instance;
        if (time != null)
        {
            time.OnDayStarted += HandleDayStarted;
            RefreshDay();
        }
        else
        {
            Debug.LogWarning("HudUI: TimeManager tidak ada di scene.", this);
        }

        suit = SuitSystem.Instance;
        if (suit != null) suit.OnEquippedChanged += HandleSuitEquippedChanged;
        RefreshSuit();
    }

    private void OnDestroy()
    {
        if (resources != null) resources.OnResourceChanged -= HandleResourceChanged;
        if (time != null) time.OnDayStarted -= HandleDayStarted;
        if (suit != null) suit.OnEquippedChanged -= HandleSuitEquippedChanged;
    }

    private void Update()
    {
        if (time == null) return;

        int seconds = Mathf.CeilToInt(time.RemainingSeconds);
        if (seconds == shownSeconds) return;

        shownSeconds = seconds;
        timerText.text = (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00");
    }

    private void HandleResourceChanged(ResourceType type, float amount, float max)
    {
        RefreshResource(type);
    }

    private void HandleDayStarted(int day)
    {
        RefreshDay();
    }

    private void HandleSuitEquippedChanged(bool equipped)
    {
        RefreshSuit();
    }

    private void RefreshSuit()
    {
        if (suitPanel == null || suitOxygenText == null) return;

        bool show = suit != null && suit.IsEquipped && resources != null;
        suitPanel.SetActive(show);
        if (!show)
        {
            shownSuitValue = -1; // paksa digambar ulang saat suit dikenakan lagi
            return;
        }

        float current = resources.Get(ResourceType.SuitOxygen);
        float max = resources.GetMax(ResourceType.SuitOxygen);

        int rounded = Mathf.RoundToInt(current);
        if (rounded == shownSuitValue) return; // oksigen suit berubah tiap frame saat di luar; hindari menulis teks yang sama
        shownSuitValue = rounded;

        suitOxygenText.text = "Suit O2: " + rounded + " / " + Mathf.RoundToInt(max);

        float fraction = max > 0f ? current / max : 0f;
        if (fraction <= suitDangerFraction) suitOxygenText.color = new Color(1f, 0.35f, 0.3f);
        else if (fraction <= suitWarnFraction) suitOxygenText.color = new Color(1f, 0.85f, 0.3f);
        else suitOxygenText.color = Color.white;
    }

    private void RefreshDay()
    {
        dayText.text = "Day " + time.CurrentDay + " / " + time.TotalDays;
        shownSeconds = -1; // paksa timer digambar ulang
    }

    private void RefreshResource(ResourceType type)
    {
        string label;
        Text target;

        switch (type)
        {
            case ResourceType.Food: label = "Food"; target = foodText; break;
            case ResourceType.Water: label = "Water"; target = waterText; break;
            case ResourceType.HabitatOxygen: label = "Oxygen"; target = oxygenText; break;
            case ResourceType.Energy: label = "Energy"; target = energyText; break;
            case ResourceType.Ice:
                if (iceText == null) return; // panel Ice opsional
                label = "Ice"; target = iceText; break;
            case ResourceType.SuitOxygen: RefreshSuit(); return;
            default: return;
        }

        target.text = label + ": " + Mathf.RoundToInt(resources.Get(type)) + " / " + Mathf.RoundToInt(resources.GetMax(type));
    }
}
