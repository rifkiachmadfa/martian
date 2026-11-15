# Technical Architecture

## Prinsip
- Audit repository sebelum mengubah arsitektur.
- Ikuti pola yang sudah ada jika sehat; jangan membuat manager/script baru hanya demi nama.
- Pisahkan tanggung jawab, hindari ketergantungan silang yang tidak perlu.
- Gunakan solusi sederhana, efisien, dan sesuai Unity 2D serta target perangkat rendah.

## Area tanggung jawab (panduan, bukan kewajiban satu kelas per sistem)
- Game/day flow: state permainan, hari, tidur, ending.
- Resource: stok, konsumsi, produksi, validasi perubahan.
- Player: movement, animasi, collision, interaksi.
- Suit: status suit dan Suit Oxygen outdoor.
- Farming: plot, penyiraman, tahap Potato, panen.
- Machines: purifier dan oxygen generator.
- Solar: produksi Energy dan kondisi panel.
- TARS: pemicu pesan/pengingat.
- Save: progres yang perlu dipertahankan.

## Audit teknis wajib
Periksa `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, struktur `Assets/`, scene, prefab, script, `.meta`, Input System, physics, dan Animator sebelum memberi instruksi yang bergantung padanya.

## Implementasi saat ini (2026-11-15, commit `e6feb586`)
- **Versi:** Unity 6000.6.8f1, URP 17.6.0, Input System 1.20.1 (Active Input Handling = Both), Cinemachine 6.6.0, uGUI 2.6.0. [Repo]
- **Input:** kode memakai API lama (`Input.GetAxisRaw`, `Input.GetKeyDown`). Aman karena mode Both.
- **Interaksi:** pola `IInteractable` + `InteractionDetector` (Player) + `InteractionPromptUI`. Objek baru (bed, mesin, plot) cukup mengimplementasikan `IInteractable`; detector tidak perlu diubah. Interaksi tahan (misal Ice) belum didukung.
- **Dialog:** `DialogueUI` hanya tampilan. Pemanggil mengirim nama pembicara, baris teks, dan callback selesai. TARS memakai teks tetap dari Inspector (sesuai GDD: predefined, tanpa AI generatif).
- **TARS:** `TarsWanderer` memakai Rigidbody2D Dynamic agar bertabrakan dengan dinding; deteksi macet memilih target baru.
- **UI:** UnityEngine.UI.Text (legacy), bukan TextMeshPro.
- **Resource (2026-11-15):** `ResourceManager` (`Assets/Script/Resource/`) adalah satu-satunya penyimpan stok. Akses lewat `ResourceManager.Instance`: `Get`, `GetMax`, `Has`, `Add` (dibatasi kapasitas, mengembalikan jumlah yang masuk), `TryConsume` (false jika stok kurang). Event `OnResourceChanged(type, nilai, max)` untuk HUD. Tanpa `DontDestroyOnLoad` karena game satu scene. HabitatOxygen dan SuitOxygen terpisah. Belum dihubungkan ke sistem lain. [Repo, belum dites di Unity]
- **Waktu (2026-11-15):** `TimeManager` (`Assets/Script/Time/`) via `TimeManager.Instance`: `CurrentDay`, `TotalDays`, `RemainingSeconds`, `IsDayOver`, `IsPaused`, `StartDay(int)`; event `OnDayStarted(day)` dan `OnDayTimeEnded`. Timer berhenti selama `DialogueUI.IsOpen` (dibaca tiap frame; DialogueUI tidak diubah). Saat waktu habis timer berhenti di 0 dan event terpicu sekali; belum ada efek lain. [Repo, belum dites di Unity]
- **HUD (2026-11-15):** `HudUI` (`Assets/Script/UI/`) hanya tampilan: berlangganan `ResourceManager.OnResourceChanged` dan membaca `TimeManager`. Berlangganan di `Start` (bukan `OnEnable`) karena urutan Awake/OnEnable antar objek tidak terjamin. Menampilkan Food, Water, HabitatOxygen (label "Oxygen"), Energy. Dibuat lewat menu `Mars > Setup HUD and Time`, yang memakai helper `internal` dari `MarsUISetup`.
- **Tidur (2026-11-15):** `Bed` (`Assets/Script/Interaction/`, IInteractable) menjalankan coroutine: `Player.MovementEnabled=false`, `TimeManager.ExternalPause=true`, `FadeUI.FadeToBlack`, `TimeManager.StartDay(hari+1)`, pindah ke SpawnPoint, `FadeFromBlack`, buka kunci. `FadeUI` hanya tampilan (CanvasGroup, selalu sibling terakhir di Canvas). `Bed` tidak memegang state hari; state di TimeManager. Cooldown singkat setelah dialog mencegah dialog terbuka ulang (pola sama dengan TARS). Pesan Bed memakai `DialogueUI.Show` dengan pembicara "Sistem". [Repo, belum dites di Unity]
- **Belum ada:** GameManager, SaveSystem, TARSSystem, dan sistem lain di GDD. Tidak ada namespace; script berada di global namespace.
- **Rencana integrasi:** pause timer saat dialog akan memakai event dari `DialogueUI` setelah TimeManager ada.
