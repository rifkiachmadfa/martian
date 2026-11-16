# Changelog

Catat perubahan penting pada kode, desain, dokumentasi, dan keputusan. Entri baru di paling atas. Wajib diperbarui di setiap patch.

Format:
## YYYY-MM-DD
- **Changed:** perubahan yang dilakukan.
- **Reason:** alasan.
- **Files/systems:** area terdampak.
- **Test:** pengujian dan hasil.
- **Follow-up:** pekerjaan tersisa.

## 2026-11-16 (Konsumsi oksigen habitat)
- **Changed:** Script baru `HabitatOxygenDrain`: oksigen habitat (`HabitatOxygen`) berkurang 0,05 per detik (USULAN) tiap tick 1 detik selama gameplay berjalan, berhenti saat `TimeManager.IsPaused`. Komponen dibuat otomatis saat game mulai bila belum ada di scene (boleh dipasang manual). Event `OnHabitatOxygenDepleted` dipicu sekali saat stok 0 (hanya log).
- **Reason:** Laporan developer 2026-11-16: oksigen habitat tidak berkurang. Analisis: tidak ada kode yang mengurangi `HabitatOxygen` selain pengisian suit, dan generator mengisinya sampai penuh. Developer menyetujui konsumsi pasif (revisi GDD; GDD v1.2 hanya melarang pengurangan pasif Water dan Energy).
- **Files/systems:** `Assets/Script/Survival/HabitatOxygenDrain.cs` (baru, `.meta` dibuat Unity saat import), `AI_CONTEXT/{08_DECISIONS,09_TEST_CHECKLIST,10_CHANGELOG}.md`. Scene, prefab, dan aset biner tidak diubah.
- **Test:** `git apply --check` terhadap klon bersih `ca830881`. Tidak dikompilasi dan tidak dijalankan di Unity (tidak ada compiler/Unity di lingkungan AI). Daftar tes baru di `09_TEST_CHECKLIST.md`.
- **Follow-up:** Game Over oksigen habitat kritis (`13.2.3`) dan peringatan TARS untuk oksigen habitat rendah belum dibuat; laju 0,05/detik perlu playtest (bersama keseimbangan Energy generator); simpan state di SaveSystem tidak diperlukan (tidak ada state baru); GDD docx 6.3, 7.2, 8 perlu direvisi developer.

## 2026-11-16 (Tas pemain, storage, bibit; makan dari tas)
- **Changed:** Tas pemain baru (`InventorySystem`): Kentang maks 10, Es maks 10, Bibit maks 5. Tombol 1/2/3 memilih slot (tekan lagi = batal), E memakai item. `InteractionDetector` kini memilih aksi: item pada objek penerima (`IItemReceiver`), aksi bawaan item (makan kentang saat lapar), lalu interaksi biasa; `InteractionPromptUI` membaca `detector.CurrentPrompt`. `FarmingPlot` tanam butuh Bibit dari tas, panen memasukkan 4 kentang (+1 bibit) ke tas dan ditunda bila tas tidak muat. `IceDeposit` mengisi tas Es (ditolak bila penuh). `WaterPurifier` menerima Es dari tas (pilih slot 2, tekan E). `FoodStorage` kini storage: membuka `StorageUI` (klik mouse: Simpan/Ambil 1 atau semua). Kentang di storage = stok Food di HUD; es dan bibit di storage disimpan `InventorySystem`. `HungerSystem.Eat` diganti `Feed`; 1 kentang = +12,5 kenyang. Baru: `InventoryHotbarUI`, `StorageUI`, `ItemType`, `IItemReceiver`, menu `Mars > Setup Inventory and Storage`. `HudUI` tidak lagi menampilkan Ice dari ResourceManager; `ResourceMonitor` menampilkan isi tas dan storage.
- **Reason:** Permintaan developer 2026-11-16: pemain harus punya tas (bukan storage); item penuh tidak bisa diambil dan harus disimpan di storage. Jawaban developer: Food HUD = isi storage; bibit (bukan pupuk) di slot 3.
- **Files/systems:** `Assets/Script/Inventory/{ItemType,InventorySystem}.cs`, `Assets/Script/Interaction/{IItemReceiver,InteractionDetector,InteractionPromptUI}.cs`, `Assets/Script/UI/{InventoryHotbarUI,StorageUI,HudUI}.cs`, `Assets/Script/Station/{FoodStorage,ResourceMonitor}.cs`, `Assets/Script/Survival/{HungerSystem,HungerReminder}.cs`, `Assets/Script/Outdoor/IceDeposit.cs`, `Assets/Script/Machines/WaterPurifier.cs`, `Assets/Script/Farming/FarmingPlot.cs`, `Assets/Editor/{MarsInventorySetup,MarsHungerSetup}.cs`, `AI_CONTEXT/*`. `ResourceType.Ice` tidak dipakai gameplay lagi (enum tidak diubah). Scene diubah lewat menu Editor, bukan oleh patch.
- **Test:** `git apply --check` terhadap klon bersih (basis `d86f3c28` + patch suit + patch hunger). Tidak dikompilasi dan tidak dijalankan di Unity (tidak ada compiler/Unity di lingkungan AI). Daftar tes di `09_TEST_CHECKLIST.md`.
- **Follow-up:** Developer menjalankan `Mars > Setup Inventory and Storage` dan mengubah Food di Inspector ResourceManager (Start 10, Max 50, usulan). Angka (panen 4 kentang +1 bibit, kapasitas storage es 30 / bibit 20, bibit awal 5) perlu playtest. Revisi GDD docx: bagian 3.1 (storage), 9.1, 10 (bibit dan tas), 13.3 (tombol 1-3), 21. Tas dan storage belum masuk SaveSystem. Day 1 tutorial perlu menjelaskan tas.

