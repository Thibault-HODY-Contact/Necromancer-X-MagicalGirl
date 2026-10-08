using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
internal static class SceneRecoveryValidation
{
    private const string Work = "C:/Users/Ryad/Documents/GitHub/Necromancer x Magical Girl/Recovery-20261008-Scenes";
    private static readonly string[] Paths = {
        "Assets/_game/Scenes/Ryad/Arena.unity",
        "Assets/_game/Scenes/SceneThibault/Ui/HomeScreen.unity",
        "Assets/_game/Scenes/SceneThibault/Levels/Level1.unity"
    };
    private static int _next;
    private static double _time;

    [Serializable] private class Report
    {
        public string path;
        public int objects;
        public int renderers;
        public int lights;
        public string[] missingPrefabs;
        public string[] missingScripts;
        public string[] missingMeshes;
        public string[] missingMaterials;
        public string[] shaderErrors;
    }

    static SceneRecoveryValidation() { EditorApplication.update += Tick; }

    private static void Tick()
    {
        if (EditorApplication.timeSinceStartup < _time) return;
        _time = EditorApplication.timeSinceStartup + 1d;
        if (File.Exists(Work + "/Unity-completed.txt") || File.Exists(Work + "/Unity-error.txt"))
        { EditorApplication.update -= Tick; return; }
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode || PrefabStageUtility.GetCurrentPrefabStage() != null)
        { File.WriteAllText(Work + "/Unity-waiting.txt", "Quitter Play et le mode Prefab pour vérifier les scènes restaurées."); return; }
        try
        {
            string path = Paths[_next];
            Scene scene = EditorSceneManager.OpenPreviewScene(path);
            try
            {
                Transform[] transforms = Components<Transform>(scene).ToArray();
                Renderer[] renderers = Components<Renderer>(scene).ToArray();
                Report report = new Report {
                    path = path,
                    objects = transforms.Length,
                    renderers = renderers.Length,
                    lights = Components<Light>(scene).Count(),
                    missingPrefabs = transforms.Where(t => PrefabUtility.IsPrefabAssetMissing(t.gameObject)).Select(ObjectPath).ToArray(),
                    missingScripts = transforms.Where(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) != 0).Select(ObjectPath).ToArray(),
                    missingMeshes = Components<MeshFilter>(scene).Where(f => f.sharedMesh == null && f.GetComponent<MeshRenderer>() != null).Select(f => ObjectPath(f.transform)).Concat(Components<SkinnedMeshRenderer>(scene).Where(r => r.sharedMesh == null).Select(r => ObjectPath(r.transform))).ToArray(),
                    missingMaterials = renderers.Where(r => r.sharedMaterials.Any(m => m == null)).Select(r => ObjectPath(r.transform)).ToArray(),
                    shaderErrors = renderers.SelectMany(r => r.sharedMaterials).Where(m => m != null && (m.shader == null || ShaderUtil.ShaderHasError(m.shader))).Select(m => AssetDatabase.GetAssetPath(m)).Distinct().ToArray()
                };
                File.WriteAllText(Work + "/" + Path.GetFileNameWithoutExtension(path) + ".json", JsonUtility.ToJson(report, true));
                Capture(scene, Path.GetFileNameWithoutExtension(path));
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
            _next++;
            if (_next == Paths.Length) File.WriteAllText(Work + "/Unity-completed.txt", "Les trois scènes restaurées ont été chargées et capturées dans des scènes de prévisualisation. Aucun mode Play lancé, aucune scène enregistrée par le diagnostic.");
        }
        catch (Exception exception)
        { File.WriteAllText(Work + "/Unity-error.txt", exception.ToString()); }
    }

    private static IEnumerable<T> Components<T>(Scene scene) where T : Component
    { return scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true)); }
    private static string ObjectPath(Transform transform)
    { return transform.parent == null ? transform.name : ObjectPath(transform.parent) + "/" + transform.name; }

    private static void Capture(Scene scene, string name)
    {
        GameObject cameraObject = new GameObject("Recovery preview camera") { hideFlags = HideFlags.HideAndDontSave };
        SceneManager.MoveGameObjectToScene(cameraObject, scene);
        Camera camera = cameraObject.AddComponent<Camera>();
        UniversalAdditionalCameraData data = cameraObject.AddComponent<UniversalAdditionalCameraData>();
        if (name == "HomeScreen")
        {
            Camera source = Components<Camera>(scene).First(c => c != camera && c.name == "Main Camera");
            camera.CopyFrom(source);
            camera.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
            UniversalAdditionalCameraData sourceData = source.GetComponent<UniversalAdditionalCameraData>();
            if (sourceData != null) EditorUtility.CopySerialized(sourceData, data);
            foreach (Canvas canvas in Components<Canvas>(scene).Where(c => c.isRootCanvas && c.renderMode != RenderMode.WorldSpace))
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 1f;
            }
            Canvas.ForceUpdateCanvases();
        }
        else
        {
            Renderer[] visible = Components<Renderer>(scene).Where(r => r.gameObject.activeInHierarchy && r.enabled && !(r is ParticleSystemRenderer)).ToArray();
            Bounds bounds = visible[0].bounds;
            foreach (Renderer renderer in visible) bounds.Encapsulate(renderer.bounds);
            camera.orthographic = true;
            camera.orthographicSize = Mathf.Max(bounds.size.z * 0.56f, bounds.size.x * 0.34f);
            camera.transform.position = bounds.center + new Vector3(0f, Mathf.Max(bounds.size.x, bounds.size.z) * 0.85f, -bounds.size.z * 0.22f);
            camera.transform.LookAt(bounds.center);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 500f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.032f, 0.055f);
            data.renderPostProcessing = true;
        }
        camera.scene = scene;
        camera.overrideSceneCullingMask = EditorSceneManager.GetSceneCullingMask(scene);
        camera.aspect = 16f / 9f;
        camera.allowHDR = true;
        RenderTexture previous = RenderTexture.active;
        RenderTexture target = RenderTexture.GetTemporary(1280, 720, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        Texture2D image = null;
        try
        {
            camera.targetTexture = target;
            RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
            RenderTexture.active = target;
            image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0f, 0f, 1280f, 720f), 0, 0);
            image.Apply();
            File.WriteAllBytes(Work + "/" + name + ".png", image.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = null;
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
            if (image != null) UnityEngine.Object.DestroyImmediate(image);
            UnityEngine.Object.DestroyImmediate(cameraObject);
        }
    }
}
