# Project Status

Terakhir diperbarui: 2026-11-16
Dasar audit: repo `rifkiachmadfa/martian`, branch `main`, commit `d96d9b6f` (dicek ulang 2026-11-16; kode tidak berubah sejak `afc2453c`). Patch objek interaktif sudah ter-commit [Repo]. Developer melaporkan tes interaksi objek interaktif lolos semua [Dev]; scene lokal belum di-commit.

Label sumber: **[Repo]** terverifikasi dari file di repo, **[Dev]** dilaporkan developer (belum bisa dicek AI), **[?]** belum diverifikasi.

## Tahap saat ini
Fondasi pemain dan NPC. Setara sekitar hari produksi 1–2 dari jadwal 15 hari: movement, sistem interaksi modular, dan TARS yang berjalan acak serta bisa diajak bicara. Sudah ada ResourceManager (M1), TimeManager + HUD (M2), dan Bed/tidur/pergantian hari (M3, menunggu tes). Suit (Milestone A) dan objek interaktif Outdoor/Farming Area/Control Room (patch 2026-11-15, hanya script + menu setup) sudah dibuat tetapi belum dikompilasi dan dites di Unity. Belum ada minigame kabel, TARSSystem, Victory/Game Over, atau save.

## Status fitur
| Fitur | Status | Bukti / catatan |
|---|---|---|
| Movement & collision | Ada | [Repo] `Assets/Script/PlayerMovement.cs` (Rigidbody2D.MovePosition, `Input.GetAxisRaw`). Tes collision: [?] |
| Camera follow | Ada | [Repo] `CinemachineCamera` di `SampleScene` |
| Directional animation player | Bermasalah menurut catatan lama | [Repo] Animator `Square.controller` hanya punya state `Down` dan `up`; belum ada kiri/kanan. Re-test: [?] |
| Sistem interaksi (`IInteractable`, `InteractionDetector`, prompt UI) | Ada | [Repo] kode di `Assets/Script/Interaction/`. Berfungsi di Unity: [Dev] |
| TARS berjalan acak di area | Ada | [Repo] `Assets/Script/NPC/TarsWanderer.cs`. Berfungsi di Unity: [Dev] |
| TARS berhenti saat interaksi + dialog | Ada | [Repo] `Assets/Script/Dialogue/DialogueUI.cs`. Berfungsi di Unity: [Dev] |
| Menu setup UI (Mars > Setup Dialogue and Prompt UI) | Ada | [Repo] `Assets/Editor/MarsUISetup.cs`. Sudah dijalankan: [Dev] |
| Sprite animasi TARS | Sebagian | [Repo] folder `Assets/Robot` punya Left, Right, Up (3 frame tiap arah). Folder **Down tidak ada** |
| Resource systems | Ada | [Repo] `Assets/Script/Resource/`. Playtest panel uji: [Dev] berjalan baik. Komponen di scene lokal dan belum ter-push ke GitHub saat audit 2026-11-15: [Dev] |
| Timer harian (TimeManager) | Ada | [Repo] `Assets/Script/Time/TimeManager.cs`. Playtest: [Dev] "sudah bagus" (butir tidak dirinci). Sejak 2026-11-15 timer tetap berjalan saat dialog (flag `pauseDuringDialogue`, default mati); `ExternalPause` untuk transisi |
| HUD (Day, timer, 4 resource) | Ada | [Repo] `Assets/Script/UI/HudUI.cs`, `Assets/Editor/MarsHudSetup.cs`. Playtest: [Dev] "sudah bagus" |
| Bed, tidur, fade, pergantian hari | Script dibuat (Milestone 3) | [Repo] `Assets/Script/Interaction/Bed.cs`, `Assets/Script/UI/FadeUI.cs`, `Assets/Editor/MarsSleepSetup.cs`. Belum dites di Unity |
| Ending Victory / Game Over | Belum ada | [Repo] tidak ada script. Bed hari terakhir hanya menampilkan pesan placeholder |
| Suit, Suit Station, gerbang keluar, oksigen suit (berkurang saat dikenakan) | Script dibuat (Milestone A + revisi: tombol Suit Station hanya kenakan/lepas dengan isi tabung otomatis 2026-11-16, lepas otomatis saat tidur, pengingat TARS `SuitReminder`) | [Repo setelah patch] `Assets/Script/Suit/*`, `Assets/Editor/MarsSuitSetup.cs`, `HudUI` (panel suit). Belum dikompilasi/dites di Unity [?]. Objek scene (SuitStation, IndoorZone, SuitGate) dibuat lewat menu dan diposisikan developer [?] |
| Hunger dan makan (kentang dari tas) | Script dibuat 2026-11-16 (belum dites) | [Repo setelah patch] `Assets/Script/Survival/{HungerSystem,HungerReminder}.cs`, `Assets/Editor/MarsHungerSetup.cs`, `HudUI.hungerText`, baris Kenyang di `ResourceMonitor` (sudah dihapus 2026-11-16). Belum dikompilasi/dites di Unity [?]. Game Over kelaparan belum ada (hanya event) |
| Tas pemain, hotbar, storage, bibit | Script dibuat 2026-11-16 (belum dites) | [Repo setelah patch] `Assets/Script/Inventory/*`, `Assets/Script/UI/{InventoryHotbarUI,StorageUI}.cs`, `Assets/Script/Station/FoodStorage.cs`, `IItemReceiver`, perubahan di `InteractionDetector`, `FarmingPlot`, `IceDeposit`, `WaterPurifier`, `Assets/Editor/MarsInventorySetup.cs`. Belum dikompilasi/dites di Unity [?]. Objek scene dibuat lewat menu `Mars > Setup Inventory and Storage`; Food di Inspector ResourceManager perlu diubah manual [?] |
| Countdown darurat / Game Over oksigen suit | Belum ada | `SuitSystem.OnSuitOxygenEmpty` hanya event + log |
| Ice Deposit (kumpul es, inventory Ice) | Script dibuat; tes interaksi dilaporkan lolos | [Repo setelah patch] `Assets/Script/Outdoor/IceDeposit.cs`; HUD menampilkan Ice lewat `HudUI.iceText`. Tes interaksi: [Dev] lolos 2026-11-16 (butir tidak dirinci; scene lokal belum di-commit) |
| Water Purifier | Script dibuat; tes interaksi dilaporkan lolos | [Repo setelah patch] `Assets/Script/Machines/WaterPurifier.cs`. Tes interaksi: [Dev] lolos 2026-11-16 (butir tidak dirinci; scene lokal belum di-commit) |
| Oxygen Generator | Script dibuat; tes interaksi dilaporkan lolos | [Repo setelah patch] `Assets/Script/Machines/OxygenGenerator.cs` (saklar E = usulan di luar GDD). Tes interaksi: [Dev] lolos 2026-11-16 (butir tidak dirinci; scene lokal belum di-commit) |
| Solar Panel (produksi Energy, status rusak) | Script dibuat; tes interaksi dilaporkan lolos; pemicu kerusakan acak 1x per hari dibuat 2026-11-16 (belum dites) | [Repo setelah patch] `Assets/Script/Outdoor/SolarPanel.cs`. Pemicu otomatis: `Assets/Script/Outdoor/SolarPanelDamageScheduler.cs` [Repo setelah patch, belum dites di Unity]. Tes interaksi: [Dev] lolos 2026-11-16 (butir tidak dirinci; scene lokal belum di-commit) |
| Repair Point | Script dibuat; kini membuka minigame kabel (patch 2026-11-16, belum dites); tes lama (perbaikan langsung) dilaporkan lolos | [Repo setelah patch] `Assets/Script/Outdoor/RepairPoint.cs`. Tes interaksi: [Dev] lolos 2026-11-16 (butir tidak dirinci; scene lokal belum di-commit) |
| Wiring minigame | Script dibuat (belum dites) | [Repo setelah patch] `Assets/Script/UI/WiringMinigameUI.cs`, `Assets/Editor/MarsRepairSetup.cs`. Belum dikompilasi/dites di Unity [?] |
| Resource Monitor | DIHAPUS 2026-11-16 (permintaan developer); HUD dan Storage menggantikan, fungsi laporan fasilitas pindah ke TARS (`PHASE.md` 11.4) | [Dev] |
| Terminal komunikasi | DIHAPUS 2026-11-16 (permintaan developer); pesan Bumi pindah ke TARS (`PHASE.md` 11.4.1) | [Dev] |
| Farming / Potato | Script dibuat; tes interaksi dilaporkan lolos (satu petak per objek, tanpa sprite) | [Repo setelah patch] `Assets/Script/Farming/FarmingPlot.cs`: tanam, siram, tumbuh saat hari berganti, panen. Tes interaksi: [Dev] lolos 2026-11-16 (butir tidak dirinci; scene lokal belum di-commit) |
| TARS reminder berbasis resource | Belum ada | Dialog TARS saat ini teks tetap lewat Inspector |
| Save system | Belum ada | [Repo] |
| Tiga scene area (Control Room, Farming, Outdoor) | Belum ada | [Repo] hanya `SampleScene` |

