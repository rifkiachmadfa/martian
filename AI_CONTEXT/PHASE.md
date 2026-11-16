# PHASE.md — Pelacak Fase MARS: LAST HARVEST

> File konteks untuk AI dan developer. Baca bersama GDD v1.2 dan Project Instructions.
> GDD = otoritas desain. Repository = otoritas implementasi. File ini = peta kemajuan per milestone dan subphase.
> **WAJIB:** setiap patch yang mengubah kode, scene, aset, desain, atau keputusan harus ikut memperbarui file ini (lihat bagian 1).

## 0. Snapshot

| Item | Nilai |
|---|---|
| Repo | https://github.com/rifkiachmadfa/martian.git |
| Branch | `main` |
| Revisi terakhir diperiksa | `16ca3c15` (2026-11-16, clone `main`); patch Milestone 10 dibuat di atas revisi ini |
| Unity | 6000.6.8f1 |
| Paket penting | Input System 1.20.1 (kode memakai `Input.GetAxisRaw` / `KeyCode`), URP 17.6.0, 2D Animation 16.0.1, Tilemap, Cinemachine |
| Scene | Satu scene: `Assets/Scenes/SampleScene.unity` |
| Fase aktif | Fitur inti milestone 1–9 sudah ada di kode; butir sisa tiap milestone tercantum di bagian 3. Fase berikutnya menunggu keputusan developer (bagian 2) |
| Catatan | AI tidak menjalankan Unity atau compiler. Status berasal dari membaca repo. Objek interaktif di scene: lokal developer, belum di-commit (**Dilaporkan developer**). |

## 1. Aturan PHASE.md (WAJIB)

**Legenda tanda** (menunjukkan *implementasi di repo*, bukan hasil tes):
- `[x]` sudah diimplementasikan di `main` (kode/aset ada).
- `[~]` sebagian, placeholder, atau sementara (alasan ditulis di butir).
- `[ ]` belum dikerjakan.

**Struktur:** Milestone (jadwal 15 hari GDD) → subphase (`N.M`) → fitur (`N.M.K`); fitur dapat berkelompok (`N.M.K.J`), contoh Resource System di 3.3. ID bersifat tetap; jangan dipakai ulang bila butir dihapus.

**Setiap perubahan wajib:**
1. Sebelum membuat perubahan: baca PHASE.md, tentukan milestone dan subphase yang terdampak, dan periksa tidak melanggar daftar Won't Have (GDD 18.3).
2. Di patch yang sama: ubah tanda butir yang terdampak. Fitur baru yang belum punya tempat ditambahkan sebagai subphase atau butir baru dengan ID berikutnya (setelah dicek terhadap GDD).
3. Perbarui tabel ringkasan di bagian 2 (jumlah dan status milestone), snapshot revisi di bagian 0, dan baris baru di Riwayat (bagian 9).
4. Perbarui bagian 4 (penyimpangan) dan bagian 5 (keputusan) bila ada.
5. Jangan menandai `[x]` untuk fitur yang hanya direncanakan. Patch tanpa pembaruan PHASE.md dianggap belum lengkap.

Hasil tes (bila ada) dicatat terpisah di `09_TEST_CHECKLIST.md`, bukan di sini.

## 2. Ringkasan progres

Dihitung dari checklist bagian 3 (revisi `16ca3c15` + patch Milestone 10).

| # | Milestone | Status | Selesai | Sebagian | Belum | Total |
|---|---|---|---|---|---|---|
| 1 | Setup Unity, input, movement, kamera, collision | SEBAGIAN | 8 | 1 | 0 | 9 |
| 2 | Tiga lokasi Tilemap dan perpindahan area | SEBAGIAN | 4 | 5 | 3 | 12 |
| 3 | Interaction System, Resource System, data persisten | SEBAGIAN | 17 | 0 | 2 | 19 |
| 4 | HUD, Resource Monitor, timer, pergantian hari | SEBAGIAN | 10 | 0 | 2 | 12 |
| 5 | Astronaut Suit, Suit Station, oksigen suit | SEBAGIAN | 8 | 1 | 0 | 9 |
| 6 | Ice Gathering dan Water Purifier | SEBAGIAN | 6 | 0 | 1 | 7 |
| 7 | Farming Plot (tanam, siram, tumbuh, panen) | SEBAGIAN | 6 | 2 | 1 | 9 |
| 8 | Oxygen Generator, alur Water/Energy/Oxygen | SEBAGIAN | 5 | 1 | 0 | 6 |
| 9 | Solar Panel dan produksi Energy | SEBAGIAN | 7 | 0 | 1 | 8 |
| 10 | Panel rusak dan minigame kabel | SEBAGIAN | 10 | 1 | 0 | 11 |
| 11 | TARS monitoring dan reminder | SEBAGIAN | 3 | 0 | 10 | 13 |
| 12 | Integrasi loop dan tutorial Day 1 | BELUM | 0 | 0 | 11 | 11 |
| 13 | Day 2–7, Game Over/Victory, balancing | SEBAGIAN | 1 | 1 | 12 | 14 |
| 14 | Audio, animasi, polish | SEBAGIAN | 1 | 2 | 11 | 14 |
| 15 | Playtest penuh dan build | BELUM | 0 | 0 | 4 | 4 |
| | **Total** | | **86** | **14** | **58** | **158** |

