# Code & AI Workflow Rules

## Coding
- Gunakan C# yang kompatibel dengan versi Unity dan package aktual.
- Pertahankan gaya, namespace, naming, dan pola proyek yang sudah ada.
- Hindari overengineering, duplikasi state, magic numbers, dan perubahan luas tanpa alasan.
- Validasi null/reference Inspector dan lifecycle Unity bila relevan.
- Jangan mengubah Input API sebelum memeriksa konfigurasi Active Input Handling dan package.

## Patch-only workflow
1. Pastikan file dan isi yang akan diubah benar-benar tersedia.
2. Buat patch unified diff minimal, dengan path relatif dari root repository.
3. Jangan mengedit repository/GitHub secara langsung; jangan commit atau push.
4. Pengguna memeriksa dan menerapkan patch secara lokal.
5. Sertakan langkah manual Unity Editor untuk scene, prefab, Animator, Inspector, asset, atau referensi serialized yang tidak tercakup patch.

## Perintah pengguna
```bash
git apply --check changes.patch
git apply changes.patch
git diff
```
Rollback patch:
```bash
git apply -R changes.patch
```
Jelaskan dampaknya sebelum menyarankan perintah yang berisiko.

## Format jawaban teknis
Ringkasan → bukti/root cause → solusi → file terdampak → patch → langkah Unity Editor → cara tes → rollback.

## Sinkronisasi AI_CONTEXT (WAJIB)
Setiap patch yang mengubah kode, scene, aset, desain, atau keputusan **harus menyertakan pembaruan AI_CONTEXT di patch yang sama**. Patch tanpa pembaruan ini dianggap belum lengkap.

| Jenis perubahan | File AI_CONTEXT yang diperbarui |
|---|---|
| Perubahan apa pun | `10_CHANGELOG.md` (entri baru, selalu) dan `06_PROJECT_STATUS.md` (status, bukti, yang belum diverifikasi) |
| Script/scene/prefab/aset ditambah, diubah, dipindah | `05_SCENES_ASSETS.md` |
| Struktur kode, pola, atau ketergantungan antar sistem | `03_TECHNICAL_ARCHITECTURE.md` |
| Keputusan desain/teknis baru | `08_DECISIONS.md` |
| Fitur baru yang bisa dites | `09_TEST_CHECKLIST.md` |
| Urutan atau status pekerjaan berubah | `07_BACKLOG.md` |

Aturan isi:
- Tulis hanya fakta yang sudah diperiksa. Beri label sumber: **Terverifikasi di repo**, **Dilaporkan developer** (belum bisa dicek AI), atau **Belum diverifikasi**.
- Jangan menandai fitur "Selesai" hanya karena script ada. Perlu bukti tes di Unity.
- Perubahan scene/Inspector yang belum di-commit dicatat sebagai "Dilaporkan developer".
- Gunakan tanggal absolut (YYYY-MM-DD).
