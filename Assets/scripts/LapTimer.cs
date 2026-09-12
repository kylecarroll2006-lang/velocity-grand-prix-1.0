using TMPro;
using UnityEngine;

public class LapTimer : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI lapTimeText;
    public TextMeshProUGUI bestLapText;

    private float currentLapTime;
    private float totalRaceTime;
    private float bestLapTime = Mathf.Infinity;

    private bool timerRunning;

    private void Start()
    {
        if (lapTimeText != null)
        {
            lapTimeText.text = "00:00.00";
        }

        if (bestLapText != null)
        {
            bestLapText.text = "--:--.--";
        }
    }

    private void Update()
    {
        if (!timerRunning)
        {
            return;
        }

        currentLapTime += Time.deltaTime;
        totalRaceTime += Time.deltaTime;

        UpdateCurrentTimeText();
    }

    public void StartTimer()
    {
        currentLapTime = 0f;
        totalRaceTime = 0f;
        bestLapTime = Mathf.Infinity;
        timerRunning = true;

        UpdateCurrentTimeText();

        if (bestLapText != null)
        {
            bestLapText.text = "--:--.--";
        }
    }

    public void CompleteLap()
    {
        if (!timerRunning)
        {
            return;
        }

        if (currentLapTime < bestLapTime)
        {
            bestLapTime = currentLapTime;

            if (bestLapText != null)
            {
                bestLapText.text = FormatTime(bestLapTime);
            }
        }

        currentLapTime = 0f;
        UpdateCurrentTimeText();
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    public string GetFormattedRaceTime()
    {
        return FormatTime(totalRaceTime);
    }

    public string GetFormattedBestLap()
    {
        if (bestLapTime == Mathf.Infinity)
        {
            return "--:--.--";
        }

        return FormatTime(bestLapTime);
    }

    private void UpdateCurrentTimeText()
    {
        if (lapTimeText != null)
        {
            lapTimeText.text = FormatTime(currentLapTime);
        }
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        float seconds = time % 60f;

        return string.Format(
            "{0:00}:{1:00.00}",
            minutes,
            seconds
        );
    }
}