Usulan urutan berikutnya (**belum disetujui developer**): 13 (loop menang/kalah minimal: Food berkurang, Game Over, Victory, perilaku `Bed` di Day 7) agar game bisa dimainkan Day 1 sampai Day 7, lalu 11 (`TARSSystem`; milestone 10 sudah dikerjakan, menunggu tes Unity), 12 (tutorial), 14, 15. Urutan jadwal GDD: 10, 11, 12, 13, 14, 15. Build WebGL percobaan sebaiknya lebih awal.
Jika jadwal mundur: kurangi polish dan fitur Should Have dulu; jangan korbankan loop resource inti dan sistem tujuh hari.

## 3. Checklist per milestone

### Milestone 1 — Setup Unity, input, movement, kamera, collision
Acuan: GDD 19 hari 1. Status: **SEBAGIAN** (8 selesai, 1 sebagian, 0 belum dari 9).

**1.1 Setup proyek** (3/3)
- [x] `1.1.1` Unity 6000.6.8f1 dengan URP 17.6.0, 2D Animation, Tilemap, Cinemachine
- [x] `1.1.2` Input memakai Input Manager lama (`Input.GetAxisRaw`/`KeyCode`); paket Input System terpasang tetapi belum dipakai (jangan ganti tanpa konfirmasi)
- [x] `1.1.3` Layer `Interactable` terdaftar di `TagManager`

**1.2 Player** (3/4)
- [x] `1.2.1` Gerak WASD/Arrow dengan `Rigidbody2D`, input dinormalisasi (`PlayerMovement`)
- [x] `1.2.2` `MovementEnabled` untuk mengunci gerak (tidur, mengumpulkan es)
- [x] `1.2.3` Parameter Animator `IsMoving`, `DirectionX`, `DirectionY`
- [~] `1.2.4` Animasi arah Player: controller `Square` baru punya state `Down` dan `up` (kiri/kanan belum)

**1.3 Kamera** (1/1)
- [x] `1.3.1` `CinemachineCamera` mengikuti Player

**1.4 Collision** (1/1)
- [x] `1.4.1` Collision dinding lewat `TilemapCollider2D` di scene

### Milestone 2 — Tiga lokasi Tilemap dan perpindahan area
Acuan: GDD 19 hari 2; GDD 3. Status: **SEBAGIAN** (4 selesai, 5 sebagian, 3 belum dari 12).

**2.1 Control Room** (2/3)
- [x] `2.1.1` Tilemap interior dan dinding (`Ground`, `Wall`, `Tangga`) serta `IndoorZone`
- [x] `2.1.2` `Bed` + `SpawnPoint`, `SuitStation`, dan `TARS` ada di scene
- [~] `2.1.3` `ResourceMonitor` dan `CommunicationTerminal` ada di scene lokal developer, belum di-commit

**2.2 Outdoor** (1/2)
- [x] `2.2.1` Tilemap permukaan Mars (`PasirMars`) dan `SuitGate` di pintu keluar
- [~] `2.2.2` `IceDeposit`, `SolarPanel`, `RepairPoint` ada di scene lokal developer, belum di-commit

**2.3 Farming Area** (0/2)
- [ ] `2.3.1` Tilemap/area Farming Area
- [~] `2.3.2` `WaterPurifier`, `OxygenGenerator`, `FarmingPlot` ada di scene lokal developer, belum di-commit

**2.4 Perpindahan area** (1/3)
- [x] `2.4.1` Control Room ke Outdoor lewat `SuitGate`/`IndoorZone` (satu scene)
- [ ] `2.4.2` Farming Area terhubung ke Control Room
- [~] `2.4.3` Keputusan struktur scene (P-3): saat ini satu scene; rekomendasi AI tetap satu scene

**2.5 Aset visual lingkungan** (0/2)
- [~] `2.5.1` Objek interaktif memakai placeholder kotak berwarna
- [ ] `2.5.2` Sprite final objek interaktif dan dekorasi Tilemap

### Milestone 3 — Interaction System, Resource System, data persisten
Acuan: GDD 19 hari 3; GDD 7, 16. Status: **SEBAGIAN** (17 selesai, 0 sebagian, 2 belum dari 19).

**3.1 Interaction System** (3/3)
- [x] `3.1.1` `IInteractable` (prompt, `CanInteract`, `Interact`)
- [x] `3.1.2` `InteractionDetector` (OverlapCircle, layer `Interactable`, tombol E)
- [x] `3.1.3` `InteractionPromptUI` menampilkan "[E] ..."

**3.2 Dialogue UI** (3/3)
- [x] `3.2.1` `DialogueUI` baris demi baris (E, Spasi, klik)
- [x] `3.2.2` Tombol X menutup dialog langsung
- [x] `3.2.3` Callback saat dialog ditutup

