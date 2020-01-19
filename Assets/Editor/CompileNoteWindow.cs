using CustomNotes;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CompileNoteWindow : EditorWindow
{
    private static IEnumerable<NoteDescriptor> notes = Enumerable.Empty<NoteDescriptor>();

    // Current scroll position
    private Vector2 scrollPosition = Vector2.zero;

    [MenuItem("Window/Note Exporter")]
    public static void ShowWindow()
    {
        GetWindow(typeof(CompileNoteWindow), false, "Note Exporter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Notes", EditorStyles.boldLabel);
        GUILayout.Space(20);

        // Editor scroll height
        int scrollSpace = 50;
        int extraSpace = 0;
        foreach (NoteDescriptor note in notes)
        {
            if (note != null)
            {
                scrollSpace += CalculateAdditionScrollSpace(note, extraSpace);
                extraSpace += 2;
            }
        }

        // Scroll bar
        EditorWindow window = GetWindow(typeof(CompileNoteWindow), false, "Note Exporter", false);
        Rect rectPosition = new Rect(0, 0, EditorGUIUtility.currentViewWidth, window.position.size.y);
        Rect rectViewPosition = new Rect(0, 0, EditorGUIUtility.currentViewWidth - 20, scrollSpace);
        scrollPosition = GUI.BeginScrollView(rectPosition, scrollPosition, rectViewPosition, false, false);

        float currentWindowWidth = EditorGUIUtility.currentViewWidth;
        float windowWidthIncludingScrollbar = currentWindowWidth;
        if (window.position.size.y >= scrollSpace)
        {
            windowWidthIncludingScrollbar += 30;
        }

        // Editor content
        foreach (NoteDescriptor note in notes)
        {
            bool isMissingAuthorName = string.IsNullOrWhiteSpace(note.AuthorName);
            bool isMissingNoteName = string.IsNullOrWhiteSpace(note.NoteName);
            bool isMissingNoteLeft = !note.transform.Find("NoteLeft");
            bool isMissingNoteRight = !note.transform.Find("NoteRight");
            bool isMissingNoteDotLeft = note.DisableBaseNoteArrows && !note.transform.Find("NoteDotLeft");
            bool isMissingNoteDotRight = note.DisableBaseNoteArrows && !note.transform.Find("NoteDotRight");

            // Object options
            GUILayout.Label("GameObject: " + note.NoteName, EditorStyles.boldLabel, GUILayout.Height(16));
            note.AuthorName = EditorGUILayout.TextField("Author name", note.AuthorName, GUILayout.Width(windowWidthIncludingScrollbar - 40), GUILayout.Height(17));
            note.NoteName = EditorGUILayout.TextField("Note name", note.NoteName, GUILayout.Width(windowWidthIncludingScrollbar - 40), GUILayout.Height(17));
            note.Description = EditorGUILayout.TextField("Note description", note.Description, GUILayout.Width(windowWidthIncludingScrollbar - 40), GUILayout.Height(17));
            note.DisableBaseNoteArrows = EditorGUILayout.Toggle("Disable Base Note Arrows", note.DisableBaseNoteArrows, GUILayout.Width(windowWidthIncludingScrollbar - 40), GUILayout.Height(17));
            note.UsesNoteColor = EditorGUILayout.Toggle("Uses Note Color", note.UsesNoteColor, GUILayout.Width(windowWidthIncludingScrollbar - 40), GUILayout.Height(17));
            note.NoteColorStrength = EditorGUILayout.FloatField("Note Color Strength", note.NoteColorStrength, GUILayout.Width(windowWidthIncludingScrollbar - 40), GUILayout.Height(17));
            note.Icon = (Texture2D)EditorGUILayout.ObjectField("Cover Image", note.Icon, typeof(Texture2D), false, GUILayout.Width(windowWidthIncludingScrollbar - 40), GUILayout.Height(75));

            bool disableExportButton = false;
            if (isMissingAuthorName || isMissingNoteName
                || isMissingNoteLeft || isMissingNoteRight
                || isMissingNoteDotLeft || isMissingNoteDotRight)
            {
                disableExportButton = true;
            }

            EditorGUI.BeginDisabledGroup(disableExportButton);

            if (GUILayout.Button($"Export {note.NoteName}", GUILayout.Width(windowWidthIncludingScrollbar - 40), GUILayout.Height(20)))
            {
                GameObject noteObject = note.gameObject;
                if (noteObject != null && note != null)
                {
                    string path = EditorUtility.SaveFilePanel("Save note (bloq) file", "", $"{note.NoteName}.bloq", "bloq");

                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        string guid = $"{{{GUID.Generate()}}}";
                        string fileName = $"{Path.GetFileName(path)}_{guid}";
                        string folderPath = Path.GetDirectoryName(path);

                        Selection.activeObject = noteObject;
                        EditorUtility.SetDirty(note);
                        EditorSceneManager.MarkSceneDirty(noteObject.scene);
                        EditorSceneManager.SaveScene(noteObject.scene);
                        PrefabUtility.CreatePrefab("Assets/_CustomNote.prefab", Selection.activeObject as GameObject);
                        AssetBundleBuild assetBundleBuild = default(AssetBundleBuild);
                        assetBundleBuild.assetNames = new string[] {
                            "Assets/_CustomNote.prefab"
                        };

                        assetBundleBuild.assetBundleName = fileName;

                        BuildTargetGroup selectedBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
                        BuildTarget activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;

                        BuildPipeline.BuildAssetBundles(Application.temporaryCachePath, new AssetBundleBuild[] { assetBundleBuild }, 0, EditorUserBuildSettings.activeBuildTarget);
                        EditorPrefs.SetString("currentBuildingAssetBundlePath", folderPath);
                        EditorUserBuildSettings.SwitchActiveBuildTarget(selectedBuildTargetGroup, activeBuildTarget);
                        AssetDatabase.DeleteAsset("Assets/_CustomNote.prefab");

                        if (File.Exists(path))
                        {
                            bool isDirectory = (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
                            if (!isDirectory)
                            {
                                File.Delete(path);
                            }
                        }

                        File.Move(Path.Combine(Application.temporaryCachePath, fileName), path);
                        AssetDatabase.Refresh();
                        EditorUtility.DisplayDialog("Export Successful!", "Export Successful!", "OK");
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Export Failed!", "GameObject is missing.", "OK");
                }
            }

            EditorGUI.EndDisabledGroup();

            if (isMissingNoteLeft)
                GUILayout.Label("NoteLeft gameObject is missing", EditorStyles.boldLabel);

            if (isMissingNoteRight)
                GUILayout.Label("NoteRight gameObject is missing", EditorStyles.boldLabel);

            if (isMissingNoteDotLeft)
                GUILayout.Label("Disabling Base Game Note Arrows is enabled and NoteDotLeft gameObject is missing", EditorStyles.boldLabel);

            if (isMissingNoteDotRight)
                GUILayout.Label("Disabling Base Game Note Arrows is enabled and NoteDotRight gameObject is missing", EditorStyles.boldLabel);

            if (isMissingAuthorName)
                GUILayout.Label("Author name is empty", EditorStyles.boldLabel);

            if (isMissingNoteName)
                GUILayout.Label("Note name is empty", EditorStyles.boldLabel);

            GUILayout.Space(20);
        }

        GUI.EndScrollView();
    }

    private void OnFocus()
    {
        notes = FindObjectsOfType<NoteDescriptor>();
    }

    private int CalculateAdditionScrollSpace(NoteDescriptor note, int extraSpace = 0)
    {
        int additionalSpace = 253 + extraSpace;
        int labelHeight = 22;

        if (string.IsNullOrEmpty(note.NoteName))
            additionalSpace += labelHeight;

        if (string.IsNullOrEmpty(note.AuthorName))
            additionalSpace += labelHeight;

        if (!note.transform.Find("NoteLeft"))
            additionalSpace += labelHeight;

        if (!note.transform.Find("NoteRight"))
            additionalSpace += labelHeight;

        if (note.DisableBaseNoteArrows)
        {
            if (!note.transform.Find("NoteDotLeft"))
                additionalSpace += labelHeight;

            if (!note.transform.Find("NoteDotRight"))
                additionalSpace += labelHeight;
        }

        return additionalSpace;
    }
}
