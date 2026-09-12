using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(Rigidbody))]
public class AICarController : MonoBehaviour
{
    [Header("References")]
    public SplineContainer trackSpline;
    public Transform playerCar;

    [Header("Spline Following")]
    public float lookAheadAmount = 0.02f;

    [Header("Driving")]
    public float acceleration = 45f;
    public float maxSpeed = 55f;
    public float steeringPower = 85f;
    public float brakingPower = 40f;

    [Header("Cornering")]
    public float minimumCornerSpeedMultiplier = 0.45f;
    public float cornerAngleStart = 5f;
    public float cornerAngleMaximum = 45f;

    [Header("Drifting")]
    public float normalGrip = 14f;
    public float driftGrip = 4f;
    public float driftStartAngle = 24f;
    public float driftTurnBoost = 1.2f;

    [Header("Boost")]
    public float boostForce = 65f;
    public float boostedMaxSpeed = 68f;
    public float boostDuration = 1.1f;
    public float boostCooldown = 4f;
    public float boostStraightAngle = 8f;
    public float minimumBoostSpeed = 20f;

    [Header("Rubber Banding")]
    public bool useRubberBanding = true;

    [Tooltip("How far ahead the player must be before the AI gets help.")]
    public float catchUpStartGap = 0.025f;

    [Tooltip("Gap where the AI receives its maximum catch-up bonus.")]
    public float maximumCatchUpGap = 0.10f;

    [Tooltip("Maximum speed increase when the AI is far behind.")]
    public float maximumCatchUpSpeedMultiplier = 1.15f;

    [Tooltip("Maximum acceleration increase when the AI is far behind.")]
    public float maximumCatchUpAccelerationMultiplier = 1.12f;

    [Tooltip("How much the AI slows down when far ahead.")]
    public float aheadSpeedMultiplier = 0.94f;

    [Header("Stability")]
    public float downforce = 30f;

    private Rigidbody rb;
    private float currentProgress;
    private bool drivingEnabled = true;

    private bool isDrifting;
    private bool isBoosting;
    private float boostTimer;
    private float boostCooldownTimer;

    private float rubberBandSpeedMultiplier = 1f;
    private float rubberBandAccelerationMultiplier = 1f;