**3.3 Resource System (`ResourceManager`)** (10/10)
- **3.3.1 Model data**
  - [x] `3.3.1.1` Enum `ResourceType`: Ice, Water, HabitatOxygen, SuitOxygen, Energy, Food
  - [x] `3.3.1.2` Stok awal dan kapasitas tiap resource lewat Inspector (angka usulan)
  - [x] `3.3.1.3` Satu stok Water bersama (GDD 7.1)
  - [x] `3.3.1.4` HabitatOxygen dan SuitOxygen terpisah (GDD 7.2)
  - [x] `3.3.1.5` Ice sebagai inventory terbatas (kapasitas 5, usulan)
- **3.3.2 API**
  - [x] `3.3.2.1` `Get`, `GetMax`, `Has`, `Add`, `TryConsume`
  - [x] `3.3.2.2` Event `OnResourceChanged` untuk HUD
  - [x] `3.3.2.3` Singleton `ResourceManager.Instance`
- **3.3.3 Aturan konsumsi**
  - [x] `3.3.3.1` Water dipakai Farming dan Oxygen Generator; Energy dipakai Water Purifier dan Oxygen Generator
- **3.3.4 Alat bantu**
  - [x] `3.3.4.1` `ResourceDebugPanel` (alat uji sementara)

**3.4 Persistensi** (1/3)
- [x] `3.4.1` State resource tetap selama satu scene (tidak reset antar lokasi)
- [ ] `3.4.2` `SaveSystem` (simpan antar hari/scene ke disk)
- [ ] `3.4.3` Simpan state saat tidur

### Milestone 4 — HUD, Resource Monitor, timer, pergantian hari
Acuan: GDD 19 hari 4; GDD 5, 13. Status: **SEBAGIAN** (10 selesai, 0 sebagian, 2 belum dari 12).

**4.1 TimeManager** (3/5)
- [x] `4.1.1` Hari 1 sampai 7, timer 600 detik per hari (usulan)
- [x] `4.1.2` Event `OnDayStarted`, `OnDayTimeEnded`, method `StartDay`
- [x] `4.1.3` `ExternalPause` dan opsi `pauseDuringDialogue`
- [ ] `4.1.4` Grace period saat waktu habis di Outdoor
- [ ] `4.1.5` Peringatan akhir hari (warna timer)

**4.2 HUD** (3/3)
- [x] `4.2.1` HUD Day, timer, Food, Water, Oxygen, Energy lewat event (`HudUI`)
- [x] `4.2.2` Panel Suit O2 (kuning di 30 persen, merah di 10 persen)
- [x] `4.2.3` Panel Ice (opsional)

**4.3 Resource Monitor** (1/1)
- [x] `4.3.1` `ResourceMonitor`: dialog hanya-baca (waktu, stok, suit, panel, mesin)

**4.4 Tidur dan pergantian hari** (3/3)
- [x] `4.4.1` `Bed` + `FadeUI`: kunci gerak, fade hitam, `StartDay(hari+1)`, pindah ke `SpawnPoint`
- [x] `4.4.2` Suit dilepas otomatis saat tidur
- [x] `4.4.3` Aturan tidur: hanya saat sisa waktu <= 120 detik atau waktu habis (usulan)

### Milestone 5 — Astronaut Suit, Suit Station, oksigen suit
Acuan: GDD 19 hari 5; GDD 9. Status: **SEBAGIAN** (8 selesai, 1 sebagian, 0 belum dari 9).

**5.1 SuitSystem** (5/5)
- [x] `5.1.1` Kenakan dan lepas suit; isi ulang dari HabitatOxygen
- [x] `5.1.2` Oksigen suit berkurang selama suit dikenakan, termasuk di dalam habitat (P-1, disetujui 2026-11-15; GDD belum direvisi)
- [x] `5.1.3` Oksigen minimum untuk keluar (20, usulan)
- [x] `5.1.4` `IsOutdoors` ditentukan lewat `IndoorZone`
- [x] `5.1.5` Event `OnSuitOxygenEmpty`

**5.2 Suit Station dan gerbang** (3/3)
- [x] `5.2.1` `SuitStation`: kenakan, isi, lepas
- [x] `5.2.2` `SuitGate` menahan pemain tanpa suit/oksigen minimum, TARS menjelaskan alasan
- [x] `5.2.3` `IndoorZone` (trigger area dalam habitat)

**5.3 Pengingat suit** (0/1)
- [~] `5.3.1` `SuitReminder` (lepas suit di dalam habitat), sementara sampai `TARSSystem` ada

### Milestone 6 — Ice Gathering dan Water Purifier
Acuan: GDD 19 hari 6; GDD 9.1, 11.1. Status: **SEBAGIAN** (6 selesai, 0 sebagian, 1 belum dari 7).

**6.1 Ice Deposit** (3/4)
- [x] `6.1.1` Interaksi E, pemain berhenti sekitar 3 detik, Ice +1, batas kapasitas
- [x] `6.1.2` Wajib mengenakan suit
- [x] `6.1.3` Deposit tidak habis (usulan; keputusan terbuka)
- [ ] `6.1.4` Sprite dan efek deposit es

