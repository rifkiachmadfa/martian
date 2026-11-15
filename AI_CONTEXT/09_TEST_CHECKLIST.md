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
- [ ] (DIUBAH 2026-11-15; sebelumnya: timer berhenti saat dialog) Buka dialog TARS: timer HUD tetap berjalan selama dialog terbuka; stok resource yang sedang berubah (misalnya Suit O2) tetap berubah.
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

## Suit, Suit Station, gerbang keluar (ditambahkan 2026-11-15, belum dites di Unity)
- [ ] Menu `Mars > Setup Suit and Gate` jalan tanpa error; ada `SuitSystem`, `SuitStation`, `IndoorZone`, `SuitGate`, dan `HudRoot/SuitPanel`. Menjalankannya dua kali tidak membuat objek ganda.
- [ ] Setelah IndoorZone menutupi interior: Play, Console tidak menampilkan peringatan "tidak ada IndoorZone" atau error SuitSystem/SuitGate/SuitStation.
- [ ] Panel "Suit O2" tidak tampil sebelum suit dikenakan.
- [ ] Dekati Suit Station tanpa suit: prompt "[E] Kenakan suit". E: dialog "Suit dikenakan. Oksigen suit berkurang selama suit dikenakan... Oksigen suit terisi ...". Panel Suit O2 muncul.
- [ ] Setelah E: HabitatOxygen turun dan SuitOxygen naik dengan jumlah sama (cek HUD Oxygen dan panel Suit O2; atau ResourceDebugPanel). Total oksigen tidak bertambah/hilang.
- [ ] Suit dikenakan dan tabung belum penuh: prompt "[E] Isi oksigen suit"; E mengisi sampai penuh. Setelah penuh, prompt menjadi "[E] Lepas suit".
- [ ] HabitatOxygen 0 dan tabung belum penuh: prompt "[E] Lepas suit" (tidak macet); E melepas suit.
- [ ] Tanpa suit (belum E di Suit Station): berjalan ke pintu terhalang; TARS menjelaskan perlu suit; pesan tidak muncul lagi sebelum 8 detik jika terus menabrak.
- [ ] Suit dikenakan tetapi oksigen suit < 20: pintu tetap terhalang dengan pesan oksigen minimum.
- [ ] Suit dikenakan, oksigen >= 20: pemain bisa keluar. Suit O2 berkurang sekitar 1 per detik, baik di dalam maupun di luar habitat.
- [ ] Teks Suit O2 putih, lalu kuning di <= 30%, merah di <= 10%.
- [ ] Di luar, oksigen suit turun di bawah 20: pemain tetap bisa masuk kembali ke habitat (tembok mati saat di luar).
- [ ] Buka dialog (dalam atau luar) saat suit dikenakan: oksigen suit tetap berkurang selama dialog terbuka (default); dengan `pauseDuringDialogue` dicentang di TimeManager: berhenti, lalu lanjut setelah dialog ditutup.
- [ ] Oksigen suit mencapai 0 di luar: Console menulis peringatan sekali (belum ada Game Over, itu disengaja). Mencapai 0 di dalam habitat: tidak ada peringatan; pemain bisa melepas suit.
- [ ] Tidur lewat Bed: stok oksigen suit tidak berubah.
- [ ] Lepas suit: E di Suit Station saat prompt "Lepas suit": dialog "Suit dilepas...", panel Suit O2 hilang, oksigen suit tidak berkurang lagi dan nilainya bertahan.
- [ ] Setelah suit dilepas, berjalan ke pintu: terhalang lagi (perlu mengenakan suit).
- [ ] Kenakan suit lagi: tabung diisi dari habitat hingga penuh (jika stok ada).
- [ ] Tidur lewat Bed saat suit dikenakan (Bed di dalam IndoorZone): oksigen suit tidak berkurang selama transisi; setelah bangun suit sudah dilepas (panel Suit O2 hilang), nilai oksigen tabung sama seperti sebelum tidur.
- [ ] Pengingat: kenakan suit, diam di dalam habitat 30 detik: dialog TARS "Anda masih mengenakan suit di dalam habitat..." muncul. Tutup dialog: pengingat berikutnya baru muncul setelah sekitar 90 detik jika suit masih dikenakan.
- [ ] Pengingat tidak muncul saat suit dilepas, saat pemain di luar, atau saat dialog lain sedang terbuka; hitungan mulai ulang dari 30 detik setelah suit dilepas lalu dikenakan lagi.
- [ ] Console tidak menampilkan error SuitReminder. Menu `Mars > Setup Suit and Gate` dijalankan ulang menambahkan SuitReminder tanpa duplikat.

## Dialog: input dan waktu (ditambahkan 2026-11-15, belum dites di Unity)
- [ ] Menu `Mars > Setup Dialogue and Prompt UI` dijalankan ulang: muncul tombol "X" di pojok kanan atas kotak dialog (`DialogueRoot/CloseButton`); menjalankannya lagi tidak menduplikasi.
- [ ] Dialog dengan beberapa baris (TARS): E, Spasi, dan klik kiri di area kosong layar masing-masing memajukan satu baris; pada baris terakhir menutup dialog.
- [ ] Klik tombol X menutup dialog langsung walau masih ada baris tersisa, dan TIDAK melompat ke baris berikutnya dulu (tidak terhitung ganda).
- [ ] Setelah dialog ditutup lewat Spasi/klik/X, TARS berjalan lagi (callback `Resume` jalan) dan dialog tidak terbuka ulang sendiri.
- [ ] Tombol yang membuka dialog (E) tidak langsung menutupnya pada frame yang sama.
- [ ] Selama dialog terbuka, timer HUD terus berkurang dan Food/Water/Oxygen/Energy tetap berjalan sesuai sistemnya.
- [ ] Dialog Bed, Suit Station, SuitGate, dan SuitReminder juga bisa ditutup dengan Spasi/klik/X. Tidak ada error di Console.
