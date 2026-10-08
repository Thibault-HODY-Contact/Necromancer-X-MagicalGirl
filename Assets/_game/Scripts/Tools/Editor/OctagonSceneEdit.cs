using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
internal static class OctagonSceneEdit
{
    private const string Work = "C:/Users/Ryad/Documents/GitHub/Necromancer x Magical Girl/SceneEdit-20261008-Octagon";
    private const string ScenePath = "Assets/_game/Scenes/Final/Levels/Level2/Level2.unity";
    private static double _next;

    [Serializable] private class Item
    {
        public string name;
        public string newName;
        public string parent;
        public bool remove;
        public bool move;
        public Vector3 position;
    }
    [Serializable] private class Plan
    {
        public string expectedHash;
        public string[] groups;
        public Item[] items;
    }
    [Serializable] private class Report
    {
        public string[] roots;
        public string[] sides;
        public int walls;
        public int pillars;
        public int floors;
        public int colliders;
        public Vector3 wallCenter;
        public Vector3 arenaPosition;
        public float floorTopMin;
        public float floorTopMax;
        public string[] missingReferences;
        public string[] shaderErrors;
    }

    static OctagonSceneEdit() { EditorApplication.update += Tick; }

    private static void Tick()
    {
        if (EditorApplication.timeSinceStartup < _next) return;
        _next = EditorApplication.timeSinceStartup + 1d;
        if (File.Exists(Work + "/Edit-completed.txt") || File.Exists(Work + "/Edit-error.txt"))
        { EditorApplication.update -= Tick; return; }
        if (!File.Exists(Work + "/Plan.json") || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode || PrefabStageUtility.GetCurrentPrefabStage() != null)
        { File.WriteAllText(Work + "/Edit-waiting.txt", "Quitter Play et le mode Prefab."); return; }
        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        if (scene.IsValid() && scene.isLoaded && scene.isDirty)
        { File.WriteAllText(Work + "/Edit-waiting.txt", "Enregistrer Level2 pour preserver les changements ouverts."); return; }
        try
        {
            Plan plan = JsonUtility.FromJson<Plan>(File.ReadAllText(Work + "/Plan.json"));
            string hash;
            using (SHA256 sha = SHA256.Create())
                hash = BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(ScenePath))).Replace("-", "").ToLowerInvariant();
            if (hash != plan.expectedHash) throw new InvalidOperationException("Level2 a change depuis l'inspection ; aucune modification effectuee.");
            bool wasOpen = scene.IsValid() && scene.isLoaded;
            if (!wasOpen) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                GameObject[] roots = scene.GetRootGameObjects();
                foreach (Item item in plan.items)
                    if (roots.Count(root => root.name == item.name) != 1)
                        throw new InvalidOperationException("Objet ambigu ou absent : " + item.name);
                int undoGroup = Undo.GetCurrentGroup();
                Undo.SetCurrentGroupName("Resize and organize Level2 octagon");
                Dictionary<string, Transform> groups = new Dictionary<string, Transform>();
                foreach (string path in plan.groups)
                {
                    int slash = path.LastIndexOf('/');
                    GameObject group = new GameObject(slash < 0 ? path : path.Substring(slash + 1));
                    SceneManager.MoveGameObjectToScene(group, scene);
                    Undo.RegisterCreatedObjectUndo(group, "Create arena hierarchy group");
                    if (slash >= 0) group.transform.SetParent(groups[path.Substring(0, slash)], false);
                    groups.Add(path, group.transform);
                }
                foreach (Item item in plan.items)
                {
                    GameObject root = roots.Single(go => go.name == item.name);
                    if (item.remove) { Undo.DestroyObjectImmediate(root); continue; }
                    Undo.RecordObject(root, "Rename arena object");
                    Undo.RecordObject(root.transform, "Move arena object");
                    if (!string.IsNullOrEmpty(item.newName)) root.name = item.newName;
                    if (item.move) root.transform.position = item.position;
                    if (!string.IsNullOrEmpty(item.parent)) Undo.SetTransformParent(root.transform, groups[item.parent], "Organize arena hierarchy");
                }
                foreach (Transform group in groups.Values)
                    foreach (Transform child in group.Cast<Transform>().OrderBy(t => t.name, StringComparer.Ordinal).ToArray()) child.SetAsLastSibling();
                Undo.CollapseUndoOperations(undoGroup);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Level2 n'a pas pu etre enregistree.");
            }
            finally { if (!wasOpen && scene.IsValid()) EditorSceneManager.CloseScene(scene, true); }
            Validate();
            File.WriteAllText(Work + "/Edit-completed.txt", "Level2 modifiee, enregistree et rechargee pour validation. Aucun mode Play lance.");
        }
        catch (Exception exception) { File.WriteAllText(Work + "/Edit-error.txt", exception.ToString()); }
    }

    private static void Validate()
    {
        Scene scene = EditorSceneManager.OpenPreviewScene(ScenePath);
        try
        {
            Transform[] transforms = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
            Renderer[] renderers = transforms.Select(t => t.GetComponent<Renderer>()).Where(r => r != null).ToArray();
            Renderer[] walls = renderers.Where(r => r.transform.parent != null && r.transform.parent.parent != null && r.transform.parent.parent.name == "Walls").ToArray();
            Renderer[] floors = renderers.Where(r => r.transform.parent != null && r.transform.parent.name == "Floor").ToArray();
            Transform arena = transforms.Single(t => t.name == "Arena" && t.parent == null);
            Bounds bounds = walls[0].bounds;
            foreach (Renderer wall in walls) bounds.Encapsulate(wall.bounds);
            Report report = new Report {
                roots = scene.GetRootGameObjects().Select(go => go.name).ToArray(),
                sides = transforms.Where(t => t.parent != null && t.parent.name == "Walls").Select(t => t.name + ": " + t.childCount).ToArray(),
                walls = walls.Length,
                pillars = transforms.Count(t => t.parent != null && t.parent.parent != null && t.parent.parent.name == "Pillars"),
                floors = floors.Length,
                colliders = transforms.Count(t => t.GetComponent<Collider>() != null),
                arenaPosition = arena.position,
                wallCenter = new Vector3(bounds.center.x, 0f, bounds.center.z),
                floorTopMin = floors.Min(r => r.bounds.max.y),
                floorTopMax = floors.Max(r => r.bounds.max.y),
                missingReferences = transforms.Where(t => PrefabUtility.IsPrefabAssetMissing(t.gameObject) || GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) != 0 || (t.GetComponent<MeshFilter>() != null && t.GetComponent<MeshFilter>().sharedMesh == null) || (t.GetComponent<Renderer>() != null && t.GetComponent<Renderer>().sharedMaterials.Any(m => m == null))).Select(t => t.name).ToArray(),
                shaderErrors = renderers.SelectMany(r => r.sharedMaterials).Where(m => m != null && (m.shader == null || ShaderUtil.ShaderHasError(m.shader))).Select(m => AssetDatabase.GetAssetPath(m)).Distinct().ToArray()
            };
            File.WriteAllText(Work + "/Validation.json", JsonUtility.ToJson(report, true));
            if (report.walls != 16 || report.pillars != 16 || report.sides.Length != 8 || report.sides.Any(s => !s.EndsWith(": 2")) || report.arenaPosition.sqrMagnitude > 0.000001f || report.wallCenter.sqrMagnitude > 0.0001f || Mathf.Abs(report.floorTopMin) > 0.001f || Mathf.Abs(report.floorTopMax) > 0.001f || report.missingReferences.Length != 0 || report.shaderErrors.Length != 0)
                throw new InvalidOperationException("Controle de la scene non satisfait : consulter Validation.json.");
            Capture(scene);
        }
        finally { EditorSceneManager.ClosePreviewScene(scene); }
    }

    private static void Capture(Scene scene)
    {
        GameObject cameraObject = new GameObject("Octagon validation camera") { hideFlags = HideFlags.HideAndDontSave };
        SceneManager.MoveGameObjectToScene(cameraObject, scene);
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<UniversalAdditionalCameraData>();
        camera.orthographic = true;
        camera.orthographicSize = 18f;
        camera.aspect = 16f / 9f;
        camera.transform.position = new Vector3(0f, 30f, -20f);
        camera.transform.LookAt(Vector3.zero);
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 200f;
        camera.scene = scene;
        camera.overrideSceneCullingMask = EditorSceneManager.GetSceneCullingMask(scene);
        camera.clearFlags = CameraClearFlags.Skybox;
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
            File.WriteAllBytes(Work + "/Level2-after.png", image.EncodeToPNG());
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
