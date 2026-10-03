using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

[RequireComponent(typeof(Rigidbody))]
public class AICarController : MonoBehaviour
{
    public SplineContainer trackSpline;

    [Header("Base Speed")]
    public float maxSpeed = 60f;
    public float acceleration = 42f;
    public float cornerSpeed = 38f;

    [Header("Steering")]
    public float steeringStrength = 4.5f;
    public float lookAhead = 0.025f;

    [Header("Drifting")]
    public float driftStrength = 0.65f;
    public float driftAngle = 25f;

    [Header("AI Boost")]
    public float boostSpeed = 70f;
    public float boostDuration = 0.8f;
    public float boostCooldown = 6f;

    [Header("Rubber Band")]
    public Transform player;
    public float catchUpDistance = 5f;
    public float catchUpSpeed = 72f;
    public float farBehindDistance = 18f;

    [Header("Passing")]
    public float passingSpeed = 68f;
    public float passingMinDistance = 3f;
    public float passingMaxDistance = 12f;

    [Header("AI Personality")]
    [Range(0f, 1f)]
    public float aggression = 0.75f;

    [Range(0f, 1f)]
    public float consistency = 0.8f;

    [Header("Runtime")]
    public float personalitySpeed;
    public float personalityAcceleration;
    public float personalityCornerSpeed;

    private Rigidbody rb;

    private float progress;
    private bool drivingEnabled = true;

    private float boostTimer;
    private float boostCooldownTimer;

    private float randomFactor;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError(
                "AI: Rigidbody is missing from " + gameObject.name
            );

