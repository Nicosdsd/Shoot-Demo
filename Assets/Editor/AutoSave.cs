using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class AutoSave
{
    private static float SaveIntervalMinutes = 5f;
    private static double NextSaveTimestamp;

    static AutoSave()
    {
        EditorApplication.update += OnEditorApplicationUpdate;
        NextSaveTimestamp = GetTimestampInMinutes() + SaveIntervalMinutes;
        Debug.Log("[AutoSave] AutoSave initialized. Saving every " + SaveIntervalMinutes + " minutes.");
    }

    private static void OnEditorApplicationUpdate()
    {
        if (!EditorApplication.isPlaying && GetTimestampInMinutes() >= NextSaveTimestamp)
        {
            SaveAssetsAndScene();
            NextSaveTimestamp = GetTimestampInMinutes() + SaveIntervalMinutes;
        }
    }

    private static void SaveAssetsAndScene()
    {
        Debug.Log("[AutoSave] Performing auto save...");

        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveOpenScenes();

        EditorUtility.ClearProgressBar();
        Debug.LogFormat("[AutoSave] Auto save completed at {0}", System.DateTime.Now.ToString("T"));
    }

    private static double GetTimestampInMinutes()
    {
        return EditorApplication.timeSinceStartup / 60.0;
    }
}