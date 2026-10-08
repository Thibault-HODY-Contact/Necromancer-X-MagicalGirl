//using System;
//using System.IO;
//using System.Linq;
//using UnityEditor;
//using UnityEditor.SceneManagement;
//using UnityEngine;
//using UnityEngine.SceneManagement;

//[InitializeOnLoad]
//internal static class OctagonSceneInspection
//{
//    private const string Work = "C:/Users/Ryad/Documents/GitHub/Necromancer x Magical Girl/SceneEdit-20261008-Octagon";
//    private static double _next;

//    [Serializable] private class Item
//    {
//        public string name;
//        public long id;
//        public Vector3 position;
//        public Quaternion rotation;
//        public Vector3 scale;
//        public Vector3 boundsCenter;
//        public Vector3 boundsSize;
//        public Vector3 meshBoundsCenter;
//        public Vector3 meshBoundsSize;
//        public string mesh;
//        public string[] materials;
//        public string[] components;
//    }

//    [Serializable] private class Report
//    {
//        public string path;
//        public string[] openScenes;
//        public Item[] items;
//    }

//    static OctagonSceneInspection() { EditorApplication.update += Tick; }

//    private static void Tick()
//    {
//        if (EditorApplication.timeSinceStartup < _next) return;
//        _next = EditorApplication.timeSinceStartup + 1d;
//        if (File.Exists(Work + "/Inspection.json") || File.Exists(Work + "/Inspection-error.txt"))
//        { EditorApplication.update -= Tick; return; }
//        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
//        if (PrefabStageUtility.GetCurrentPrefabStage() != null) return;
//        string path = File.ReadAllText(Work + "/InspectPath.txt").Trim();
//        try
//        {
//            string[] openScenes = Enumerable.Range(0, SceneManager.sceneCount).Select(i => SceneManager.GetSceneAt(i).path).ToArray();
//            Scene scene = EditorSceneManager.OpenPreviewScene(path);
//            try
//            {
//                Item[] items = scene.GetRootGameObjects().Select(root => {
//                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(root, out string guid, out long id);
//                    Renderer renderer = root.GetComponent<Renderer>();
//                    MeshFilter filter = root.GetComponent<MeshFilter>();
//                    Mesh mesh = filter != null ? filter.sharedMesh : null;
//                    return new Item {
//                        name = root.name, id = id,
//                        position = root.transform.position, rotation = root.transform.rotation, scale = root.transform.lossyScale,
//                        boundsCenter = renderer != null ? renderer.bounds.center : root.transform.position,
//                        boundsSize = renderer != null ? renderer.bounds.size : Vector3.zero,
//                        meshBoundsCenter = mesh != null ? mesh.bounds.center : Vector3.zero,
//                        meshBoundsSize = mesh != null ? mesh.bounds.size : Vector3.zero,
//                        mesh = mesh != null ? mesh.name : "",
//                        materials = renderer != null ? renderer.sharedMaterials.Select(m => m != null ? AssetDatabase.GetAssetPath(m) : "MISSING").ToArray() : new string[0],
//                        components = root.GetComponents<Component>().Select(c => c != null ? c.GetType().Name : "MISSING").ToArray()
//                    };
//                }).ToArray();
//                File.WriteAllText(Work + "/Inspection.json", JsonUtility.ToJson(new Report { path = path, openScenes = openScenes, items = items }, true));
//            }
//            finally { EditorSceneManager.ClosePreviewScene(scene); }
//        }
//        catch (Exception exception) { File.WriteAllText(Work + "/Inspection-error.txt", exception.ToString()); }
//    }
//}