## 2026-11-16 (Hunger dan makan; perbaikan tombol Suit Station)
- **Changed:** (1) Sistem hunger baru: `HungerSystem` (kenyang 0-100, berkurang pelan selama gameplay, `Eat()` memakai Food), `HungerReminder` (dialog TARS teks tetap di batas 30%, 10%, dan 0), `FoodStorage` (IInteractable "Makan" di Control Room), menu Editor `Mars > Setup Hunger and Food Storage` (`MarsHungerSetup`). `HudUI` mendapat `hungerText` opsional; `ResourceMonitor` menampilkan baris Kenyang. Stok Food tidak lagi direncanakan berkurang sendiri; Food berkurang hanya saat pemain makan. (2) `SuitStation`: tombol hanya "Kenakan suit" / "Lepas suit"; tabung diisi otomatis dari oksigen habitat saat suit dikenakan dan saat dilepas (tombol "Isi oksigen suit" dihapus; bug suit tidak bisa dilepas karena `CanRefill` hampir selalu true).
- **Reason:** (1) Food belum punya fungsi konsumsi: tidak ada hunger dan tidak ada cara memakan Food (GDD 7.4, 8: Food memenuhi kebutuhan makan, kelaparan bisa game over). Permintaan developer 2026-11-16. (2) Laporan bug developer 2026-11-16.
- **Files/systems:** `Assets/Script/Survival/{HungerSystem,HungerReminder}.cs` (baru), `Assets/Script/Station/FoodStorage.cs` (baru), `Assets/Editor/MarsHungerSetup.cs` (baru), `Assets/Script/UI/HudUI.cs`, `Assets/Script/Station/ResourceMonitor.cs`, `Assets/Script/Suit/SuitStation.cs`, `AI_CONTEXT/*`. File `.meta` baru dibuat Unity saat import. Scene diubah developer lewat menu Editor, bukan oleh patch.
- **Test:** `git apply --check` terhadap klon bersih `d86f3c28` (+ patch suit). Tidak dikompilasi dan tidak dijalankan di Unity (tidak ada compiler/Unity di lingkungan AI). Daftar tes baru di `09_TEST_CHECKLIST.md`.
- **Follow-up:** Game Over kelaparan belum dibuat (hanya event `OnStarvationFailure`); simpan Satiety di SaveSystem nanti; GDD docx 7.4, 8, 3.1, 13.1 perlu direvisi developer; Day 1 tutorial perlu menjelaskan makan; angka hunger perlu playtest; `TARSSystem` mengambil alih `HungerReminder`.

