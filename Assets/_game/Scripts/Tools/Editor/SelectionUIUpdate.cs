using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
internal static class SelectionUIUpdate
{
    private const string WorkDirectory = "C:/Users/Ryad/AppData/Local/Temp/NMG-SelectionStates-da3e5b778b3342ce925d28f168c3b9dc";
    private const string ScenePath = "Assets/_game/Scenes/SceneThibault/Ui/HomeScreen.unity";
    private static double _nextCheck;

    static SelectionUIUpdate() { EditorApplication.update += Tick; }

    private static void Tick()
    {
        if (EditorApplication.timeSinceStartup < _nextCheck) return;
        _nextCheck = EditorApplication.timeSinceStartup + 1d;
        if (File.Exists(WorkDirectory + "/Completed.txt") || File.Exists(WorkDirectory + "/Error.txt"))
        { EditorApplication.update -= Tick; return; }
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode || PrefabStageUtility.GetCurrentPrefabStage() != null)
        { File.WriteAllText(WorkDirectory + "/Waiting.txt", "Quitter Play et le mode Prefab pour appliquer les couleurs fixes."); return; }
        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        if (scene.IsValid() && scene.isDirty)
        { File.WriteAllText(WorkDirectory + "/Waiting.txt", "Enregistrer HomeScreen avant de raccorder les couleurs."); return; }
        Scene previous = SceneManager.GetActiveScene();
        bool opened = !scene.IsValid() || !scene.isLoaded;
        try
        {
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            string before = Composition(scene);
            File.Copy(ScenePath, WorkDirectory + "/HomeScreen.before.unity", true);
            for (int player = 1; player <= 2; player++)
            {
                string path = "Canvas/Selection/Buttons/J" + player + "/J" + player + "SelectionInput";
                UnityEngine.UI.Button button = Find(scene, path).GetComponent<UnityEngine.UI.Button>();
                SelectionButtonFeedback feedback = button.GetComponent<SelectionButtonFeedback>();
                string palette = player == 1 ? "nec" : "mg";
                Sprite normal = Sprite(palette, "normal");
                Sprite hover = Sprite(palette, "hover");
                Sprite pressed = Sprite(palette, "pressed");
                SerializedObject fields = new SerializedObject(feedback);
                fields.FindProperty("_button").objectReferenceValue = button;
                fields.FindProperty("_normal").objectReferenceValue = normal;
                fields.FindProperty("_hover").objectReferenceValue = hover;
                fields.FindProperty("_pressed").objectReferenceValue = pressed;
                fields.ApplyModifiedPropertiesWithoutUndo();
                button.image.sprite = normal;
                button.image.overrideSprite = null;
                button.spriteState = new UnityEngine.UI.SpriteState { highlightedSprite = hover, selectedSprite = hover, pressedSprite = pressed, disabledSprite = normal };
            }
            if (before != Composition(scene)) throw new InvalidOperationException("La composition a changé.");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Validate();
            File.WriteAllText(WorkDirectory + "/Completed.txt", "États et déplacements validés : droite -> milieu (arrêt) -> gauche sur deux pressions, bords et occupation respectés. J1 vert et J2 rose à toutes les positions. Composition préservée.");
        }
        catch (Exception exception)
        { File.WriteAllText(WorkDirectory + "/Error.txt", exception.ToString()); Debug.LogException(exception); }
        finally
        {
            if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            if (opened && scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static IEnumerable<T> Components<T>(Scene scene) where T : Component
    { return scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true)); }
    private static string Path(Transform transform)
    { return transform.parent == null ? transform.name : Path(transform.parent) + "/" + transform.name; }
    private static Transform Find(Scene scene, string path)
    { return Components<Transform>(scene).First(transform => Path(transform) == path); }
    private static Sprite Sprite(string palette, string state)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_game/Art/Ui/ToonKit_v2/button_" + palette + "_" + state + ".png");
        if (sprite == null) throw new InvalidOperationException("Sprite manquant : " + palette + " " + state);
        return sprite;
    }
    private static string Composition(Scene scene)
    {
        return string.Join("\n", Components<Transform>(scene).Where(transform => !(transform is RectTransform)).Select(transform =>
            GlobalObjectId.GetGlobalObjectIdSlow(transform) + ":" + transform.localPosition.ToString("R") + transform.localRotation.ToString("R") + transform.localScale.ToString("R")));
    }
    private static void Require(bool result, string message)
    { if (!result) throw new InvalidOperationException(message); }

    private static void Validate()
    {
        Scene preview = EditorSceneManager.OpenPreviewScene(ScenePath);
        try
        {
            UiCharacterSelection selection = Find(preview, "Canvas/Selection").GetComponent<UiCharacterSelection>();
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            List<int> choices = (List<int>)typeof(UiCharacterSelection).GetField("_CharacterChoose", flags).GetValue(selection);
            MethodInfo left = typeof(UiCharacterSelection).GetMethod("onLeftButton", flags);
            MethodInfo right = typeof(UiCharacterSelection).GetMethod("onRightButton", flags);
            choices[0] = 1; choices[1] = 0;
            left.Invoke(selection, new object[] { 0 });
            Require(selection.GetCharacterChoice(0) == 0, "Une pression depuis la droite n'arrête pas au milieu.");
            left.Invoke(selection, new object[] { 0 });
            Require(selection.GetCharacterChoice(0) == -1, "La deuxième pression ne sélectionne pas la gauche.");
            left.Invoke(selection, new object[] { 0 });
            Require(selection.GetCharacterChoice(0) == -1, "Le bord gauche n'est pas borné.");
            right.Invoke(selection, new object[] { 0 });
            Require(selection.GetCharacterChoice(0) == 0, "Une pression depuis la gauche n'arrête pas au milieu.");
            right.Invoke(selection, new object[] { 0 });
            Require(selection.GetCharacterChoice(0) == 1, "La deuxième pression ne sélectionne pas la droite.");
            right.Invoke(selection, new object[] { 0 });
            Require(selection.GetCharacterChoice(0) == 1, "Le bord droit n'est pas borné.");
            choices[0] = -1; choices[1] = 0;
            left.Invoke(selection, new object[] { 1 });
            Require(selection.GetCharacterChoice(1) == 0, "Le héros gauche peut être pris deux fois.");
            choices[0] = 0; choices[1] = 1;
            right.Invoke(selection, new object[] { 0 });
            Require(selection.GetCharacterChoice(0) == 0, "Le héros droit peut être pris deux fois.");

            MethodInfo advance = typeof(SelectionMovement).GetMethod("Advance", flags);
            for (int player = 1; player <= 2; player++)
            {
                Transform root = Find(preview, "Canvas/Selection/Buttons/J" + player);
                RectTransform rect = root.GetComponent<RectTransform>();
                SelectionMovement movement = root.GetComponent<SelectionMovement>();
                rect.anchoredPosition = new Vector2(500f, rect.anchoredPosition.y);
                movement.MoveTo(0);
                advance.Invoke(movement, new object[] { 0.15f });
                Require(Mathf.Abs(rect.anchoredPosition.x - 250f) < 0.01f, "Le déplacement n'est pas interpolé.");
                advance.Invoke(movement, new object[] { 0.15f });
                advance.Invoke(movement, new object[] { 1f });
                Require(Mathf.Abs(rect.anchoredPosition.x) < 0.01f, "Le bouton repart automatiquement du milieu.");
                movement.MoveTo(-1);
                advance.Invoke(movement, new object[] { 0.3f });
                Require(Mathf.Abs(rect.anchoredPosition.x + 500f) < 0.01f, "La seconde transition n'atteint pas la gauche.");
                string path = "Canvas/Selection/Buttons/J" + player + "/J" + player + "SelectionInput";
                SelectionButtonFeedback feedback = Find(preview, path).GetComponent<SelectionButtonFeedback>();
                UnityEngine.UI.Button button = feedback.GetComponent<UnityEngine.UI.Button>();
                Sprite expected = Sprite(player == 1 ? "nec" : "mg", "normal");
                foreach (int position in new[] { -1, 0, 1 })
                {
                    choices[player - 1] = position;
                    rect.anchoredPosition = new Vector2(position * 500f, rect.anchoredPosition.y);
                    feedback.SendMessage("OnEnable");
                    Require(button.image.sprite == expected, "La couleur dépend encore de la position.");
                }
            }
        }
        finally { EditorSceneManager.ClosePreviewScene(preview); }
    }
}
