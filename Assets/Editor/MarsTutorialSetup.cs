#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Menu Editor: membuat objek TutorialManager (Milestone 12) di scene.
/// Aman dijalankan berulang kali; objek yang sudah ada tidak ditimpa.
/// Mars > Setup Tutorial
/// Juga memasang TutorialGuideUI (garis arah tutorial) pada Canvas.
/// </summary>
public static class MarsTutorialSetup
{
    [MenuItem("Mars/Setup Tutorial")]
    public static void Setup()
    {
        StringBuilder report = new StringBuilder();

        if (Object.FindFirstObjectByType<TimeManager>() == null)
            report.AppendLine("- PERINGATAN: TimeManager tidak ada. Jalankan Mars > Setup HUD and Time.");
        if (Object.FindFirstObjectByType<TARSSystem>() == null)
            report.AppendLine("- PERINGATAN: TARSSystem tidak ada; arahan tutorial hanya masuk Console. Jalankan Mars > Setup TARS System.");

        TutorialManager manager = Object.FindFirstObjectByType<TutorialManager>();
        if (manager == null)
        {
            GameObject go = new GameObject("TutorialManager");
            Undo.RegisterCreatedObjectUndo(go, "Create TutorialManager");
            manager = Undo.AddComponent<TutorialManager>(go);
            report.AppendLine("- TutorialManager dibuat.");
        }
        else
        {
            report.AppendLine("- TutorialManager sudah ada: " + manager.gameObject.name);
        }

        // Garis arah tutorial: komponen di Canvas (UI dibuat lewat kode saat Play).
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            report.AppendLine("- PERINGATAN: Canvas tidak ditemukan; garis arah tidak dipasang. Jalankan Mars > Setup Dialogue and Prompt UI lalu menu ini lagi.");
        }
        else if (Object.FindFirstObjectByType<TutorialGuideUI>() == null)
        {
            Undo.AddComponent<TutorialGuideUI>(canvas.gameObject);
            report.AppendLine("- TutorialGuideUI dipasang di " + canvas.gameObject.name + ".");
        }
        else
        {
            report.AppendLine("- TutorialGuideUI sudah ada.");
        }

        EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
        report.AppendLine();
        report.Append("Tekan Ctrl+S untuk menyimpan scene. Ini baru kerangka: langkah belum maju otomatis (syarat belum diisi). " +
                      "Teks arahan di Inspector TutorialManager masih PLACEHOLDER.");
        Debug.Log("MarsTutorialSetup:\n" + report);
        EditorUtility.DisplayDialog("Mars Setup", report.ToString(), "OK");
    }
}
#endif
