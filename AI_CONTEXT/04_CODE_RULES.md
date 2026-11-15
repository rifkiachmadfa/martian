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
