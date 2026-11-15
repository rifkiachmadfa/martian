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
| `Assets/Script/PlayerMovement.cs` | Gerak 4 arah, arah terakhir, parameter Animator `IsMoving`, `DirectionX`, `DirectionY`; `MovementEnabled` untuk mengunci gerak |
| `Assets/Script/Interaction/IInteractable.cs` | Kontrak: `InteractionPrompt`, `CanInteract`, `Interact` |
| `Assets/Script/Interaction/InteractionDetector.cs` | Di Player. Cari objek interaktif terdekat (OverlapCircle + layer), tombol E |
| `Assets/Script/Interaction/InteractionPromptUI.cs` | Menampilkan "[E] ..." (UI Text legacy) |
| `Assets/Script/Dialogue/DialogueUI.cs` | Kotak dialog; E/Spasi/klik di mana saja untuk lanjut, tombol Tutup (opsional) untuk menutup langsung; callback saat selesai |
| `Assets/Script/NPC/TarsWanderer.cs` | TARS acak di `BoxCollider2D` area; state Idle/Moving/Interacting; sprite per arah lewat array Inspector |
| `Assets/Script/Resource/ResourceType.cs` | Enum jenis resource (Ice, Water, HabitatOxygen, SuitOxygen, Energy, Food) |
| `Assets/Script/Resource/ResourceManager.cs` | Stok, kapasitas, `Add`/`TryConsume`, event perubahan. Dipasang pada GameObject `ResourceManager` di scene |
| `Assets/Script/Resource/ResourceDebugPanel.cs` | Alat uji sementara (OnGUI). Hapus saat HUD sungguhan ada |
| `Assets/Script/Time/TimeManager.cs` | Hari, timer harian, timer harian (pause saat dialog opsional lewat `pauseDuringDialogue`, default mati), event hari. Dipasang pada GameObject `TimeManager` |
| `Assets/Script/UI/HudUI.cs` | Tampilan Day, timer, Food, Water, Oxygen, Energy. Dipasang pada `HudRoot` di Canvas |
| `Assets/Script/Interaction/Bed.cs` | Tempat tidur (IInteractable): tidur, fade, hari berikutnya. Dipasang pada GameObject `Bed` (layer `Interactable`, BoxCollider2D, anak `SpawnPoint`) |
| `Assets/Script/UI/FadeUI.cs` | Layar hitam untuk fade. Dipasang pada `Canvas/FadeOverlay` |
| `Assets/Editor/MarsUISetup.cs` | Menu Editor: membuat/memperbaiki Canvas, dialog, prompt, dan menyambungkan ke TARS/Player. Helper `internal` dipakai `MarsHudSetup` |
| `Assets/Editor/MarsHudSetup.cs` | Menu Editor `Mars > Setup HUD and Time`: membuat TimeManager, HudRoot (TimePanel, ResourcePanel), dan menyambungkan referensi |
| `Assets/Script/Suit/SuitSystem.cs` | Status suit, isi ulang dari oksigen habitat, lepas suit, konsumsi oksigen suit selama dikenakan. Dipasang pada GameObject `SuitSystem` |
| `Assets/Script/Suit/SuitStation.cs` | Suit Station (IInteractable): kenakan + isi, isi ulang, atau lepas suit. Layer `Interactable`, Collider2D Is Trigger |
| `Assets/Script/Suit/IndoorZone.cs` | Trigger area dalam habitat; di luar semua zona = pemain di luar |
| `Assets/Script/Suit/SuitReminder.cs` | Pengingat TARS "lepas suit" (dialog, cooldown). Dipasang pada GameObject `SuitSystem` oleh menu setup |
| `Assets/Script/Suit/SuitGate.cs` | Tembok tak terlihat di pintu keluar (BoxCollider2D solid) |
| `Assets/Editor/MarsSuitSetup.cs` | Menu Editor `Mars > Setup Suit and Gate`: membuat SuitSystem, SuitStation, IndoorZone, SuitGate, dan `HudRoot/SuitPanel` |
| `Assets/Editor/MarsSleepSetup.cs` | Menu Editor `Mars > Setup Bed and Fade`: membuat FadeOverlay dan Bed (+SpawnPoint), menyambungkan referensi tanpa menimpa isian manual |
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
**ResourceManager:** GameObject kosong `ResourceManager` di scene dengan komponen `ResourceManager` dan `ResourceDebugPanel` (dipasang manual oleh developer; tidak ada di patch) [Dev].
**TimeManager dan HUD:** dibuat lewat menu Mars > Setup HUD and Time (GameObject `TimeManager`; `Canvas/HudRoot` berisi `TimePanel` dan `ResourcePanel`). Perubahan scene dilakukan developer, belum diverifikasi AI [?].
**Bed dan Fade:** dibuat lewat menu Mars > Setup Bed and Fade (`Canvas/FadeOverlay`; GameObject `Bed` dengan anak `SpawnPoint`). Sprite Bed belum ada (placeholder oleh developer). Posisi di scene: [Dev], belum diverifikasi [?].
**Suit dan Gate:** dibuat lewat menu Mars > Setup Suit and Gate (placeholder di sekitar Player). WAJIB diposisikan developer: `IndoorZone` menutupi seluruh interior dengan tepi sejajar pintu; `SuitGate` di pintu dan menutup seluruh lebar pintu; `SuitStation` di Control Room dengan sprite. Posisi akhir: [Dev], belum diverifikasi [?].
**Canvas:** dibuat lewat menu Mars > Setup Dialogue and Prompt UI (berisi `DialogueUI` dan `InteractionPromptUI`).

## Saat menambah/mengubah asset
- Pertahankan pasangan `.meta` dan GUID.
- Jangan menghapus atau mengganti asset yang mungkin direferensikan scene/prefab tanpa pemeriksaan.
- Berikan langkah Inspector secara eksplisit: GameObject, komponen, field, nilai, dan referensi yang perlu diisi.
- Untuk perubahan binary/serialized yang tidak aman dibuat lewat patch teks, jelaskan langkah manual.
