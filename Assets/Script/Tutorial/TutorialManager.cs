using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tutorial Day 1 (Milestone 12, PHASE.md 12.1 dan 12.2): sembilan langkah arahan TARS. BUKAN quest harian.
///
/// Prinsip (GDD 5.2, 12):
/// - Hanya di Day 1 (tutorialDay). Mulai Day 2 tutorial selesai sendiri, apa pun langkah terakhirnya.
/// - Arahan lewat TARSSystem.Announce (satu-satunya pemilik waktu tampil pesan TARS). Tutorial TIDAK mengunci
///   gerak, tidak mengunci urutan, dan tidak memberi hadiah.
/// - Tutorial hanya MEMBACA keadaan sistem lain; tidak mengubah resource.
/// - Syarat tiap langkah "dikunci" (latched) begitu terpenuhi, kapan pun, termasuk SEBELUM langkahnya jadi
///   aktif. Pemain boleh mengerjakan urutan apa pun; langkah yang sudah terpenuhi langsung dilewati saat tiba
///   gilirannya.
///
/// Syarat selesai (semua USULAN, ubah bila perlu):
/// - Intro: pemain mengajak TARS bicara (E) DAN membuka Storage (hanya yang ada di scene yang dihitung).
/// - Suit: suit dikenakan (SuitSystem.IsEquipped).
/// - CollectIce: kumpulkan iceToCollect (default 5) es di Ice Deposit (event IceDeposit.OnCollected), atau tas es penuh.
/// - PurifyWater: satu batch Water Purifier selesai (OnProcessingChanged true lalu false).
/// - RemoveSuit: suit dilepas di Suit Station (SuitSystem.OnEquippedChanged false; hanya bisa di dalam habitat).
///   Langkah ini diletakkan setelah PurifyWater (pemain sudah kembali ke habitat); urutan ada di array steps.
/// - Farming: ada FarmingPlot dengan tanaman yang sudah disiram (tumbuh waktu nyata setelah siram, lihat FarmingPlot).
/// - OxygenGenerator: Oxygen Generator menjadi target interaksi pemain (pemain mendekatinya); penjelasan
///   hubungan Water/Energy ada di arahan TARS. Mesin menyala sejak awal (P-5), jadi status "menyala" tidak dipakai.
/// - Eat: pemain memakan kentang (HungerSystem.OnSatietyChanged naik; satu-satunya jalan kenyang naik adalah Feed).
/// - Sleep: Day 2 dimulai (Bed). Arahan dan garis arah Sleep baru muncul SETELAH timer hari habis (TimeManager.IsDayOver),
///   bukan saat langkahnya mulai, agar pemain tidak disuruh tidur saat waktu masih berjalan.
///
/// Status "pernah selesai" disimpan di PlayerPrefs (USULAN sederhana; SaveSystem belum ada). Bila pernah selesai,
/// TARS memberi tahu bahwa tutorial bisa dilewati dengan tombol skipKey (GDD 18.2 Should Have).
/// Hanya logika; tidak punya tampilan sendiri. Garis arah ke tujuan langkah digambar TutorialGuideUI (GetGuideTarget).
/// </summary>
public class TutorialManager : MonoBehaviour
{
    private const string CompletedPrefsKey = "Mars.TutorialCompleted";

    public static TutorialManager Instance { get; private set; }

    [Header("Pengaturan (USULAN)")]
    [Tooltip("Hari tempat tutorial berjalan. GDD: Day 1.")]
    [SerializeField, Min(1)] private int tutorialDay = 1;
    [Tooltip("Centang untuk melewati tutorial saat uji (tanpa arahan TARS).")]
    [SerializeField] private bool skipTutorial = false;
    [Tooltip("Jeda pengecekan syarat langkah (detik).")]
    [SerializeField, Min(0.05f)] private float pollIntervalSeconds = 0.25f;
    [Tooltip("Arahan langkah yang belum selesai diumumkan ulang setelah jeda ini (detik) agar tidak hilang bila antrean TARS " +
             "sempat penuh. 0 = tidak diulang.")]
    [SerializeField, Min(0f)] private float reminderSeconds = 120f;

