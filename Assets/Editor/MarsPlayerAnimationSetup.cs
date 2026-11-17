#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Menu Editor: menyiapkan animasi pemain biasa (tanpa suit) dari sprite EMSI di Assets/Player.
/// Cara kerjanya sama dengan Mars > Setup Suit Player Animation:
/// - Mengatur import 9 PNG (Sprite, Point filter, tanpa kompresi, 100 PPU).
/// - Membuat 6 animation clip (idle dan jalan untuk depan, belakang, samping) dan SquarePlayer.controller
///   di Assets/Player/Animation/. Sprite samping hanya menghadap kanan; sisi kiri di-flip oleh PlayerSuitVisual.
/// - Memasang controller itu pada Animator Player dan mengisi normalController di PlayerSuitVisual (bila ada).
/// Parameter Animator sama dengan controller astronaut (IsMoving, DirectionX, DirectionY) sehingga PlayerMovement tidak berubah.
/// Aman dijalankan berulang kali; clip/controller yang sudah ada tidak ditimpa.
/// Mars > Setup Player Animation
/// </summary>
public static class MarsPlayerAnimationSetup
{
    private const string SpriteFolder = "Assets/Player/";
    private const string AnimFolder = "Assets/Player/Animation/";
    private const string ControllerPath = AnimFolder + "SquarePlayer.controller";
    private const float FrameDuration = 0.15f;

    private const string FrontIdle = "EMSI FRONT (5).png";
    private const string FrontStepA = "EMSI FRONT (5) (1).png";
    private const string FrontStepB = "EMSI FRONT (5) (2).png";
    private const string BackIdle = "EMSI BACK.png";
    private const string BackStepA = "EMSI BACK walk.png";
    private const string BackStepB = "EMSI BACK WALK 2.png";
    private const string SideIdle = "EMSI SIDE KANAN.png";
    private const string SideStepA = "EMSI SIDE KANAN jalan1.png";
    private const string SideStepB = "EMSI SIDE KANAN jalan2.png";

