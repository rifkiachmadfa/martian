using UnityEngine;

/// <summary>
/// ALAT UJI SEMENTARA. Menampilkan semua stok di pojok layar dan tombol +/- untuk mengetes ResourceManager
/// sebelum HUD sungguhan ada. Boleh dihapus (script dan komponennya) saat HUD selesai.
/// </summary>
public class ResourceDebugPanel : MonoBehaviour
{
    [SerializeField] private float step = 10f;

    private void OnGUI()
    {
        ResourceManager manager = ResourceManager.Instance;
        if (manager == null)
        {
            GUI.Label(new Rect(10, 10, 400, 25), "ResourceDebugPanel: ResourceManager belum ada di scene.");
            return;
        }

        GUILayout.BeginArea(new Rect(10, 10, 320, 400));

        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{type}: {manager.Get(type):0.#} / {manager.GetMax(type):0.#}", GUILayout.Width(190));

            if (GUILayout.Button($"+{step}")) manager.Add(type, step);

            if (GUILayout.Button($"-{step}") && !manager.TryConsume(type, step))
            {
                Debug.Log($"ResourceDebugPanel: {type} tidak cukup untuk dikurangi {step}.");
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndArea();
    }
}
