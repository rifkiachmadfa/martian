# PHASE.md — Status Pengembangan MARS: LAST HARVEST

> File konteks untuk AI. Baca bersama GDD v1.2 dan Project Instructions.
> GDD = otoritas desain. Repository = otoritas implementasi. File ini = status terkini dan keputusan yang sudah diambil.
> Perbarui file ini setiap satu milestone selesai (lihat bagian "Cara memperbarui").

## 0. Snapshot

| Item | Nilai |
|---|---|
| Repo | https://github.com/rifkiachmadfa/martian.git |
| Branch | `main` |
| Revisi terakhir diperiksa | `f0df08f9` "tars mvoement" (2026-11-15) |
| Unity | 6000.6.8f1 |
| Paket penting | Input System 1.20.1 (kode saat ini memakai `Input.GetAxisRaw` / `KeyCode`), URP 17.6.0, 2D Animation 16.0.1, Tilemap, Cinemachine |
| Scene | Satu scene: `Assets/Scenes/SampleScene.unity` |
| Fase aktif | **Milestone 6 — Ice Gathering + Water Purifier (belum dimulai)** |
| Catatan | Tidak ada Unity yang dijalankan oleh AI. Semua status di bawah berasal dari membaca kode dan scene, bukan dari playtest. |

## 1. Status per milestone (jadwal 15 hari GDD bagian 19)

Legenda: SELESAI / SEBAGIAN / BELUM

| # | Milestone | Status | Catatan |
|---|---|---|---|
| 1 | Setup Unity, input, movement, kamera, collision | SELESAI | `PlayerMovement` (WASD/Arrow, Rigidbody2D), `CinemachineCamera` |
| 2 | Tiga lokasi Tilemap dan perpindahan area | SEBAGIAN | Ada area dalam habitat dan area luar (`Ground`, `PasirMars`, `Wall`, `Tangga`, `IndoorZone`, `SuitGate`). Farming Area belum ada. Belum ada pemisahan scene. |
| 3 | Interaction System, ResourceManager, data persisten | SEBAGIAN | Interaction dan `ResourceManager` selesai. `SaveSystem` belum ada (belum perlu selama satu scene). |
| 4 | HUD, Resource Monitor, timer, pergantian hari | SEBAGIAN (hampir selesai) | HUD, `TimeManager`, `Bed` + `FadeUI` jalan. Resource Monitor sebagai objek interaktif di Control Room belum ada. HUD belum menampilkan Ice. |
| 5 | Astronaut Suit, Suit Station, oksigen suit | SELESAI* | Lihat penyimpangan P-1. Game Over / countdown darurat saat oksigen suit habis belum ada. |
| 6 | Ice Gathering dan Water Purifier | **BELUM** | Fase aktif berikutnya. |
| 7 | Farming Plot (tanam, siram, tumbuh, panen) | BELUM | |
| 8 | Oxygen Generator, alur Water/Energy/Oxygen | BELUM | |
| 9 | Solar Panel dan produksi Energy | BELUM | Energy saat ini hanya stok awal, tidak ada produksi. |
| 10 | Panel rusak dan minigame kabel | BELUM | |
| 11 | TARS monitoring dan reminder | SEBAGIAN kecil | TARS berjalan, berdialog statis, dan ada `SuitReminder`. Belum ada `TARSSystem` berbasis ambang resource. |
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
| `NPC/TarsWanderer` | TARS jalan 4 arah langkah pendek, cek rintangan (`Rigidbody2D.Cast`), animasi frame 1-2-3-2 per arah, menembus Player (`IgnoreCollision`), dialog saat E |
| `Dialogue/DialogueUI` | Dialog baris demi baris (E/Spasi/klik) |
| `UI/HudUI`, `UI/FadeUI` | HUD (hari, timer, Food, Water, Oxygen, Energy, panel Suit Oxygen) dan layar fade |

