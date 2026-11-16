#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Menu Editor: membuat objek FloatingPopupManager (popup ikon + teks di dekat objek saat panen, ambil es, dst).
/// Aman dijalankan berulang kali; objek yang sudah ada dan sprite yang sudah Anda isi tidak ditimpa.
/// Mars > Setup Floating Popups
/// </summary>
public static class MarsPopupSetup
{
    [MenuItem("Mars/Setup Floating Popups")]
    public static void Setup()
    {
        StringBuilder report = new StringBuilder();

        FloatingPopupManager manager = Object.FindFirstObjectByType<FloatingPopupManager>();
        if (manager == null)
        {
            GameObject go = new GameObject("FloatingPopupManager");
            Undo.RegisterCreatedObjectUndo(go, "Create FloatingPopupManager");
            manager = Undo.AddComponent<FloatingPopupManager>(go);
            report.AppendLine("- FloatingPopupManager dibuat (ikon pixel bawaan dipakai).");
        }
        else
        {
            report.AppendLine("- FloatingPopupManager sudah ada; tidak diubah.");
        }

        EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
        report.AppendLine();
        report.AppendLine("Untuk ikon final: impor sprite (Texture Type Sprite, Filter Mode Point, Compression None), lalu seret ke slot ikon di Inspector FloatingPopupManager.");
        report.AppendLine("Untuk font pixel: seret file TTF ke slot Font.");
        report.Append("Tekan Ctrl+S untuk menyimpan scene.");
        Debug.Log("MarsPopupSetup:\n" + report);
        EditorUtility.DisplayDialog("Mars Setup", report.ToString(), "OK");
    }
}
#endif
