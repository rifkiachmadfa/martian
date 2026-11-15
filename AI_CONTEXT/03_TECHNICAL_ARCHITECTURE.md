# Technical Architecture

## Prinsip
- Audit repository sebelum mengubah arsitektur.
- Ikuti pola yang sudah ada jika sehat; jangan membuat manager/script baru hanya demi nama.
- Pisahkan tanggung jawab, hindari ketergantungan silang yang tidak perlu.
- Gunakan solusi sederhana, efisien, dan sesuai Unity 2D serta target perangkat rendah.

## Area tanggung jawab (panduan, bukan kewajiban satu kelas per sistem)
- Game/day flow: state permainan, hari, tidur, ending.
- Resource: stok, konsumsi, produksi, validasi perubahan.
- Player: movement, animasi, collision, interaksi.
- Suit: status suit dan Suit Oxygen outdoor.
- Farming: plot, penyiraman, tahap Potato, panen.
- Machines: purifier dan oxygen generator.
- Solar: produksi Energy dan kondisi panel.
- TARS: pemicu pesan/pengingat.
- Save: progres yang perlu dipertahankan.

## Audit teknis wajib
Periksa `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, struktur `Assets/`, scene, prefab, script, `.meta`, Input System, physics, dan Animator sebelum memberi instruksi yang bergantung padanya.

## Implementasi saat ini (2026-11-15, commit `e6feb586`)
- **Versi:** Unity 6000.6.8f1, URP 17.6.0, Input System 1.20.1 (Active Input Handling = Both), Cinemachine 6.6.0, uGUI 2.6.0. [Repo]
- **Input:** kode memakai API lama (`Input.GetAxisRaw`, `Input.GetKeyDown`). Aman karena mode Both.
- **Interaksi:** pola `IInteractable` + `InteractionDetector` (Player) + `InteractionPromptUI`. Objek baru (bed, mesin, plot) cukup mengimplementasikan `IInteractable`; detector tidak perlu diubah. Interaksi tahan (misal Ice) belum didukung.
- **Dialog:** `DialogueUI` hanya tampilan. Pemanggil mengirim nama pembicara, baris teks, dan callback selesai. TARS memakai teks tetap dari Inspector (sesuai GDD: predefined, tanpa AI generatif).
- **TARS:** `TarsWanderer` memakai Rigidbody2D Dynamic agar bertabrakan dengan dinding; deteksi macet memilih target baru.
- **UI:** UnityEngine.UI.Text (legacy), bukan TextMeshPro.
- **Belum ada:** GameManager, TimeManager, ResourceManager, SaveSystem, TARSSystem, dan sistem lain di GDD. Tidak ada namespace; script berada di global namespace.
- **Rencana integrasi:** pause timer saat dialog akan memakai event dari `DialogueUI` setelah TimeManager ada.
