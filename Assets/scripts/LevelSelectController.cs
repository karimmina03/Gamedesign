using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    // Load a specific level by name
    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    // Return to main menu
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // Start the game (assuming first level is "Level1")
    public void StartGame()
    {
        SceneManager.LoadScene("scene");
    }

    // Quit the game
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in editor
#else
            Application.Quit(); // Quit the built game
#endif
    }

   
}