    [Header("Langkah CollectIce (USULAN)")]
    [Tooltip("Jumlah es yang harus dikumpulkan di Ice Deposit pada tutorial. Dibatasi kapasitas tas es. " +
             "Langkah juga selesai bila tas es penuh, agar tutorial tidak macet.")]
    [SerializeField, Min(1)] private int iceToCollect = 5;

    [Header("Lewati tutorial (GDD 18.2, Should Have; USULAN)")]
    [Tooltip("Simpan status 'tutorial pernah selesai' ke PlayerPrefs dan tawarkan melewati tutorial di permainan berikutnya.")]
    [SerializeField] private bool rememberCompletion = true;
    [Tooltip("Tombol melewati tutorial; hanya berlaku bila tutorial pernah diselesaikan sebelumnya.")]
    [SerializeField] private KeyCode skipKey = KeyCode.Tab;

    [Header("Langkah (urutan = GDD 5.2 / PHASE 12.1). Teks bawaan PLACEHOLDER")]
    [SerializeField] private TutorialStepData[] steps = CreateDefaultSteps();

    private readonly HashSet<TutorialStepId> achieved = new HashSet<TutorialStepId>();

    private TimeManager time;
    private InventorySystem inventory;
    private InteractionDetector detector;
    private TarsWanderer[] tarsNpcs = new TarsWanderer[0];
    private WaterPurifier[] purifiers = new WaterPurifier[0];
    private FarmingPlot[] plots = new FarmingPlot[0];
    private IceDeposit[] iceDeposits = new IceDeposit[0];
    private FoodStorage storage;
    private SuitStation suitStation;
    private SuitSystem suitSystem;
    private HungerSystem hunger;
    private float lastSatiety;
    private OxygenGenerator oxygenGenerator;
    private Bed bed;

    private bool hasTars;
    private bool hasStorage;
    private bool talkedToTars;
    private bool storageOpened;
    private int iceCollected;
    private int currentIndex = -1;
    private float nextPollTime;
    private float nextReminderTime;

    /// <summary>True selama tutorial berjalan (sudah dimulai dan belum selesai).</summary>
    public bool IsActive { get; private set; }

    /// <summary>True setelah tutorial selesai atau dilewati pada sesi ini.</summary>
    public bool IsCompleted { get; private set; }

    /// <summary>True bila tutorial pernah diselesaikan penuh pada permainan sebelumnya (PlayerPrefs).</summary>
    public bool HasCompletedBefore => rememberCompletion && PlayerPrefs.GetInt(CompletedPrefsKey, 0) == 1;

    public int StepCount => steps != null ? steps.Length : 0;
    public int CurrentStepIndex => currentIndex;

    /// <summary>Langkah yang sedang berjalan, atau null bila tutorial tidak aktif.</summary>
    public TutorialStepData CurrentStep =>
        IsActive && steps != null && currentIndex >= 0 && currentIndex < steps.Length ? steps[currentIndex] : null;

    /// <summary>Dipanggil saat langkah dimulai (data langkah, indeks).</summary>
    public event Action<TutorialStepData, int> OnStepStarted;

    /// <summary>Dipanggil saat langkah selesai (data langkah, indeks).</summary>
    public event Action<TutorialStepData, int> OnStepCompleted;

    /// <summary>Dipanggil sekali saat tutorial selesai (parameter: true bila dilewati sebelum semua langkah selesai).</summary>
    public event Action<bool> OnTutorialFinished;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("TutorialManager: sudah ada instance lain, objek ini dihapus.", this);
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        time = TimeManager.Instance;

        if (time == null)
        {
            Debug.LogError("TutorialManager: TimeManager tidak ada di scene. Tutorial tidak berjalan.", this);
            return;
        }

