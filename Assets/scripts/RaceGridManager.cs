using UnityEngine;

public class RaceGridManager : MonoBehaviour
{
    [Header("Cars")]
    public Transform playerCar;
    public Transform aiCar;

    [Header("Grid Positions")]
    public Transform playerStart;
    public Transform aiStart;

    private Rigidbody playerRigidbody;
    private Rigidbody aiRigidbody;

    private void Awake()
    {
        if (playerCar != null)
        {
            playerRigidbody = playerCar.GetComponent<Rigidbody>();
        }

        if (aiCar != null)
        {
            aiRigidbody = aiCar.GetComponent<Rigidbody>();
        }
    }

    private void Start()
    {
        PlaceCarsOnGrid();
    }

    public void PlaceCarsOnGrid()
    {
        PlaceCar(playerCar, playerStart, playerRigidbody);
        PlaceCar(aiCar, aiStart, aiRigidbody);
    }

    private void PlaceCar(
        Transform car,
        Transform startPoint,
        Rigidbody carRigidbody)
    {
        if (car == null || startPoint == null)
        {
            return;
        }

        if (carRigidbody != null)
        {
            carRigidbody.linearVelocity = Vector3.zero;
            carRigidbody.angularVelocity = Vector3.zero;
        }

        car.SetPositionAndRotation(
            startPoint.position,
            startPoint.rotation
        );
    }
}