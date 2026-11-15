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
| Timer harian | Usulan: 600 detik/hari, 7 hari (Inspector `TimeManager`). Timer berhenti saat dialog terbuka. Saat waktu habis: berhenti di 0 + event; pergantian hari lewat bed (milestone berikutnya) | Usulan sementara, belum final |
| HUD | Menampilkan Day, timer, Food, Water, Oxygen habitat, Energy (GDD 13.1). Ice dan Suit Oxygen menyusul bersama suit/outdoor. UI dibuat lewat menu Editor | Diterapkan 2026-11-15 |
| Aturan tidur | USULAN (butuh persetujuan): tidur hanya jika sisa waktu <= 120 detik atau waktu habis; sebelum itu pesan "Belum waktunya tidur". Alasan: Food berkurang seiring waktu, sehingga tidur langsung di awal hari bisa memotong tantangan survival. Diatur lewat `earliestSleepRemainingSeconds` di Bed (isi 600 agar boleh kapan saja). GDD tidak menetapkan | Usulan sementara, terbuka |
| Hari terakhir | Bed pada Day 7 hanya menampilkan pesan placeholder, hari tidak naik. Trigger Victory (timer habis atau tidur) belum diputuskan | Terbuka |
| Pause timer | `TimeManager.IsPaused` = dialog terbuka ATAU `ExternalPause` (dipakai transisi tidur; minigame nanti) | Diterapkan 2026-11-15 |
| Arsitektur final | Belum diputuskan; mengikuti kebutuhan tiap milestone | Terbuka |
| Angka balancing | Belum final | Terbuka |

Format keputusan baru:
- Tanggal:
- Konteks/masalah:
- Opsi:
- Keputusan:
- Alasan:
- Dampak file/sistem:
