#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menu Editor: membuat objek interaktif placeholder (IceDeposit, WaterPurifier, OxygenGenerator, FarmingPlot,
/// SolarPanel + RepairPoint) dan panel Ice di HUD.
/// Aman dijalankan berulang kali; objek yang sudah ada dan referensi yang sudah Anda isi tidak ditimpa.
/// Mars > Setup Interactive Objects
/// Objek baru diletakkan di sekitar Player dengan sprite kotak placeholder: Anda yang memindahkannya ke
/// lokasi akhir (Control Room, Farming Area, Outdoor) dan menggantinya dengan sprite sungguhan.
/// </summary>
public static class MarsObjectsSetup
{
    [MenuItem("Mars/Setup Interactive Objects")]
    public static void Setup()
    {
        StringBuilder report = new StringBuilder();

        PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
        if (player == null)
        {
            EditorUtility.DisplayDialog("Mars Setup", "PlayerMovement tidak ditemukan di scene.", "OK");
            return;
        }

        if (Object.FindFirstObjectByType<DialogueUI>() == null)
            report.AppendLine("- PERINGATAN: DialogueUI tidak ditemukan. Jalankan Mars > Setup Dialogue and Prompt UI.");
        if (Object.FindFirstObjectByType<ResourceManager>() == null)
            report.AppendLine("- PERINGATAN: ResourceManager tidak ada di scene.");
        if (Object.FindFirstObjectByType<TimeManager>() == null)
            report.AppendLine("- PERINGATAN: TimeManager tidak ada di scene (tanaman tidak akan tumbuh).");

        Vector3 origin = player.transform.position;
        int layer = LayerMask.NameToLayer("Interactable");
        if (layer < 0)
            report.AppendLine("- PERINGATAN: layer 'Interactable' tidak ditemukan. Set layer tiap objek secara manual.");

        // Outdoor
        EnsureInteractable<IceDeposit>("IceDeposit", origin + new Vector3(3f, 3f, 0f),
            new Color(0.6f, 0.85f, 1f), layer, report, "Pindahkan ke Outdoor.");

        SolarPanel panel = Object.FindFirstObjectByType<SolarPanel>();
        if (panel == null)
        {
            GameObject go = new GameObject("SolarPanel", typeof(SpriteRenderer));
            Undo.RegisterCreatedObjectUndo(go, "Create SolarPanel");
            go.transform.position = origin + new Vector3(5f, 3f, 0f);
            ApplyPlaceholderSprite(go.GetComponent<SpriteRenderer>(), new Color(0.3f, 0.5f, 0.9f));
            panel = Undo.AddComponent<SolarPanel>(go);
            report.AppendLine("- SolarPanel dibuat. Pindahkan ke Outdoor. Duplikat objek ini untuk menambah panel.");
        }
        else
        {
            report.AppendLine("- SolarPanel sudah ada: " + panel.gameObject.name);
        }
        SetRefIfEmpty(panel, "spriteRenderer", panel.GetComponent<SpriteRenderer>());

        RepairPoint repair = EnsureInteractable<RepairPoint>("RepairPoint", origin + new Vector3(5f, 1.5f, 0f),
            new Color(1f, 0.8f, 0.2f), layer, report, "Letakkan dekat SolarPanel; isi field Panel bila ada banyak panel.");
        SetRefIfEmpty(repair, "panel", panel);

        // Farming Area
        EnsureInteractable<WaterPurifier>("WaterPurifier", origin + new Vector3(-3f, 3f, 0f),
            new Color(0.4f, 0.7f, 0.8f), layer, report, "Pindahkan ke Farming Area.");
        EnsureInteractable<OxygenGenerator>("OxygenGenerator", origin + new Vector3(-5f, 3f, 0f),
            new Color(0.3f, 0.8f, 0.7f), layer, report, "Pindahkan ke Farming Area.");

        FarmingPlot plot = EnsureInteractable<FarmingPlot>("FarmingPlot", origin + new Vector3(-3f, 5f, 0f),
            Color.white, layer, report, "Pindahkan ke Farming Area. Duplikat objek ini untuk menambah petak.");
        SetRefIfEmpty(plot, "spriteRenderer", plot.GetComponent<SpriteRenderer>());

        // HUD: panel Ice
        SetupIceHud(report);

        EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
        report.AppendLine();
        report.Append("Tekan Ctrl+S untuk menyimpan scene.");
        Debug.Log("MarsObjectsSetup:\n" + report);
        EditorUtility.DisplayDialog("Mars Setup", report.ToString(), "OK");
    }

