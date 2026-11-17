#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Menu Editor: memasang sprite dari Assets/object ke tanaman dan ikon popup.
/// - FarmingPlot: growingSprite = potato1 (belum siap panen), matureSprite = potato2 (siap panen).
/// - FloatingPopupManager: potatoIcon = potato (3) (kentang/food), seedIcon = bibit.
/// Import sprite hanya diubah ke Point filter dan tanpa kompresi (pixel art tajam); mode sprite dan rect tidak diubah.
/// Aman dijalankan berulang kali; slot yang sudah Anda isi tidak ditimpa.
/// Mars > Setup Farming Sprites
/// </summary>
public static class MarsFarmingSpriteSetup
{
    private const string Folder = "Assets/object/";
    private const string PotatoIconFile = "potato (3).png";
    private const string GrowingFile = "potato1.png";
    private const string MatureFile = "potato2.png";
    private const string SeedFile = "bibit.png";

    [MenuItem("Mars/Setup Farming Sprites")]
    public static void Setup()
    {
        StringBuilder report = new StringBuilder();

        string[] files = { PotatoIconFile, GrowingFile, MatureFile, SeedFile };
        int changed = 0;
        foreach (string file in files)
        {
            if (ConfigureImporter(Folder + file)) changed++;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        report.AppendLine("- Import sprite diperbarui: " + changed + " file.");

        Sprite potatoIcon = LoadSprite(PotatoIconFile, report);
        Sprite growing = LoadSprite(GrowingFile, report);
        Sprite mature = LoadSprite(MatureFile, report);
        Sprite seed = LoadSprite(SeedFile, report);

        bool sceneChanged = false;

        FarmingPlot[] plots = Object.FindObjectsByType<FarmingPlot>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (plots.Length == 0) report.AppendLine("- PERINGATAN: FarmingPlot tidak ditemukan di scene yang sedang dibuka.");
        foreach (FarmingPlot plot in plots)
        {
            SerializedObject so = new SerializedObject(plot);
            bool a = FillIfEmpty(so, "growingSprite", growing);
            bool b = FillIfEmpty(so, "matureSprite", mature);
            if (a || b)
            {
                Undo.RecordObject(plot, "Setup Farming Sprites");
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(plot);
                sceneChanged = true;
            }
            report.AppendLine("- " + plot.gameObject.name + ": " + ((a || b) ? "sprite tanaman diisi." : "sprite tanaman sudah terisi, tidak ditimpa."));
        }

        FloatingPopupManager popup = Object.FindFirstObjectByType<FloatingPopupManager>();
        if (popup == null)
        {
            report.AppendLine("- PERINGATAN: FloatingPopupManager tidak ditemukan. Jalankan menu Mars > Setup Floating Popups.");
        }
        else
        {
            SerializedObject so = new SerializedObject(popup);
            bool a = FillIfEmpty(so, "potatoIcon", potatoIcon);
            bool b = FillIfEmpty(so, "seedIcon", seed);
            if (a || b)
            {
                Undo.RecordObject(popup, "Setup Farming Sprites");
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(popup);
                sceneChanged = true;
            }
            report.AppendLine("- FloatingPopupManager: " + ((a || b) ? "ikon kentang/bibit diisi." : "ikon sudah terisi, tidak ditimpa."));
        }

        if (sceneChanged) EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        report.AppendLine();
        report.AppendLine("Simpan scene (Ctrl+S) agar perubahan tersimpan.");
        Debug.Log("[MarsFarmingSpriteSetup]\n" + report);
        EditorUtility.DisplayDialog("Mars Setup - Farming Sprites", report.ToString(), "OK");
    }

    private static Sprite LoadSprite(string file, StringBuilder report)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Folder + file);
        if (sprite == null) report.AppendLine("- PERINGATAN: sprite tidak dimuat: " + file);
        return sprite;
    }

    private static bool FillIfEmpty(SerializedObject so, string propertyName, Sprite sprite)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop == null || sprite == null || prop.objectReferenceValue != null) return false;
        prop.objectReferenceValue = sprite;
        return true;
    }

    private static bool ConfigureImporter(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning("MarsFarmingSpriteSetup: file tidak ditemukan: " + path);
            return false;
        }

        bool dirty = false;
        if (importer.filterMode != FilterMode.Point) { importer.filterMode = FilterMode.Point; dirty = true; }
        if (importer.textureCompression != TextureImporterCompression.Uncompressed) { importer.textureCompression = TextureImporterCompression.Uncompressed; dirty = true; }
        if (importer.mipmapEnabled) { importer.mipmapEnabled = false; dirty = true; }

        if (dirty) importer.SaveAndReimport();
        return dirty;
    }
}
#endif