        // Berlangganan di Start (bukan OnEnable) karena urutan Awake/OnEnable antar objek tidak terjamin.
        time.OnDayStarted += HandleDayStarted;
        time.OnDayTimeEnded += HandleDayTimeEnded;

        if (skipTutorial)
        {
            FinishTutorial(true);
            return;
        }

        if (time.CurrentDay != tutorialDay) return;

        FindWatchedObjects();
        SubscribeWatchers();
        BeginTutorial();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (time != null)
        {
            time.OnDayStarted -= HandleDayStarted;
            time.OnDayTimeEnded -= HandleDayTimeEnded;
        }
        UnsubscribeWatchers();
    }

    private void Update()
    {
        if (!IsActive) return;

        if (HasCompletedBefore && Input.GetKeyDown(skipKey))
        {
            FinishTutorial(true);
            return;
        }

        if (Time.time >= nextPollTime)
        {
            nextPollTime = Time.time + pollIntervalSeconds;
            EvaluateConditions();
            AdvanceWhileAchieved();
        }

        if (IsActive && reminderSeconds > 0f && Time.time >= nextReminderTime)
        {
            nextReminderTime = Time.time + reminderSeconds;
            if (CurrentStep != null) AnnounceStep(CurrentStep);
        }
    }

    // ---------- API untuk sistem lain ----------

    /// <summary>
    /// Melaporkan bahwa langkah tertentu terpenuhi (dikunci, kapan pun). Bila itu langkah yang sedang berjalan,
    /// langkah selesai; bila belum giliran, langkah langsung dilewati saat tiba gilirannya.
    /// Mengembalikan true bila langkah aktif diselesaikan.
    /// </summary>
    public bool TryCompleteStep(TutorialStepId id)
    {
        if (!IsActive) return false;

        achieved.Add(id);
        TutorialStepData step = CurrentStep;
        if (step == null || step.Id != id) return false;

        CompleteCurrentStep();
        return true;
    }

    // ---------- Alur ----------

    private void BeginTutorial()
    {
        if (IsActive || IsCompleted || steps == null || steps.Length == 0) return;

        IsActive = true;
        WarnAboutMissingObjects();

        if (HasCompletedBefore && TARSSystem.Instance != null)
        {
            string hint = "[PLACEHOLDER] TARS: Tutorial ini pernah Anda selesaikan. Tekan " + skipKey + " untuk melewatinya.";
            TARSSystem.Instance.Announce("tutorial.skiphint", TARSSystem.Priority.Info, hint);
        }

        StartStep(0);
        EvaluateConditions();
        AdvanceWhileAchieved();
    }

    private void StartStep(int index)
    {
        currentIndex = index;
        TutorialStepData step = steps[index];
        nextPollTime = 0f;
        nextReminderTime = reminderSeconds > 0f ? Time.time + reminderSeconds : float.MaxValue;

        AnnounceStep(step);
        OnStepStarted?.Invoke(step, index);
    }

    private void CompleteCurrentStep()
    {
        TutorialStepData step = CurrentStep;
        if (step == null) return;

        OnStepCompleted?.Invoke(step, currentIndex);

        int next = currentIndex + 1;
        if (next >= steps.Length) FinishTutorial(false);
        else StartStep(next);
    }

    // Melewati langkah-langkah yang syaratnya sudah terpenuhi lebih dulu. Sleep hanya selesai lewat Day 2.
    private void AdvanceWhileAchieved()
    {
        int guard = steps != null ? steps.Length : 0;
        while (guard-- > 0)
        {
            TutorialStepData step = CurrentStep;
            if (step == null || step.Id == TutorialStepId.Sleep || !achieved.Contains(step.Id)) return;
            CompleteCurrentStep();
        }
    }

    private void FinishTutorial(bool skipped)
    {
        if (IsCompleted) return;

        IsActive = false;
        IsCompleted = true;
        currentIndex = -1;

        if (!skipped && rememberCompletion)
        {
            PlayerPrefs.SetInt(CompletedPrefsKey, 1);
            PlayerPrefs.Save();
        }

        UnsubscribeWatchers();
        OnTutorialFinished?.Invoke(skipped);
    }

    private void HandleDayStarted(int day)
    {
        if (!IsActive || day <= tutorialDay) return;

        // Pemain tidur dan masuk Day 2: langkah terakhir (Sleep) terpenuhi, tutorial selesai (GDD 5.2, 5.3).
        // Bila pemain tidur sebelum semua langkah selesai, tutorial tetap berakhir; Day 2 dst. adalah survival bebas.
        bool finishedAllSteps = CurrentStep != null && CurrentStep.Id == TutorialStepId.Sleep;
        if (finishedAllSteps) OnStepCompleted?.Invoke(CurrentStep, currentIndex);

        FinishTutorial(!finishedAllSteps);
    }

    // Langkah Sleep sedang berjalan dan waktu hari baru saja habis: sekarang arahan tidur boleh tampil.
    private void HandleDayTimeEnded()
    {
        TutorialStepData step = CurrentStep;
        if (IsActive && step != null && step.Id == TutorialStepId.Sleep) AnnounceStep(step);
    }

    // Arahan dan garis arah tidur hanya muncul setelah timer hari habis (permintaan developer).
    private bool IsSleepWindowOpen => time != null && time.IsDayOver;

    private void AnnounceStep(TutorialStepData step)
    {
        TARSSystem tars = TARSSystem.Instance;
        string[] lines = step.TarsLines;
        if (lines == null || lines.Length == 0) return;
        if (step.Id == TutorialStepId.Sleep && !IsSleepWindowOpen) return; // tunggu waktu habis; HandleDayTimeEnded mengumumkannya

        if (tars == null)
        {
            Debug.Log("TutorialManager: [" + step.Title + "] " + string.Join(" ", lines));
            return;
        }

        // Pesan dibuang bila langkahnya sudah lewat sebelum sempat tampil (isStillValid).
        TutorialStepId id = step.Id;
        tars.Announce("tutorial." + id, TARSSystem.Priority.Warning, () => lines,
            () => CurrentStep != null && CurrentStep.Id == id);
    }

    // ---------- Pemantau syarat ----------

    private void FindWatchedObjects()
    {
        inventory = InventorySystem.Instance;
        detector = FindFirstObjectByType<InteractionDetector>();
        tarsNpcs = FindObjectsByType<TarsWanderer>(FindObjectsSortMode.None);
        purifiers = FindObjectsByType<WaterPurifier>(FindObjectsSortMode.None);
        plots = FindObjectsByType<FarmingPlot>(FindObjectsSortMode.None);

        iceDeposits = FindObjectsByType<IceDeposit>(FindObjectsSortMode.None);
        storage = FindFirstObjectByType<FoodStorage>();
        suitStation = FindFirstObjectByType<SuitStation>();
        suitSystem = SuitSystem.Instance;
        hunger = HungerSystem.Instance;
        lastSatiety = hunger != null ? hunger.Satiety : 0f;
        oxygenGenerator = FindFirstObjectByType<OxygenGenerator>();
        bed = FindFirstObjectByType<Bed>();

        hasTars = tarsNpcs.Length > 0;
        hasStorage = storage != null;
    }

    private void SubscribeWatchers()
    {
        foreach (IceDeposit deposit in iceDeposits) deposit.OnCollected += HandleIceCollected;
        foreach (TarsWanderer npc in tarsNpcs) npc.OnPlayerTalked += HandlePlayerTalked;
        foreach (WaterPurifier purifier in purifiers) purifier.OnProcessingChanged += HandlePurifierProcessing;
        if (suitSystem != null) suitSystem.OnEquippedChanged += HandleSuitEquippedChanged;
        if (hunger != null) hunger.OnSatietyChanged += HandleSatietyChanged;
    }

    private void UnsubscribeWatchers()
    {
        foreach (IceDeposit deposit in iceDeposits) if (deposit != null) deposit.OnCollected -= HandleIceCollected;
        foreach (TarsWanderer npc in tarsNpcs) if (npc != null) npc.OnPlayerTalked -= HandlePlayerTalked;
        foreach (WaterPurifier purifier in purifiers) if (purifier != null) purifier.OnProcessingChanged -= HandlePurifierProcessing;
        if (suitSystem != null) suitSystem.OnEquippedChanged -= HandleSuitEquippedChanged;
        if (hunger != null) hunger.OnSatietyChanged -= HandleSatietyChanged;
    }

    private void HandlePlayerTalked() => talkedToTars = true;

    // Es yang dikumpulkan di Ice Deposit dihitung langsung dari event deposit (tidak bergantung pada IsOutdoors,
    // dan es yang diambil dari storage tidak dihitung). Selesai bila target tercapai atau tas es penuh.
    private void HandleIceCollected(int amount)
    {
        iceCollected += amount;
        if (IsIceTargetReached()) achieved.Add(TutorialStepId.CollectIce);
    }

    private int IceTarget
    {
        get
        {
            int bagMax = inventory != null ? inventory.GetBagMax(ItemType.Ice) : iceToCollect;
            return Mathf.Max(1, Mathf.Min(iceToCollect, bagMax));
        }
    }

    private bool IsIceTargetReached()
    {
        if (iceCollected >= IceTarget) return true;
        return inventory != null && inventory.GetBag(ItemType.Ice) > 0 && inventory.BagSpace(ItemType.Ice) == 0;
    }

    // SetProcessing hanya memicu event saat nilai berubah, jadi 'false' berarti satu batch selesai.
    private void HandlePurifierProcessing(bool processing)
    {
        if (!processing) achieved.Add(TutorialStepId.PurifyWater);
    }

    // Suit hanya bisa dilepas di dalam habitat (SuitSystem.Unequip), jadi event 'false' berarti pemain melepas suit
    // di Suit Station (atau Bed saat tidur). Dikunci seperti syarat lain, kapan pun terjadi.
    private void HandleSuitEquippedChanged(bool equipped)
    {
        if (!equipped) achieved.Add(TutorialStepId.RemoveSuit);
    }

    // Kenyang berkurang pelan dan HANYA naik lewat HungerSystem.Feed (makan kentang), jadi kenaikan = pemain makan.
    private void HandleSatietyChanged(float satiety, float max)
    {
        if (satiety > lastSatiety + 0.001f) achieved.Add(TutorialStepId.Eat);
        lastSatiety = satiety;
    }

    // Syarat yang tidak punya event: dicek berkala dan dikunci.
    private void EvaluateConditions()
    {
        SuitSystem suit = SuitSystem.Instance;
        if (suit != null && suit.IsEquipped) achieved.Add(TutorialStepId.Suit);

        if (StorageUI.IsOpen) storageOpened = true;
        if ((!hasTars || talkedToTars) && (!hasStorage || storageOpened)) achieved.Add(TutorialStepId.Intro);

        // Cadangan: tas es penuh (misalnya es diambil dari storage) tidak boleh membuat langkah macet.
        if (inventory != null && IsIceTargetReached()) achieved.Add(TutorialStepId.CollectIce);

        foreach (FarmingPlot plot in plots)
        {
            if (plot != null && plot.HasPlant && plot.IsWatered) { achieved.Add(TutorialStepId.Farming); break; }
        }

        if (detector != null && detector.CurrentTarget is OxygenGenerator) achieved.Add(TutorialStepId.OxygenGenerator);
    }

    // ---------- Arah tujuan (dipakai TutorialGuideUI) ----------

    /// <summary>
    /// Objek tujuan untuk langkah yang sedang berjalan, atau null bila tidak ada (tutorial tidak aktif atau objek tidak ada).
    /// Hanya petunjuk arah; tidak mengubah apa pun. 'from' = posisi pemain (untuk memilih objek terdekat).
    /// Tujuan (USULAN): Intro = Storage lalu TARS; Suit dan RemoveSuit = Suit Station; CollectIce = Ice Deposit terdekat;
    /// PurifyWater = Ice Deposit bila tas belum berisi es, selain itu Water Purifier; Farming = petak bertanaman yang belum
    /// disiram, atau petak kosong bila ada bibit di tas, selain itu Storage (ambil bibit); OxygenGenerator; Eat = Storage bila
    /// tas tanpa kentang; Sleep = Bed, hanya setelah timer hari habis.
    /// </summary>
    public Transform GetGuideTarget(Vector3 from)
    {
        TutorialStepData step = CurrentStep;
        if (step == null) return null;

        switch (step.Id)
        {
            case TutorialStepId.Intro:
                if (hasStorage && !storageOpened) return storage.transform;
                if (hasTars && !talkedToTars) return Nearest(tarsNpcs, from);
                return null;

            case TutorialStepId.Suit:
            case TutorialStepId.RemoveSuit:
                return suitStation != null ? suitStation.transform : null;

            case TutorialStepId.CollectIce:
                return Nearest(iceDeposits, from);

            case TutorialStepId.PurifyWater:
                if (inventory != null && inventory.GetBag(ItemType.Ice) >= 1) return Nearest(purifiers, from);
                return Nearest(iceDeposits, from);

            case TutorialStepId.Farming:
                return GetFarmingGuideTarget(from);

            case TutorialStepId.OxygenGenerator:
                return oxygenGenerator != null ? oxygenGenerator.transform : null;

            case TutorialStepId.Eat:
                // Tas berisi kentang: tidak ada tujuan (pemain pilih kentang dengan tombol 1 lalu E). Tas kosong: ke Storage.
                if (inventory != null && inventory.GetBag(ItemType.Potato) >= 1) return null;
                return hasStorage ? storage.transform : null;

            case TutorialStepId.Sleep:
                // Garis ke tempat tidur baru muncul setelah timer hari habis.
                return IsSleepWindowOpen && bed != null ? bed.transform : null;

            default:
                return null;
        }
    }

    private Transform GetFarmingGuideTarget(Vector3 from)
    {
        Transform unwatered = null, empty = null;
        float unwateredDist = float.MaxValue, emptyDist = float.MaxValue;

        foreach (FarmingPlot plot in plots)
        {
            if (plot == null) continue;

            float dist = (plot.transform.position - from).sqrMagnitude;
            if (plot.HasPlant && !plot.IsWatered && dist < unwateredDist) { unwatered = plot.transform; unwateredDist = dist; }
            else if (!plot.HasPlant && dist < emptyDist) { empty = plot.transform; emptyDist = dist; }
        }

        if (unwatered != null) return unwatered;
        if (empty != null && inventory != null && inventory.GetBag(ItemType.Seed) >= 1) return empty;
        if (hasStorage) return storage.transform;
        return empty;
    }

    private static Transform Nearest<T>(T[] items, Vector3 from) where T : Component
    {
        Transform best = null;
        float bestDist = float.MaxValue;

        foreach (T item in items)
        {
            if (item == null) continue;

            float dist = (item.transform.position - from).sqrMagnitude;
            if (dist < bestDist) { best = item.transform; bestDist = dist; }
        }

        return best;
    }

    private void WarnAboutMissingObjects()
    {
        if (SuitSystem.Instance == null) Debug.LogWarning("TutorialManager: SuitSystem tidak ada; langkah Suit tidak bisa selesai otomatis.", this);
        if (inventory == null) Debug.LogWarning("TutorialManager: InventorySystem tidak ada; langkah CollectIce tidak bisa selesai otomatis.", this);
        if (purifiers.Length == 0) Debug.LogWarning("TutorialManager: WaterPurifier tidak ada; langkah PurifyWater tidak bisa selesai otomatis.", this);
        if (plots.Length == 0) Debug.LogWarning("TutorialManager: FarmingPlot tidak ada; langkah Farming tidak bisa selesai otomatis.", this);
        if (detector == null) Debug.LogWarning("TutorialManager: InteractionDetector tidak ada; langkah OxygenGenerator tidak bisa selesai otomatis.", this);
        if (oxygenGenerator == null) Debug.LogWarning("TutorialManager: OxygenGenerator tidak ada; langkah OxygenGenerator tidak bisa selesai otomatis.", this);
    }

    // ---------- Alat uji ----------

    [ContextMenu("Debug: Selesaikan langkah saat ini")]
    private void DebugCompleteCurrentStep()
    {
        if (IsActive) CompleteCurrentStep();
    }

    [ContextMenu("Debug: Lewati tutorial")]
    private void DebugSkipTutorial()
    {
        if (IsActive) FinishTutorial(true);
    }

    [ContextMenu("Debug: Hapus status tutorial tersimpan")]
    private void DebugClearSavedCompletion()
    {
        PlayerPrefs.DeleteKey(CompletedPrefsKey);
        PlayerPrefs.Save();
    }

    // ---------- Data bawaan ----------

    private static TutorialStepData[] CreateDefaultSteps()
    {
        return new[]
        {
            new TutorialStepData(TutorialStepId.Intro, "Mengenal habitat",
                "[PLACEHOLDER] TARS: Selamat datang. Ini Control Room. Angka di HUD menunjukkan Food, Water, Oxygen, dan Energy. Ajak saya bicara untuk laporan.",
                "[PLACEHOLDER] TARS: Buka Storage untuk melihat bibit, es, dan kentang yang tersimpan."),
            new TutorialStepData(TutorialStepId.Suit, "Astronaut Suit",
                "[PLACEHOLDER] TARS: Kenakan Astronaut Suit di Suit Station. Tabungnya terisi dari oksigen habitat."),
            new TutorialStepData(TutorialStepId.CollectIce, "Kumpulkan Ice",
                "[PLACEHOLDER] TARS: Keluar lewat gerbang dan kumpulkan 5 es di Ice Deposit. Awasi oksigen suit."),
            new TutorialStepData(TutorialStepId.PurifyWater, "Olah Ice menjadi Water",
                "[PLACEHOLDER] TARS: Bawa es ke Water Purifier. Pilih es di tas, lalu tekan E. Mesin memakai Energy untuk mengubahnya menjadi air."),
            new TutorialStepData(TutorialStepId.RemoveSuit, "Lepas suit",
                "[PLACEHOLDER] TARS: Anda sudah di dalam habitat. Oksigen suit tetap berkurang selama suit dikenakan. Lepas suit di Suit Station; tabungnya terisi lagi dari oksigen habitat."),
            new TutorialStepData(TutorialStepId.Farming, "Tanam dan siram",
                "[PLACEHOLDER] TARS: Tanam bibit kentang di Farming Plot, lalu siram. Cukup siram sekali; tanaman matang dalam sekitar 1 sampai 2 menit, lalu panen untuk mendapat kentang dan bibit."),
            new TutorialStepData(TutorialStepId.OxygenGenerator, "Oxygen Generator",
                "[PLACEHOLDER] TARS: Dekati Oxygen Generator. Mesin ini memakai Water dan Energy untuk menghasilkan oksigen habitat, jadi air dibagi antara tanaman dan oksigen."),
            new TutorialStepData(TutorialStepId.Eat, "Makan",
                "[PLACEHOLDER] TARS: Rasa kenyang Anda berkurang seiring waktu. Ambil kentang dari Storage, pilih kentang di tas (tombol 1), lalu tekan E untuk makan."),
            new TutorialStepData(TutorialStepId.Sleep, "Kembali dan tidur",
                "[PLACEHOLDER] TARS: Waktu hari ini habis. Kembali ke Control Room dan tidur di tempat tidur untuk mengakhiri hari.")
        };
    }
}