    // Membuat objek interaktif (SpriteRenderer + BoxCollider2D trigger + komponen T) bila belum ada di scene.
    private static T EnsureInteractable<T>(string objectName, Vector3 position, Color color, int layer,
        StringBuilder report, string hint) where T : MonoBehaviour
    {
        T existing = Object.FindFirstObjectByType<T>();
        if (existing != null)
        {
            report.AppendLine("- " + objectName + " sudah ada: " + existing.gameObject.name);
            return existing;
        }

        GameObject go = new GameObject(objectName, typeof(SpriteRenderer), typeof(BoxCollider2D));
        Undo.RegisterCreatedObjectUndo(go, "Create " + objectName);
        if (layer >= 0) go.layer = layer;
        go.transform.position = position;

        // Ukuran diisi eksplisit: collider yang dibuat sebelum sprite ada akan berukuran hampir nol.
        BoxCollider2D box = go.GetComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(1.5f, 1.5f);

        ApplyPlaceholderSprite(go.GetComponent<SpriteRenderer>(), color);

        T component = Undo.AddComponent<T>(go);
        report.AppendLine("- " + objectName + " dibuat. " + hint);
        return component;
    }

    // Kotak placeholder bawaan Unity supaya objek terlihat sebelum sprite sungguhan ada.
    private static void ApplyPlaceholderSprite(SpriteRenderer renderer, Color color)
    {
        Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (sprite != null)
        {
            renderer.sprite = sprite;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = new Vector2(1.5f, 1.5f);
        }

        renderer.color = color;
        renderer.sortingOrder = 10;
    }

    private static void SetupIceHud(StringBuilder report)
    {
        HudUI hud = Object.FindFirstObjectByType<HudUI>();
        if (hud == null)
        {
            report.AppendLine("- PERINGATAN: HudUI tidak ditemukan. Jalankan Mars > Setup HUD and Time, lalu jalankan menu ini lagi.");
            return;
        }

        if (MarsUISetup.IsConfigured(hud, "iceText"))
        {
            report.AppendLine("- Panel Ice di HUD sudah ada.");
            return;
        }

        bool created;
        GameObject panel = MarsUISetup.GetOrCreatePanel(hud.transform, "IcePanel", out created);
        if (created)
        {
            // Kanan atas, tepat di bawah ResourcePanel (tinggi 190 mulai dari -20).
            RectTransform r = panel.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(1f, 1f);
            r.anchorMax = new Vector2(1f, 1f);
            r.pivot = new Vector2(1f, 1f);
            r.anchoredPosition = new Vector2(-20f, -220f);
            r.sizeDelta = new Vector2(300f, 56f);
        }

        Text iceText = MarsUISetup.GetOrCreateText(panel.transform, "IceText", "Ice: 0 / 0", 30,
            Color.white, TextAnchor.MiddleLeft, out created);
        if (created)
        {
            RectTransform r = iceText.rectTransform;
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(15f, 5f);
            r.offsetMax = new Vector2(-15f, -5f);
        }

        MarsUISetup.SetRef(hud, "iceText", iceText);
        report.AppendLine("- Panel Ice dibuat di kanan atas HUD, di bawah panel resource.");
    }

    private static void SetRefIfEmpty(Object target, string fieldName, Object value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty prop = so.FindProperty(fieldName);
        if (prop != null && prop.objectReferenceValue == null) MarsUISetup.SetRef(target, fieldName, value);
    }
}
#endif
