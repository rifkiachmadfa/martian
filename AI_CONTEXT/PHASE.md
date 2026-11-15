# PHASE.md — Status Pengembangan MARS: LAST HARVEST

> File konteks untuk AI. Baca bersama GDD v1.2 dan Project Instructions.
> GDD = otoritas desain. Repository = otoritas implementasi. File ini = status terkini dan keputusan yang sudah diambil.
> Perbarui file ini setiap satu milestone selesai (lihat bagian "Cara memperbarui").

## 0. Snapshot

| Item | Nilai |
|---|---|
| Repo | https://github.com/rifkiachmadfa/martian.git |
| Branch | `main` |
| Revisi terakhir diperiksa | `d96d9b6f` "phase" (2026-11-16, clone bersih `main`). Kode tidak berubah sejak `afc2453c` (hanya dokumen). Developer melaporkan tes interaksi objek interaktif lolos semua di scene lokal; `SampleScene.unity` di repo belum memuat objek tersebut (belum di-commit) |
| Unity | 6000.6.8f1 |
| Paket penting | Input System 1.20.1 (kode saat ini memakai `Input.GetAxisRaw` / `KeyCode`), URP 17.6.0, 2D Animation 16.0.1, Tilemap, Cinemachine |
| Scene | Satu scene: `Assets/Scenes/SampleScene.unity` |
| Fase aktif | **Verifikasi objek interaktif selesai (dilaporkan developer). Fase berikutnya menunggu keputusan developer (lihat bagian 6)** |
| Catatan | Tidak ada Unity atau compiler C# yang dijalankan oleh AI. Status di bawah berasal dari membaca kode dan scene di repo. Tanda * berarti hasil tes dilaporkan developer (belum bisa diverifikasi AI karena scene lokal belum di-commit). |

## 1. Status per milestone (jadwal 15 hari GDD bagian 19)

Legenda: SELESAI / SEBAGIAN / BELUM

| # | Milestone | Status | Catatan |
|---|---|---|---|
| 1 | Setup Unity, input, movement, kamera, collision | SELESAI | `PlayerMovement` (WASD/Arrow, Rigidbody2D), `CinemachineCamera` |
| 2 | Tiga lokasi Tilemap dan perpindahan area | SEBAGIAN | Ada area dalam habitat dan area luar (`Ground`, `PasirMars`, `Wall`, `Tangga`, `IndoorZone`, `SuitGate`). Farming Area belum ada. Belum ada pemisahan scene. |
| 3 | Interaction System, ResourceManager, data persisten | SEBAGIAN | Interaction dan `ResourceManager` selesai. `SaveSystem` belum ada (belum perlu selama satu scene). |
| 4 | HUD, Resource Monitor, timer, pergantian hari | SEBAGIAN (hampir selesai) | HUD, `TimeManager`, `Bed` + `FadeUI` jalan. `ResourceMonitor` lolos tes interaksi*. Tes `Bed`/pergantian hari belum dilaporkan. |
| 5 | Astronaut Suit, Suit Station, oksigen suit | SELESAI* | Lihat penyimpangan P-1. Game Over / countdown darurat saat oksigen suit habis belum ada. |
| 6 | Ice Gathering dan Water Purifier | SELESAI* | `IceDeposit`, `WaterPurifier`. Tes interaksi lolos*. Angka masih usulan. |
| 7 | Farming Plot (tanam, siram, tumbuh, panen) | SELESAI* | `FarmingPlot`: satu petak per objek, pertumbuhan lewat `OnDayStarted`. Tes interaksi lolos*. Belum ada sprite. |
| 8 | Oxygen Generator, alur Water/Energy/Oxygen | SELESAI* | `OxygenGenerator` dengan saklar E (usulan di luar GDD, lihat P-5). Tes interaksi lolos*. |
| 9 | Solar Panel dan produksi Energy | SELESAI* | `SolarPanel` menghasilkan Energy; tes interaksi lolos*. Kerusakan hanya lewat menu Debug (pemicu otomatis masuk milestone 10). |
| 10 | Panel rusak dan minigame kabel | SEBAGIAN kecil | `RepairPoint` memperbaiki langsung (sementara; tes interaksi lolos*). Minigame kabel dan pemicu kerusakan belum ada. |
| 11 | TARS monitoring dan reminder | SEBAGIAN kecil | TARS berjalan, berdialog statis, dan ada `SuitReminder`. Belum ada `TARSSystem` berbasis ambang resource (termasuk laporan panel rusak). |
| 12 | Integrasi loop dan tutorial Day 1 | BELUM | |
| 13 | Day 2–7, Victory/Game Over, balancing | BELUM | `Bed` memblokir tidur di Day 7 ("Ending Victory belum dibuat"). Food belum berkurang. |
| 14 | Audio, animasi, polish | SEBAGIAN kecil | Animasi TARS selesai. Audio belum ada. |
| 15 | Playtest penuh, build Windows/WebGL | BELUM | |

