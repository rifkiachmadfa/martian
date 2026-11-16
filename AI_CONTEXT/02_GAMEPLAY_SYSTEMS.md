# Gameplay Systems

| Sistem | Aturan inti |
|---|---|
| Resource | Food, Water, Oxygen, Energy dikelola sebagai stok survival. |
| Ice / Purifier | Ice + Energy → Water. |
| Oxygen Generator | Water + Energy → Oxygen habitat. |
| Habitat vs Suit | Oxygen habitat adalah stok bersama; Suit Oxygen adalah tangki terpisah. **Revisi GDD 2026-11-15:** Suit Oxygen berkurang selama suit DIKENAKAN (di dalam maupun di luar habitat) dan tidak berkurang saat suit dilepas. Suit dilepas di Suit Station. |
| Solar Panel | Menghasilkan Energy; panel rusak menurunkan produksi. Satu panel rusak tidak mematikan seluruh habitat. |
| Outdoor | Gunakan suit; kumpulkan Ice dan perbaiki panel; kembali sebelum Suit Oxygen habis. |
| Farming | Potato saja; 2–3 tahap pertumbuhan. Satu kali siram, tanaman tumbuh waktu nyata 60 sampai 120 detik (acak), lalu panen 2 sampai 4 kentang dan 1 sampai 2 bibit (revisi developer 2026-11-17, menyimpang dari GDD 5.5); tanpa siram tidak tumbuh. |
| Food dan Hunger | **Revisi GDD 2026-11-16:** pemain punya kenyang (hunger) yang berkurang seiring waktu. Pemain makan kentang dari tas (tombol 1, lalu E). Food di HUD = jumlah kentang di storage. Kenyang 0 terlalu lama = kegagalan. |
| Tas dan storage | **Revisi GDD 2026-11-16:** tas pemain: kentang 10, es 10, bibit 5. Tombol 1-3 memilih slot, E memakai (makan / masukkan es ke Water Purifier / tanam bibit). Tas penuh = item tidak bisa diambil; simpan di storage (Control Room). Panen = kentang ke tas. |
| Water | Penyiraman dan mesin memakai stok Water bersama sesuai rancangan GDD. |
| Wire minigame | Hubungkan 3–4 kabel berwarna dengan drag/drop; sederhana, target 15–30 detik. |
| TARS | Dialog/pengingat berdasarkan event atau ambang; bukan rangkaian quest. |
| Sleep / Day | Tidur menyimpan stok, kondisi tanaman/panel, dan progres hari; lalu hari bertambah. |

**Catatan:** Nilai konsumsi, kapasitas, durasi proses, hasil produksi, dan ambang Game Over belum dianggap final kecuali telah diputuskan dan dicatat di `08_DECISIONS.md`.
