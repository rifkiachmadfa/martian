#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Menu Editor: menyiapkan animasi jalan TARS dari sprite di Assets/Robot.
/// - Mengatur import 12 PNG TARS (Sprite, Point filter, tanpa kompresi agar pixel art tajam).
/// - Mengisi downFrames / upFrames / leftFrames / rightFrames pada TarsWanderer
///   dengan urutan siklus jalan 1-2-3-2 (frame pertama = pose diam).
/// Aman dijalankan berulang kali; array yang sudah Anda isi tidak ditimpa.
/// Mars > Setup TARS Animation
/// </summary>
public static class MarsTarsAnimationSetup
{
    private const string Folder = "Assets/Robot/";

    // Urutan siklus jalan: frame 1 (diam) - 2 - 3 - 2.
    private static readonly string[] DownFiles  = { "robot design front1(new).png", "robot design front2.png", "robot design front3.png", "robot design front2.png" };
    private static readonly string[] UpFiles    = { "robot design behind 1.png", "robot design behind 2.png", "robot design behind 3.png", "robot design behind 2.png" };
    private static readonly string[] LeftFiles  = { "robot side kiri 1.png", "robot side kiri 2.png", "robot side kiri 3.png", "robot side kiri 2.png" };
    private static readonly string[] RightFiles = { "robot side kanan 1.png", "robot side kanan 2.png", "robot side kanan 3.png", "robot side kanan 2.png" };

    [MenuItem("Mars/Setup TARS Animation")]
    public static void Setup()
    {
        StringBuilder report = new StringBuilder();

        TarsWanderer tars = Object.FindFirstObjectByType<TarsWanderer>();
        if (tars == null)
        {
            EditorUtility.DisplayDialog("Mars Setup", "TarsWanderer tidak ditemukan di scene yang sedang dibuka.", "OK");
            return;
        }

        // 1. Import settings sprite
        int changed = 0;
        foreach (string file in AllFiles())
        {
            if (ConfigureImporter(Folder + file)) changed++;
        }
        AssetDatabase.SaveAssets();
        report.AppendLine("- Import sprite diperbarui: " + changed + " file.");

        // 2. Isi array frame pada TarsWanderer
        SerializedObject so = new SerializedObject(tars);
        FillFrames(so, "downFrames", DownFiles, report);
        FillFrames(so, "upFrames", UpFiles, report);
        FillFrames(so, "leftFrames", LeftFiles, report);
        FillFrames(so, "rightFrames", RightFiles, report);

        SerializedProperty rendererProp = so.FindProperty("spriteRenderer");
        if (rendererProp != null && rendererProp.objectReferenceValue == null)
        {
            SpriteRenderer sr = tars.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                rendererProp.objectReferenceValue = sr;
                report.AppendLine("- spriteRenderer diisi otomatis.");
            }
            else
            {
                report.AppendLine("- PERINGATAN: TARS tidak punya SpriteRenderer.");
            }
        }

        Undo.RecordObject(tars, "Setup TARS Animation");
        so.ApplyModifiedProperties();

        // 3. Tampilkan pose diam (menghadap bawah) di Scene View
        SpriteRenderer preview = tars.GetComponentInChildren<SpriteRenderer>();
        Sprite idle = AssetDatabase.LoadAssetAtPath<Sprite>(Folder + DownFiles[0]);
        if (preview != null && idle != null)
        {
            Undo.RecordObject(preview, "Setup TARS Animation");
            preview.sprite = idle;
            EditorUtility.SetDirty(preview);
        }

        EditorUtility.SetDirty(tars);
        EditorSceneManager.MarkSceneDirty(tars.gameObject.scene);

        report.AppendLine();
        report.AppendLine("Simpan scene (Ctrl+S) agar perubahan tersimpan.");
        Debug.Log("[MarsTarsAnimationSetup]\n" + report);
        EditorUtility.DisplayDialog("Mars Setup - TARS Animation", report.ToString(), "OK");
    }

    private static IEnumerable<string> AllFiles()
    {
        HashSet<string> set = new HashSet<string>();
        foreach (string f in DownFiles) set.Add(f);
        foreach (string f in UpFiles) set.Add(f);
        foreach (string f in LeftFiles) set.Add(f);
        foreach (string f in RightFiles) set.Add(f);
        return set;
    }

    private static bool ConfigureImporter(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning("MarsTarsAnimationSetup: file tidak ditemukan: " + path);
            return false;
        }

        bool dirty = false;

        if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; dirty = true; }
        if (importer.spriteImportMode != SpriteImportMode.Single) { importer.spriteImportMode = SpriteImportMode.Single; dirty = true; }
        if (importer.spritePixelsPerUnit != 100f) { importer.spritePixelsPerUnit = 100f; dirty = true; }
        if (importer.filterMode != FilterMode.Point) { importer.filterMode = FilterMode.Point; dirty = true; }
        if (importer.textureCompression != TextureImporterCompression.Uncompressed) { importer.textureCompression = TextureImporterCompression.Uncompressed; dirty = true; }
        if (importer.mipmapEnabled) { importer.mipmapEnabled = false; dirty = true; }
        if (!importer.alphaIsTransparency) { importer.alphaIsTransparency = true; dirty = true; }

        if (dirty) importer.SaveAndReimport();
        return dirty;
    }

    private static void FillFrames(SerializedObject so, string propertyName, string[] files, StringBuilder report)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop == null)
        {
            report.AppendLine("- PERINGATAN: field " + propertyName + " tidak ditemukan di TarsWanderer.");
            return;
        }

        if (prop.arraySize > 0)
        {
            report.AppendLine("- " + propertyName + " sudah terisi, tidak ditimpa.");
            return;
        }

        prop.arraySize = files.Length;
        for (int i = 0; i < files.Length; i++)
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(Folder + files[i]);
            if (s == null) report.AppendLine("- PERINGATAN: sprite tidak dimuat: " + files[i]);
            prop.GetArrayElementAtIndex(i).objectReferenceValue = s;
        }
        report.AppendLine("- " + propertyName + " diisi " + files.Length + " frame.");
    }
}
#endif