**6.2 Water Purifier** (3/3)
- [x] `6.2.1` Ice + Energy menjadi Water per batch (1 + 5 menjadi 4 dalam 4 detik; usulan)
- [x] `6.2.2` Penolakan bila Ice atau Energy kurang, atau tangki Water tidak muat
- [x] `6.2.3` Indikator aktif/nonaktif (slot GameObject)

### Milestone 7 — Farming Plot (tanam, siram, tumbuh, panen)
Acuan: GDD 19 hari 7; GDD 10. Status: **SEBAGIAN** (6 selesai, 2 sebagian, 1 belum dari 9).

**7.1 Tanam dan siram** (2/2)
- [x] `7.1.1` Tanam benih kentang (gratis, tanpa stok seed)
- [x] `7.1.2` Siram memakai 2 Water (usulan), satu kali per hari

**7.2 Pertumbuhan** (2/2)
- [x] `7.2.1` 3 tahap, maju satu tahap saat `OnDayStarted` bila disiram
- [x] `7.2.2` Tanaman tidak disiram tidak maju dan tidak mati

**7.3 Panen** (2/2)
- [x] `7.3.1` Panen menambah 20 Food (usulan)
- [x] `7.3.2` Panen ditunda bila Food penuh

**7.4 Visual dan penataan** (0/3)
- [~] `7.4.1` Warna placeholder sebagai penanda tahap
- [ ] `7.4.2` Sprite 2 sampai 3 tahap tanaman
- [~] `7.4.3` Satu objek = satu petak (petak tambahan lewat duplikasi); penataan beberapa petak di Farming Area belum ada

### Milestone 8 — Oxygen Generator, alur Water/Energy/Oxygen
Acuan: GDD 19 hari 8; GDD 11.2. Status: **SEBAGIAN** (5 selesai, 1 sebagian, 0 belum dari 6).

**8.1 Produksi** (3/3)
- [x] `8.1.1` Otomatis per siklus 5 detik: 1 Water + 2 Energy menjadi 5 Oxygen habitat (usulan)
- [x] `8.1.2` Berhenti sendiri bila Water/Energy kurang atau oksigen habitat penuh
- [x] `8.1.3` Indikator aktif/nonaktif (slot GameObject)

**8.2 Kontrol pemain** (0/1)
- [~] `8.2.1` Saklar E menyalakan/mematikan mesin (di luar GDD; menunggu keputusan P-5)

**8.3 Alur** (2/2)
- [x] `8.3.1` HabitatOxygen menjadi sumber pengisian suit
- [x] `8.3.2` Status mesin dilaporkan di Resource Monitor

### Milestone 9 — Solar Panel dan produksi Energy
Acuan: GDD 19 hari 9; GDD 11.3. Status: **SEBAGIAN** (7 selesai, 0 sebagian, 1 belum dari 8).

**9.1 Produksi Energy** (3/3)
- [x] `9.1.1` Energy otomatis per tick (0,5 per detik per panel normal; usulan)
- [x] `9.1.2` Banyak panel menjumlah
- [x] `9.1.3` Produksi berhenti saat jeda (`TimeManager.IsPaused`)

**9.2 Status rusak** (3/3)
- [x] `9.2.1` `Damage()`/`Repair()`, `IsDamaged`, event `OnDamagedChanged`
- [x] `9.2.2` Panel rusak menghasilkan 25 persen (usulan), tidak mematikan habitat
- [x] `9.2.3` Tampilan rusak lewat tint atau slot sprite rusak

**9.3 Konsumsi Energy** (1/2)
- [x] `9.3.1` Water Purifier dan Oxygen Generator mengonsumsi Energy
- [ ] `9.3.2` Sprite panel normal/rusak final

### Milestone 10 — Panel rusak dan minigame kabel
Acuan: GDD 19 hari 10; GDD 11.4. Status: **SEBAGIAN** (10 selesai, 1 sebagian, 0 belum dari 11). Kode ditulis 2026-11-16; BELUM dikompilasi/dites di Unity.

**10.1 Pemicu kerusakan** (3/3)
- [x] `10.1.1` Menu konteks Debug tetap ada untuk uji (`SolarPanel` dan `SolarPanelDamageScheduler`)
- [x] `10.1.2` Keputusan pemicu: **satu kejadian acak per hari** (developer, 2026-11-16)
- [x] `10.1.3` Pemicu otomatis: `SolarPanelDamageScheduler` (waktu acak dalam hari, satu panel normal dipilih acak, pengumuman dialog TARS)

**10.2 Repair Point** (2/2)
- [x] `10.2.1` `RepairPoint` terhubung ke satu `SolarPanel`
- [x] `10.2.2` `RepairPoint` membuka minigame; perbaikan langsung hanya sebagai cadangan bila `WiringMinigameUI` tidak ada di scene

