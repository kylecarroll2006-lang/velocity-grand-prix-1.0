using System.Collections.Generic;
using UnityEngine;

public class StartFinishTrigger : MonoBehaviour
{
    [Header("Player")]
    public LapManager lapManager;

    [Header("AI")]
    public AILapTracker[] aiLapTrackers;

    // Prevents multiple colliders from the same car
    // triggering the line at the same time.
    private HashSet<GameObject> carsInside =
        new HashSet<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        // =====================================================
        // PLAYER
        // =====================================================

        if (other.CompareTag("Player"))
        {
            if (lapManager != null)
            {
                lapManager.CrossStartFinish();
            }

            return;
        }

        // =====================================================
        // AI
        // =====================================================

        AILapTracker ai =
            other.GetComponentInParent<AILapTracker>();

        if (ai == null)
        {
            return;
        }

        GameObject aiRoot =
            ai.gameObject;

        // Don't allow multiple colliders on the same AI
        // to trigger the finish line repeatedly.
        if (carsInside.Contains(aiRoot))
        {
            return;
        }

        carsInside.Add(aiRoot);

        ai.CrossStartFinish();
    }

    // =========================================================
    // EXIT
    // =========================================================

    private void OnTriggerExit(Collider other)
    {
        // =====================================================
        // AI
        // =====================================================

        AILapTracker ai =
            other.GetComponentInParent<AILapTracker>();

        if (ai != null)
        {
            GameObject aiRoot =
                ai.gameObject;

            if (carsInside.Contains(aiRoot))
            {
                carsInside.Remove(aiRoot);
            }

            ai.ResetStartFinishCrossing();

            return;
        }

        // =====================================================
        // PLAYER
        // =====================================================

        if (other.CompareTag("Player"))
        {
            return;
        }
    }
}