using UnityEngine;

/// <summary>
/// Opsional, dipasang bersama IInteractable pada objek yang bisa MENERIMA item dari tas pemain
/// (misalnya Farming Plot menerima bibit, Water Purifier menerima es).
/// InteractionDetector memakainya saat pemain memilih slot tas (tombol 1 sampai 3) lalu menekan E di dekat objek.
/// </summary>
public interface IItemReceiver
{
    /// <summary>True bila objek ini bisa memakai item tersebut saat ini (dipakai untuk prompt dan tombol E).</summary>
    bool CanUseItem(ItemType item);

    /// <summary>Teks prompt, misalnya "Tanam bibit kentang".</summary>
    string GetItemPrompt(ItemType item);

    /// <summary>Dipanggil saat pemain menekan E dengan item terpilih di dekat objek ini.</summary>
    void UseItem(GameObject user, ItemType item);
}
