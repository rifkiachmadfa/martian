#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menu Editor: membuat TimeManager dan HUD (hari, timer, resource) di Canvas lalu menyambungkan referensinya.
/// Aman dijalankan berulang kali; bagian yang sudah lengkap tidak diubah.
/// Mars > Setup HUD and Time
/// Memakai helper dari MarsUISetup.
/// </summary>
public static class MarsHudSetup
{
    [MenuItem("Mars/Setup HUD and Time")]
    public static void Setup()
    {
        StringBuilder report = new StringBuilder();

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Mars Setup",
                "Canvas tidak ditemukan. Jalankan dulu menu Mars > Setup Dialogue and Prompt UI.", "OK");
            return;
        }

        // 1. TimeManager
        DialogueUI dialogueUI = Object.FindFirstObjectByType<DialogueUI>();
        TimeManager timeManager = Object.FindFirstObjectByType<TimeManager>();
        if (timeManager == null)
        {
            GameObject go = new GameObject("TimeManager");
            Undo.RegisterCreatedObjectUndo(go, "Create TimeManager");
            timeManager = Undo.AddComponent<TimeManager>(go);
            report.AppendLine("- TimeManager dibuat.");
        }
        else
        {
            report.AppendLine("- TimeManager sudah ada.");
        }
        MarsUISetup.SetRef(timeManager, "dialogueUI", dialogueUI);
        if (dialogueUI == null)
            report.AppendLine("- PERINGATAN: DialogueUI tidak ditemukan; timer tidak akan berhenti saat dialog.");

        if (Object.FindFirstObjectByType<ResourceManager>() == null)
            report.AppendLine("- PERINGATAN: ResourceManager tidak ada di scene; HUD resource akan kosong.");

        // 2. HUD
        HudUI hud = Object.FindFirstObjectByType<HudUI>();
        if (hud != null && MarsUISetup.IsConfigured(hud, "dayText", "timerText", "foodText", "waterText", "oxygenText", "energyText"))
        {
            report.AppendLine("- HUD sudah lengkap.");
        }
        else
        {
            Transform rootTransform = canvas.transform.Find("HudRoot");
            GameObject hudRoot;
            if (rootTransform != null)
            {
                hudRoot = rootTransform.gameObject;
            }
            else
            {
                hudRoot = new GameObject("HudRoot", typeof(RectTransform));
                Undo.RegisterCreatedObjectUndo(hudRoot, "Create HudRoot");
                hudRoot.transform.SetParent(canvas.transform, false);
                RectTransform rootRect = hudRoot.GetComponent<RectTransform>();
                rootRect.anchorMin = Vector2.zero;
                rootRect.anchorMax = Vector2.one;
                rootRect.offsetMin = Vector2.zero;
                rootRect.offsetMax = Vector2.zero;
            }

            bool created;

            // Panel waktu: tengah atas
            GameObject timePanel = MarsUISetup.GetOrCreatePanel(hudRoot.transform, "TimePanel", out created);
            if (created) Place(timePanel.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(360f, 110f));

            Text dayText = MarsUISetup.GetOrCreateText(timePanel.transform, "DayText", "Day 1 / 7", 32,
                new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleCenter, out created);
            if (created)
            {
                dayText.fontStyle = FontStyle.Bold;
                Place(dayText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                    new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(-20f, 44f));
            }

            Text timerText = MarsUISetup.GetOrCreateText(timePanel.transform, "TimerText", "10:00", 44,
                Color.white, TextAnchor.MiddleCenter, out created);
            if (created)
            {
                RectTransform r = timerText.rectTransform;
                r.anchorMin = Vector2.zero;
                r.anchorMax = Vector2.one;
                r.offsetMin = new Vector2(10f, 8f);
                r.offsetMax = new Vector2(-10f, -56f);
            }

            // Panel resource: kanan atas
            GameObject resourcePanel = MarsUISetup.GetOrCreatePanel(hudRoot.transform, "ResourcePanel", out created);
            if (created) Place(resourcePanel.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(1f, 1f), new Vector2(-20f, -20f), new Vector2(300f, 190f));

            Text foodText = CreateResourceLine(resourcePanel.transform, "FoodText", "Food", 0);
            Text waterText = CreateResourceLine(resourcePanel.transform, "WaterText", "Water", 1);
            Text oxygenText = CreateResourceLine(resourcePanel.transform, "OxygenText", "Oxygen", 2);
            Text energyText = CreateResourceLine(resourcePanel.transform, "EnergyText", "Energy", 3);

            if (hud == null) hud = Undo.AddComponent<HudUI>(hudRoot);
            MarsUISetup.SetRef(hud, "dayText", dayText);
            MarsUISetup.SetRef(hud, "timerText", timerText);
            MarsUISetup.SetRef(hud, "foodText", foodText);
            MarsUISetup.SetRef(hud, "waterText", waterText);
            MarsUISetup.SetRef(hud, "oxygenText", oxygenText);
            MarsUISetup.SetRef(hud, "energyText", energyText);
            report.AppendLine("- HUD dibuat/diperbaiki di: " + hudRoot.name);
        }

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        report.AppendLine();
        report.Append("Tekan Ctrl+S untuk menyimpan scene.");
        Debug.Log("MarsHudSetup:\n" + report);
        EditorUtility.DisplayDialog("Mars Setup", report.ToString(), "OK");
    }

    private static Text CreateResourceLine(Transform parent, string name, string label, int index)
    {
        bool created;
        Text text = MarsUISetup.GetOrCreateText(parent, name, label + ": 0 / 0", 30,
            Color.white, TextAnchor.MiddleLeft, out created);
        if (created)
        {
            Place(text.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -10f - index * 42f), new Vector2(-30f, 40f));
        }
        return text;
    }

    private static void Place(RectTransform r, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        r.anchorMin = anchorMin;
        r.anchorMax = anchorMax;
        r.pivot = pivot;
        r.anchoredPosition = anchoredPosition;
        r.sizeDelta = sizeDelta;
    }
}
#endif