    [MenuItem("Mars/Setup Player Animation")]
    public static void Setup()
    {
        StringBuilder report = new StringBuilder();

        PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
        if (player == null)
        {
            EditorUtility.DisplayDialog("Mars Setup", "PlayerMovement tidak ditemukan di scene yang sedang dibuka.", "OK");
            return;
        }

        // 1. Import settings sprite
        int changed = 0;
        foreach (string file in AllFiles())
        {
            if (ConfigureImporter(SpriteFolder + file)) changed++;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        report.AppendLine("- Import sprite diperbarui: " + changed + " file.");

        if (!AssetDatabase.IsValidFolder("Assets/Player/Animation"))
            AssetDatabase.CreateFolder("Assets/Player", "Animation");

        // 2. Animation clip (loop). Idle = 1 frame, jalan = diam - langkah A - diam - langkah B.
        AnimationClip idleDown = GetOrCreateClip("Player_Idle_Down", new[] { FrontIdle }, report);
        AnimationClip walkDown = GetOrCreateClip("Player_Walk_Down", new[] { FrontIdle, FrontStepA, FrontIdle, FrontStepB }, report);
        AnimationClip idleUp = GetOrCreateClip("Player_Idle_Up", new[] { BackIdle }, report);
        AnimationClip walkUp = GetOrCreateClip("Player_Walk_Up", new[] { BackIdle, BackStepA, BackIdle, BackStepB }, report);
        AnimationClip idleSide = GetOrCreateClip("Player_Idle_Side", new[] { SideIdle }, report);
        AnimationClip walkSide = GetOrCreateClip("Player_Walk_Side", new[] { SideIdle, SideStepA, SideIdle, SideStepB }, report);

        if (idleDown == null || walkDown == null || idleUp == null || walkUp == null || idleSide == null || walkSide == null)
        {
            report.AppendLine("- GAGAL: ada clip yang tidak bisa dibuat (sprite tidak dimuat). Controller tidak dibuat.");
            ShowReport(report);
            return;
        }

        // 3. Animator Controller
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            controller = BuildController(idleDown, walkDown, idleUp, walkUp, idleSide, walkSide);
            report.AppendLine("- SquarePlayer.controller dibuat.");
        }
        else
        {
            report.AppendLine("- SquarePlayer.controller sudah ada, tidak ditimpa. (Hapus file ini bila ingin dibuat ulang.)");
        }

        // 4. Pasang pada Animator Player (hanya bila kosong/hilang, controller lain yang valid tidak ditimpa)
        Animator animator = player.GetComponent<Animator>();
        if (animator == null)
        {
            report.AppendLine("- PERINGATAN: Player tidak punya Animator.");
        }
        else if (animator.runtimeAnimatorController == null)
        {
            Undo.RecordObject(animator, "Setup Player Animation");
            animator.runtimeAnimatorController = controller;
            EditorUtility.SetDirty(animator);
            report.AppendLine("- Animator Player kosong (controller lama hilang), diisi SquarePlayer.controller.");
        }
        else if (animator.runtimeAnimatorController != controller)
        {
            report.AppendLine("- Animator Player memakai controller lain: " + animator.runtimeAnimatorController.name +
                ". Tidak ditimpa. Seret SquarePlayer.controller ke kolom Controller bila ingin memakainya.");
        }

        // 5. normalController pada PlayerSuitVisual (controller saat suit dilepas)
        PlayerSuitVisual visual = player.GetComponent<PlayerSuitVisual>();
        if (visual == null)
        {
            report.AppendLine("- PlayerSuitVisual belum ada di Player. Jalankan Mars > Setup Suit Player Animation (urutan bebas).");
        }
        else
        {
            SerializedObject so = new SerializedObject(visual);
            SerializedProperty prop = so.FindProperty("normalController");
            if (prop != null && prop.objectReferenceValue == null)
            {
                prop.objectReferenceValue = controller;
                Undo.RecordObject(visual, "Setup Player Animation");
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(visual);
                report.AppendLine("- normalController pada PlayerSuitVisual diisi.");
            }
            else
            {
                report.AppendLine("- normalController pada PlayerSuitVisual sudah terisi, tidak ditimpa.");
            }
        }

        EditorSceneManager.MarkSceneDirty(player.gameObject.scene);

        report.AppendLine();
        report.AppendLine("Simpan scene (Ctrl+S) agar perubahan tersimpan.");
        ShowReport(report);
    }

    private static void ShowReport(StringBuilder report)
    {
        Debug.Log("[MarsPlayerAnimationSetup]\n" + report);
        EditorUtility.DisplayDialog("Mars Setup - Player Animation", report.ToString(), "OK");
    }

    private static IEnumerable<string> AllFiles()
    {
        return new[] { FrontIdle, FrontStepA, FrontStepB, BackIdle, BackStepA, BackStepB, SideIdle, SideStepA, SideStepB };
    }

    private static bool ConfigureImporter(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning("MarsPlayerAnimationSetup: file tidak ditemukan: " + path);
            return false;
        }

        bool dirty = false;

