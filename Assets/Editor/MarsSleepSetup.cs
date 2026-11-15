#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menu Editor: membuat FadeOverlay di Canvas dan objek Bed (beserta SpawnPoint), lalu menyambungkan referensinya.
/// Aman dijalankan berulang kali; referensi yang sudah Anda isi tidak ditimpa.
/// Mars > Setup Bed and Fade
/// </summary>
public static class MarsSleepSetup
{
    [MenuItem("Mars/Setup Bed and Fade")]
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

        // 1. FadeOverlay
        FadeUI fade = Object.FindFirstObjectByType<FadeUI>();
        if (fade == null)
        {
            GameObject overlay = new GameObject("FadeOverlay", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            Undo.RegisterCreatedObjectUndo(overlay, "Create FadeOverlay");
            overlay.transform.SetParent(canvas.transform, false);

            RectTransform rect = overlay.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = overlay.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 1f);
            image.raycastTarget = false;

            CanvasGroup group = overlay.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;

            fade = Undo.AddComponent<FadeUI>(overlay);
            MarsUISetup.SetRef(fade, "group", group);
            overlay.transform.SetAsLastSibling();
            report.AppendLine("- FadeOverlay dibuat.");
        }
        else
        {
            report.AppendLine("- FadeOverlay sudah ada.");
        }

        // 2. Bed
        PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
        TimeManager timeManager = Object.FindFirstObjectByType<TimeManager>();
        DialogueUI dialogueUI = Object.FindFirstObjectByType<DialogueUI>();

        Bed bed = Object.FindFirstObjectByType<Bed>();
        if (bed == null)
        {
            GameObject bedObject = new GameObject("Bed", typeof(SpriteRenderer), typeof(BoxCollider2D));
            Undo.RegisterCreatedObjectUndo(bedObject, "Create Bed");

            int interactableLayer = LayerMask.NameToLayer("Interactable");
            if (interactableLayer >= 0) bedObject.layer = interactableLayer;
            else report.AppendLine("- PERINGATAN: layer 'Interactable' tidak ditemukan. Set layer Bed secara manual.");

            if (player != null) bedObject.transform.position = player.transform.position + new Vector3(2f, 0f, 0f);

            GameObject spawn = new GameObject("SpawnPoint");
            Undo.RegisterCreatedObjectUndo(spawn, "Create SpawnPoint");
            spawn.transform.SetParent(bedObject.transform, false);
            spawn.transform.localPosition = new Vector3(0f, -1.2f, 0f);

            bed = Undo.AddComponent<Bed>(bedObject);
            report.AppendLine("- Bed dibuat dekat Player (pindahkan ke posisi yang diinginkan; SpawnPoint adalah anaknya).");
        }
        else
        {
            report.AppendLine("- Bed sudah ada: " + bed.gameObject.name);
        }

        Transform spawnChild = bed.transform.Find("SpawnPoint");
        SetRefIfEmpty(bed, "fadeUI", fade);
        SetRefIfEmpty(bed, "player", player);
        SetRefIfEmpty(bed, "timeManager", timeManager);
        SetRefIfEmpty(bed, "dialogueUI", dialogueUI);
        SetRefIfEmpty(bed, "spawnPoint", spawnChild);

        if (player == null) report.AppendLine("- PERINGATAN: PlayerMovement tidak ditemukan.");
        if (timeManager == null) report.AppendLine("- PERINGATAN: TimeManager tidak ada. Jalankan Mars > Setup HUD and Time.");
        if (spawnChild == null) report.AppendLine("- PERINGATAN: anak 'SpawnPoint' tidak ada di Bed; isi field Spawn Point manual.");

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        report.AppendLine();
        report.Append("Tekan Ctrl+S untuk menyimpan scene.");
        Debug.Log("MarsSleepSetup:\n" + report);
        EditorUtility.DisplayDialog("Mars Setup", report.ToString(), "OK");
    }

    private static void SetRefIfEmpty(Object target, string fieldName, Object value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty prop = so.FindProperty(fieldName);
        if (prop != null && prop.objectReferenceValue == null) MarsUISetup.SetRef(target, fieldName, value);
    }
}
#endif