**10.3 Minigame kabel** (5/6)
- [x] `10.3.1` UI 3 sampai 4 kabel berwarna dan konektor (`WiringMinigameUI`, dibuat lewat kode di bawah Canvas)
- [x] `10.3.2` Drag-and-drop dengan mouse
- [x] `10.3.3` Salah sambung boleh dicoba lagi
- [~] `10.3.4` Durasi sekitar 15 sampai 30 detik: tidak ada batas waktu; hanya ukuran teka-teki (4 kabel). Durasi nyata belum diukur
- [x] `10.3.5` Timer hari berhenti selama minigame (`ExternalPause`, nilai sebelumnya dipulihkan)
- [x] `10.3.6` Sukses memanggil `SolarPanel.Repair()` (lewat callback `RepairPoint`)

### Milestone 11 — TARS monitoring dan reminder
Acuan: GDD 19 hari 11; GDD 12. Status: **SEBAGIAN** (3 selesai, 0 sebagian, 10 belum dari 13).

**11.1 TARS (NPC)** (3/3)
- [x] `11.1.1` `TarsWanderer`: jalan 4 arah, langkah pendek, cek rintangan, menembus Player
- [x] `11.1.2` Animasi sprite 3 frame per arah (atas, bawah, kiri, kanan)
- [x] `11.1.3` Dialog saat E; TARS berhenti dan menghadap Player

**11.2 TARSSystem** (0/8)
- **11.2.1 Pemantauan**
  - [ ] `11.2.1.1` Pemantau ambang resource (Food, Water, Oxygen, Energy)
  - [ ] `11.2.1.2` Cooldown dan prioritas agar tidak spam
  - [ ] `11.2.1.3` Dialog predefined berbasis event, tanpa AI generatif (aturan proyek)
- **11.2.2 Reminder**
  - [ ] `11.2.2.1` Resource rendah (Water, Food, Energy)
  - [ ] `11.2.2.2` Isi suit sebelum keluar
  - [ ] `11.2.2.3` Oksigen suit menipis (30 persen dan 10 persen, dapat dikonfigurasi)
  - [ ] `11.2.2.4` Solar Panel rusak
  - [ ] `11.2.2.5` Akhir hari dan pergantian hari

**11.3 Migrasi dan variasi** (0/2)
- [ ] `11.3.1` Pindahkan logika `SuitReminder` ke `TARSSystem`
- [ ] `11.3.2` Variasi komentar TARS (Should Have)

### Milestone 12 — Integrasi loop dan tutorial Day 1
Acuan: GDD 19 hari 12; GDD 5.2. Status: **BELUM** (0 selesai, 0 sebagian, 11 belum dari 11).

**12.1 Alur tutorial Day 1** (0/7)
- [ ] `12.1.1` Mengenal Control Room, TARS, Resource Monitor
- [ ] `12.1.2` Mengambil dan mengisi Astronaut Suit
- [ ] `12.1.3` Keluar dan mengumpulkan Ice dalam jumlah kecil
- [ ] `12.1.4` Membawa Ice ke Water Purifier dan menghasilkan Water
- [ ] `12.1.5` Menanam dan menyiram di Farming Plot
- [ ] `12.1.6` Mengenal Oxygen Generator dan hubungan Water/Energy
- [ ] `12.1.7` Kembali ke Control Room dan tidur

**12.2 Mekanik tutorial** (0/3)
- [ ] `12.2.1` Arahan TARS langkah demi langkah (bukan quest harian)
- [ ] `12.2.2` Status tutorial selesai tersimpan
- [ ] `12.2.3` Tutorial dapat dilewati setelah pernah dimainkan (Should Have)

**12.3 Integrasi** (0/1)
- [ ] `12.3.1` Loop harian berjalan dari awal sampai akhir tanpa urutan wajib

### Milestone 13 — Day 2–7, Game Over/Victory, balancing
Acuan: GDD 19 hari 13; GDD 5, 6, 17. Status: **SEBAGIAN** (1 selesai, 1 sebagian, 12 belum dari 14).

**13.1 Food dan kelaparan** (0/2)
- [ ] `13.1.1` Food berkurang bertahap selama gameplay aktif
- [ ] `13.1.2` Kelaparan mencapai batas gagal

**13.2 Game Over** (0/5)
- [ ] `13.2.1` Countdown darurat saat oksigen suit habis di Outdoor
- [ ] `13.2.2` Game Over: Food habis dan kelaparan
- [ ] `13.2.3` Game Over: oksigen habitat kritis
- [ ] `13.2.4` Peringatan sebelum Game Over (indikator dan TARS)
- [ ] `13.2.5` Layar Game Over

**13.3 Victory dan Day 7** (0/4)
- [~] `13.3.1` `Bed` memblokir tidur di Day 7 dengan pesan placeholder
- [ ] `13.3.2` Akhir Day 7 tanpa Game Over memicu Victory
- [ ] `13.3.3` Konfirmasi Bumi dan pesan penutup TARS
- [ ] `13.3.4` Layar Victory

**13.4 Terminal komunikasi** (1/1)
- [x] `13.4.1` `CommunicationTerminal`: pesan Bumi per hari (teks placeholder; bukan quest)

**13.5 Balancing** (0/2)
- [ ] `13.5.1` Tetapkan angka final lewat playtest (stok awal, laju, kapasitas, ambang)
- [ ] `13.5.2` Pastikan Energy cukup menopang mesin (satu panel normal hanya sedikit di atas konsumsi Oxygen Generator)

