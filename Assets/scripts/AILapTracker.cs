using UnityEngine;

public class AILapTracker : MonoBehaviour
{
    [Header("Race Settings")]
    public int currentLap = 1;
    public int totalLaps = 3;

    [Header("References")]
    public RacePositionManager positionManager;
    public RaceFinishManager raceFinishManager;

    private bool hitCheckpoint;
    private bool raceFinished;

    public void HitCheckpoint()
    {
        if (raceFinished)
        {
            return;
        }

        hitCheckpoint = true;
    }

    public void CrossStartFinish()
    {
        if (raceFinished)
        {
            return;
        }

        if (!hitCheckpoint)
        {
            return;
        }

        currentLap++;
        hitCheckpoint = false;

        if (currentLap > totalLaps)
        {
            currentLap = totalLaps;
            raceFinished = true;

            if (positionManager != null)
            {
                positionManager.SetAICurrentLap(currentLap);
            }

            if (raceFinishManager != null)
            {
                raceFinishManager.AIFinished();
            }

            return;
        }

        if (positionManager != null)
        {
            positionManager.SetAICurrentLap(currentLap);
        }

        Debug.Log(
            "AI Lap: " +
            currentLap +
            " / " +
            totalLaps
        );
    }
}