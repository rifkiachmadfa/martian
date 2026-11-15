using System;
using UnityEngine;

/// <summary>
/// Satu Solar Panel di Outdoor. Menghasilkan Energy ke ResourceManager secara otomatis.
/// Panel rusak menghasilkan sebagian kecil saja (bukan nol, dan tidak mematikan fasilitas lain).
/// Beberapa panel di scene saling menjumlah. Produksi ikut jeda TimeManager.IsPaused (transisi tidur).
/// Belum ada pemicu kerusakan otomatis (belum diputuskan): pakai Damage() dari kode lain nanti atau menu
/// konteks Debug di Inspector. Perbaikan lewat RepairPoint. Informasi ke pemain lewat TARS menunggu TARSSystem.
/// Angka produksi adalah USULAN.
/// </summary>
public class SolarPanel : MonoBehaviour
{
    [Header("Produksi (USULAN, ubah lewat Inspector setelah playtest)")]
    [SerializeField, Min(0f)] private float normalEnergyPerSecond = 0.5f;
    [Tooltip("Pengali produksi saat rusak (0 = mati total, 1 = tidak berpengaruh).")]
    [SerializeField, Range(0f, 1f)] private float damagedOutputMultiplier = 0.25f;
    [Tooltip("Energy ditambahkan ke stok setiap sekian detik (agar event HUD tidak terpicu tiap frame).")]
    [SerializeField, Min(0.1f)] private float tickSeconds = 1f;
    [SerializeField] private bool startDamaged;

    [Header("Tampilan opsional")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite damagedSprite;
    [Tooltip("Dikalikan ke warna asli SpriteRenderer saat rusak, hanya bila Damaged Sprite kosong.")]
    [SerializeField] private Color damagedTint = new Color(1f, 0.45f, 0.35f);

    private Color baseColor = Color.white;
    private float tickTimer;

    /// <summary>True bila panel sedang rusak.</summary>
    public bool IsDamaged { get; private set; }

    /// <summary>Energy per detik yang dihasilkan panel ini sekarang.</summary>
    public float CurrentEnergyPerSecond => normalEnergyPerSecond * (IsDamaged ? damagedOutputMultiplier : 1f);

    /// <summary>Dipanggil saat status berubah (parameter: true bila rusak).</summary>
    public event Action<bool> OnDamagedChanged;

    private void Awake()
    {
        if (spriteRenderer != null) baseColor = spriteRenderer.color;
        IsDamaged = startDamaged;
        ApplyVisual();
    }

    private void Update()
    {
        TimeManager time = TimeManager.Instance;
        if (time != null && time.IsPaused) return;

        tickTimer += Time.deltaTime;
        if (tickTimer < tickSeconds) return;

        float amount = CurrentEnergyPerSecond * tickTimer;
        tickTimer = 0f;

        ResourceManager resources = ResourceManager.Instance;
        if (resources != null && amount > 0f) resources.Add(ResourceType.Energy, amount);
    }

    /// <summary>Merusak panel. Tidak melakukan apa pun bila sudah rusak.</summary>
    public void Damage() => SetDamaged(true);

    /// <summary>Memperbaiki panel. Tidak melakukan apa pun bila sudah normal.</summary>
    public void Repair() => SetDamaged(false);

    private void SetDamaged(bool damaged)
    {
        if (IsDamaged == damaged) return;

        IsDamaged = damaged;
        ApplyVisual();
        OnDamagedChanged?.Invoke(damaged);
    }

    private void ApplyVisual()
    {
        if (spriteRenderer == null) return;

        Sprite sprite = IsDamaged ? damagedSprite : normalSprite;
        if (sprite != null)
        {
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = baseColor;
            return;
        }

        // Tanpa sprite khusus: warna menjadi penanda.
        spriteRenderer.color = IsDamaged ? baseColor * damagedTint : baseColor;
    }

    [ContextMenu("Debug: Rusakkan panel")]
    private void DebugDamage() => Damage();

    [ContextMenu("Debug: Perbaiki panel")]
    private void DebugRepair() => Repair();
}
