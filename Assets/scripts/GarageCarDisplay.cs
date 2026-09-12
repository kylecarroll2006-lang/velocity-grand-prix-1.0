using UnityEngine;

public class GarageCarDisplay : MonoBehaviour
{
    public float rotationSpeed = 20f;

    private void Update()
    {
        transform.Rotate(
            0f,
            rotationSpeed * Time.deltaTime,
            0f
        );
    }
}