## 2026-11-16 (Milestone 10: kerusakan panel acak dan minigame kabel)
- **Changed:** Script baru `SolarPanelDamageScheduler` (satu kerusakan acak per hari), `WiringMinigameUI` (minigame kabel drag-and-drop), `Assets/Editor/MarsRepairSetup.cs` (menu `Mars > Setup Panel Damage and Wiring Minigame`). `RepairPoint` kini membuka minigame; `SolarPanel.Repair()` dipanggil hanya setelah sukses. Perbaikan langsung tetap sebagai cadangan bila minigame tidak ada di scene.
- **Reason:** Milestone 10 (GDD 11.3, 11.4). Keputusan developer 2026-11-16: pemicu kerusakan = event acak, hanya 1x per hari.
- **Files/systems:** `Assets/Script/Outdoor/{SolarPanelDamageScheduler,RepairPoint}.cs`, `Assets/Script/UI/WiringMinigameUI.cs`, `Assets/Editor/MarsRepairSetup.cs` (+ `.meta` baru), `AI_CONTEXT/*`. `SolarPanel`, `TimeManager`, scene, dan aset biner tidak diubah oleh patch (scene diubah lewat menu Editor oleh developer).
- **Test:** Belum dikompilasi atau dijalankan di Unity (tidak ada Unity/compiler di lingkungan AI). Daftar tes ada di `09_TEST_CHECKLIST.md`.
- **Follow-up:** Developer menjalankan menu setup, tes, lalu menyimpan scene. Angka usulan (Day 2 mulai, 15-70 persen hari, 4 kabel) perlu playtest. `TARSSystem` mengambil alih pengumuman panel rusak.

## 2026-11-16 (checklist dan PHASE.md sesuai hasil tes developer)
- **Changed:** `09_TEST_CHECKLIST.md` bagian "Objek interaktif": seluruh butir lama ditandai [x] (dilaporkan developer), instruksi setup dihapus, dua butir baru (BARU) ditambahkan, butir Repair Point disesuaikan dengan kode. `PHASE.md`: revisi `d96d9b6f`; milestone 6–9 menjadi SELESAI* (dilaporkan); bagian 5 ditulis ulang; bagian 6 memuat usulan urutan; utang teknis diperbarui. `06`, `07`, `05` disesuaikan.
- **Reason:** developer melaporkan tes interaksi lolos semua; dokumen masih menyebut belum dites. Pemeriksaan kode mendapati angka dan perilaku di checklist cocok dengan script.
- **Files/systems:** `AI_CONTEXT/{PHASE,05_SCENES_ASSETS,06_PROJECT_STATUS,07_BACKLOG,09_TEST_CHECKLIST,10_CHANGELOG}.md`. Tidak ada kode, scene, atau aset yang berubah.
- **Test:** Dicek dari clone bersih `main` (`d96d9b6f`): GUID script objek interaktif tidak ada di `SampleScene.unity`; default angka dibaca dari script. Hasil tes Unity berasal dari laporan developer, tidak diverifikasi AI.
- **Follow-up:** Commit scene lokal; putuskan P-1, P-5, dan fase berikutnya; hapus `phase-sync.patch` dan tambahkan `*.patch` serta `Assets/_Recovery/` ke `.gitignore`.

