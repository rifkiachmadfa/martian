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

## ResourceManager (ditambahkan 2026-11-15; dilaporkan developer lolos)
- [x] Play: panel uji muncul di kiri atas dengan 6 resource dan nilai awal usulan.
- [x] Tombol +10 pada Water: naik dan berhenti di kapasitas 30.
- [x] Tombol -10 pada Ice (stok 0): nilai tetap 0 dan Console menulis pesan "tidak cukup".
- [x] HabitatOxygen dan SuitOxygen berubah sendiri-sendiri.
- [x] Console tidak menampilkan error (khususnya "Dialogue Root belum diisi" atau missing script).

## TimeManager dan HUD (ditambahkan 2026-11-15; dilaporkan developer lolos, butir tidak dirinci)
- [x] Menu `Mars > Setup HUD and Time` jalan tanpa error; setelah itu ada objek `TimeManager` dan `Canvas/HudRoot`.
- [x] Play: HUD menampilkan "Day 1 / 7", timer 10:00 yang menghitung mundur, dan Food/Water/Oxygen/Energy sesuai nilai awal.
- [x] Tombol +/- di panel uji mengubah angka HUD seketika (Food, Water, Oxygen, Energy).
- [x] Buka dialog TARS: timer berhenti; tutup dialog: timer lanjut.
- [x] Klik kanan komponen TimeManager > Debug: Sisakan 5 detik: timer berhenti di 00:00 dan Console menulis "waktu hari 1 habis" sekali saja.
- [x] Klik kanan > Debug: Mulai hari berikutnya: HUD menjadi "Day 2 / 7" dan timer kembali 10:00.
- [x] Menjalankan menu setup kedua kali tidak membuat HUD ganda.
- [x] Console tidak menampilkan error atau peringatan HudUI/TimeManager.

## Bed, tidur, pergantian hari (ditambahkan 2026-11-15, belum dites di Unity)
- [ ] Menu `Mars > Setup Bed and Fade` jalan tanpa error; ada `Canvas/FadeOverlay` dan `Bed` (dengan anak `SpawnPoint`).
- [ ] Mendekati Bed memunculkan prompt "[E] Tidur".
- [ ] E di awal hari: dialog "Belum waktunya tidur. Sisa waktu hari ini: mm:ss." Setelah dialog ditutup, dialog tidak terbuka lagi sendiri.
- [ ] Setelah "Debug: Sisakan 5 detik" lalu E: layar memudar ke hitam, hari menjadi "Day 2 / 7", timer 10:00, Player muncul di SpawnPoint, layar memudar terang.
- [ ] Selama transisi Player tidak bisa digerakkan dan timer tidak berjalan; setelahnya gerakan normal dan timer mulai dari 10:00.
- [ ] Stok resource tidak berubah setelah tidur.
- [ ] Layar hitam tidak menghalangi klik/tombol setelah fade selesai.
- [ ] Day 7 setelah waktu habis: E memunculkan pesan "Hari terakhir telah selesai", hari tetap 7.
- [ ] Console tidak menampilkan error atau peringatan Bed/FadeUI.
