# Changelog

Catat perubahan penting pada kode, desain, dokumentasi, dan keputusan. Entri baru di paling atas. Wajib diperbarui di setiap patch.

Format:
## YYYY-MM-DD
- **Changed:** perubahan yang dilakukan.
- **Reason:** alasan.
- **Files/systems:** area terdampak.
- **Test:** pengujian dan hasil.
- **Follow-up:** pekerjaan tersisa.

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
