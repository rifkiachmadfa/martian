# Testing Checklist

Gunakan checklist ini saat fitur terkait sudah tersedia. Catat versi build, langkah reproduksi, hasil, dan bukti.

## Player & world
- [ ] Gerak 4 arah/diagonal sesuai desain dan tidak menembus wall.
- [ ] Animasi arah dan idle sesuai input terakhir.
- [ ] Kamera mengikuti player dengan benar.
- [ ] Sorting dan perpindahan area tampil benar.
- [ ] Interaksi hanya aktif pada objek/range yang sesuai.

## Survival loop
- [ ] Resource bertambah/berkurang sesuai aksi.
- [ ] Ice dapat diproses menjadi Water.
- [ ] Water dan Energy dikonsumsi mesin secara benar.
- [ ] Habitat Oxygen terpisah dari Suit Oxygen.
- [ ] Suit Oxygen hanya terkuras saat outdoor.
- [ ] Panel rusak mengurangi produksi, bukan mematikan semua panel.

## Farming & progression
- [ ] Potato tumbuh sesuai tahap dan aturan penyiraman.
- [ ] Tanaman tanpa air tidak maju tahap.
- [ ] Tidur menyimpan state dan memajukan hari.
- [ ] Hari 7 menghasilkan ending sesuai kondisi.
- [ ] Kondisi Victory/Game Over dan TARS terpicu benar.

## Quality
- [ ] Tidak ada error/exception di Console.
- [ ] Tidak ada missing reference/asset.
- [ ] Build berjalan pada target.
- [ ] Performa layak pada perangkat spesifikasi rendah.

## Interaksi & TARS (ditambahkan 2026-11-15)
Status [x] berarti dilaporkan developer, belum diverifikasi AI.
- [x] TARS berjalan acak dan tetap di dalam area jelajah.
- [ ] TARS tidak menembus dinding dan tidak macet di sudut.
- [x] Mendekati TARS memunculkan prompt "[E] Bicara dengan TARS".
- [x] Menekan E membuka dialog; TARS berhenti dan menghadap Player.
- [x] E melanjutkan baris dialog; setelah baris terakhir TARS berjalan lagi.
- [ ] E terakhir tidak membuka dialog baru secara langsung.
- [ ] Console tidak menampilkan error atau peringatan `TarsWanderer`/`DialogueUI`.
- [ ] Animasi TARS benar untuk arah kiri, kanan, dan atas.
- [ ] Animasi arah bawah (menunggu sprite Down).
