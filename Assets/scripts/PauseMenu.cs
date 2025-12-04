using UnityEngine;
using UnityEngine.SceneManagement;
// REMOVED UnityEditor line (It causes errors in built games)

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void ResumeGame()
    {
        Debug.Log("resume");

        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        // NEW: Lock cursor back to game so you can drive
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RestartLevel()
    {
        Debug.Log("restart");

        Time.timeScale = 1f;
        // NEW: Reloads scene by index (safer than name)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitToMainMenu()
    {
        Debug.Log("exit");

        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        // NEW: UNLOCK CURSOR (Critical for buttons to work)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}