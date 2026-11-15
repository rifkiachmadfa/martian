# Scenes & Assets

## Area desain yang direncanakan
- Control Room
- Farming Area
- Outdoor

Nama scene, hierarchy, prefab, sprite, tilemap, Animator Controller, sorting layer, dan asset yang benar-benar ada **belum ditetapkan di dokumen ini**. Audit repository sebelum menyebut nama atau mengubah referensi.

## Saat menambah/mengubah asset
- Pertahankan pasangan `.meta` dan GUID.
- Jangan menghapus atau mengganti asset yang mungkin direferensikan scene/prefab tanpa pemeriksaan.
- Berikan langkah Inspector secara eksplisit: GameObject, komponen, field, nilai, dan referensi yang perlu diisi.
- Untuk perubahan binary/serialized yang tidak aman dibuat lewat patch teks, jelaskan langkah manual.
