using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Level 1");
    }

    public void ExitGame()
    {
        Debug.Log("[MenuManager] ExitGame called");

#if UNITY_EDITOR
        // Stop Play mode in the editor
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
    // In WebGL builds you can't quit the browser tab — show a message or redirect.
    Debug.Log("WebGL build: cannot quit application. Consider showing a 'Thanks for playing' screen.");
#else
    // In standalone builds this will actually quit the application
    Application.Quit();
#endif
    }

    public void LoadLevelSelect()
    {
        SceneManager.LoadScene("LevelSelect"); // change to the exact name of your level select scene
    }

}
