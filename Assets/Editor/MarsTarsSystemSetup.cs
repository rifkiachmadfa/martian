#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Menu Editor: membuat objek TARSSystem (Milestone 11) dan menyambungkan referensinya ke DialogueUI dan TARS (TarsWanderer).
/// Aman dijalankan berulang kali; objek yang sudah ada dan referensi yang sudah Anda isi tidak ditimpa.
/// Mars > Setup TARS System
/// </summary>
public static class MarsTarsSystemSetup
{
    [MenuItem("Mars/Setup TARS System")]
    public static void Setup()
    {
        StringBuilder report = new StringBuilder();

        DialogueUI dialogueUI = Object.FindFirstObjectByType<DialogueUI>();
        if (dialogueUI == null) report.AppendLine("- PERINGATAN: DialogueUI tidak ditemukan. Jalankan Mars > Setup Dialogue and Prompt UI.");
        if (Object.FindFirstObjectByType<ResourceManager>() == null) report.AppendLine("- PERINGATAN: ResourceManager tidak ada di scene.");
        if (Object.FindFirstObjectByType<TimeManager>() == null) report.AppendLine("- PERINGATAN: TimeManager tidak ada. Jalankan Mars > Setup HUD and Time.");

        // 1. TARSSystem
        TARSSystem tars = Object.FindFirstObjectByType<TARSSystem>();
        if (tars == null)
        {
            GameObject go = new GameObject("TARSSystem");
            Undo.RegisterCreatedObjectUndo(go, "Create TARSSystem");
            tars = Undo.AddComponent<TARSSystem>(go);
            report.AppendLine("- TARSSystem dibuat.");
        }
        else
        {
            report.AppendLine("- TARSSystem sudah ada: " + tars.gameObject.name);
        }
        SetRefIfEmpty(tars, "dialogueUI", dialogueUI);

        // 2. TARS (NPC) memakai TARSSystem untuk laporan saat diajak bicara.
        TarsWanderer[] wanderers = Object.FindObjectsByType<TarsWanderer>(FindObjectsSortMode.None);
        foreach (TarsWanderer wanderer in wanderers) SetRefIfEmpty(wanderer, "tarsSystem", tars);
        report.AppendLine("- " + wanderers.Length + " TarsWanderer disambungkan ke TARSSystem.");
        if (wanderers.Length == 0)
            report.AppendLine("- PERINGATAN: objek TARS (TarsWanderer) tidak ditemukan; laporan lewat dialog E tidak aktif.");

        // 3. Komponen pengingat lama yang sudah dipindahkan.
        if (Object.FindFirstObjectByType<SuitReminder>() != null)
            report.AppendLine("- Info: SuitReminder masih ada; ia menonaktifkan diri otomatis saat TARSSystem ada. Boleh dihapus komponennya dari objek SuitSystem.");

        EditorSceneManager.MarkSceneDirty(tars.gameObject.scene);
        report.AppendLine();
        report.Append("Tekan Ctrl+S untuk menyimpan scene. Isi 'Earth Messages' di Inspector TARSSystem dengan teks cerita Anda (bawaannya placeholder).");
        Debug.Log("MarsTarsSystemSetup:\n" + report);
        EditorUtility.DisplayDialog("Mars Setup", report.ToString(), "OK");
    }

    private static void SetRefIfEmpty(Object target, string fieldName, Object value)
    {
        if (target == null || value == null) return;

        SerializedObject so = new SerializedObject(target);
        SerializedProperty prop = so.FindProperty(fieldName);
        if (prop != null && prop.objectReferenceValue == null) MarsUISetup.SetRef(target, fieldName, value);
    }
}
#endif
