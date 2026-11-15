# Project Status

Terakhir diperbarui: 2026-11-15
Dasar audit: repo `rifkiachmadfa/martian`, branch `main`, commit `e6feb586` (3 commit: init, dialogue, ui).

Label sumber: **[Repo]** terverifikasi dari file di repo, **[Dev]** dilaporkan developer (belum bisa dicek AI), **[?]** belum diverifikasi.

## Tahap saat ini
Fondasi pemain dan NPC. Setara sekitar hari produksi 1–2 dari jadwal 15 hari: movement, sistem interaksi modular, dan TARS yang berjalan acak serta bisa diajak bicara. Belum ada sistem resource, waktu, atau mesin.

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
| Resource systems | Belum ada | [Repo] tidak ada script |
| Day flow / timer / sleep / ending | Belum ada | [Repo] tidak ada script |
| Suit / outdoor | Belum ada | [Repo] |
| Machines / solar / wiring minigame | Belum ada | [Repo] |
| Farming / Potato | Belum ada | [Repo] |
| TARS reminder berbasis resource | Belum ada | Dialog TARS saat ini teks tetap lewat Inspector |
| Save system | Belum ada | [Repo] |
| Tiga scene area (Control Room, Farming, Outdoor) | Belum ada | [Repo] hanya `SampleScene` |

## Kondisi scene (penting)
- **[Repo]** `SampleScene.unity` di GitHub hanya berisi: Main Camera, CinemachineCamera, Global Light 2D, Player, Grid (Ground, Wall, PasirMars, Tangga). Tidak ada referensi ke `TarsWanderer`, `DialogueUI`, `InteractionDetector`, atau `InteractionPromptUI`.
- **[Dev]** Scene lokal developer berisi tambahan: `TARS`, `TarsArea`, `Canvas` (DialogueRoot, PromptRoot), `EventSystem`, dan `InteractionDetector` di Player. Belum di-commit.
- **[Dev]** Layer `Interactable` dipakai oleh TARS. Layer ini belum ada di `TagManager.asset` di repo.

## Belum diverifikasi / risiko
- Isi Inspector TARS dan Player di scene lokal (belum di-commit).
- Tidak ada compile atau run Unity oleh AI; semua kode hanya diperiksa lewat `git apply --check`.
- Gambar arah bawah untuk TARS (`Down Frames`) belum ada.
- Collider Player di scene belum diperiksa.
- Player masih bisa bergerak saat dialog terbuka; timer harian belum ada untuk dijeda.

## Housekeeping
- Empat file `.patch` ter-commit di root repo (`tars_interaction`, `tars_dialogue`, `mars_ui_setup`, `mars_ui_setup_fix`). Tidak dipakai lagi; boleh dihapus atau di-ignore.
- Ada `Assets/popopop.meta` tanpa folder/aset pasangannya.
- `.gitignore` belum mengecualikan `.vscode/`.

## Aturan update
Cantumkan tanggal, bukti file/scene, hasil tes, dan hal yang belum diverifikasi. Jangan tandai Selesai hanya karena script ada. Pembaruan wajib ikut di setiap patch (lihat `04_CODE_RULES.md`).