## 2. Inventaris implementasi saat ini

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
| `Outdoor/RepairPoint` | Memperbaiki satu `SolarPanel` (langsung, sementara) |
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
`Mars > Setup Dialogue and Prompt UI`, `Setup HUD and Time`, `Setup Bed and Fade`, `Setup Suit and Gate`, `Setup TARS Animation`, `Setup Interactive Objects` (baru: 8 objek placeholder + panel Ice).
Milestone baru sebaiknya memakai pola yang sama (satu menu setup per milestone, objek placeholder di sekitar Player).

### Objek penting di SampleScene
`Player`, `TARS`, `TarsArea`, `Bed`, `SpawnPoint`, `SuitStation`, `SuitGate`, `IndoorZone`, `SuitSystem`, `ResourceManager`, `TimeManager`, `DialogueManager`, `Canvas` (`HudRoot`, `DialogueRoot`, `PromptRoot`, `FadeOverlay`), `CinemachineCamera`, `Grid`, `Ground`, `PasirMars`, `Wall`, `Tangga`.
**Catatan verifikasi (`d96d9b6f`):** objek pada baris ini belum ada di `SampleScene.unity` yang ter-commit (tidak ada satu pun GUID script objek interaktif di scene). Developer melaporkan setup sudah dilakukan di scene lokal (**Dilaporkan developer**, belum di-commit).
Setelah menjalankan `Mars > Setup Interactive Objects` bertambah: `IceDeposit`, `SolarPanel`, `RepairPoint`, `WaterPurifier`, `OxygenGenerator`, `FarmingPlot`, `ResourceMonitor`, `CommunicationTerminal`, `HudRoot/IcePanel` (masih di sekitar Player; developer memindahkan ke lokasi akhir).
Layer `Interactable` sudah ada (layer 3).

### Aset
`Assets/Robot` (12 sprite TARS, sudah diimpor sebagai Sprite Point filter), `Assets/Player` (animasi Player), `Assets/COPY SPRIGHT`.

## 3. Penyimpangan dari GDD v1.2 yang perlu keputusan

| ID | Temuan | Dampak | Status |
|---|---|---|---|
| P-1 | `SuitSystem` mengurangi oksigen suit **selama suit dikenakan, termasuk di dalam habitat** (komentar kode: "revisi GDD 2026-11-15"). GDD v1.2 bagian 7.2 dan 9 menyatakan hanya berkurang di Outdoor. | Mempengaruhi `SuitReminder`, balancing, dan alur Day 1 | **Menunggu keputusan developer.** Jika sengaja, perbarui GDD ke v1.3. |
| P-2 | Dialog tidak menjeda timer (`pauseDuringDialogue = false`). GDD: timer "dapat" berhenti saat dialog penting. | Kecil | Dianggap sesuai (opsional). |
| P-3 | Satu scene, bukan scene terpisah `ControlRoom`/`FarmingArea`/`Outdoor` seperti GDD 16.1. | Menentukan perlu tidaknya `SaveSystem` lintas scene | **Menunggu keputusan.** Rekomendasi AI: tetap satu scene. |
| P-4 | Perilaku `Bed` di Day 7 belum ada (menunggu Victory). | Blok akhir game | Dikerjakan di milestone 13. |
| P-5 | `OxygenGenerator` punya tombol E untuk menyalakan/mematikan. GDD 11.2 hanya menyebut mesin bekerja otomatis. Alasan: pemain perlu menghemat Water untuk tanaman (GDD 7.1). Default menyala, sehingga Water awal bisa habis dalam ±50 dtk bila tidak dimatikan. | Kecil di kode (`allowManualToggle`), memengaruhi balancing dan tutorial Day 1 | **Menunggu keputusan developer.** Jika ditolak, matikan `allowManualToggle`. |

