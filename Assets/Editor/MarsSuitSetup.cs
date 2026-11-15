#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menu Editor: membuat SuitSystem, Suit Station, IndoorZone, SuitGate (tembok pintu keluar), dan panel
/// oksigen suit di HUD, lalu menyambungkan referensinya.
/// Aman dijalankan berulang kali; objek yang sudah ada dan referensi yang sudah Anda isi tidak ditimpa.
/// Mars > Setup Suit and Gate
/// Objek baru diletakkan di sekitar Player sebagai placeholder: Anda yang memindahkannya ke posisi akhir.
/// </summary>
public static class MarsSuitSetup
{
    [MenuItem("Mars/Setup Suit and Gate")]
    public static void Setup()
    {
        StringBuilder report = new StringBuilder();

        PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
        if (player == null)
        {
            EditorUtility.DisplayDialog("Mars Setup", "PlayerMovement tidak ditemukan di scene.", "OK");
            return;
        }

        DialogueUI dialogueUI = Object.FindFirstObjectByType<DialogueUI>();
        if (dialogueUI == null) report.AppendLine("- PERINGATAN: DialogueUI tidak ditemukan. Jalankan Mars > Setup Dialogue and Prompt UI.");
        if (Object.FindFirstObjectByType<ResourceManager>() == null) report.AppendLine("- PERINGATAN: ResourceManager tidak ada di scene.");

        Vector3 origin = player.transform.position;
        int interactableLayer = LayerMask.NameToLayer("Interactable");

        // 1. SuitSystem
        SuitSystem suit = Object.FindFirstObjectByType<SuitSystem>();
        if (suit == null)
        {
            GameObject go = new GameObject("SuitSystem");
            Undo.RegisterCreatedObjectUndo(go, "Create SuitSystem");
            suit = Undo.AddComponent<SuitSystem>(go);
            report.AppendLine("- SuitSystem dibuat.");
        }
        else
        {
            report.AppendLine("- SuitSystem sudah ada.");
        }

        // 1b. SuitReminder (pengingat TARS "lepas suit"), dipasang pada objek SuitSystem
        if (Object.FindFirstObjectByType<SuitReminder>() == null)
        {
            Undo.AddComponent<SuitReminder>(suit.gameObject);
            report.AppendLine("- SuitReminder ditambahkan pada objek SuitSystem.");
        }
        else
        {
            report.AppendLine("- SuitReminder sudah ada.");
        }

        // 2. Suit Station
        SuitStation station = Object.FindFirstObjectByType<SuitStation>();
        if (station == null)
        {
            GameObject go = new GameObject("SuitStation", typeof(SpriteRenderer), typeof(BoxCollider2D));
            Undo.RegisterCreatedObjectUndo(go, "Create SuitStation");

            if (interactableLayer >= 0) go.layer = interactableLayer;
            else report.AppendLine("- PERINGATAN: layer 'Interactable' tidak ditemukan. Set layer SuitStation secara manual.");

            go.transform.position = origin + new Vector3(-2f, 0f, 0f);

            // Ukuran diisi eksplisit: collider yang dibuat sebelum sprite ada akan berukuran hampir nol.
            BoxCollider2D box = go.GetComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(1.5f, 1.5f);

            station = Undo.AddComponent<SuitStation>(go);
            report.AppendLine("- SuitStation dibuat di sebelah kiri Player. Pindahkan ke Control Room dan pasang sprite pada SpriteRenderer.");
        }
        else
        {
            report.AppendLine("- SuitStation sudah ada: " + station.gameObject.name);
        }
        SetRefIfEmpty(station, "suit", suit);
        SetRefIfEmpty(station, "dialogueUI", dialogueUI);

        // 3. IndoorZone
        if (Object.FindObjectsByType<IndoorZone>(FindObjectsSortMode.None).Length == 0)
        {
            GameObject go = new GameObject("IndoorZone", typeof(BoxCollider2D));
            Undo.RegisterCreatedObjectUndo(go, "Create IndoorZone");
            go.transform.position = origin;

            BoxCollider2D box = go.GetComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(14f, 10f);

            Undo.AddComponent<IndoorZone>(go);
            report.AppendLine("- IndoorZone dibuat di sekitar Player. WAJIB diubah: atur posisi dan Size agar menutupi seluruh interior habitat, dengan tepi sejajar pintu keluar.");
        }
        else
        {
            report.AppendLine("- IndoorZone sudah ada.");
        }

        // 4. SuitGate
        SuitGate gate = Object.FindFirstObjectByType<SuitGate>();
        if (gate == null)
        {
            GameObject go = new GameObject("SuitGate", typeof(BoxCollider2D));
            Undo.RegisterCreatedObjectUndo(go, "Create SuitGate");
            go.transform.position = origin + new Vector3(0f, 3f, 0f);

            BoxCollider2D box = go.GetComponent<BoxCollider2D>();
            box.isTrigger = false;
            box.size = new Vector2(3f, 0.5f);

            gate = Undo.AddComponent<SuitGate>(go);
            report.AppendLine("- SuitGate dibuat di atas Player. WAJIB dipindahkan ke pintu keluar dan di-resize agar menutup seluruh lebar pintu.");
        }
        else
        {
            report.AppendLine("- SuitGate sudah ada: " + gate.gameObject.name);
        }
        SetRefIfEmpty(gate, "wallCollider", gate.GetComponent<BoxCollider2D>());
        SetRefIfEmpty(gate, "suit", suit);
        SetRefIfEmpty(gate, "player", player);
        SetRefIfEmpty(gate, "dialogueUI", dialogueUI);

        // 5. Panel oksigen suit di HUD
        HudUI hud = Object.FindFirstObjectByType<HudUI>();
        if (hud == null)
        {
            report.AppendLine("- PERINGATAN: HudUI tidak ditemukan. Jalankan Mars > Setup HUD and Time, lalu jalankan menu ini lagi.");
        }
        else if (MarsUISetup.IsConfigured(hud, "suitPanel", "suitOxygenText"))
        {
            report.AppendLine("- Panel oksigen suit di HUD sudah ada.");
        }
        else
        {
            bool created;
            GameObject panel = MarsUISetup.GetOrCreatePanel(hud.transform, "SuitPanel", out created);
            if (created)
            {
                RectTransform r = panel.GetComponent<RectTransform>();
                r.anchorMin = Vector2.zero;
                r.anchorMax = Vector2.zero;
                r.pivot = Vector2.zero;
                r.anchoredPosition = new Vector2(20f, 20f);
                r.sizeDelta = new Vector2(340f, 60f);
            }

            Text suitText = MarsUISetup.GetOrCreateText(panel.transform, "SuitOxygenText", "Suit O2: 0 / 0", 30,
                Color.white, TextAnchor.MiddleCenter, out created);
            if (created)
            {
                RectTransform r = suitText.rectTransform;
                r.anchorMin = Vector2.zero;
                r.anchorMax = Vector2.one;
                r.offsetMin = new Vector2(10f, 5f);
                r.offsetMax = new Vector2(-10f, -5f);
            }

            MarsUISetup.SetRef(hud, "suitPanel", panel);
            MarsUISetup.SetRef(hud, "suitOxygenText", suitText);
            report.AppendLine("- Panel oksigen suit dibuat di kiri bawah HUD (tampil hanya saat suit dikenakan).");
        }

        EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
        report.AppendLine();
        report.Append("Tekan Ctrl+S untuk menyimpan scene.");
        Debug.Log("MarsSuitSetup:\n" + report);
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
