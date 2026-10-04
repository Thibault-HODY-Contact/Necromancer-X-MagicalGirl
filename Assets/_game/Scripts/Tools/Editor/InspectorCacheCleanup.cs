using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
internal static class InspectorCacheCleanup
{
    private static readonly FieldInfo _trackerField = typeof(Editor).Assembly
        .GetType("UnityEditor.PropertyEditor")?
        .GetField("m_Tracker", BindingFlags.Instance | BindingFlags.NonPublic);

    static InspectorCacheCleanup()
    {
        AssemblyReloadEvents.beforeAssemblyReload += Cleanup;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        PrefabStage.prefabStageClosing += OnPrefabStageClosing;
        QueueCleanup();
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode)
            Cleanup();
        else
            QueueCleanup();
    }

    private static void OnPrefabStageClosing(PrefabStage stage)
    {
        QueueCleanup();
    }

    private static void QueueCleanup()
    {
        EditorApplication.delayCall -= Cleanup;
        EditorApplication.delayCall += Cleanup;
    }

    private static void Cleanup()
    {
        // Unity 6.3 can retain built-in editors whose targets have been destroyed.
        // Their OnEnable then throws on every domain reload, including entering Play.
        // Dispose only orphaned editors; active Inspectors keep their selection and locks.
        if (_trackerField == null) return;

        HashSet<Editor> activeEditors = new HashSet<Editor>(ActiveEditorTracker.sharedTracker.activeEditors);
        foreach (EditorWindow window in Resources.FindObjectsOfTypeAll<EditorWindow>())
        {
            if (!_trackerField.DeclaringType.IsInstanceOfType(window)) continue;
            if (!(_trackerField.GetValue(window) is ActiveEditorTracker tracker)) continue;
            foreach (Editor editor in tracker.activeEditors)
                activeEditors.Add(editor);
        }

        foreach (Editor editor in Resources.FindObjectsOfTypeAll<Editor>())
        {
            if (editor == null || activeEditors.Contains(editor)) continue;
            string typeName = editor.GetType().FullName;
            if (typeName != "UnityEditor.GameObjectInspector" && typeName != "UnityEditor.TransformInspector") continue;
            if (editor.hideFlags != HideFlags.HideAndDontSave) continue;
            if (!HasOnlyMissingTargets(editor)) continue;

            Object.DestroyImmediate(editor);
        }
    }

    private static bool HasOnlyMissingTargets(Editor editor)
    {
        Object[] targets = editor.targets;
        if (targets == null || targets.Length == 0) return false;
        foreach (Object target in targets)
            if (target != null) return false;
        return true;
    }
}
