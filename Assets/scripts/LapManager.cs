
using TMPro;
using UnityEngine;

public class LapManager : MonoBehaviour
{
    [Header("Race Settings")]
    public int totalLaps = 3;
    public int currentLap = 1;

    [Header("References")]
    public LapTimer lapTimer;
    public RaceFinishManager raceFinishManager;

    [Header("UI")]
    public TextMeshProUGUI lapText;

    private bool raceStarted;
    private bool hitCheckpoint;
    private bool raceFinished;

    public bool IsRaceStarted => raceStarted;

    private void Start()
    {
        UpdateLapText();
    }

    public void StartRace()
    {
        currentLap = 1;
        raceStarted = true;
        hitCheckpoint = false;
        raceFinished = false;

        UpdateLapText();
    }

    public void HitCheckpoint()
    {
        if (!raceStarted || raceFinished)
            return;

        hitCheckpoint = true;

        Debug.Log("PLAYER CHECKPOINT HIT");
    }

    public void CrossStartFinish()
    {
        if (!raceStarted || raceFinished)
            return;

        if (!hitCheckpoint)
        {
            Debug.Log("PLAYER START/FINISH REJECTED - CHECKPOINT NOT HIT");
            return;
        }

        if (lapTimer != null)
        {
            lapTimer.CompleteLap();
        }

        currentLap++;
        hitCheckpoint = false;

        if (currentLap > totalLaps)
        {
            currentLap = totalLaps;
            raceFinished = true;

            if (lapText != null)
            {
                lapText.text = "FINISH";
            }

            if (lapTimer != null)
            {
                lapTimer.StopTimer();
            }

            if (raceFinishManager != null)
            {
                raceFinishManager.PlayerFinished();
            }

            return;
        }

        UpdateLapText();
    }

    private void UpdateLapText()
    {
        if (lapText != null)
        {
            lapText.text = currentLap + " / " + totalLaps;
        }
    }
}