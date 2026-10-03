using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class RacePositionManager : MonoBehaviour
{
    [Header("Race References")]
    public Transform playerCar;
    public Transform[] aiCars;
    public SplineContainer trackSpline;

    [Header("Lap References")]
    public LapManager playerLapManager;
    public int[] aiCurrentLaps;

    [Header("UI")]
    public TextMeshProUGUI positionText;

    private class Racer
    {
        public int lap;
        public float progress;
        public float raceProgress;
        public bool isPlayer;
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (aiCars == null)
        {
            aiCars = new Transform[0];
        }

        aiCurrentLaps =
            new int[aiCars.Length];

        for (int i = 0; i < aiCurrentLaps.Length; i++)
        {
            aiCurrentLaps[i] = 1;
        }

        UpdatePositionText(1);
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (positionText == null)
            return;

        if (playerCar == null)
            return;

        if (trackSpline == null)
            return;

        if (playerLapManager == null)
            return;

        if (!playerLapManager.IsRaceStarted)
        {
            UpdatePositionText(1);
            return;
        }

        int totalRacers =
            1 + aiCars.Length;

        Racer[] racers =
            new Racer[totalRacers];

        // =====================================================
        // PLAYER
        // =====================================================

        Racer playerRacer =
            new Racer();

        playerRacer.lap =
            Mathf.Max(
                1,
                playerLapManager.currentLap
            );

        playerRacer.progress =
            GetSplineProgress(
                playerCar.position
            );

        playerRacer.raceProgress =
            (playerRacer.lap - 1) +
            playerRacer.progress;

        playerRacer.isPlayer = true;

        racers[0] =
            playerRacer;

        // =====================================================
        // AI
        // =====================================================

        for (int i = 0; i < aiCars.Length; i++)
        {
            Transform ai =
                aiCars[i];

            if (ai == null)
            {
                racers[i + 1] = null;
                continue;
            }

            Racer aiRacer =
                new Racer();

            // -------------------------------------------------
            // GET AI LAP TRACKER
            // -------------------------------------------------

            AILapTracker aiLapTracker =
                ai.GetComponentInParent<AILapTracker>();

            if (aiLapTracker != null)
            {
                aiRacer.lap =
                    Mathf.Max(
                        1,
                        aiLapTracker.GetCurrentLap()
                    );
            }
            else
            {
                // Fallback only if AI Lap Tracker is missing.
                aiRacer.lap =
                    Mathf.Max(
                        1,
                        aiCurrentLaps[i]
                    );
            }

            // -------------------------------------------------
            // GET AI SPLINE PROGRESS
            // -------------------------------------------------

            AICarController aiController =
                ai.GetComponentInParent<AICarController>();

            if (aiController != null)
            {
                aiRacer.progress =
                    Mathf.Clamp01(
                        aiController.GetSplineProgress()
                    );
            }
            else
            {
                aiRacer.progress =
                    GetSplineProgress(
                        ai.position
                    );
            }

            // -------------------------------------------------
            // TOTAL RACE PROGRESS
            // -------------------------------------------------

            aiRacer.raceProgress =
                (aiRacer.lap - 1) +
                aiRacer.progress;

            aiRacer.isPlayer = false;

            racers[i + 1] =
                aiRacer;
        }

        // =====================================================
        // SORT
        // =====================================================

        System.Array.Sort(
            racers,
            (a, b) =>
            {
                if (a == null && b == null)
                    return 0;

                if (a == null)
                    return 1;

                if (b == null)
                    return -1;

                return b.raceProgress.CompareTo(
                    a.raceProgress
                );
            }
        );

        // =====================================================
        // FIND PLAYER POSITION
        // =====================================================

        int playerPosition =
            totalRacers;

        for (int i = 0; i < racers.Length; i++)
        {
            if (racers[i] == null)
                continue;

            if (racers[i].isPlayer)
            {
                playerPosition =
                    i + 1;

                break;
            }
        }

        UpdatePositionText(
            playerPosition
        );
    }

    // =========================================================
    // SPLINE PROGRESS
    // =========================================================

    private float GetSplineProgress(
        Vector3 worldPosition
    )
    {
        if (trackSpline == null)
            return 0f;

        Vector3 localPosition =
            trackSpline.transform.InverseTransformPoint(
                worldPosition
            );

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

        return Mathf.Clamp01(progress);
    }

    // =========================================================
    // AI LAP - SINGLE AI
    // =========================================================

    public void SetAICurrentLap(int lap)
    {
        if (aiCurrentLaps == null ||
            aiCurrentLaps.Length == 0)
        {
            return;
        }

        aiCurrentLaps[0] =
            Mathf.Max(
                1,
                lap
            );
    }

    // =========================================================
    // AI LAP - MULTIPLE AI
    // =========================================================

    public void SetAICurrentLap(
        int aiIndex,
        int lap
    )
    {
        if (aiCurrentLaps == null)
            return;

        if (aiIndex < 0 ||
            aiIndex >= aiCurrentLaps.Length)
        {
            return;
        }

        aiCurrentLaps[aiIndex] =
            Mathf.Max(
                1,
                lap
            );
    }

    // =========================================================
    // UI
    // =========================================================

    private void UpdatePositionText(
        int position
    )
    {
        if (positionText == null)
            return;

        positionText.text =
            "POS " +
            position +
            " / " +
            (1 + aiCars.Length);
    }
}
