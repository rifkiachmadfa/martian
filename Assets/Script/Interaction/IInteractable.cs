using UnityEngine;

/// <summary>
/// Kontrak untuk semua objek yang bisa diinteraksi pemain (TARS, bed, mesin, dll).
/// </summary>
public interface IInteractable
{
    /// <summary>Teks yang ditampilkan di prompt, misalnya "Bicara dengan TARS".</summary>
    string InteractionPrompt { get; }

    /// <summary>Apakah objek bisa diinteraksi saat ini.</summary>
    bool CanInteract(GameObject interactor);

    /// <summary>Dipanggil saat pemain menekan tombol interaksi.</summary>
    void Interact(GameObject interactor);
}