## 2026-11-15 (sinkronisasi PHASE.md dengan repo afc2453c)
- **Changed:** `PHASE.md` diperbarui ke revisi `afc2453c`: patch objek interaktif dicatat sebagai sudah diterapkan/di-commit; ditambahkan catatan bahwa scene commit belum memuat objek barunya; utang teknis `interactive-objects.patch` dan `Assets/_Recovery` dicatat; riwayat ditambah. `06_PROJECT_STATUS.md` disesuaikan (commit dasar audit, kondisi scene, housekeeping).
- **Reason:** `PHASE.md` masih menyebut patch belum diterapkan dan revisi `a9357e29`, tidak sesuai kondisi repo.
- **Files/systems:** `AI_CONTEXT/PHASE.md`, `AI_CONTEXT/06_PROJECT_STATUS.md`, `AI_CONTEXT/10_CHANGELOG.md`. Tidak ada kode, scene, atau aset yang berubah.
- **Test:** Fakta dicek dari clone bersih `main` (`git show --stat`, isi `SampleScene.unity`, `TagManager.asset`, `git ls-files`). Kompilasi dan run Unity: BELUM.
- **Follow-up:** Developer menjalankan `Mars > Setup Interactive Objects`, menyimpan dan meng-commit scene; hapus `interactive-objects.patch`; tambahkan `_Recovery/` ke `.gitignore`; lanjut tes checklist "Objek interaktif".

