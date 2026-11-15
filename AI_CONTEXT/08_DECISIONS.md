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
| Arsitektur final | Belum diputuskan; mengikuti kebutuhan tiap milestone | Terbuka |
| Angka balancing | Belum final | Terbuka |

Format keputusan baru:
- Tanggal:
- Konteks/masalah:
- Opsi:
- Keputusan:
- Alasan:
- Dampak file/sistem:
