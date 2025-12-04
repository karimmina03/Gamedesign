using UnityEngine;
using TMPro;

public class EndingSummary : MonoBehaviour
{
    public TMP_Text level1Text;
    public TMP_Text level2Text;
    public TMP_Text level3Text;

    void Start()
    {
        level1Text.text = $"Level 1: {FormatTime(GameTimes.Level1Time)}";
        level2Text.text = $"Level 2: {FormatTime(GameTimes.Level2Time)}";
        level3Text.text = $"Level 3: {FormatTime(GameTimes.Level3Time)}";
    }

    private string FormatTime(float t)
    {
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        int milliseconds = Mathf.FloorToInt((t * 100f) % 100f);
        return $"{minutes:00}:{seconds:00}:{milliseconds:00}";
    }
}
