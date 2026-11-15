using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD gameplay: hari, timer, serta Food, Water, Oxygen (habitat), dan Energy.
/// Hanya tampilan: membaca ResourceManager (lewat event) dan TimeManager.
/// Suit Oxygen dan Ice belum ditampilkan; menunggu milestone suit/outdoor.
/// </summary>
public class HudUI : MonoBehaviour
{
    [SerializeField] private Text dayText;
    [SerializeField] private Text timerText;
    [SerializeField] private Text foodText;
    [SerializeField] private Text waterText;
    [SerializeField] private Text oxygenText;
    [SerializeField] private Text energyText;

    private ResourceManager resources;
    private TimeManager time;
    private int shownSeconds = -1;

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
    }

    private void OnDestroy()
    {
        if (resources != null) resources.OnResourceChanged -= HandleResourceChanged;
        if (time != null) time.OnDayStarted -= HandleDayStarted;
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
            default: return; // Ice dan Suit Oxygen belum ada di HUD
        }

        target.text = label + ": " + Mathf.RoundToInt(resources.Get(type)) + " / " + Mathf.RoundToInt(resources.GetMax(type));
    }
}
