#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Menu Editor: membuat InventorySystem, memasang InventoryHotbarUI dan StorageUI pada Canvas, memastikan ada
/// objek storage (FoodStorage), dan menyembunyikan panel Ice lama di HUD (Ice kini ada di tas).
/// Aman dijalankan berulang kali; objek yang sudah ada dan referensi yang sudah Anda isi tidak ditimpa.
/// Mars > Setup Inventory and Storage
/// </summary>
public static class MarsInventorySetup
{
    [MenuItem("Mars/Setup Inventory and Storage")]
    public static void Setup()
    {
        StringBuilder report = new StringBuilder();

        PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (player == null || canvas == null)
        {
            EditorUtility.DisplayDialog("Mars Setup",
                "PlayerMovement atau Canvas tidak ditemukan. Jalankan dulu Mars > Setup Dialogue and Prompt UI.", "OK");
            return;
        }

        DialogueUI dialogueUI = Object.FindFirstObjectByType<DialogueUI>();

        // 1. InventorySystem
        InventorySystem inventory = Object.FindFirstObjectByType<InventorySystem>();
        if (inventory == null)
        {
            GameObject go = new GameObject("InventorySystem");
            Undo.RegisterCreatedObjectUndo(go, "Create InventorySystem");
            inventory = Undo.AddComponent<InventorySystem>(go);
            report.AppendLine("- InventorySystem dibuat.");
        }
        else
        {
            report.AppendLine("- InventorySystem sudah ada.");
        }
        SetRefIfEmpty(inventory, "dialogueUI", dialogueUI);
        SetRefIfEmpty(inventory, "player", player);

        // 2. Hotbar dan panel storage pada Canvas
        if (Object.FindFirstObjectByType<InventoryHotbarUI>() == null)
        {
            Undo.AddComponent<InventoryHotbarUI>(canvas.gameObject);
            report.AppendLine("- InventoryHotbarUI dipasang di " + canvas.gameObject.name + " (hotbar di bawah tengah layar).");
        }
        else
        {
            report.AppendLine("- InventoryHotbarUI sudah ada.");
        }

        StorageUI storageUI = Object.FindFirstObjectByType<StorageUI>();
        if (storageUI == null)
        {
            storageUI = Undo.AddComponent<StorageUI>(canvas.gameObject);
            report.AppendLine("- StorageUI dipasang di " + canvas.gameObject.name + ".");
        }
        else
        {
            report.AppendLine("- StorageUI sudah ada.");
        }
        SetRefIfEmpty(storageUI, "player", player);

        // 3. Objek storage (class FoodStorage)
        FoodStorage storage = Object.FindFirstObjectByType<FoodStorage>();
        if (storage == null)
        {
            int layer = LayerMask.NameToLayer("Interactable");
            GameObject go = new GameObject("Storage", typeof(SpriteRenderer), typeof(BoxCollider2D));
            Undo.RegisterCreatedObjectUndo(go, "Create Storage");

            if (layer >= 0) go.layer = layer;
            else report.AppendLine("- PERINGATAN: layer 'Interactable' tidak ditemukan. Set layer Storage secara manual.");

            go.transform.position = player.transform.position + new Vector3(2f, 0f, 0f);

            BoxCollider2D box = go.GetComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(1.5f, 1.5f);

            storage = Undo.AddComponent<FoodStorage>(go);
            report.AppendLine("- Storage dibuat di sebelah kanan Player. Pindahkan ke Control Room dan pasang sprite pada SpriteRenderer.");
        }
        else
        {
            report.AppendLine("- Storage sudah ada: " + storage.gameObject.name);
        }
        SetRefIfEmpty(storage, "storageUI", storageUI);
        SetRefIfEmpty(storage, "dialogueUI", dialogueUI);

        // 4. Panel Ice lama di HUD disembunyikan (Ice sekarang ada di tas)
        HudUI hud = Object.FindFirstObjectByType<HudUI>();
        if (hud != null)
        {
            Transform icePanel = hud.transform.Find("IcePanel");
            if (icePanel != null && icePanel.gameObject.activeSelf)
            {
                Undo.RecordObject(icePanel.gameObject, "Hide IcePanel");
                icePanel.gameObject.SetActive(false);
                report.AppendLine("- IcePanel lama di HUD dinonaktifkan (Ice kini tampil di hotbar tas).");
            }
        }

        report.AppendLine();
        report.AppendLine("WAJIB (manual): di Inspector ResourceManager, ubah Food (kentang di storage) menjadi Start Amount 10 dan Max Amount 50 (usulan). Nilai lama (Food 50/100) terlalu banyak untuk satuan kentang.");
        report.AppendLine("Tekan Ctrl+S untuk menyimpan scene.");

        EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
        Debug.Log("MarsInventorySetup:\n" + report);
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
