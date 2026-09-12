using UnityEngine;

[CreateAssetMenu(
    fileName = "NewCarStats",
    menuName = "Velocity Grand Prix/Car Stats"
)]
public class CarStats : ScriptableObject
{
    [Header("Identity")]
    public string carName = "New Car";

    [Header("Driving")]
    public float acceleration = 60f;
    public float brakeForce = 45f;
    public float maxSpeed = 85f;
    public float steeringPower = 80f;

    [Header("Drifting")]
    public float normalGrip = 14f;
    public float driftGrip = 4f;
    public float driftTurnBoost = 1.2f;
    public float minimumDriftSpeed = 8f;
    public float minimumSidewaysSpeed = 2f;

    [Header("Stability")]
    public float downforce = 28f;

    [Header("Nitro")]
    public float boostForce = 85f;
    public float boostDuration = 1.25f;
    public float boostedSpeedMultiplier = 1.21f;
}