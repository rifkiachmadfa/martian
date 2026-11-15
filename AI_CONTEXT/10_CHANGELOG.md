# Changelog

Catat perubahan penting pada kode, desain, dokumentasi, dan keputusan. Entri baru di paling atas. Wajib diperbarui di setiap patch.

Format:
## YYYY-MM-DD
- **Changed:** perubahan yang dilakukan.
- **Reason:** alasan.
- **Files/systems:** area terdampak.
- **Test:** pengujian dan hasil.
- **Follow-up:** pekerjaan tersisa.

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
