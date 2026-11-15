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
