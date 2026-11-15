#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menu Editor: membuat/memperbaiki Canvas, kotak dialog, dan prompt interaksi lalu
/// menyambungkannya ke TARS dan Player. Aman dijalankan berulang kali.
/// Mars > Setup Dialogue and Prompt UI
/// </summary>
public static class MarsUISetup
{
    [MenuItem("Mars/Setup Dialogue and Prompt UI")]
    public static void Setup()
    {
        StringBuilder report = new StringBuilder();

        // 1. Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Canvas");
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            report.AppendLine("- Canvas dibuat.");
        }

        // 2. Dialog
        DialogueUI dialogueUI = null;
        int removedDialogue = 0;
        foreach (DialogueUI candidate in Object.FindObjectsByType<DialogueUI>(FindObjectsSortMode.None))
        {
            if (dialogueUI == null && IsConfigured(candidate, "dialogueRoot", "lineText"))
                dialogueUI = candidate;
        }
        foreach (DialogueUI candidate in Object.FindObjectsByType<DialogueUI>(FindObjectsSortMode.None))
        {
            if (candidate != dialogueUI && !IsConfigured(candidate, "dialogueRoot", "lineText"))
            {
                Undo.DestroyObjectImmediate(candidate);
                removedDialogue++;
            }
        }

        if (dialogueUI == null)
        {
            bool created;
            GameObject dialogueRoot = GetOrCreatePanel(canvas.transform, "DialogueRoot", out created);
            if (created)
            {
                RectTransform r = dialogueRoot.GetComponent<RectTransform>();
                r.anchorMin = new Vector2(0.1f, 0f);
                r.anchorMax = new Vector2(0.9f, 0f);
                r.pivot = new Vector2(0.5f, 0f);
                r.anchoredPosition = new Vector2(0f, 30f);
                r.sizeDelta = new Vector2(0f, 180f);
            }

            Text speakerText = GetOrCreateText(dialogueRoot.transform, "SpeakerText", "TARS", 34,
                new Color(1f, 0.85f, 0.3f), TextAnchor.UpperLeft, out created);
            if (created)
            {
                RectTransform r = speakerText.rectTransform;
                r.anchorMin = new Vector2(0f, 1f);
                r.anchorMax = new Vector2(1f, 1f);
                r.pivot = new Vector2(0.5f, 1f);
                r.anchoredPosition = new Vector2(0f, -12f);
                r.sizeDelta = new Vector2(-40f, 44f);
                speakerText.fontStyle = FontStyle.Bold;
            }

            Text lineText = GetOrCreateText(dialogueRoot.transform, "LineText", "Isi dialog", 30,
                Color.white, TextAnchor.UpperLeft, out created);
            if (created)
            {
                RectTransform r = lineText.rectTransform;
                r.anchorMin = Vector2.zero;
                r.anchorMax = Vector2.one;
                r.offsetMin = new Vector2(20f, 15f);
                r.offsetMax = new Vector2(-20f, -60f);
            }

            dialogueUI = canvas.GetComponent<DialogueUI>();
            if (dialogueUI == null) dialogueUI = Undo.AddComponent<DialogueUI>(canvas.gameObject);

            SetRef(dialogueUI, "dialogueRoot", dialogueRoot);
            SetRef(dialogueUI, "speakerText", speakerText);
            SetRef(dialogueUI, "lineText", lineText);
            report.AppendLine("- DialogueUI dibuat/diperbaiki di: " + dialogueUI.gameObject.name);
        }
        else
        {
            report.AppendLine("- DialogueUI sudah lengkap di: " + dialogueUI.gameObject.name);
        }
        if (removedDialogue > 0)
            report.AppendLine("- " + removedDialogue + " komponen DialogueUI kosong (duplikat) dihapus.");

        EnsureCloseButton(dialogueUI, report);

        // 3. Prompt
        InteractionDetector detector = Object.FindFirstObjectByType<InteractionDetector>();
        InteractionPromptUI promptUI = Object.FindFirstObjectByType<InteractionPromptUI>();

        if (promptUI != null && IsConfigured(promptUI, "detector", "promptRoot", "promptText"))
        {
            report.AppendLine("- Prompt sudah lengkap.");
        }
        else
        {
            bool created;
            GameObject promptRoot = GetOrCreatePanel(canvas.transform, "PromptRoot", out created);
            if (created)
            {
                RectTransform r = promptRoot.GetComponent<RectTransform>();
                r.anchorMin = new Vector2(0.5f, 0f);
                r.anchorMax = new Vector2(0.5f, 0f);
                r.pivot = new Vector2(0.5f, 0f);
                r.anchoredPosition = new Vector2(0f, 240f);
                r.sizeDelta = new Vector2(520f, 60f);
            }

            Text promptText = GetOrCreateText(promptRoot.transform, "PromptText", "[E] Interaksi", 30,
                Color.white, TextAnchor.MiddleCenter, out created);
            if (created)
            {
                RectTransform r = promptText.rectTransform;
                r.anchorMin = Vector2.zero;
                r.anchorMax = Vector2.one;
                r.offsetMin = Vector2.zero;
                r.offsetMax = Vector2.zero;
            }

            if (promptUI == null) promptUI = Undo.AddComponent<InteractionPromptUI>(canvas.gameObject);
            SetRef(promptUI, "detector", detector);
            SetRef(promptUI, "promptRoot", promptRoot);
            SetRef(promptUI, "promptText", promptText);
            report.AppendLine("- Prompt dibuat/diperbaiki.");
        }
        if (detector == null)
            report.AppendLine("- PERINGATAN: InteractionDetector tidak ditemukan. Isi field Detector pada InteractionPromptUI secara manual.");

