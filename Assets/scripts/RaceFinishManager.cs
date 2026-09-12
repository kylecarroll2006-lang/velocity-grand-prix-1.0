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

    private void Start()
    {
        raceFinished = false;

        if (finishPanel != null)
        {
            finishPanel.SetActive(false);
        }
    }

    public void PlayerFinished()
    {
        FinishRace("FALCON GT-R WINS!");
    }

    public void AIFinished()
    {
        FinishRace("VORTEX XR WINS!");
    }

    private void FinishRace(string winnerMessage)
    {
        if (raceFinished)
        {
            return;
        }

        raceFinished = true;

        if (playerCar != null)
        {
            playerCar.SetDrivingEnabled(false);
        }

        if (aiCar != null)
        {
            aiCar.SetDrivingEnabled(false);
        }

        if (lapTimer != null)
        {
            lapTimer.StopTimer();
        }

        if (finishPanel != null)
        {
            finishPanel.SetActive(true);
        }

        if (finishTitle != null)
        {
            finishTitle.text = "FINISH!";
        }

        if (winnerText != null)
        {
            winnerText.text = winnerMessage;
        }

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
}