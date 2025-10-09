using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        // Replace "GameScene" with the name of your actual gameplay scene
        SceneManager.LoadScene("GameScene.unity");
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        // Try to unload all loaded scenes first
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded)
            {
                // Unload asynchronously; quitting the app will stop everything anyway.
                SceneManager.UnloadSceneAsync(scene);
            }
        }

        // If running in the Editor, stop Play Mode. In a built player, quit the application.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}