    public bool IsDrifting => isDrifting;
    public bool IsBoosting => isBoosting;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        FindNearestSplineProgress();
    }

    private void FixedUpdate()
    {
        if (!drivingEnabled || trackSpline == null)
        {
            return;
        }

        UpdateNearestProgress();
        UpdateRubberBanding();

        float allowedSpeed = FollowSpline();

        UpdateBoost();
        ApplyGrip();
        ApplyDownforce();

        if (isBoosting)
        {
            rb.AddForce(
                transform.forward * boostForce,
                ForceMode.Acceleration
            );

            allowedSpeed = boostedMaxSpeed;
        }

        allowedSpeed *= rubberBandSpeedMultiplier;

        LimitSpeed(allowedSpeed);
    }

    private void FindNearestSplineProgress()
    {
        if (trackSpline == null)
        {
            Debug.LogError("AI Car: Track Spline is not assigned.");
            enabled = false;
            return;
        }

        currentProgress = GetSplineProgress(transform.position);
    }

    private void UpdateNearestProgress()
    {
        currentProgress = GetSplineProgress(transform.position);
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

    private void UpdateRubberBanding()
    {
        rubberBandSpeedMultiplier = 1f;
        rubberBandAccelerationMultiplier = 1f;

        if (!useRubberBanding || playerCar == null)
        {
            return;
        }

        float playerProgress = GetSplineProgress(playerCar.position);

        // Positive means the player is ahead.
        // Negative means the AI is ahead.
        float signedGap =
            Mathf.DeltaAngle(
                currentProgress * 360f,
                playerProgress * 360f
            ) / 360f;

        if (signedGap > catchUpStartGap)
        {
            float catchUpAmount = Mathf.InverseLerp(
                catchUpStartGap,
                maximumCatchUpGap,
                signedGap
            );

            rubberBandSpeedMultiplier = Mathf.Lerp(
                1f,
                maximumCatchUpSpeedMultiplier,
                catchUpAmount
            );

            rubberBandAccelerationMultiplier = Mathf.Lerp(
                1f,
                maximumCatchUpAccelerationMultiplier,
                catchUpAmount
            );
        }
        else if (signedGap < -catchUpStartGap)
        {
            float slowDownAmount = Mathf.InverseLerp(
                catchUpStartGap,
                maximumCatchUpGap,
                Mathf.Abs(signedGap)
            );

            rubberBandSpeedMultiplier = Mathf.Lerp(
                1f,
                aheadSpeedMultiplier,
                slowDownAmount
            );
        }
    }

    private float FollowSpline()
    {
        float targetProgress = currentProgress + lookAheadAmount;

        if (targetProgress >= 1f)
        {
            targetProgress -= 1f;
        }

        float3 targetPoint =
            trackSpline.EvaluatePosition(targetProgress);

        Vector3 targetPosition = new Vector3(
            targetPoint.x,
            transform.position.y,
            targetPoint.z
        );

        Vector3 directionToTarget =
            targetPosition - transform.position;

        if (directionToTarget.sqrMagnitude < 0.01f)
        {
            return maxSpeed;
        }

        directionToTarget.Normalize();

        float signedAngle = Vector3.SignedAngle(
            transform.forward,
            directionToTarget,
            Vector3.up
        );

        float absoluteAngle = Mathf.Abs(signedAngle);

        isDrifting =
            absoluteAngle >= driftStartAngle &&
            rb.linearVelocity.magnitude > 8f;

        float steeringInput = Mathf.Clamp(
            signedAngle / 45f,
            -1f,
            1f
        );

        float speedFactor = Mathf.Clamp01(
            rb.linearVelocity.magnitude / 4f
        );

        float turnAmount =
            steeringInput *
            steeringPower *
            speedFactor *
            Time.fixedDeltaTime;

        if (isDrifting)
        {
            turnAmount *= driftTurnBoost;
        }

        rb.MoveRotation(
            rb.rotation *
            Quaternion.Euler(0f, turnAmount, 0f)
        );

        float cornerAmount = Mathf.InverseLerp(
            cornerAngleStart,
            cornerAngleMaximum,
            absoluteAngle
        );

        float allowedSpeed = Mathf.Lerp(
            maxSpeed,
            maxSpeed * minimumCornerSpeedMultiplier,
            cornerAmount
        );

        float forwardSpeed = Vector3.Dot(
            rb.linearVelocity,
            transform.forward
        );

        float activeAcceleration =
            acceleration * rubberBandAccelerationMultiplier;

        if (forwardSpeed < allowedSpeed)
        {
            rb.AddForce(
                transform.forward * activeAcceleration,
                ForceMode.Acceleration
            );
        }
        else
        {
            rb.AddForce(
                -transform.forward * brakingPower,
                ForceMode.Acceleration
            );
        }

        TryStartBoost(absoluteAngle);

        return allowedSpeed;
    }

    private void TryStartBoost(float cornerAngle)
    {
        if (isBoosting || boostCooldownTimer > 0f)
        {
            return;
        }

        bool onStraight =
            cornerAngle <= boostStraightAngle;

        bool movingFastEnough =
            rb.linearVelocity.magnitude >= minimumBoostSpeed;

        if (onStraight && movingFastEnough)
        {
            isBoosting = true;
            boostTimer = boostDuration;
            boostCooldownTimer = boostCooldown;
        }
    }

    private void UpdateBoost()
    {
        if (boostCooldownTimer > 0f)
        {
            boostCooldownTimer -= Time.fixedDeltaTime;
        }

        if (!isBoosting)
        {
            return;
        }

        boostTimer -= Time.fixedDeltaTime;

        if (boostTimer <= 0f)
        {
            isBoosting = false;
        }
    }

    private void ApplyGrip()
    {
        Vector3 sidewaysVelocity =
            Vector3.Dot(
                rb.linearVelocity,
                transform.right
            ) * transform.right;

        float activeGrip =
            isDrifting ? driftGrip : normalGrip;

        rb.AddForce(
            -sidewaysVelocity * activeGrip,
            ForceMode.Acceleration
        );
    }

    private void ApplyDownforce()
    {
        rb.AddForce(
            -transform.up * downforce,
            ForceMode.Acceleration
        );
    }

    private void LimitSpeed(float allowedSpeed)
    {
        Vector3 flatVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        if (flatVelocity.magnitude <= allowedSpeed)
        {
            return;
        }

        Vector3 limitedVelocity =
            flatVelocity.normalized * allowedSpeed;

        rb.linearVelocity = new Vector3(
            limitedVelocity.x,
            rb.linearVelocity.y,
            limitedVelocity.z
        );
    }

    public void SetDrivingEnabled(bool enabled)
    {
        drivingEnabled = enabled;

        if (!enabled)
        {
            isBoosting = false;
            isDrifting = false;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}