using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public LapManager lapManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            lapManager.HitCheckpoint();
            return;
        }

        AILapTracker aiLapTracker =
            other.GetComponentInParent<AILapTracker>();

        if (aiLapTracker != null)
        {
            aiLapTracker.HitCheckpoint();
        }
    }
}