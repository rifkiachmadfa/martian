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
