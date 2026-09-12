using System.Collections;
using TMPro;
using UnityEngine;

public class RaceCountdown : MonoBehaviour
{
    [Header("References")]
    public ArcadeCarController carController;
    public AICarController aiCar;
    public TextMeshProUGUI countdownText;
    public LapTimer lapTimer;
    public LapManager lapManager;

    [Header("Settings")]
    public float numberDelay = 1f;
    public float goDisplayTime = 1f;

    private IEnumerator Start()
    {
        if (carController == null)
        {
            Debug.LogError("RaceCountdown: Player Car Controller is not assigned.");
            yield break;
        }

        if (aiCar == null)
        {
            Debug.LogError("RaceCountdown: AI Car Controller is not assigned.");
            yield break;
        }

        if (countdownText == null)
        {
            Debug.LogError("RaceCountdown: Countdown Text is not assigned.");
            yield break;
        }

        // Lock both cars before the countdown.
        carController.SetDrivingEnabled(false);
        aiCar.SetDrivingEnabled(false);

        countdownText.gameObject.SetActive(true);

        countdownText.text = "3";
        yield return new WaitForSeconds(numberDelay);

        countdownText.text = "2";
        yield return new WaitForSeconds(numberDelay);

        countdownText.text = "1";
        yield return new WaitForSeconds(numberDelay);

        countdownText.text = "GO!";

        // Start the race.
        carController.SetDrivingEnabled(true);
        aiCar.SetDrivingEnabled(true);

        if (lapManager != null)
        {
            lapManager.StartRace();
        }

        if (lapTimer != null)
        {
            lapTimer.StartTimer();
        }

        yield return new WaitForSeconds(goDisplayTime);

        countdownText.gameObject.SetActive(false);
    }
}