### Milestone 14 — Audio, animasi, polish
Acuan: GDD 19 hari 14; GDD 14, 15. Status: **SEBAGIAN** (1 selesai, 2 sebagian, 11 belum dari 14).

**14.1 Animasi** (1/5)
- [x] `14.1.1` Animasi TARS
- [~] `14.1.2` Animasi Player idle/walk (arah kiri/kanan belum)
- [ ] `14.1.3` Animasi tahap tanaman
- [~] `14.1.4` Indikator mesin aktif/nonaktif (belum animasi)
- [ ] `14.1.5` Tampilan panel normal/rusak final

**14.2 Audio** (0/4)
- [ ] `14.2.1` Ambience Control Room dan angin Outdoor
- [ ] `14.2.2` Suara mesin dan efek interaksi (es, siram, tanam, panen, perbaikan)
- [ ] `14.2.3` Notifikasi TARS dan alarm kritis
- [ ] `14.2.4` Audio tidur dan ending

**14.3 UI** (0/2)
- [ ] `14.3.1` Main Menu (Start, Settings, Quit)
- [ ] `14.3.2` Pause dan tutup panel dengan Esc

**14.4 Pembersihan** (0/3)
- [ ] `14.4.1` Hapus `ResourceDebugPanel`
- [ ] `14.4.2` `Assets/_Recovery`, `Assets/Welcome`, dan `.meta` tanpa folder
- [ ] `14.4.3` `.gitignore` untuk `*.patch` dan `Assets/_Recovery/`

### Milestone 15 — Playtest penuh dan build
Acuan: GDD 19 hari 15; GDD 21. Status: **BELUM** (0 selesai, 0 sebagian, 4 belum dari 4).

**15.1 Playtest** (0/2)
- [ ] `15.1.1` Playtest penuh Day 1 sampai Day 7
- [ ] `15.1.2` Definition of Done (GDD 21) terpenuhi

**15.2 Build** (0/2)
- [ ] `15.2.1` Build Windows
- [ ] `15.2.2` Build WebGL (rekomendasi: percobaan lebih awal)

## 4. Penyimpangan dari GDD v1.2 dan keputusan terbuka

| ID | Temuan | Dampak | Status |
|---|---|---|---|
| P-1 | `SuitSystem` mengurangi oksigen suit **selama suit dikenakan, termasuk di dalam habitat** (komentar kode: "revisi GDD 2026-11-15"). GDD v1.2 bagian 7.2 dan 9 menyatakan hanya berkurang di Outdoor. | Mempengaruhi `SuitReminder`, balancing, dan alur Day 1 | **Menunggu keputusan developer.** Jika sengaja, perbarui GDD ke v1.3. |
| P-2 | Dialog tidak menjeda timer (`pauseDuringDialogue = false`). GDD: timer "dapat" berhenti saat dialog penting. | Kecil | Dianggap sesuai (opsional). |
| P-3 | Satu scene, bukan scene terpisah `ControlRoom`/`FarmingArea`/`Outdoor` seperti GDD 16.1. | Menentukan perlu tidaknya `SaveSystem` lintas scene | **Menunggu keputusan.** Rekomendasi AI: tetap satu scene. |
| P-4 | Perilaku `Bed` di Day 7 belum ada (menunggu Victory). | Blok akhir game | Dikerjakan di milestone 13. |
| P-5 | `OxygenGenerator` punya tombol E untuk menyalakan/mematikan. GDD 11.2 hanya menyebut mesin bekerja otomatis. Alasan: pemain perlu menghemat Water untuk tanaman (GDD 7.1). Default menyala, sehingga Water awal bisa habis dalam ±50 dtk bila tidak dimatikan. | Kecil di kode (`allowManualToggle`), memengaruhi balancing dan tutorial Day 1 | **Menunggu keputusan developer.** Jika ditolak, matikan `allowManualToggle`. |

Keputusan terbuka lain: deposit es tak terbatas (usulan: ya), angka balancing semua mesin (termasuk frekuensi kerusakan panel: satu per hari, Day 2 dan seterusnya, mungkin terlalu sering; cek saat playtest), dan urutan fase berikutnya.

## 5. Keputusan yang sudah diambil (jangan diubah tanpa persetujuan)

- TARS bukan pemberi quest; dialog predefined, tanpa AI generatif.
- TARS berjalan lurus 4 arah, langkah pendek, menembus Player; interaksi tetap lewat E.
- Satu stok Water bersama; HabitatOxygen dan SuitOxygen terpisah (`ResourceType`).
- Resource hanya diubah lewat `ResourceManager`; HUD memakai event.
- Angka balancing di kode adalah **usulan**, bukan final.
- Alur kerja perubahan: AI menganalisis repo, lalu memberi patch `.patch`. Developer yang menerapkan (`git apply`). AI tidak commit atau push.
- Terminal komunikasi hanya cerita (bukan quest); pesan bawaan adalah placeholder.
- Sistem PHASE.md (2026-11-16): checklist per milestone dengan subphase; wajib diperbarui di setiap patch.
- Pemicu kerusakan Solar Panel (2026-11-16, developer): satu event acak per hari. USULAN turunan (belum disetujui): mulai Day 2 (Day 1 tutorial bebas kerusakan, `firstDamageDay`), waktu kejadian acak 15-70 persen durasi hari, satu panel normal dipilih acak.

