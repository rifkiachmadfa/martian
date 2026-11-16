using System;
using UnityEngine;

/// <summary>
/// Data satu langkah tutorial: pengenal, judul pendek, dan baris arahan TARS (teks tetap, bukan AI generatif).
/// Hanya data; logika ada di TutorialManager. Arahan ini BUKAN quest harian: tidak ada hadiah, tidak ada
/// penguncian urutan, dan pemain tetap bebas bergerak (GDD 5.2, 12).
/// </summary>
[Serializable]
public class TutorialStepData
{
    [SerializeField] private TutorialStepId id;
    [SerializeField] private string title;
    [Tooltip("Baris arahan TARS saat langkah ini dimulai. Isi bawaan hanyalah PLACEHOLDER; tulis teks Anda sendiri.")]
    [SerializeField, TextArea(2, 4)] private string[] tarsLines;

    public TutorialStepId Id => id;
    public string Title => title;
    public string[] TarsLines => tarsLines;

    public TutorialStepData() { }

    public TutorialStepData(TutorialStepId id, string title, params string[] tarsLines)
    {
        this.id = id;
        this.title = title;
        this.tarsLines = tarsLines;
    }
}
