using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[InitializeOnLoad]
public static class NecromancerChargeSetup
{
    const string Temp = "C:/Users/Ryad/AppData/Local/Temp/NMG-NEC-Charge-1e56b3475bc54bf5a0b14c6710262d92";
    const string NecPath = "Assets/_game/Prefabs/NEC/NEC.prefab";
    const string FxPath = "Assets/_game/Prefabs/Feedback/Attack2_NEC Variant.prefab";
    const string OldFx = "C:/Users/Ryad/AppData/Local/Temp/NMG-NEC-Impact-0e41d9c247634b41bf62a509d64dbec5/Attack2.before-setup.prefab";
    const string NoisePath = "Assets/_game/Art/Animations/NEC/ChargeCameraNoise.asset";
    const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

    static NecromancerChargeSetup() { EditorApplication.update += Run; }

    static void Run()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (File.Exists(Temp + "/Completed.txt") || File.Exists(Temp + "/Error.txt")) { EditorApplication.update -= Run; return; }
        PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null && (stage.assetPath == NecPath || stage.assetPath == FxPath)) return;
        EditorApplication.update -= Run;
        try { Setup(); }
        catch (Exception exception) { File.WriteAllText(Temp + "/Error.txt", exception.ToString()); }
    }

    static void Setup()
    {
        if (!File.Exists(Temp + "/NEC.before-setup.prefab")) File.Copy(NecPath, Temp + "/NEC.before-setup.prefab");
        if (!File.Exists(Temp + "/Attack2.before-setup.prefab")) File.Copy(FxPath, Temp + "/Attack2.before-setup.prefab");
        File.Copy(OldFx, FxPath, true);
        AssetDatabase.ImportAsset(FxPath, ImportAssetOptions.ForceUpdate);
        var report = new StringBuilder("Original green FX restored from its saved pre-impact version, same asset GUID.\n");
        NoiseSettings profile = AssetDatabase.LoadAssetAtPath<NoiseSettings>(NoisePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<NoiseSettings>();
            profile.PositionNoise = new[] { new NoiseSettings.TransformNoiseParams
            {
                X = new NoiseSettings.NoiseParams { Frequency = 23f, Amplitude = 0.7f },
                Y = new NoiseSettings.NoiseParams { Frequency = 29f, Amplitude = 0.4f }
            } };
            profile.OrientationNoise = new[] { new NoiseSettings.TransformNoiseParams
            {
                Z = new NoiseSettings.NoiseParams { Frequency = 21f, Amplitude = 1.5f }
            } };
            AssetDatabase.CreateAsset(profile, NoisePath);
        }

        GameObject nec = PrefabUtility.LoadPrefabContents(NecPath);
        try
        {
            Transform render = nec.GetComponentsInChildren<Transform>(true).Single(t => t.name == "Render");
            Transform model = render.Find("Necromancer");
            Transform anchor = render.Find("Attack2Feedback");
            Attack2_NEC attack2 = nec.GetComponentInChildren<Attack2_NEC>(true);
            Feedback_NEC feedback = render.GetComponent<Feedback_NEC>();
            CinemachineCamera camera = nec.GetComponentInChildren<CinemachineCamera>(true);
            PlayerInput input = nec.GetComponentInChildren<PlayerInput>(true);
            if (model == null || anchor == null || attack2 == null || feedback == null || camera == null || input == null)
                throw new Exception("Missing NEC integration reference.");

            ChargeShakeFeedback chargeShake = render.GetComponent<ChargeShakeFeedback>() ?? render.gameObject.AddComponent<ChargeShakeFeedback>();
            References(chargeShake, ("_visualRoot", render), ("_attack2", attack2));
            CinemachineBasicMultiChannelPerlin noise = camera.GetComponent<CinemachineBasicMultiChannelPerlin>()
                ?? camera.gameObject.AddComponent<CinemachineBasicMultiChannelPerlin>();
            noise.NoiseProfile = profile; noise.AmplitudeGain = 0f; noise.FrequencyGain = 1f;
            CinemachineImpulseListener listener = camera.GetComponent<CinemachineImpulseListener>()
                ?? camera.gameObject.AddComponent<CinemachineImpulseListener>();
            listener.ApplyAfter = CinemachineCore.Stage.Noise;
            listener.ChannelMask = 1; listener.Gain = 1f; listener.UseCameraSpace = true;
            listener.SignalCombinationMode = CinemachineImpulseListener.SignalCombinationModes.UseLargest;
            CinemachineImpulseSource source = anchor.GetComponent<CinemachineImpulseSource>()
                ?? anchor.gameObject.AddComponent<CinemachineImpulseSource>();
            source.ImpulseDefinition = new CinemachineImpulseDefinition
            {
                ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform,
                ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Explosion,
                ImpulseDuration = 0.2f, ImpulseChannel = 1
            };
            source.DefaultVelocity = new Vector3(1f, 0.3f, 0f);
            CameraShakeFeedback cameraFeedback = anchor.GetComponent<CameraShakeFeedback>()
                ?? anchor.gameObject.AddComponent<CameraShakeFeedback>();
            References(cameraFeedback, ("_impulseSource", source), ("_impulseListener", listener),
                ("_playerInput", input), ("_attack2", attack2), ("_chargeNoise", noise));

            MaterialFlash flash = render.GetComponentInChildren<MaterialFlash>(true);
            if (flash == null) throw new Exception("NEC MaterialFlash missing.");
            Material white = AssetDatabase.LoadAssetAtPath<Material>("Assets/_game/Art/Materials/OnKnockback.mat");
            References(flash, ("_visualRoot", model.gameObject), ("_whiteFlashMaterial", white));
            var flashData = new SerializedObject(flash);
            flashData.FindProperty("_whiteFlashDuration").floatValue = 0.1f;
            flashData.ApplyModifiedPropertiesWithoutUndo();
            var attackData = new SerializedObject(attack2);
            attackData.FindProperty("_chargeMovementMultiplier").floatValue = 0.5f;
            attackData.ApplyModifiedPropertiesWithoutUndo();
            Add(feedback, "_onAttack2ChargeStarted", cameraFeedback.StartCharge);
            Add(feedback, "_onAttack2ChargeStoped", cameraFeedback.StopCharge);
            Add(feedback, "_onAttack2FullyCharged", flash.FlashWhite);
            Add(feedback, "_onAttack2HitboxActivated", cameraFeedback.Shake);

            GameObject effect = anchor.GetChild(0).gameObject;
            if (effect.transform.Find("BurstSparks") != null) throw new Exception("Old FX still contains added sparks.");
            effect.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(nec, NecPath);
            AssetDatabase.SaveAssets();
            report.AppendLine("NEC wired through serialized UnityEvents: charge camera starts/stops, full charge invokes FlashWhite (0.1s), arrival retains its impulse.");
            report.AppendLine("Camera noise belongs to NEC's own CinemachineCamera; PlayerInput found on its actual child, per-player arrival channel retained.");
            Validate(nec, feedback, cameraFeedback, noise, flash, white, effect, report);
        }
        finally { PrefabUtility.UnloadPrefabContents(nec); }
        File.WriteAllText(Temp + "/Completed.txt", report.ToString());
    }

    static void Add(Feedback_NEC feedback, string field, UnityAction action)
    {
        UnityEvent value = Event(feedback, field);
        if (!Enumerable.Range(0, value.GetPersistentEventCount()).Any(i => value.GetPersistentTarget(i) == action.Target as UnityEngine.Object
            && value.GetPersistentMethodName(i) == action.Method.Name))
            UnityEventTools.AddVoidPersistentListener(value, action);
        EditorUtility.SetDirty(feedback);
    }

    static UnityEvent Event(Feedback_NEC feedback, string field) => (UnityEvent)typeof(Feedback_NEC).GetField(field, Private).GetValue(feedback);
    static void References(UnityEngine.Object target, params (string name, UnityEngine.Object value)[] values)
    {
        var data = new SerializedObject(target);
        foreach (var value in values)
        {
            if (value.value == null) throw new Exception("Missing reference " + value.name);
            data.FindProperty(value.name).objectReferenceValue = value.value;
        }
        data.ApplyModifiedPropertiesWithoutUndo();
    }
    static void Invoke(object target, string method) => target.GetType().GetMethod(method, Private).Invoke(target, null);
    static void Set(object target, string field, object value) => target.GetType().GetField(field, Private).SetValue(target, value);
    static void Assert(bool valid, string message) { if (!valid) throw new Exception(message); }
    static bool Near(float a, float b) => Mathf.Abs(a - b) < 0.0001f;

    static void Validate(GameObject nec, Feedback_NEC feedback, CameraShakeFeedback camera, CinemachineBasicMultiChannelPerlin noise,
        MaterialFlash flash, Material white, GameObject effect, StringBuilder report)
    {
        Attack2_NEC ability = nec.GetComponentInChildren<Attack2_NEC>(true);
        var data = new SerializedObject(ability);
        AttackSystem attack = (AttackSystem)data.FindProperty("_attackSystem").objectReferenceValue;
        MoveSystem move = (MoveSystem)data.FindProperty("_moveSystem").objectReferenceValue;
        float baseSpeed = move.Speed;
        UnityEvent[] events = new[] { "_onAttack2ChargeStarted", "_onAttack2ChargeStoped", "_onAttack2FullyCharged",
            "_onAttack2HitboxActivated", "_onAttack2HitboxDesactivated" }.Select(n => Event(feedback, n)).ToArray();
        UnityEventCallState[][] states = events.Select(e => Enumerable.Range(0, e.GetPersistentEventCount()).Select(e.GetPersistentListenerState).ToArray()).ToArray();
        try
        {
            foreach (UnityEvent value in events)
                for (int i = 0; i < value.GetPersistentEventCount(); i++)
                    if (value.GetPersistentMethodName(i) != "Shake") value.SetPersistentListenerState(i, UnityEventCallState.EditorAndRuntime);
            Invoke(attack, "Awake"); Invoke(feedback, "OnEnable"); Invoke(flash, "Start");
            Renderer[] renderers = feedback.transform.Find("Necromancer").GetComponentsInChildren<Renderer>();
            Material[][] originals = renderers.Select(r => r.sharedMaterials).ToArray();
            int fullyCharged = 0; ability.FullyCharged += () => fullyCharged++;
            nec.transform.position += new Vector3(10000f, 0f, 10000f);
            Vector3 bodyPosition = nec.transform.position;
            Vector3 hitboxPosition = attack.Hitboxes[0].hitbox.transform.position;
            Vector3 visualPosition = feedback.transform.localPosition;

            ability.StartAttack(); Invoke(feedback, "UpdateChargeFeedback"); Invoke(camera, "Update");
            Assert(Near(move.Speed, baseSpeed * 0.5f), "Charge did not reduce movement to 50%.");
            Assert(noise.AmplitudeGain > 0f && fullyCharged == 0, "Charge camera did not start, or flashed too early.");
            Set(ability, "_chargeTimer", 0.5f); Invoke(ability, "Update"); Invoke(camera, "Update");
            Assert(noise.AmplitudeGain > 0.1f && noise.AmplitudeGain <= 0.4f, "Camera did not intensify with charge.");
            ChargeShakeFeedback shake = feedback.GetComponent<ChargeShakeFeedback>();
            Invoke(shake, "LateUpdate");
            Assert(nec.transform.position == bodyPosition && attack.Hitboxes[0].hitbox.transform.position == hitboxPosition,
                "Visual shake moved gameplay or physics.");

            Set(ability, "_chargeTimer", 1f); Invoke(ability, "Update"); Invoke(ability, "Update");
            Assert(fullyCharged == 1, "Full-charge notification repeated.");
            Assert(renderers.All(r => r.sharedMaterials.All(m => m == white)), "Full charge did not flash white.");
            var flashData = new SerializedObject(flash);
            Assert(Near(flashData.FindProperty("_flashTimer").floatValue, 0.1f), "White flash duration is not 0.1 seconds.");
            Set(flash, "_flashTimer", 0f); Invoke(flash, "Update");
            Assert(Enumerable.Range(0, renderers.Length).All(i => renderers[i].sharedMaterials.SequenceEqual(originals[i])), "White flash did not restore original materials.");
            ability.ReleaseAttack(); Invoke(camera, "Update"); Invoke(feedback, "UpdateChargeFeedback"); Invoke(shake, "LateUpdate");
            Assert(Near(move.Speed, baseSpeed) && Near(noise.AmplitudeGain, 0f), "Release did not restore speed or stop camera noise.");
            Assert((feedback.transform.localPosition - visualPosition).sqrMagnitude < 0.0000001f, "Visual shake did not restore Render position.");
            Assert(!attack.IsAttacking && !effect.activeSelf && !attack.Hitboxes[0].hitbox.Collider.enabled, "Departure activated the attack.");
            Set(ability, "_teleportationTimer", 0f); Invoke(ability, "Update");
            Assert(nec.transform.position != bodyPosition && effect.activeSelf && attack.Hitboxes[0].hitbox.Collider.enabled, "Arrival did not immediately enable the old FX and hitbox.");
            ability.StopAttack();
            Assert(!effect.activeSelf && effect.GetComponentsInChildren<ParticleSystem>(true).All(p => !p.IsAlive(false)), "FX did not clear with the hitbox.");
            ability.StartAttack(); Set(ability, "_chargeTimer", 1f); Invoke(ability, "Update");
            Assert(fullyCharged == 2, "Next charge did not rearm the white flash.");
            ability.StopAttack(); Invoke(feedback, "UpdateChargeFeedback");
            Assert(Near(move.Speed, baseSpeed), "Cancel did not restore movement speed.");
            Invoke(flash, "OnDisable"); Invoke(feedback, "OnDisable"); Invoke(camera, "OnDisable");
            report.AppendLine("Native checks passed outside Play: 50% movement while charging, restoration on release/cancel, progressive camera noise and zero afterward.");
            report.AppendLine("Full-charge flash occurs once per charge, lasts 0.1s, restores all original materials, and rearms for the next charge.");
            report.AppendLine("Render shake leaves player/collider fixed; arrival-only old FX and hitbox activate together, then all particles clear on stop.");
        }
        finally
        {
            for (int e = 0; e < events.Length; e++)
                for (int i = 0; i < states[e].Length; i++) events[e].SetPersistentListenerState(i, states[e][i]);
        }
    }
}
