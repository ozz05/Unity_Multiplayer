using System;
using UnityEditor;
using UnityEditor.SceneManagement;

//This class checks before hitting the play button in the editor if the player is in the first scene if not it will load it

//All scripts inside a folder with the name Editor can use special function for the editor
//This atribute makes it that it gets initialized when the editor loads
[InitializeOnLoad]
public static class StartupSceneLoader 
{
    static StartupSceneLoader()
    {
        EditorApplication.playModeStateChanged += LoadStartupScene;
    }

    private static void LoadStartupScene(PlayModeStateChange change)
    {
        if (change == PlayModeStateChange.ExitingEditMode)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        }
        if (change == PlayModeStateChange.EnteredPlayMode)
        {
            if(EditorSceneManager.GetActiveScene().buildIndex != 0)
            {
                EditorSceneManager.LoadScene(0);
            }
        }
    }
}
