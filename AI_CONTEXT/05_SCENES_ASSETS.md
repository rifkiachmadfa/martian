# Scenes & Assets

Terakhir diperbarui: 2026-11-15 (commit `e6feb586`). Label: [Repo] terverifikasi, [Dev] dilaporkan developer.

## Area desain yang direncanakan
- Control Room
- Farming Area
- Outdoor

## Scene yang ada
| Scene | Isi | Sumber |
|---|---|---|
| `Assets/Scenes/SampleScene.unity` (satu-satunya di Build Settings) | Main Camera, CinemachineCamera, Global Light 2D, Player, Grid (Ground, Wall, PasirMars, Tangga) | [Repo] |
| `SampleScene` versi lokal | Tambahan: TARS, TarsArea, Canvas (DialogueRoot, PromptRoot), EventSystem; Player memakai InteractionDetector | [Dev], belum di-commit |
| Scene area Control Room / Farming / Outdoor | Belum dibuat | [Repo] |

Scene lain di repo (`URP2DSceneTemplate`, demo asset pack `COPY SPRIGHT`) bukan bagian game.

## Script
| Path | Fungsi |
|---|---|
| `Assets/Script/PlayerMovement.cs` | Gerak 4 arah, arah terakhir, parameter Animator `IsMoving`, `DirectionX`, `DirectionY` |
| `Assets/Script/Interaction/IInteractable.cs` | Kontrak: `InteractionPrompt`, `CanInteract`, `Interact` |
| `Assets/Script/Interaction/InteractionDetector.cs` | Di Player. Cari objek interaktif terdekat (OverlapCircle + layer), tombol E |
| `Assets/Script/Interaction/InteractionPromptUI.cs` | Menampilkan "[E] ..." (UI Text legacy) |
| `Assets/Script/Dialogue/DialogueUI.cs` | Kotak dialog; E untuk lanjut baris; callback saat selesai |
| `Assets/Script/NPC/TarsWanderer.cs` | TARS acak di `BoxCollider2D` area; state Idle/Moving/Interacting; sprite per arah lewat array Inspector |
| `Assets/Editor/MarsUISetup.cs` | Menu Editor: membuat/memperbaiki Canvas, dialog, prompt, dan menyambungkan ke TARS/Player |
| `Assets/Welcome/Welcome2DScript.cs` | Bawaan template Unity; bukan bagian game |

## Aset
| Path | Isi |
|---|---|
| `Assets/Player/` | Sprite pemain (depan, atas), `Down.anim`, `Top/up.anim`, `Square.controller` |
| `Assets/Robot/` | Sprite TARS: `Left/` (3), `Right/` (3), `Up/` (3). **Folder Down belum ada** |
| `Assets/COPY SPRIGHT/` | Asset pack astronot (demo scene) |

## Setup objek di scene (agar bisa direproduksi)
**TARS:** SpriteRenderer, Rigidbody2D (Dynamic, Gravity Scale 0, Freeze Rotation Z), BoxCollider2D (bukan trigger), layer `Interactable`, komponen `TarsWanderer` (isi Wander Area, Dialogue UI, array frame per arah).
**TarsArea:** GameObject dengan BoxCollider2D, Is Trigger dicentang.
**Player:** komponen `InteractionDetector` dengan Interactable Layer = `Interactable`.
**Canvas:** dibuat lewat menu Mars > Setup Dialogue and Prompt UI (berisi `DialogueUI` dan `InteractionPromptUI`).

## Saat menambah/mengubah asset
- Pertahankan pasangan `.meta` dan GUID.
- Jangan menghapus atau mengganti asset yang mungkin direferensikan scene/prefab tanpa pemeriksaan.
- Berikan langkah Inspector secara eksplisit: GameObject, komponen, field, nilai, dan referensi yang perlu diisi.
- Untuk perubahan binary/serialized yang tidak aman dibuat lewat patch teks, jelaskan langkah manual.
