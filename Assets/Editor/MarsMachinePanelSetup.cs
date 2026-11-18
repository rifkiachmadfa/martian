#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Menu Editor: memasang MachinePanelUI pada Canvas (panel Water Purifier, Oxygen Generator, Solar Panel) dan
/// menyambungkan WaterPurifier, OxygenGenerator, dan RepairPoint ke panel itu. Aman dijalankan berulang kali;
/// referensi yang sudah Anda isi tidak ditimpa. Tampilan panel dibuat lewat kode saat Play.
/// Mars > Setup Machine Panels
/// </summary>
public static class MarsMachinePanelSetup
{
    [MenuItem("Mars/Setup Machine Panels")]
    public static void Setup()
    {
        StringBuilder report = new StringBuilder();

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Mars Setup",
                "Canvas tidak ditemukan. Jalankan dulu menu Mars > Setup Dialogue and Prompt UI.", "OK");
            return;
        }

        MachinePanelUI panelUI = Object.FindFirstObjectByType<MachinePanelUI>();
        if (panelUI == null)
        {
            panelUI = Undo.AddComponent<MachinePanelUI>(canvas.gameObject);
            report.AppendLine("- MachinePanelUI dipasang di " + canvas.gameObject.name + ".");
        }
        else
        {
            report.AppendLine("- MachinePanelUI sudah ada di: " + panelUI.gameObject.name);
        }
        SetRefIfEmpty(panelUI, "player", player);

        WaterPurifier[] purifiers = Object.FindObjectsByType<WaterPurifier>(FindObjectsSortMode.None);
        foreach (WaterPurifier purifier in purifiers) SetRefIfEmpty(purifier, "panelUI", panelUI);
        report.AppendLine("- " + purifiers.Length + " WaterPurifier disambungkan.");

        OxygenGenerator[] generators = Object.FindObjectsByType<OxygenGenerator>(FindObjectsSortMode.None);
        foreach (OxygenGenerator generator in generators) SetRefIfEmpty(generator, "panelUI", panelUI);
        report.AppendLine("- " + generators.Length + " OxygenGenerator disambungkan.");

        RepairPoint[] repairPoints = Object.FindObjectsByType<RepairPoint>(FindObjectsSortMode.None);
        foreach (RepairPoint repair in repairPoints) SetRefIfEmpty(repair, "panelUI", panelUI);
        report.AppendLine("- " + repairPoints.Length + " RepairPoint (panel Solar Panel) disambungkan.");

        if (purifiers.Length == 0) report.AppendLine("- PERINGATAN: WaterPurifier tidak ada di scene.");
        if (generators.Length == 0) report.AppendLine("- PERINGATAN: OxygenGenerator tidak ada di scene.");
        if (repairPoints.Length == 0) report.AppendLine("- PERINGATAN: RepairPoint tidak ada di scene.");

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        report.AppendLine();
        report.Append("Tekan Ctrl+S untuk menyimpan scene.");
        Debug.Log("MarsMachinePanelSetup:\n" + report);
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