## 6. Inventaris implementasi saat ini

### Script (Assets/Script)
| Script | Fungsi singkat |
|---|---|
| `PlayerMovement` | Gerak 4 arah, `MovementEnabled` untuk mengunci gerak, kirim `IsMoving`/`DirectionX`/`DirectionY` ke Animator |
| `Interaction/IInteractable`, `InteractionDetector`, `InteractionPromptUI` | Deteksi objek interaktif dengan `OverlapCircle` (layer `Interactable`, trigger dihitung), tombol E, prompt "[E] ..." |
| `Interaction/Bed` | Tidur: kunci gerak, fade, lepas suit, `StartDay(day+1)`, pindah ke `SpawnPoint`. Boleh tidur bila sisa waktu <= 120 dtk (usulan). |
| `Resource/ResourceManager` | Satu-satunya stok: Ice, Water, HabitatOxygen, SuitOxygen, Energy, Food. API: `Get`, `GetMax`, `Has`, `Add`, `TryConsume`, event `OnResourceChanged`. |
| `Resource/ResourceType` | Enum 6 jenis stok |
| `Resource/ResourceDebugPanel` | Alat uji sementara, boleh dihapus kelak |
| `Time/TimeManager` | Hari, timer 600 dtk (usulan), 7 hari, `ExternalPause`, `OnDayStarted`, `OnDayTimeEnded`. Dialog tidak menjeda timer secara default. |
| `Suit/SuitSystem` | Equip/Unequip, refill dari habitat, drain oksigen, `IsOutdoors`, event `OnSuitOxygenEmpty` |
| `Suit/SuitStation`, `SuitGate`, `IndoorZone`, `SuitReminder` | Interaksi suit, tembok pintu keluar, zona dalam habitat, pengingat "lepas suit" |
| `Outdoor/IceDeposit` | Deposit es: E, pemain berhenti ±3 dtk, Ice +1 (batas kapasitas Ice), perlu suit (usulan) |
| `Outdoor/SolarPanel` | Energy otomatis per tick; `Damage()`/`Repair()`, `IsDamaged`, `OnDamagedChanged`; banyak panel menjumlah |
| `Outdoor/RepairPoint` | Membuka `WiringMinigameUI`; memperbaiki satu `SolarPanel` setelah minigame sukses (cadangan langsung bila minigame tidak ada) |
| `Outdoor/SolarPanelDamageScheduler` | Satu kejadian acak per hari: merusak satu panel normal pada waktu acak, mengumumkan lewat dialog TARS (sementara pengganti `TARSSystem`) |
| `UI/WiringMinigameUI` | Minigame kabel drag-and-drop mouse; UI dibuat lewat kode di bawah Canvas; menjeda `TimeManager` dan mengunci gerak Player selama terbuka; Esc membatalkan |
| `Machines/WaterPurifier` | Ice + Energy -> Water per batch (usulan 1 + 5 -> 4 dalam 4 dtk), `IsProcessing`, `OnProcessingChanged` |
| `Machines/OxygenGenerator` | Water + Energy -> HabitatOxygen otomatis per siklus (usulan 1 + 2 -> 5 per 5 dtk), saklar E, `IsOn`/`IsRunning` |
| `Farming/FarmingPlot` | Tanam, siram (usulan 2 Water), tumbuh 3 tahap saat hari berganti bila disiram, panen (usulan +20 Food) |
| `Station/ResourceMonitor` | Dialog ringkasan hari, stok, suit, panel, mesin (hanya baca) |
| `Station/CommunicationTerminal` | Pesan Bumi per hari (placeholder, bukan quest) |
| `NPC/TarsWanderer` | TARS jalan 4 arah langkah pendek, cek rintangan (`Rigidbody2D.Cast`), animasi frame 1-2-3-2 per arah, menembus Player (`IgnoreCollision`), dialog saat E |
| `Dialogue/DialogueUI` | Dialog baris demi baris (E/Spasi/klik) |
| `UI/HudUI`, `UI/FadeUI` | HUD (hari, timer, Food, Water, Oxygen, Energy, Ice opsional, panel Suit Oxygen) dan layar fade |

### Editor setup scripts (Assets/Editor) — pola kerja proyek
Menu yang aman dijalankan berulang, tidak menimpa referensi yang sudah diisi:
`Mars > Setup Dialogue and Prompt UI`, `Setup HUD and Time`, `Setup Bed and Fade`, `Setup Suit and Gate`, `Setup TARS Animation`, `Setup Interactive Objects` (8 objek placeholder + panel Ice), `Setup Panel Damage and Wiring Minigame` (baru: pasang `WiringMinigameUI` di Canvas, buat `SolarPanelDamageScheduler`, sambungkan `RepairPoint`).
Milestone baru sebaiknya memakai pola yang sama (satu menu setup per milestone, objek placeholder di sekitar Player).

