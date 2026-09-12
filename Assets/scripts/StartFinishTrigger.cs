using UnityEngine;

public class StartFinishTrigger : MonoBehaviour
{
    public LapManager lapManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            lapManager.CrossStartFinish();
            return;
        }

        AILapTracker aiLapTracker =
            other.GetComponentInParent<AILapTracker>();

        if (aiLapTracker != null)
        {
            aiLapTracker.CrossStartFinish();
        }
    }
}