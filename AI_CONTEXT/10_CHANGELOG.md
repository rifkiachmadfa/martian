# Changelog

Catat perubahan penting pada kode, desain, dokumentasi, dan keputusan. Entri baru di paling atas. Wajib diperbarui di setiap patch.

Format:
## YYYY-MM-DD
- **Changed:** perubahan yang dilakukan.
- **Reason:** alasan.
- **Files/systems:** area terdampak.
- **Test:** pengujian dan hasil.
- **Follow-up:** pekerjaan tersisa.

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