## 2026-11-15 (Objek interaktif: Ice, Purifier, Oxygen Generator, Farming Plot, Solar Panel + Repair Point, Resource Monitor, Terminal)
- **Changed:** Menambah script `IceDeposit`, `SolarPanel`, `RepairPoint` (Outdoor), `WaterPurifier`, `OxygenGenerator` (Machines), `FarmingPlot` (Farming), `ResourceMonitor`, `CommunicationTerminal` (Station), dan menu Editor `Mars > Setup Interactive Objects` (`MarsObjectsSetup`). Semua `IInteractable`; stok hanya lewat `ResourceManager`. `HudUI`: `iceText` opsional (menampilkan Ice). Perilaku (angka = USULAN): Ice 3 detik per +1 (perlu suit); Purifier 1 Ice + 5 Energy -> 4 Water dalam 4 detik; Oxygen Generator otomatis 1 Water + 2 Energy -> 5 Oxygen per 5 detik dengan saklar E; Farming Plot 3 tahap, siram 2 Water, tumbuh saat hari berganti, panen +20 Food; Solar Panel 0,5 Energy/detik (rusak 25 persen); Repair Point memperbaiki langsung (sementara); Resource Monitor dan Terminal hanya dialog (teks Terminal placeholder). Ditambah file `.meta` baru untuk 9 script dan 4 folder.
- **Reason:** Permintaan developer menyelesaikan seluruh daftar "Objek interaktif baru" (GDD 3.1 sampai 3.3, 10, 11).
- **Files/systems:** `Assets/Script/{Outdoor,Machines,Farming,Station}/*` (baru), `Assets/Editor/MarsObjectsSetup.cs` (baru), `Assets/Script/UI/HudUI.cs` (diubah); AI_CONTEXT `README`, `PHASE`, `03`, `05`, `06`, `07`, `08`, `09`, `10`. Scene diubah developer lewat menu Editor dan penempatan manual, bukan oleh patch.
- **Test:** `git apply --check` terhadap klon bersih `a9357e29`. TIDAK dikompilasi dan TIDAK dijalankan di Unity (tidak ada compiler C# atau Unity di lingkungan AI); kode hanya diperiksa dengan membaca. Semua butir di `09_TEST_CHECKLIST.md` bagian "Objek interaktif" berstatus belum dites.
- **Follow-up:** Keputusan terbuka: saklar Oxygen Generator (di luar GDD 11.2), deposit es tak terbatas, pemicu kerusakan Solar Panel. Belum ada: minigame kabel (Repair Point sementara langsung), `TARSSystem` (laporan panel rusak dan reminder resource), Food berkurang, Victory/Game Over, save, sprite sungguhan. Water awal bisa habis cepat bila Oxygen Generator tidak dimatikan; perlu playtest.

## 2026-11-15 (Dialog TARS: waktu tetap berjalan + input lanjut/tutup)
- **Changed:** (1) `TimeManager`: dialog tidak lagi menjeda timer; flag baru `pauseDuringDialogue` (default mati) mengembalikan perilaku lama. Efeknya juga ke sistem yang mengikuti `IsPaused` (oksigen suit tetap berkurang saat dialog). `ExternalPause` (transisi tidur) tidak berubah. (2) `DialogueUI`: lanjut/tutup dengan E, Spasi, atau klik kiri di mana saja; field baru `closeButton` (tombol X, menutup langsung, klik pada tombol tidak terhitung ganda), `alternateAdvanceKey`, `clickAnywhereToAdvance`. (3) `MarsUISetup`: langkah baru yang membuat `DialogueRoot/CloseButton` dan menyambungkannya, tanpa menimpa tombol yang sudah ada. Komentar `SuitSystem` disesuaikan.
- **Reason:** Permintaan developer: waktu dan resource harus tetap berjalan saat UI TARS tampil; dialog perlu input lebih mudah (Spasi, klik, tombol keluar).
- **Files/systems:** `Assets/Script/Time/TimeManager.cs`, `Assets/Script/Dialogue/DialogueUI.cs`, `Assets/Script/Suit/SuitSystem.cs` (komentar), `Assets/Editor/MarsUISetup.cs`; AI_CONTEXT `03`, `05`, `06`, `08`, `09`, `10`. Scene: tombol X dibuat lewat menu Editor oleh developer, bukan oleh patch.
- **Test:** `git apply --check` terhadap keadaan "HEAD `bcf3a834` + patch v2 + v3". Belum dikompilasi atau dijalankan di Unity. Butir tes TimeManager lama \"dialog menjeda timer\" diubah dan ditandai belum dites.
- **Follow-up:** Konflik GDD yang dicatat di `08_DECISIONS.md` (5.4 dan 20) perlu direvisi developer. Teks dialog sebaiknya singkat karena membaca memakan waktu hari. Esc untuk menutup dialog belum dipakai (menunggu menu pause). Player masih bisa bergerak saat dialog terbuka.

## 2026-11-15 (Revisi GDD: oksigen suit berkurang saat dikenakan + lepas suit)
- **Changed:** Keputusan developer: oksigen suit berkurang selama suit dikenakan, di dalam maupun di luar habitat (revisi GDD v1.2 bagian 7.2/9). `SuitSystem`: konsumsi tidak lagi bergantung pada `IsOutdoors`; field `outdoorDrainPerSecond` menjadi `drainPerSecond` (`FormerlySerializedAs`, nilai Inspector terbawa); menambah `Unequip()` (hanya di dalam habitat) dan `CanRefill`; `OnSuitOxygenEmpty` hanya dipicu bila kosong saat di luar. `SuitStation`: E bergantung keadaan (kenakan + isi / isi ulang / lepas suit), prompt dinamis. Komentar `IndoorZone` disesuaikan. Tambahan: `Bed` melepas suit otomatis saat tidur (setelah layar hitam; field opsional `suit`, dicari otomatis). `SuitReminder` (baru): dialog TARS "lepas suit" bila suit dikenakan di dalam habitat 30 detik, lalu cooldown 90 detik (usulan). `MarsSuitSetup` memasang `SuitReminder` pada objek `SuitSystem`.
- **Reason:** Menambah keputusan kapan memakai suit; pemain harus melepas suit saat tidak ke luar.
- **Files/systems:** `Assets/Script/Suit/SuitSystem.cs`, `SuitStation.cs`, `IndoorZone.cs`, `SuitReminder.cs` (baru); `Assets/Script/Interaction/Bed.cs`, `Assets/Editor/MarsSuitSetup.cs`; AI_CONTEXT `02`, `03`, `05`, `06`, `07`, `08`, `09`, `10`. Dokumen GDD docx BELUM direvisi (tugas developer; daftar bagian ada di jawaban chat).
- **Test:** `git apply --check` terhadap keadaan "HEAD `bcf3a834` + patch v2". Belum dikompilasi/dijalankan di Unity.
- **Follow-up:** Peringatan oksigen suit 30%/10% (menunggu TARSSystem); Day 1 tutorial menjelaskan melepas suit; balancing konsumsi (usulan 1/detik) perlu playtest karena kini terpakai di dalam habitat.

## 2026-11-15 (Milestone A: Suit, Suit Station, gerbang keluar)
- **Changed:** Menambah `SuitSystem` (status suit, isi ulang dari HabitatOxygen, oksigen suit berkurang saat pemain di luar habitat), `SuitStation` (IInteractable: kenakan suit + isi oksigen), `IndoorZone` (trigger area dalam habitat), `SuitGate` (tembok di pintu keluar yang menahan pemain tanpa suit/oksigen minimum, TARS menjelaskan alasannya), dan menu Editor `Mars > Setup Suit and Gate` (`MarsSuitSetup`). `HudUI`: panel oksigen suit opsional (tampil hanya saat suit dikenakan; kuning/merah di bawah 30%/10%). Penentuan "di luar" = tidak berada di IndoorZone mana pun. Oksigen suit berhenti berkurang saat dialog/transisi tidur (mengikuti `TimeManager.IsPaused`).
- **Reason:** Jadwal GDD hari 5 (Astronaut Suit, Suit Station, konsumsi Oxygen di Outdoor). Syarat untuk Ice gathering dan Solar Panel di Outdoor.
- **Files/systems:** `Assets/Script/Suit/{SuitSystem,SuitStation,IndoorZone,SuitGate}.cs`, `Assets/Editor/MarsSuitSetup.cs` (baru); `Assets/Script/UI/HudUI.cs` (diubah). Scene diubah developer lewat menu Editor dan penempatan manual (IndoorZone, SuitGate, SuitStation), bukan oleh patch.
- **Test:** `git apply --check` pada klon bersih HEAD `bcf3a834`. Kompilasi dan run di Unity: BELUM (tidak ada Unity/compiler C# di lingkungan AI). Semua butir di `09_TEST_CHECKLIST.md` bagian Suit berstatus belum dites.
- **Follow-up:** Countdown darurat dan Game Over saat oksigen suit habis (event `OnSuitOxygenEmpty` sudah ada, belum dipakai); grace period timer habis di luar; reminder TARS 30%/10% (menunggu TARSSystem); Ice gathering; suit belum bisa dilepas dan belum direset saat tidur (belum diputuskan). Angka (minimum keluar 20, konsumsi 1/detik) adalah USULAN.

## 2026-11-15 (Milestone 3: Bed, tidur, pergantian hari)
- **Changed:** Menambah `Bed` (IInteractable), `FadeUI`, dan menu Editor `Mars > Setup Bed and Fade` (`MarsSleepSetup`). Alur tidur: kunci gerakan, timer ditahan, fade to black, `TimeManager.StartDay(hari+1)`, Player dipindah ke SpawnPoint, fade in. `PlayerMovement`: properti `MovementEnabled`. `TimeManager`: properti `ExternalPause`. Aturan usulan: tidur hanya boleh saat sisa waktu <= 120 detik atau waktu habis; sebelum itu muncul pesan "Belum waktunya tidur". Hari terakhir: hanya pesan placeholder (Victory belum dibuat).
- **Reason:** Menjalankan pergantian hari sesuai GDD 5.4 dan menutup loop harian. Jadwal GDD hari 4 (bagian hari).
- **Files/systems:** `Assets/Script/Interaction/Bed.cs`, `Assets/Script/UI/FadeUI.cs`, `Assets/Editor/MarsSleepSetup.cs` (baru); `Assets/Script/PlayerMovement.cs`, `Assets/Script/Time/TimeManager.cs` (diubah kecil). Scene diubah lewat menu Editor oleh developer, bukan oleh patch.
- **Test:** Kompilasi dan logika (tidur ditolak terlalu awal, gerakan terkunci, pause, hari naik, timer reset, pindah ke spawn, hari terakhir tidak naik) dicek dengan stub di luar Unity. Belum dijalankan di Unity. Milestone 2 dilaporkan developer berjalan baik ("sudah bagus").
- **Follow-up:** Keputusan desain terbuka: aturan kapan boleh tidur (lihat 08). Belum ada: save ke disk, Victory/Game Over, efek pertumbuhan tanaman saat tidur, dialog TARS pergantian hari, sprite Bed.

## 2026-11-15 (Milestone 2: TimeManager dan HUD)
- **Changed:** Menambah `TimeManager` (Day 1-7, timer 600 detik per hari, berhenti saat dialog terbuka, event `OnDayStarted` dan `OnDayTimeEnded`, method `StartDay`), `HudUI` (Day, timer, Food, Water, Oxygen habitat, Energy), dan menu Editor `Mars > Setup HUD and Time` (`MarsHudSetup`). `MarsUISetup`: empat helper `private` menjadi `internal` agar dipakai ulang (perilaku tidak berubah). Koreksi dokumen: jumlah commit repo 4, bukan 5.
- **Reason:** Alur hari dan tampilan status, jadwal GDD hari 4. Syarat untuk milestone bed/tidur dan reminder TARS.
- **Files/systems:** `Assets/Script/Time/TimeManager.cs`, `Assets/Script/UI/HudUI.cs`, `Assets/Editor/MarsHudSetup.cs` (baru); `Assets/Editor/MarsUISetup.cs` (4 kata); scene diubah lewat menu Editor oleh developer, bukan oleh patch.
- **Test:** Kompilasi semua skrip terkait dan logika TimeManager (hitung mundur, event sekali, StartDay, batas hari) dicek dengan stub di luar Unity. Belum dikompilasi atau dijalankan di Unity. Milestone 1 dites developer: lolos.
- **Follow-up:** Milestone 3: bed, tidur, fade, pergantian hari (memakai `StartDay`). Warna peringatan timer, grace period outdoor, dan pause menu belum ada.

## 2026-11-15 (Milestone 1: ResourceManager)
- **Changed:** Menambah `ResourceManager` (stok Ice, Water, HabitatOxygen, SuitOxygen, Energy, Food + event `OnResourceChanged`), enum `ResourceType`, dan `ResourceDebugPanel` (alat uji sementara). Keputusan: seluruh game memakai satu scene.
- **Reason:** Fondasi semua sistem lain (HUD, mesin, suit, farming, TARS, save). Jadwal GDD hari 3.
- **Files/systems:** `Assets/Script/Resource/*` (3 file baru). Scene belum diubah oleh patch; komponen dipasang manual di Unity.
- **Test:** Sintaks dan logika dicek dengan stub di luar Unity. Playtest di Unity: dilaporkan developer berjalan baik (2026-11-15).
- **Follow-up:** Milestone 2: HUD dan TimeManager. Angka awal/kapasitas masih usulan.

## 2026-11-15 (dokumentasi)
- **Changed:** AI_CONTEXT diperbarui sesuai kondisi repo (commit `e6feb586`); ditambah aturan wajib sinkronisasi AI_CONTEXT di setiap patch.
- **Reason:** status lama masih template, dan dokumentasi harus selalu mengikuti perubahan.
- **Files/systems:** `README.md`, `03`, `04`, `05`, `06`, `07`, `08`, `09`, `10`.
- **Test:** isi dicocokkan dengan hasil audit repo (script, scene, aset, versi Unity). Tidak ada kode yang berubah.
- **Follow-up:** commit `SampleScene.unity` dan layer `Interactable` agar status scene bisa diverifikasi; hapus file `.patch` di root.

## 2026-11-15 (fitur)
- **Changed:** Menambah sistem interaksi modular, TARS berjalan acak, dialog TARS, dan menu Editor setup UI.
- **Reason:** Tahap awal TARS dan fondasi interaksi untuk objek lain (bed, mesin, plot).
- **Files/systems:** `Assets/Script/Interaction/*`, `Assets/Script/Dialogue/DialogueUI.cs`, `Assets/Script/NPC/TarsWanderer.cs`, `Assets/Editor/MarsUISetup.cs`, `Assets/Robot/*` (sprite).
- **Test:** `git apply --check` berurutan lolos pada klon bersih. Fungsi di Unity: dilaporkan developer berjalan (TARS jalan acak, dialog muncul, TARS berhenti saat dialog). Belum dikompilasi atau dijalankan oleh AI.
- **Follow-up:** sprite arah bawah TARS; pause timer saat dialog (menunggu TimeManager); dialog berbasis resource (menunggu ResourceManager).
