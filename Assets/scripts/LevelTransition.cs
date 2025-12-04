using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public static class GameTimes
{
    public static float Level1Time;
    public static float Level2Time;
    public static float Level3Time;
}

public class LevelTransition : MonoBehaviour
{
    public TMP_Text messageText;
    public float displayTime = 5f;


    private void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        float levelTime = 0f;
        string nextLevel = "";

        switch (currentScene)
        {
            case "Level 1 Transition":
                levelTime = GameTimes.Level1Time;
                nextLevel = "Level 2";
                break;

            case "Level 2 Transition":
                levelTime = GameTimes.Level2Time;
                nextLevel = "Level 3";
                break;

            
        }

        if (messageText != null)
            messageText.text = $"{currentScene.Replace(" Transition", "")} Completed!\nTime: {FormatTime(levelTime)}";

        if (!string.IsNullOrEmpty(nextLevel))
            StartCoroutine(LoadNextLevel(nextLevel));
    }

    

    private IEnumerator LoadNextLevel(string nextLevel)
    {
        yield return new WaitForSeconds(displayTime);
        SceneManager.LoadScene(nextLevel);
    }

    private string FormatTime(float t)
    {
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        int milliseconds = Mathf.FloorToInt((t * 100f) % 100f);
        return $"{minutes:00}:{seconds:00}:{milliseconds:00}";
    }
}