            enabled = false;
            return;
        }

        rb.interpolation =
            RigidbodyInterpolation.Interpolate;
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        randomFactor =
            UnityEngine.Random.Range(-1f, 1f);

        personalitySpeed =
            maxSpeed *
            (1f + randomFactor * 0.04f);

        personalityAcceleration =
            acceleration *
            (1f + randomFactor * 0.10f);

        personalityCornerSpeed =
            cornerSpeed *
            (1f + randomFactor * 0.08f);

        aggression =
            Mathf.Clamp01(
                aggression +
                UnityEngine.Random.Range(-0.15f, 0.15f)
            );

        if (trackSpline == null)
        {
            Debug.LogError(
                "AI: Spline is NOT assigned on " +
                gameObject.name
            );
        }
    }

    // =========================================================
    // FIXED UPDATE
    // =========================================================

    private void FixedUpdate()
    {
        if (!drivingEnabled)
        {
            return;
        }

        if (trackSpline == null)
        {
            return;
        }

        if (rb == null)
        {
            return;
        }

        FindSplinePosition();

        Vector3 target =
            GetTargetPoint();

        Vector3 direction =
            target -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
        {
            return;
        }

        direction.Normalize();

        // =====================================================
        // STEERING
        // =====================================================

        Vector3 currentForward =
            transform.forward;

        currentForward.y = 0f;

        if (currentForward.sqrMagnitude < 0.01f)
        {
            return;
        }

        currentForward.Normalize();

        float turnAmount =
            Vector3.Angle(
                currentForward,
                direction
            );

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        float steering =
            steeringStrength +
            aggression * 0.8f;

        rb.MoveRotation(
            Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                steering *
                Time.fixedDeltaTime
            )
        );

        // =====================================================
        // BASE SPEED
        // =====================================================

        float targetSpeed =
            personalitySpeed;

        if (turnAmount > driftAngle)
        {
            targetSpeed =
                personalityCornerSpeed;
        }

        // =====================================================
        // PLAYER / RUBBER BAND / PASSING
        // =====================================================

        if (player != null)
        {
            float playerDistance =
                Vector3.Distance(
                    transform.position,
                    player.position
                );

            // CATCH UP
            if (playerDistance >
                catchUpDistance)
            {
                targetSpeed =
                    Mathf.Max(
                        targetSpeed,
                        catchUpSpeed
                    );
            }

            // PASSING
            if (playerDistance >
                    passingMinDistance &&
                playerDistance <
                    passingMaxDistance)
            {
                targetSpeed =
                    Mathf.Max(
                        targetSpeed,
                        passingSpeed
                    );
            }

            // FAR BEHIND = BOOST
            if (playerDistance >
                farBehindDistance)
            {
                StartBoost();
            }
        }

        // =====================================================
        // BOOST
        // =====================================================

        if (boostTimer > 0f)
        {
            boostTimer -=
                Time.fixedDeltaTime;

            targetSpeed =
                boostSpeed;
        }

        if (boostCooldownTimer > 0f)
        {
            boostCooldownTimer -=
                Time.fixedDeltaTime;
        }

        // =====================================================
        // ACCELERATION
        // =====================================================

        float currentSpeed =
            rb.linearVelocity.magnitude;

        float newSpeed =
            Mathf.MoveTowards(
                currentSpeed,
                targetSpeed,
                personalityAcceleration *
                Time.fixedDeltaTime
            );

        // =====================================================
        // DRIFT
        // =====================================================

        if (turnAmount > driftAngle)
        {
            float drift =
                Mathf.Clamp01(
                    turnAmount /
                    90f
                );

            float aiDrift =
                driftStrength *
                (
                    1f +
                    aggression * 0.35f
                );

            Vector3 velocity =
                transform.forward *
                newSpeed;

            velocity +=
                transform.right *
                drift *
                aiDrift *
                newSpeed;

            rb.linearVelocity =
                velocity;
        }
        else
        {
            rb.linearVelocity =
                transform.forward *
                newSpeed;
        }

        // =====================================================
        // HARD SPEED LIMIT
        // =====================================================

        float maxAllowedSpeed =
            personalitySpeed;

        if (player != null)
        {
            float distance =
                Vector3.Distance(
                    transform.position,
                    player.position
                );

            // Catch-up speed
            if (distance >
                catchUpDistance)
            {
                maxAllowedSpeed =
                    Mathf.Max(
                        maxAllowedSpeed,
                        catchUpSpeed
                    );
            }

            // Passing speed
            if (distance >
                    passingMinDistance &&
                distance <
                    passingMaxDistance)
            {
                maxAllowedSpeed =
                    Mathf.Max(
                        maxAllowedSpeed,
                        passingSpeed
                    );
            }
        }

        // Boost speed
        if (boostTimer > 0f)
        {
            maxAllowedSpeed =
                boostSpeed;
        }

        if (rb.linearVelocity.magnitude >
            maxAllowedSpeed)
        {
            rb.linearVelocity =
                rb.linearVelocity.normalized *
                maxAllowedSpeed;
        }
    }

    // =========================================================
    // FIND SPLINE POSITION
    // =========================================================

    private void FindSplinePosition()
    {
        if (trackSpline == null)
        {
            return;
        }

        Vector3 localPosition =
            trackSpline.transform.InverseTransformPoint(
                transform.position
            );

        SplineUtility.GetNearestPoint(
            trackSpline.Spline,

            new float3(
                localPosition.x,
                localPosition.y,
                localPosition.z
            ),

            out float3 nearestPoint,
            out progress
        );
    }

    // =========================================================
    // TARGET POINT
    // =========================================================

    private Vector3 GetTargetPoint()
    {
        if (trackSpline == null)
        {
            return transform.position;
        }

        float targetProgress =
            Mathf.Repeat(
                progress +
                lookAhead,
                1f
            );

        return trackSpline.EvaluatePosition(
            targetProgress
        );
    }

    // =========================================================
    // BOOST
    // =========================================================

    private void StartBoost()
    {
        if (boostCooldownTimer > 0f)
        {
            return;
        }

        boostTimer =
            boostDuration;

        boostCooldownTimer =
            boostCooldown;
    }

    // =========================================================
    // ENABLE / DISABLE
    // =========================================================

    public void SetDrivingEnabled(bool enabled)
    {
        drivingEnabled =
            enabled;

        // IMPORTANT:
        // RaceCountdown can call this before Start(),
        // so make sure Rigidbody exists first.

        if (!enabled && rb != null)
        {
            rb.linearVelocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;
        }
    }

    // =========================================================
    // SPLINE PROGRESS
    // =========================================================

    public float GetSplineProgress()
    {
        return progress;
    }
}