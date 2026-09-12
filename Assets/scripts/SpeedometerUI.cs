using TMPro;
using UnityEngine;

public class SpeedometerUI : MonoBehaviour
{
    public Rigidbody carRigidbody;
    public TextMeshProUGUI speedText;

    private void Update()
    {
        if (carRigidbody == null || speedText == null)
        {
            return;
        }

        float speedMPH = carRigidbody.linearVelocity.magnitude * 2.237f;

        speedText.text = Mathf.RoundToInt(speedMPH).ToString();
    }
}