# Decisions Log

Catat keputusan yang sudah disetujui. Jangan mengubah keputusan diam-diam.

| Topik | Keputusan saat ini | Status |
|---|---|---|
| Game scope | Mengikuti GDD v1.2 dan daftar Must/Should/Won't Have | Acuan |
| Hari permainan | 7 hari; target sekitar 10 menit per hari | Acuan desain |
| Tanaman | Potato saja | Acuan desain |
| Habitat Oxygen vs Suit Oxygen | Dua stok berbeda | Acuan desain |
| Unity version / packages | Unity 6000.6.8f1, URP 17.6.0, Input System 1.20.1 (Both), Cinemachine 6.6.0 | Terverifikasi 2026-11-15 |
| Input API | Memakai API lama (`Input.*`) selama mode Both | Berlaku |
| Arsitektur interaksi | `IInteractable` + detector di Player + prompt UI terpisah | Diterapkan 2026-11-15 |
| Dialog TARS | Teks tetap (predefined) dari Inspector; bukan AI generatif | Diterapkan 2026-11-15 |
| TARS bergerak | Rigidbody2D Dynamic, area jelajah BoxCollider2D trigger | Diterapkan 2026-11-15 |
| UI text | UnityEngine.UI.Text (legacy) untuk menghindari impor TMP Essentials | Sementara; boleh diganti TMP |
| Alur kerja AI | Patch-only; setiap patch wajib memperbarui AI_CONTEXT | Berlaku sejak 2026-11-15 |
| Struktur scene | Satu scene (`SampleScene`) untuk semua area (Control Room, Farming, Outdoor). Menggantikan daftar scene terpisah di GDD 16.1; gameplay tidak berubah. ResourceManager berupa komponen scene biasa tanpa `DontDestroyOnLoad` | Disetujui developer 2026-11-15 |
| Angka awal resource | Usulan: Ice 0/5, Water 10/30, HabitatOxygen 50/100, SuitOxygen 0/100, Energy 50/100, Food 50/100. Diubah lewat Inspector | Usulan sementara, belum final |
| Timer harian | Usulan: 600 detik/hari, 7 hari (Inspector `TimeManager`). Timer tetap berjalan saat dialog terbuka (lihat "Pause timer"). Saat waktu habis: berhenti di 0 + event; pergantian hari lewat bed (milestone berikutnya) | Usulan sementara, belum final |
| HUD | Menampilkan Day, timer, Food, Water, Oxygen habitat, Energy (GDD 13.1). Ice dan Suit Oxygen menyusul bersama suit/outdoor. UI dibuat lewat menu Editor | Diterapkan 2026-11-15 |
| Aturan tidur | USULAN (butuh persetujuan): tidur hanya jika sisa waktu <= 120 detik atau waktu habis; sebelum itu pesan "Belum waktunya tidur". Alasan: Food berkurang seiring waktu, sehingga tidur langsung di awal hari bisa memotong tantangan survival. Diatur lewat `earliestSleepRemainingSeconds` di Bed (isi 600 agar boleh kapan saja). GDD tidak menetapkan | Usulan sementara, terbuka |
| Hari terakhir | Bed pada Day 7 hanya menampilkan pesan placeholder, hari tidak naik. Trigger Victory (timer habis atau tidur) belum diputuskan | Terbuka |
| Deteksi "di luar" | USULAN: pemain dianggap di luar jika tidak berada di `IndoorZone` mana pun (trigger, boleh lebih dari satu). Dipakai oleh `SuitGate` dan larangan melepas suit di luar; TIDAK lagi menentukan konsumsi oksigen (lihat "Konsumsi oksigen suit") | Usulan sementara, terbuka |
| Gerbang keluar | USULAN: `SuitGate` = tembok solid di pintu yang aktif hanya saat pemain di dalam dan (suit belum dikenakan atau oksigen suit < minimum); mati otomatis saat pemain di luar agar tidak terjebak. TARS memberi alasan saat menabrak (cooldown 8 detik). GDD 9: "tidak dapat keluar tanpa suit atau oksigen minimum" | Usulan sementara, terbuka |
| Angka suit | USULAN: minimum oksigen suit untuk keluar 20, konsumsi 1 unit/detik selama suit dikenakan, kapasitas tabung 100 (dari ResourceManager). Dengan angka ini tabung penuh habis dalam sekitar 100 detik pemakaian (di dalam + di luar). Ubah lewat Inspector `SuitSystem` (`drainPerSecond`, nilai lama `outdoorDrainPerSecond` ikut terbawa) | Usulan sementara, belum final |
| Melepas suit | USULAN: tombol E di Suit Station: suit belum dikenakan = kenakan + isi; dikenakan dan tabung bisa diisi = isi ulang; dikenakan dan tidak ada yang bisa diisi (penuh atau stok habitat habis) = lepas suit. Hanya bisa di dalam habitat. Suit dilepas otomatis saat tidur (lihat baris berikutnya) | Usulan sementara, terbuka |
| Suit saat tidur | USULAN (disetujui developer 2026-11-15): `Bed` melepas suit setelah layar hitam. Oksigen tabung tetap. Hanya berhasil jika Bed berada di dalam `IndoorZone` | Diterapkan, belum dites |
| Pengingat "lepas suit" | USULAN (disetujui developer 2026-11-15): `SuitReminder` menampilkan dialog TARS (teks tetap) bila suit dikenakan di dalam habitat selama 30 detik, lalu cooldown 90 detik. Hitungan berhenti saat dialog/transisi tidur dan diulang saat suit dilepas atau pemain keluar. Sementara pengganti TARSSystem | Usulan sementara, belum final |
| Oksigen suit saat jeda | Tidak berkurang saat transisi tidur (mengikuti `TimeManager.IsPaused`); saat dialog terbuka tetap berkurang kecuali `pauseDuringDialogue` dicentang | Diterapkan 2026-11-15 |
| Konsumsi oksigen suit (REVISI GDD) | **2026-11-15, disetujui developer.** Konteks: GDD v1.2 (7.2, 9) menyatakan tabung suit hanya berkurang di Outdoor. Keputusan: tabung berkurang selama suit dikenakan, di dalam maupun di luar habitat, sehingga pemain perlu melepas suit saat tidak ke luar. Alasan: menambah keputusan kapan memakai suit. Dampak: GDD 7.2, 8 (tabel Oxygen), 9, 17, 21 dan instruksi proyek perlu direvisi oleh developer; Day 1 tutorial perlu menjelaskan melepas suit | Disetujui, GDD docx belum direvisi |
| Pause timer (REVISI) | **2026-11-15, disetujui developer:** dialog TARS/UI TIDAK menghentikan timer dan resource. `TimeManager.IsPaused` = `ExternalPause` (transisi tidur; minigame nanti) atau, hanya bila `pauseDuringDialogue` dicentang, dialog terbuka. Konteks: GDD 5.4 menyebut timer "dapat" berhenti saat dialog penting dan GDD 20 memakai pause dialog sebagai mitigasi timer terlalu menekan; keputusan ini membalik default itu. Dampak: waktu membaca dialog memakan waktu hari dan oksigen suit; teks dialog sebaiknya singkat; GDD 5.4 dan 20 perlu direvisi developer. Mudah dikembalikan lewat Inspector | Disetujui, GDD docx belum direvisi |
| Input dialog | E, Spasi, atau klik kiri di mana saja = lanjut/tutup; tombol Tutup = tutup langsung (melewati sisa baris). Esc belum dipakai | Diterapkan 2026-11-15, belum dites |
| Ice Deposit | USULAN: E memulai pengumpulan 3 detik (pemain terkunci), +1 Ice; batas = kapasitas Ice di ResourceManager (5); deposit tidak habis; perlu suit dikenakan. Pesan hanya saat gagal (suit belum dikenakan, inventory penuh) | Usulan sementara, terbuka |
| Water Purifier | USULAN: satu E = satu batch; 1 Ice + 5 Energy -> 4 Water dalam 4 detik; Ice dan Energy dikurangi di awal, Water masuk di akhir; ditolak jika Ice/Energy kurang atau tangki Water tidak muat 4 | Usulan sementara, belum final |
| Oxygen Generator | USULAN: otomatis per siklus 5 detik: 1 Water + 2 Energy -> 5 Oxygen habitat; berhenti sendiri jika Water/Energy kurang atau oksigen habitat penuh. **TAMBAHAN di luar GDD 11.2:** tombol E menyalakan/mematikan mesin agar pemain bisa menghemat Water untuk tanaman (GDD 7.1 membahas pembagian Water). Default menyala. Dapat dinonaktifkan (`allowManualToggle`). Dampak: tanpa dimatikan, Water awal bisa habis cepat | Usulan, BUTUH PERSETUJUAN |
| Farming Plot | USULAN: 3 tahap (tahap terakhir = matang), siram memakai 2 Water, panen +20 Food, benih gratis (tidak ada stok seed), panen ditunda jika Food penuh. Tanaman disiram maju satu tahap saat `TimeManager.OnDayStarted`; tidak disiram tidak maju dan tidak mati (GDD 10). Satu objek = satu petak | Usulan sementara, belum final |
| Solar Panel | USULAN: 0,5 Energy/detik per panel normal, panel rusak 25 persen dari itu (bukan nol), Energy ditambah tiap 1 detik. Pemicu kerusakan otomatis BELUM diputuskan (sementara hanya menu konteks Debug) | Usulan sementara, terbuka |
| Repair Point | Perbaikan langsung (E) sementara; minigame kabel (GDD 11.4) menggantikannya di milestone berikutnya dan memanggil `SolarPanel.Repair()` | Sementara |
| Resource Monitor | Dialog hanya-baca: hari/waktu, stok, suit, panel, mesin. Tidak memberi tugas | Diterapkan, belum dites |
| Terminal komunikasi | Pesan Bumi per hari (array Inspector). Teks bawaan = PLACEHOLDER. Bukan quest/objective. Konfirmasi penjemputan Day 7 milik ending Victory | Diterapkan, belum dites; teks menunggu developer |
| Penempatan objek | Objek dibuat placeholder oleh menu Editor di sekitar Player; developer memindahkan ke Control Room/Farming Area/Outdoor dalam satu scene (lihat keputusan "Struktur scene") | Berlaku |
| Arsitektur final | Belum diputuskan; mengikuti kebutuhan tiap milestone | Terbuka |
| Angka balancing | Belum final | Terbuka |

Format keputusan baru:
- Tanggal:
- Konteks/masalah:
- Opsi:
- Keputusan:
- Alasan:
- Dampak file/sistem:
