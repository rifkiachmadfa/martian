/// <summary>
/// Pengenal tujuh langkah tutorial Day 1 (GDD 5.2, PHASE.md 12.1). Urutan enum = urutan langkah bawaan.
/// ID bersifat tetap; jangan dipakai ulang bila langkah dihapus.
/// </summary>
public enum TutorialStepId
{
    Intro,          // 12.1.1 Mengenal Control Room, TARS, HUD, dan Storage
    Suit,           // 12.1.2 Mengambil dan mengisi Astronaut Suit
    CollectIce,     // 12.1.3 Keluar dan mengumpulkan Ice dalam jumlah kecil
    PurifyWater,    // 12.1.4 Membawa Ice ke Water Purifier dan menghasilkan Water
    Farming,        // 12.1.5 Menanam dan menyiram di Farming Plot
    OxygenGenerator,// 12.1.6 Mengenal Oxygen Generator dan hubungan Water/Energy
    Sleep           // 12.1.7 Kembali ke Control Room dan tidur
}