## Kondisi scene (penting)
- **[Repo]** `SampleScene.unity` di GitHub hanya berisi: Main Camera, CinemachineCamera, Global Light 2D, Player, Grid (Ground, Wall, PasirMars, Tangga). Tidak ada referensi ke `TarsWanderer`, `DialogueUI`, `InteractionDetector`, atau `InteractionPromptUI`.
- **[Dev]** Scene lokal developer berisi tambahan: `TARS`, `TarsArea`, `Canvas` (DialogueRoot, PromptRoot), `EventSystem`, dan `InteractionDetector` di Player. Belum di-commit.
- **[Dev]** Layer `Interactable` dipakai oleh TARS. Layer ini belum ada di `TagManager.asset` di repo.

## Belum diverifikasi / risiko
- Isi Inspector TARS dan Player di scene lokal (belum di-commit).
- Milestone 3: kompilasi dan logika hanya dicek dengan stub di luar Unity [?]. Menu `Mars > Setup Bed and Fade`, fade, perpindahan Player ke SpawnPoint, dan interaksi E dengan Bed belum dites di Unity [?]. Collider dan posisi Bed/SpawnPoint ditentukan developer di scene [?].
- GitHub belum memuat Milestone 1 (komponen ResourceManager di scene dan file .meta) saat audit; basis patch 2 adalah `20c1fc28` + patch 1 [?].
- Tidak ada compile atau run Unity oleh AI; semua kode hanya diperiksa lewat `git apply --check`.
- Gambar arah bawah untuk TARS (`Down Frames`) belum ada.
- Collider Player di scene belum diperiksa.
- Player masih bisa bergerak saat dialog terbuka (kini disengaja: waktu dan resource tetap berjalan saat dialog).
- **[Repo, d96d9b6f]** `SampleScene.unity` yang ter-commit belum berisi objek interaktif baru (tidak ada GUID script objek interaktif di scene). **[Dev]** setup di scene lokal sudah dilakukan dan tes interaksi lolos; belum di-commit. Layer `Interactable` ada di `TagManager.asset` [Repo].
- Objek interaktif (patch 2026-11-15): tidak ada compiler C# atau Unity di lingkungan AI; kode hanya diperiksa dengan membaca dan `git apply --check` [?]. Posisi, sprite, dan collider objek di scene ditentukan developer [?].
- Keseimbangan: `OxygenGenerator` menyala sejak awal dan memakai Water (usulan 1 per 5 detik) sehingga Water awal (10) bisa habis dalam sekitar 50 detik bila tidak dimatikan; Energy tidak dihasilkan kecuali ada `SolarPanel` di scene. Angka perlu playtest [?].
- Satu `FarmingPlot` = satu petak; petak tambahan dibuat dengan menduplikasi objek. Pertumbuhan hanya terjadi lewat `TimeManager.OnDayStarted` (jadi lewat Bed atau menu Debug TimeManager) [?].

- Milestone 10 (2026-11-16): kode ditulis tanpa compiler/Unity [?]. Belum diverifikasi: kompilasi, menu `Setup Panel Damage and Wiring Minigame`, tampilan dan hit-test drag-and-drop pada Canvas (Scale With Screen Size), jeda timer/oksigen suit selama minigame, durasi nyata 15-30 detik, urutan dialog TARS saat panel rusak. Frekuensi kerusakan (1 per hari mulai Day 2) mungkin tinggi untuk balancing.

## Housekeeping
- `phase-sync.patch` ter-commit di root repo (`interactive-objects.patch` sudah dihapus di `b09da2e3`). File `.patch` tidak perlu ikut repo; hapus dan tambahkan `*.patch` ke `.gitignore`.
- `Assets/_Recovery/0.unity` masih ter-commit [Repo].
- Ada `Assets/popopop.meta` tanpa folder/aset pasangannya.
- `.gitignore` belum mengecualikan `.vscode/`.

## Aturan update
Cantumkan tanggal, bukti file/scene, hasil tes, dan hal yang belum diverifikasi. Jangan tandai Selesai hanya karena script ada. Pembaruan wajib ikut di setiap patch (lihat `04_CODE_RULES.md`).