        // 4. TARS
        TarsWanderer tars = Object.FindFirstObjectByType<TarsWanderer>();
        if (tars != null)
        {
            SetRef(tars, "dialogueUI", dialogueUI);
            report.AppendLine("- TARS: Dialogue UI tersambung.");
        }
        else
        {
            report.AppendLine("- PERINGATAN: TarsWanderer tidak ditemukan di scene ini.");
        }

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        report.AppendLine();
        report.Append("Tekan Ctrl+S untuk menyimpan scene.");
        Debug.Log("MarsUISetup:\n" + report);
        EditorUtility.DisplayDialog("Mars Setup", report.ToString(), "OK");
    }

    // Membuat tombol Tutup di kotak dialog bila belum ada, lalu menyambungkannya ke DialogueUI.closeButton.
    // Tidak menimpa tombol yang sudah Anda isi.
    private static void EnsureCloseButton(DialogueUI dialogueUI, StringBuilder report)
    {
        if (dialogueUI == null) return;

        SerializedObject so = new SerializedObject(dialogueUI);
        SerializedProperty buttonProp = so.FindProperty("closeButton");
        SerializedProperty rootProp = so.FindProperty("dialogueRoot");
        if (buttonProp == null || rootProp == null || rootProp.objectReferenceValue == null) return;

        if (buttonProp.objectReferenceValue != null)
        {
            report.AppendLine("- Tombol Tutup dialog sudah ada.");
            return;
        }

        GameObject root = (GameObject)rootProp.objectReferenceValue;
        Transform existing = root.transform.Find("CloseButton");
        GameObject buttonGO;

        if (existing != null)
        {
            buttonGO = existing.gameObject;
        }
        else
        {
            buttonGO = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            Undo.RegisterCreatedObjectUndo(buttonGO, "Create CloseButton");
            buttonGO.transform.SetParent(root.transform, false);

            RectTransform r = buttonGO.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(1f, 1f);
            r.anchorMax = new Vector2(1f, 1f);
            r.pivot = new Vector2(1f, 1f);
            r.anchoredPosition = new Vector2(-12f, -12f);
            r.sizeDelta = new Vector2(56f, 44f);

            Image image = buttonGO.GetComponent<Image>();
            image.color = new Color(0.55f, 0.15f, 0.15f, 0.95f);
            image.raycastTarget = true;

            bool created;
            Text label = GetOrCreateText(buttonGO.transform, "Label", "X", 30, Color.white, TextAnchor.MiddleCenter, out created);
            if (created)
            {
                RectTransform lr = label.rectTransform;
                lr.anchorMin = Vector2.zero;
                lr.anchorMax = Vector2.one;
                lr.offsetMin = Vector2.zero;
                lr.offsetMax = Vector2.zero;
                label.fontStyle = FontStyle.Bold;
            }
        }

        Button button = buttonGO.GetComponent<Button>();
        if (button == null) button = Undo.AddComponent<Button>(buttonGO);
        if (button.targetGraphic == null) button.targetGraphic = buttonGO.GetComponent<Image>();

        SetRef(dialogueUI, "closeButton", button);
        report.AppendLine("- Tombol Tutup dialog dibuat di pojok kanan atas kotak dialog.");
    }

    internal static bool IsConfigured(Object target, params string[] fieldNames)
    {
        SerializedObject so = new SerializedObject(target);
        foreach (string fieldName in fieldNames)
        {
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop == null || prop.objectReferenceValue == null) return false;
        }
        return true;
    }

    internal static GameObject GetOrCreatePanel(Transform parent, string name, out bool created)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            created = false;
            return existing.gameObject;
        }

        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.8f);
        created = true;
        return go;
    }

    internal static Text GetOrCreateText(Transform parent, string name, string content, int size,
        Color color, TextAnchor alignment, out bool created)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            Text existingText = existing.GetComponent<Text>();
            if (existingText != null)
            {
                created = false;
                return existingText;
            }
        }

        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);

        Text text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = content;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        created = true;
        return text;
    }

    internal static void SetRef(Object target, string fieldName, Object value)
    {
        if (target == null) return;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty prop = so.FindProperty(fieldName);
        if (prop == null)
        {
            Debug.LogError("MarsUISetup: field '" + fieldName + "' tidak ditemukan di " + target.GetType().Name);
            return;
        }
        prop.objectReferenceValue = value;
        so.ApplyModifiedProperties();
    }
}
#endif