## 4. Keputusan yang sudah diambil (jangan diubah tanpa persetujuan)

- TARS bukan pemberi quest; dialog predefined, tanpa AI generatif.
- TARS berjalan lurus 4 arah, langkah pendek, menembus Player; interaksi tetap lewat E.
- Satu stok Water bersama; HabitatOxygen dan SuitOxygen terpisah (`ResourceType`).
- Resource hanya diubah lewat `ResourceManager`; HUD memakai event.
- Angka balancing di kode adalah **usulan**, bukan final.
- Alur kerja perubahan: AI menganalisis repo, lalu memberi patch `.patch`. Developer yang menerapkan (`git apply`). AI tidak commit atau push.
- Terminal komunikasi hanya cerita (bukan quest); pesan bawaan adalah placeholder.

## 5. Fase aktif: verifikasi objek interaktif selesai, menunggu keputusan fase berikutnya

Status: developer melaporkan seluruh tes interaksi objek interaktif lolos (2026-11-16): Ice Deposit, Water Purifier, Oxygen Generator, Solar Panel + Repair Point, Farming Plot, Resource Monitor, Terminal. Butir tidak dirinci dan scene lokal belum di-commit, sehingga belum bisa diverifikasi AI. Kode tidak berubah sejak `afc2453c`.

Kriteria fase ini (dilaporkan lolos):
- Loop Ice -> Water -> tanaman -> Food berjalan tanpa error Console.
- Energy dari Solar Panel menjalankan Purifier dan Oxygen Generator; panel rusak hanya menurunkan produksi.
- Resource Monitor dan Terminal menampilkan data/pesan yang benar.
- Tidak ada fitur dari daftar Won't Have.

Langkah developer yang tersisa:
1. Commit dan push `SampleScene.unity` (scene lokal berisi objek interaktif) agar bisa diverifikasi dari repo.
2. Pindahkan objek ke lokasi akhir dan pasang sprite (placeholder kotak berwarna sementara).
3. Tentukan fase berikutnya (bagian 6) dan jawab keputusan terbuka di bawah.

Keputusan terbuka: P-1 (oksigen suit berkurang di dalam habitat), P-5 (saklar Oxygen Generator), deposit es tak terbatas (usulan: ya), pemicu kerusakan Solar Panel (usulan: acak jarang atau per hari tertentu, belum dipilih), angka balancing semua mesin.

Catatan balancing (hitungan dari angka default, belum diplaytest): satu Solar Panel normal menghasilkan 0,5 Energy/dtk, sedikit di atas konsumsi Oxygen Generator (2 Energy per 5 dtk = 0,4/dtk); panel rusak (0,125/dtk) tidak cukup menopangnya. Water awal 10 dapat habis dalam sekitar 50 detik bila Oxygen Generator menyala terus.

Di luar lingkup saat ini: minigame kabel, `TARSSystem`, Food berkurang, Game Over/Victory, tutorial Day 1, SaveSystem, audio.

## 6. Urutan fase berikutnya (rekomendasi)

Urutan jadwal GDD: 10 minigame kabel + pemicu kerusakan -> 11 TARSSystem -> 12 Tutorial Day 1 -> 13 Day 2–7, Game Over/Victory, Food drain, balancing -> 14 Audio/polish -> 15 build.
Usulan AI (2026-11-16, **belum disetujui developer**): kerjakan lebih dulu loop menang/kalah minimal (Food berkurang bertahap, Game Over, Victory, perilaku `Bed` di Day 7) agar game bisa dimainkan Day 1 sampai Day 7 dan dibalancing lebih awal, lalu milestone 10 dan 11. Lakukan build WebGL percobaan lebih awal.
Jika jadwal mundur: kurangi polish dan fitur Should Have dulu; jangan korbankan loop resource inti dan sistem tujuh hari.