### Objek penting di SampleScene
`Player`, `TARS`, `TarsArea`, `Bed`, `SpawnPoint`, `SuitStation`, `SuitGate`, `IndoorZone`, `SuitSystem`, `ResourceManager`, `TimeManager`, `DialogueManager`, `Canvas` (`HudRoot`, `DialogueRoot`, `PromptRoot`, `FadeOverlay`), `CinemachineCamera`, `Grid`, `Ground`, `PasirMars`, `Wall`, `Tangga`.
Objek interaktif berikut ada di scene lokal developer (**Dilaporkan developer**); belum ada di `SampleScene.unity` yang ter-commit (tidak ada GUID script objek interaktif di scene): `IceDeposit`, `SolarPanel`, `RepairPoint`, `WaterPurifier`, `OxygenGenerator`, `FarmingPlot`, `ResourceMonitor`, `CommunicationTerminal`, `HudRoot/IcePanel` (masih di sekitar Player; developer memindahkan ke lokasi akhir).
Layer `Interactable` sudah ada (layer 3).

### Aset
`Assets/Robot` (12 sprite TARS, sudah diimpor sebagai Sprite Point filter), `Assets/Player` (animasi Player), `Assets/COPY SPRIGHT`.

## 7. Utang teknis dan kebersihan repo

- `Assets/_Recovery/0.unity` (file pemulihan otomatis Unity) ikut ter-commit; sebaiknya masuk `.gitignore`. Terverifikasi masih ada di `afc2453c`.
- `phase-sync.patch` ter-commit di root repo (`interactive-objects.patch` sudah dihapus di `b09da2e3`). File `.patch` tidak perlu ikut repo; hapus dan tambahkan `*.patch` ke `.gitignore`.
- `Assets/Suit.meta` dan `Assets/popopop.meta` adalah `.meta` tanpa folder.
- `Assets/Welcome` adalah sisa template Unity.
- `ResourceDebugPanel` masih ada (alat uji sementara).
- `SuitReminder` sementara menggantikan `TARSSystem`; pindahkan logikanya saat `TARSSystem` dibuat.
- `SolarPanelDamageScheduler` mengumumkan lewat `DialogueUI` sementara; pindahkan ke `TARSSystem` saat dibuat.
- Belum ada `SaveSystem`; belum ada Game Over dan Victory.
- Kode memakai Input Manager lama (`Input.GetAxisRaw`, `KeyCode`) meski paket Input System terpasang. Jangan ganti sistem input tanpa konfirmasi.

## 8. Catatan praktis untuk patch

- File kode di repo berakhir baris LF; developer memakai Windows. Jika `git apply` gagal karena akhir baris, pakai `git apply --ignore-whitespace`.
- Patch yang membuat file baru harus menyertakan `.meta` (GUID baru) untuk script; PNG/aset biner dan perubahan scene dikerjakan lewat Editor setup script atau langkah manual.
- Perubahan scene (`SampleScene.unity`) dilakukan lewat menu Editor, lalu developer menyimpan dengan Ctrl+S. Hindari mengedit YAML scene secara manual.
- Setelah developer commit, clone ulang repo sebelum membuat patch berikutnya.

## 9. Riwayat

| Tanggal | Perubahan |
|---|---|
| 2026-11-15 | Versi pertama dibuat dari pemeriksaan repo `f0df08f9`. Animasi dan gerak TARS selesai (3 patch). Milestone 6 dijadwalkan sebagai fase berikutnya. |
| 2026-11-15 | Patch objek interaktif (berbasis `a9357e29`): IceDeposit, WaterPurifier, OxygenGenerator, FarmingPlot, SolarPanel + RepairPoint, ResourceMonitor, CommunicationTerminal, menu setup, Ice di HUD. Belum dikompilasi/dites. Milestone 6–10 menjadi SEBAGIAN; penyimpangan P-5 ditambahkan. |
| 2026-11-15 | Sinkronisasi dengan repo `afc2453c`: patch objek interaktif terkonfirmasi sudah di-commit (script, `.meta`, AI_CONTEXT). Terverifikasi dari clone: `SampleScene.unity` belum memuat objek interaktif; layer `Interactable` ada di `TagManager`; `interactive-objects.patch` dan `Assets/_Recovery` masih ter-commit. Tidak ada perubahan kode. |
| 2026-11-16 | Sistem PHASE.md dirancang ulang: checklist per milestone dengan subphase dan fitur, ringkasan progres terhitung, aturan wajib pembaruan di setiap patch. Basis repo `d96d9b6f`. Tidak ada perubahan kode. |
| 2026-11-16 | Milestone 10: `SolarPanelDamageScheduler` (satu kerusakan acak per hari), `WiringMinigameUI`, `RepairPoint` membuka minigame, menu `Setup Panel Damage and Wiring Minigame`. Basis `16ca3c15`. Belum dikompilasi/dites di Unity. Butir 10.x diperbarui, total 86/14/58. |
