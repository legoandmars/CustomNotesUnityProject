using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
public class CompileNoteWindow : EditorWindow
{

    private CustomNotes.NoteDescriptor[] notes;
    [MenuItem("Window/Note Exporter")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(CompileNoteWindow), false, "Note Exporter");
    }

    private void OnFocus()
    {
        notes = GameObject.FindObjectsOfType<CustomNotes.NoteDescriptor>();
    }
    void OnGUI()
    {
        GUILayout.Label("Notes", EditorStyles.boldLabel);

        GUILayout.Space(20);

        foreach (CustomNotes.NoteDescriptor note in notes)
        {
            GUILayout.Label("GameObject : " + note.gameObject.name, EditorStyles.boldLabel);
            note.AuthorName = EditorGUILayout.TextField("Author name", note.AuthorName);
            note.NoteName = EditorGUILayout.TextField("Note name", note.NoteName);

            EditorGUI.BeginDisabledGroup(note.transform.Find("NoteLeft") == null || note.transform.Find("NoteRight") == null || (note.DisableBaseNoteArrows == true && (note.transform.Find("NoteDotLeft") == null || note.transform.Find("NoteDotRight") == null)));
            if (GUILayout.Button("Export " + note.NoteName))
            {
                GameObject noteObject = note.gameObject;
                if (noteObject != null && note != null)
                {
                    string path = EditorUtility.SaveFilePanel("Save note file", "", note.NoteName + ".bloq", "bloq");
                    Debug.Log(path == "");

                    if (path != "")
                    {
                        string fileName = Path.GetFileName(path);
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
                            File.Delete(path);
                        }
                        File.Move(Application.temporaryCachePath + "/" + fileName, path);
                        AssetDatabase.Refresh();
                        EditorUtility.DisplayDialog("Exportation Successful!", "Exportation Successful!", "OK");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Exportation Failed!", "Path is invalid.", "OK");
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Exportation Failed!", "Note GameObject is missing.", "OK");
                }
            }
            EditorGUI.EndDisabledGroup();

            if (note.transform.Find("NoteLeft") == null)
            {
                GUILayout.Label("NoteLeft gameObject is missing", EditorStyles.boldLabel);
            }
            if (note.transform.Find("NoteRight") == null)
            {
                GUILayout.Label("NoteRight gameObject is missing", EditorStyles.boldLabel);
            }
            if (note.DisableBaseNoteArrows)
            {
                if (note.transform.Find("NoteDotLeft") == null)
                {
                    GUILayout.Label("Disabling Base Game Note Arrows is enabled and NoteDotLeft gameObject is missing", EditorStyles.boldLabel);
                }
                if (note.transform.Find("NoteDotRight") == null)
                {
                    GUILayout.Label("Disabling Base Game Note Arrows is enabled and NoteDotRight gameObject is missing", EditorStyles.boldLabel);
                }
            }
            GUILayout.Space(20);
        }
    }

}
