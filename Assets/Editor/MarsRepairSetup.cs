#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Menu Editor: memasang WiringMinigameUI pada Canvas dan membuat objek SolarPanelDamageScheduler,
/// lalu menyambungkan RepairPoint ke minigame. Aman dijalankan berulang kali; referensi yang sudah diisi tidak ditimpa.
/// Mars > Setup Panel Damage and Wiring Minigame
/// </summary>
public static class MarsRepairSetup
{
    [MenuItem("Mars/Setup Panel Damage and Wiring Minigame")]
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

        // 1. Minigame kabel (UI dibuat lewat kode saat Play, jadi cukup memasang komponen).
        WiringMinigameUI minigame = Object.FindFirstObjectByType<WiringMinigameUI>();
        if (minigame == null)
        {
            minigame = Undo.AddComponent<WiringMinigameUI>(canvas.gameObject);
            report.AppendLine("- WiringMinigameUI dipasang di " + canvas.gameObject.name + ".");
        }
        else
        {
            report.AppendLine("- WiringMinigameUI sudah ada di: " + minigame.gameObject.name);
        }

        // 2. Pemicu kerusakan (satu kejadian acak per hari).
        SolarPanelDamageScheduler scheduler = Object.FindFirstObjectByType<SolarPanelDamageScheduler>();
        if (scheduler == null)
        {
            GameObject go = new GameObject("SolarPanelDamageScheduler", typeof(SolarPanelDamageScheduler));
            Undo.RegisterCreatedObjectUndo(go, "Create SolarPanelDamageScheduler");
            scheduler = go.GetComponent<SolarPanelDamageScheduler>();
            report.AppendLine("- SolarPanelDamageScheduler dibuat.");
        }
        else
        {
            report.AppendLine("- SolarPanelDamageScheduler sudah ada: " + scheduler.gameObject.name);
        }
        SetRefIfEmpty(scheduler, "timeManager", Object.FindFirstObjectByType<TimeManager>());
        SetRefIfEmpty(scheduler, "dialogueUI", Object.FindFirstObjectByType<DialogueUI>());
        SetRefIfEmpty(scheduler, "minigame", minigame);

        // 3. RepairPoint -> minigame.
        RepairPoint[] repairPoints = Object.FindObjectsByType<RepairPoint>(FindObjectsSortMode.None);
        foreach (RepairPoint repair in repairPoints) SetRefIfEmpty(repair, "minigame", minigame);
        report.AppendLine("- " + repairPoints.Length + " RepairPoint disambungkan ke minigame.");

        if (repairPoints.Length == 0)
            report.AppendLine("- PERINGATAN: RepairPoint tidak ada. Jalankan menu Mars > Setup Interactive Objects.");
        if (Object.FindFirstObjectByType<SolarPanel>() == null)
            report.AppendLine("- PERINGATAN: SolarPanel tidak ada di scene; tidak ada yang bisa dirusak.");
        if (Object.FindFirstObjectByType<TimeManager>() == null)
            report.AppendLine("- PERINGATAN: TimeManager tidak ada. Jalankan menu Mars > Setup HUD and Time.");

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        report.AppendLine();
        report.Append("Tekan Ctrl+S untuk menyimpan scene.");
        Debug.Log("MarsRepairSetup:\n" + report);
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
