using UnityEngine;

public class AILapTracker : MonoBehaviour
{
    [Header("Race Settings")]
    public int currentLap = 1;
    public int totalLaps = 3;

    [Header("References")]
    public RacePositionManager positionManager;
    public RaceFinishManager raceFinishManager;

    [Header("Checkpoint")]
    public bool requireCheckpoint = true;

    private bool hitCheckpoint;
    private bool raceFinished;

    // Prevents multiple crossings while inside the trigger
    private bool canCrossStartFinish = true;

    // =========================================================
    // CHECKPOINT
    // =========================================================

    public void HitCheckpoint()
    {
        if (raceFinished)
            return;

        hitCheckpoint = true;

        Debug.Log(
            gameObject.name +
            " hit checkpoint"
        );
    }

    // =========================================================
    // START / FINISH
    // =========================================================

    public void CrossStartFinish()
    {
        if (raceFinished)
            return;

        if (!canCrossStartFinish)
            return;

        // AI must hit checkpoint before completing a lap.
        if (requireCheckpoint && !hitCheckpoint)
        {
            Debug.Log(
                gameObject.name +
                " crossed start/finish WITHOUT checkpoint"
            );

            return;
        }

        canCrossStartFinish = false;

        // =====================================================
        // ADVANCE LAP
        // =====================================================

        currentLap++;

        // =====================================================
        // FINISHED
        // =====================================================

        if (currentLap > totalLaps)
        {
            currentLap = totalLaps;

            raceFinished = true;

            hitCheckpoint = false;

            if (positionManager != null)
            {
                positionManager.SetAICurrentLap(
                    currentLap
                );
            }

            if (raceFinishManager != null)
            {
                raceFinishManager.AIFinished();
            }

            Debug.Log(
                "AI FINISHED: " +
                gameObject.name +
                " - Lap " +
                currentLap +
                " / " +
                totalLaps
            );

            return;
        }

        // =====================================================
        // NORMAL LAP
        // =====================================================

        hitCheckpoint = false;

        if (positionManager != null)
        {
            positionManager.SetAICurrentLap(
                currentLap
            );
        }

        Debug.Log(
            "AI LAP COMPLETE: " +
            gameObject.name +
            " - Now Lap " +
            currentLap +
            " / " +
            totalLaps
        );
    }

    // =========================================================
    // RESET START / FINISH
    // =========================================================

    public void ResetStartFinishCrossing()
    {
        canCrossStartFinish = true;
    }

    // =========================================================
    // FORCE ALLOW NEXT CROSSING
    // =========================================================

    public void AllowNextCrossing()
    {
        canCrossStartFinish = true;
    }

    // =========================================================
    // GET CURRENT LAP
    // =========================================================

    public int GetCurrentLap()
    {
        return currentLap;
    }

    // =========================================================
    // FINISHED?
    // =========================================================

    public bool HasFinished()
    {
        return raceFinished;
    }

    // =========================================================
    // RESET
    // =========================================================

    public void ResetLapTracker()
    {
        currentLap = 1;
        hitCheckpoint = false;
        raceFinished = false;
        canCrossStartFinish = true;

        if (positionManager != null)
        {
            positionManager.SetAICurrentLap(
                currentLap
            );
        }

        Debug.Log(
            gameObject.name +
            " AI LAP TRACKER RESET"
        );
    }
}