## 7. Utang teknis dan kebersihan repo

- `Assets/_Recovery/0.unity` (file pemulihan otomatis Unity) ikut ter-commit; sebaiknya masuk `.gitignore`. Terverifikasi masih ada di `afc2453c`.
- `phase-sync.patch` ter-commit di root repo (`interactive-objects.patch` sudah dihapus di `b09da2e3`). File `.patch` tidak perlu ikut repo; hapus dan tambahkan `*.patch` ke `.gitignore`.
- `Assets/Suit.meta` dan `Assets/popopop.meta` adalah `.meta` tanpa folder.
- `Assets/Welcome` adalah sisa template Unity.
- `ResourceDebugPanel` masih ada (alat uji sementara).
- `SuitReminder` sementara menggantikan `TARSSystem`; pindahkan logikanya saat `TARSSystem` dibuat.
- `RepairPoint` memperbaiki langsung sementara; ganti dengan minigame kabel.
- Belum ada `SaveSystem`; belum ada Game Over dan Victory.
- Kode memakai Input Manager lama (`Input.GetAxisRaw`, `KeyCode`) meski paket Input System terpasang. Jangan ganti sistem input tanpa konfirmasi.

## 8. Catatan praktis untuk patch

- File kode di repo berakhir baris LF; developer memakai Windows. Jika `git apply` gagal karena akhir baris, pakai `git apply --ignore-whitespace`.
- Patch yang membuat file baru harus menyertakan `.meta` (GUID baru) untuk script; PNG/aset biner dan perubahan scene dikerjakan lewat Editor setup script atau langkah manual.
- Perubahan scene (`SampleScene.unity`) dilakukan lewat menu Editor, lalu developer menyimpan dengan Ctrl+S. Hindari mengedit YAML scene secara manual.
- Setelah developer commit, clone ulang repo sebelum membuat patch berikutnya.

## 9. Cara memperbarui file ini

Setelah tiap milestone selesai dan di-commit:
1. Ubah status di bagian 1 dan perbarui bagian 0 (revisi terakhir).
2. Pindahkan hasilnya ke bagian 2 (inventaris script/objek baru).
3. Tutup atau tambah penyimpangan di bagian 3 dan keputusan di bagian 4.
4. Tulis ulang bagian 5 untuk milestone berikutnya.

## 10. Riwayat

| Tanggal | Perubahan |
|---|---|
| 2026-11-15 | Versi pertama dibuat dari pemeriksaan repo `f0df08f9`. Animasi dan gerak TARS selesai (3 patch). Milestone 6 dijadwalkan sebagai fase berikutnya. |
| 2026-11-15 | Patch objek interaktif (berbasis `a9357e29`): IceDeposit, WaterPurifier, OxygenGenerator, FarmingPlot, SolarPanel + RepairPoint, ResourceMonitor, CommunicationTerminal, menu setup, Ice di HUD. Belum dikompilasi/dites. Milestone 6–10 menjadi SEBAGIAN; penyimpangan P-5 ditambahkan. |
| 2026-11-15 | Sinkronisasi dengan repo `afc2453c`: patch objek interaktif terkonfirmasi sudah di-commit (script, `.meta`, AI_CONTEXT). Terverifikasi dari clone: `SampleScene.unity` belum memuat objek interaktif; layer `Interactable` ada di `TagManager`; `interactive-objects.patch` dan `Assets/_Recovery` masih ter-commit. Tidak ada perubahan kode. |
| 2026-11-16 | Sinkronisasi dengan repo `d96d9b6f`: PHASE.md dikembalikan ke repo. Developer melaporkan tes interaksi objek interaktif lolos semua (scene lokal belum di-commit); milestone 6–9 menjadi SELESAI* (dilaporkan). Fase berikutnya menunggu keputusan; usulan urutan ditambahkan di bagian 6. Tidak ada perubahan kode. |