        if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; dirty = true; }
        if (importer.spriteImportMode != SpriteImportMode.Single) { importer.spriteImportMode = SpriteImportMode.Single; dirty = true; }
        if (importer.spritePixelsPerUnit != 100f) { importer.spritePixelsPerUnit = 100f; dirty = true; }
        if (importer.filterMode != FilterMode.Point) { importer.filterMode = FilterMode.Point; dirty = true; }
        if (importer.textureCompression != TextureImporterCompression.Uncompressed) { importer.textureCompression = TextureImporterCompression.Uncompressed; dirty = true; }
        if (importer.mipmapEnabled) { importer.mipmapEnabled = false; dirty = true; }
        if (!importer.alphaIsTransparency) { importer.alphaIsTransparency = true; dirty = true; }

        if (dirty) importer.SaveAndReimport();
        return dirty;
    }

    private static AnimationClip GetOrCreateClip(string clipName, string[] spriteFiles, StringBuilder report)
    {
        string path = AnimFolder + clipName + ".anim";
        AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (existing != null)
        {
            report.AppendLine("- " + clipName + " sudah ada, tidak ditimpa.");
            return existing;
        }

        // Frame terakhir diulang di akhir waktu agar durasinya tampil penuh saat loop.
        ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[spriteFiles.Length + 1];
        for (int i = 0; i < spriteFiles.Length; i++)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFolder + spriteFiles[i]);
            if (sprite == null)
            {
                report.AppendLine("- PERINGATAN: sprite tidak dimuat: " + spriteFiles[i]);
                return null;
            }
            keys[i] = new ObjectReferenceKeyframe { time = i * FrameDuration, value = sprite };
        }
        keys[spriteFiles.Length] = new ObjectReferenceKeyframe
        {
            time = spriteFiles.Length * FrameDuration,
            value = keys[spriteFiles.Length - 1].value
        };

        AnimationClip clip = new AnimationClip { name = clipName, frameRate = 12f };
        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, path);
        report.AppendLine("- " + clipName + " dibuat.");
        return clip;
    }

    private static AnimatorController BuildController(
        AnimationClip idleDown, AnimationClip walkDown,
        AnimationClip idleUp, AnimationClip walkUp,
        AnimationClip idleSide, AnimationClip walkSide)
    {
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
        controller.AddParameter("DirectionX", AnimatorControllerParameterType.Float);
        controller.AddParameter("DirectionY", AnimatorControllerParameterType.Float);

        AnimatorStateMachine sm = controller.layers[0].stateMachine;

        AnimatorState sIdleDown = sm.AddState("Idle_Down", new Vector3(300, 0, 0));
        AnimatorState sWalkDown = sm.AddState("Walk_Down", new Vector3(520, 0, 0));
        AnimatorState sIdleUp = sm.AddState("Idle_Up", new Vector3(300, 80, 0));
        AnimatorState sWalkUp = sm.AddState("Walk_Up", new Vector3(520, 80, 0));
        AnimatorState sIdleSide = sm.AddState("Idle_Side", new Vector3(300, 160, 0));
        AnimatorState sWalkSide = sm.AddState("Walk_Side", new Vector3(520, 160, 0));

        sIdleDown.motion = idleDown;
        sWalkDown.motion = walkDown;
        sIdleUp.motion = idleUp;
        sWalkUp.motion = walkUp;
        sIdleSide.motion = idleSide;
        sWalkSide.motion = walkSide;
        sm.defaultState = sIdleDown;

        // Arah horizontal: DirectionX > 0.5 (kanan) atau < -0.5 (kiri) memakai sprite samping yang sama; flip dilakukan PlayerSuitVisual.
        AddTransition(sm, sWalkSide, true, AnimatorConditionMode.Greater, "DirectionX", 0.5f);
        AddTransition(sm, sWalkSide, true, AnimatorConditionMode.Less, "DirectionX", -0.5f);
        AddTransition(sm, sIdleSide, false, AnimatorConditionMode.Greater, "DirectionX", 0.5f);
        AddTransition(sm, sIdleSide, false, AnimatorConditionMode.Less, "DirectionX", -0.5f);

        AddTransition(sm, sWalkUp, true, AnimatorConditionMode.Greater, "DirectionY", 0.5f);
        AddTransition(sm, sIdleUp, false, AnimatorConditionMode.Greater, "DirectionY", 0.5f);

        AddTransition(sm, sWalkDown, true, AnimatorConditionMode.Less, "DirectionY", -0.5f);
        AddTransition(sm, sIdleDown, false, AnimatorConditionMode.Less, "DirectionY", -0.5f);

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return controller;
    }

    private static void AddTransition(AnimatorStateMachine sm, AnimatorState target, bool moving,
        AnimatorConditionMode mode, string directionParam, float threshold)
    {
        AnimatorStateTransition t = sm.AddAnyStateTransition(target);
        t.hasExitTime = false;
        t.hasFixedDuration = true;
        t.duration = 0f;
        t.canTransitionToSelf = false;
        t.AddCondition(moving ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, "IsMoving");
        t.AddCondition(mode, threshold, directionParam);
    }
}
#endif
