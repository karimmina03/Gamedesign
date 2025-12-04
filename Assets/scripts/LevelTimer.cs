using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelTimer : MonoBehaviour
{
    public TMP_Text timerText;
    private float elapsedTime;
    private bool timerRunning = false;
    private float startDelay = 0f;

    void Start()
    {
        ResetTimer();
    }
    public void SetStartDelay(float delay)
    {
        startDelay = delay;
    }


    void Update()
    {
        if (timerRunning)
        {
            elapsedTime += Time.deltaTime;  // add time since last frame
            UpdateTimerUI();
        }
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int milliseconds = Mathf.FloorToInt((elapsedTime * 100f) % 100); // 0-99
        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }

    public void StartTimer()
    {
        if (startDelay > 0)
            StartCoroutine(StartTimerWithDelay());
        else
            timerRunning = true;
    }

    private IEnumerator StartTimerWithDelay()
    {
        float delay = startDelay;
        startDelay = 0; // make sure it only applies once
        yield return new WaitForSeconds(delay);
        timerRunning = true;
    }


    public void StopAndSaveTime(string levelName)
    {
        timerRunning = false;

        switch (levelName)
        {
            case "Level 1":
                GameTimes.Level1Time = elapsedTime;
                break;
            case "Level 2":
                GameTimes.Level2Time = elapsedTime;
                break;
            case "Level 3":
                GameTimes.Level3Time = elapsedTime;
                break;
        }
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateTimerUI();
    }
}
