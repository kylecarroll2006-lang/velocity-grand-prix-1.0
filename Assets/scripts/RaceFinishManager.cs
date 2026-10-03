using TMPro;
using UnityEngine;

public class RaceFinishManager : MonoBehaviour
{
    [Header("Cars")]
    public ArcadeCarController playerCar;
    public AICarController aiCar;

    [Header("Race References")]
    public LapTimer lapTimer;

    [Header("Finish UI")]
    public GameObject finishPanel;
    public TextMeshProUGUI finishTitle;
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI raceTimeText;
    public TextMeshProUGUI bestLapText;

    private bool raceFinished;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        raceFinished = false;

        if (finishPanel != null)
        {
            finishPanel.SetActive(false);
        }
    }

    // =========================================================
    // PLAYER FINISHED
    // =========================================================

    public void PlayerFinished()
    {
        if (raceFinished)
            return;

        FinishRace(
            "FALCON GT-R WINS!"
        );
    }

    // =========================================================
    // AI FINISHED
    // =========================================================

    public void AIFinished()
    {
        if (raceFinished)
            return;

        FinishRace(
            "VORTEX XR WINS!"
        );
    }

    // =========================================================
    // FINISH RACE
    // =========================================================

    private void FinishRace(
        string winnerMessage
    )
    {
        if (raceFinished)
            return;

        raceFinished = true;

        // =====================================================
        // STOP PLAYER
        // =====================================================

        if (playerCar != null)
        {
            playerCar.SetDrivingEnabled(false);
        }

        // =====================================================
        // STOP AI
        // =====================================================

        if (aiCar != null)
        {
            aiCar.SetDrivingEnabled(false);
        }

        // =====================================================
        // STOP TIMER
        // =====================================================

        if (lapTimer != null)
        {
            lapTimer.StopTimer();
        }

        // =====================================================
        // SHOW FINISH PANEL
        // =====================================================

        if (finishPanel != null)
        {
            finishPanel.SetActive(true);
        }

        if (finishTitle != null)
        {
            finishTitle.text =
                "FINISH!";
        }

        if (winnerText != null)
        {
            winnerText.text =
                winnerMessage;
        }

        // =====================================================
        // RACE TIME
        // =====================================================

        if (raceTimeText != null)
        {
            raceTimeText.text =
                "RACE TIME\n" +
                (
                    lapTimer != null
                        ? lapTimer.GetFormattedRaceTime()
                        : "--:--.--"
                );
        }

        // =====================================================
        // BEST LAP
        // =====================================================

        if (bestLapText != null)
        {
            bestLapText.text =
                "BEST LAP\n" +
                (
                    lapTimer != null
                        ? lapTimer.GetFormattedBestLap()
                        : "--:--.--"
                );
        }
    }

    // =========================================================
    // RESET
    // =========================================================

    public void ResetRace()
    {
        raceFinished = false;

        if (finishPanel != null)
        {
            finishPanel.SetActive(false);
        }

        if (winnerText != null)
        {
            winnerText.text = "";
        }
    }

    // =========================================================
    // CHECK FINISHED
    // =========================================================

    public bool IsRaceFinished()
    {
        return raceFinished;
    }
}