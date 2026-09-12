using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class RacePositionManager : MonoBehaviour
{
    [Header("Race References")]
    public Transform playerCar;
    public Transform aiCar;
    public SplineContainer trackSpline;

    [Header("Lap References")]
    public LapManager playerLapManager;
    public int aiCurrentLap = 1;

    [Header("UI")]
    public TextMeshProUGUI positionText;

    private void Update()
    {
        if (playerCar == null ||
            aiCar == null ||
            trackSpline == null ||
            playerLapManager == null ||
            positionText == null)
        {
            return;
        }

        float playerProgress = GetSplineProgress(playerCar.position);
        float aiProgress = GetSplineProgress(aiCar.position);

        float playerRaceProgress =
            playerLapManager.currentLap + playerProgress;

        float aiRaceProgress =
            aiCurrentLap + aiProgress;

        bool playerIsAhead =
            playerRaceProgress >= aiRaceProgress;

        positionText.text =
            playerIsAhead ? "POS 1 / 2" : "POS 2 / 2";
    }

    private float GetSplineProgress(Vector3 worldPosition)
    {
        Vector3 localPosition =
            trackSpline.transform.InverseTransformPoint(worldPosition);

        SplineUtility.GetNearestPoint(
            trackSpline.Spline,
            new float3(
                localPosition.x,
                localPosition.y,
                localPosition.z
            ),
            out _,
            out float progress
        );

        return progress;
    }

    public void SetAICurrentLap(int lap)
    {
        aiCurrentLap = lap;
    }
}