### Editor setup scripts (Assets/Editor) — pola kerja proyek
Menu yang aman dijalankan berulang, tidak menimpa referensi yang sudah diisi:
`Mars > Setup Dialogue and Prompt UI`, `Setup HUD and Time`, `Setup Bed and Fade`, `Setup Suit and Gate`, `Setup TARS Animation`.
Milestone baru sebaiknya memakai pola yang sama (satu menu setup per milestone, objek placeholder di sekitar Player).

### Objek penting di SampleScene
`Player`, `TARS`, `TarsArea`, `Bed`, `SpawnPoint`, `SuitStation`, `SuitGate`, `IndoorZone`, `SuitSystem`, `ResourceManager`, `TimeManager`, `DialogueManager`, `Canvas` (`HudRoot`, `DialogueRoot`, `PromptRoot`, `FadeOverlay`), `CinemachineCamera`, `Grid`, `Ground`, `PasirMars`, `Wall`, `Tangga`.
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

## 4. Keputusan yang sudah diambil (jangan diubah tanpa persetujuan)

- TARS bukan pemberi quest; dialog predefined, tanpa AI generatif.
- TARS berjalan lurus 4 arah, langkah pendek, menembus Player; interaksi tetap lewat E.
- Satu stok Water bersama; HabitatOxygen dan SuitOxygen terpisah (`ResourceType`).
- Resource hanya diubah lewat `ResourceManager`; HUD memakai event.
- Angka balancing di kode adalah **usulan**, bukan final.
- Alur kerja perubahan: AI menganalisis repo, lalu memberi patch `.patch`. Developer yang menerapkan (`git apply`). AI tidak commit atau push.

## 5. Fase aktif: Milestone 6 — Ice Gathering + Water Purifier

Status: rencana, belum ada kode. Menunggu keputusan P-1 dan konfirmasi poin di bawah.

Lingkup:
1. `IceDeposit` (IInteractable): interaksi E, proses beberapa detik, menambah `ResourceType.Ice`. Batas inventory memakai kapasitas Ice di `ResourceManager` (saat ini 5).
2. `WaterPurifier` (IInteractable): memakai Ice + Energy, menghasilkan Water ke stok bersama. Logika terpisah dari tampilan, ada event status aktif/nonaktif.
3. Tambah tampilan Ice di `HudUI`.
4. Editor setup `Mars > Setup Ice and Water Purifier`.

Usulan angka (belum disetujui): kumpul 3 dtk per Ice; Purifier 1 Ice + 5 Energy -> 4 Water dalam 4 dtk.

Keputusan terbuka: deposit es tak terbatas atau habis (usulan: tak terbatas).

Di luar lingkup: Solar Panel, Oxygen Generator, farming, TARSSystem.

Kriteria selesai:
- Pemain bisa mengumpulkan Ice di luar dan stok Ice bertambah sampai batas.
- Purifier mengubah Ice menjadi Water dan mengurangi Energy; tidak jalan bila Ice atau Energy kurang (ada umpan balik jelas).
- HUD menampilkan Ice dan stok lain berubah lewat event.
- Tidak ada fitur dari daftar Won't Have.

## 6. Urutan fase berikutnya (rekomendasi)

7 Farming -> 8 Oxygen Generator -> 9 Solar Panel -> 10 minigame kabel -> 11 TARSSystem -> 12 Tutorial Day 1 -> 13 Day 2–7, Game Over/Victory, Food drain, balancing -> 14 Audio/polish -> 15 build.
Jika jadwal mundur: kurangi polish dan fitur Should Have dulu; jangan korbankan loop resource inti dan sistem tujuh hari.

## 7. Utang teknis dan kebersihan repo

- `Assets/_Recovery/0.unity` (file pemulihan otomatis Unity) ikut ter-commit; sebaiknya masuk `.gitignore`.
- `Assets/Suit.meta` dan `Assets/popopop.meta` adalah `.meta` tanpa folder.
- `Assets/Welcome` adalah sisa template Unity.
- `ResourceDebugPanel` masih ada (alat uji sementara).
- `SuitReminder` sementara menggantikan `TARSSystem`; pindahkan logikanya saat `TARSSystem` dibuat.
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
