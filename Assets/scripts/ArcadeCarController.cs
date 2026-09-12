using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class ArcadeCarController : MonoBehaviour
{
    [Header("Car Stats")]
    public CarStats carStats;

    [Header("Driving")]
    public float acceleration = 60f;
    public float brakeForce = 45f;
    public float maxSpeed = 85f;
    public float steeringPower = 80f;

    [Header("Drifting")]
    public float normalGrip = 14f;
    public float driftGrip = 4f;
    public float driftTurnBoost = 1.15f;
    public float minimumDriftSpeed = 8f;
    public float minimumSidewaysSpeed = 2f;

    [Header("Stability")]
    public float downforce = 28f;

    private Rigidbody rb;

    private float steerInput;
    private float accelerateInput;
    private float brakeInput;

    private bool driftButtonHeld;
    private bool nitroButtonHeld;
    private bool drivingEnabled = true;

    public bool IsDrifting { get; private set; }
    public bool IsNitroPressed => nitroButtonHeld;

    public float CurrentSpeed
    {
        get
        {
            if (rb == null)
            {
                return 0f;
            }

            return rb.linearVelocity.magnitude;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ApplyCarStats();
    }

    private void Update()
    {
        if (!drivingEnabled)
        {
            return;
        }

        ReadKeyboard();
        ReadController();
    }

    private void FixedUpdate()
    {
        if (!drivingEnabled)
        {
            return;
        }

        Drive();
        Steer();
        ApplyGrip();
        ApplyDownforce();
        LimitSpeed();
    }

    private void ApplyCarStats()
    {
        if (carStats == null)
        {
            return;
        }

        acceleration = carStats.acceleration;
        brakeForce = carStats.brakeForce;
        maxSpeed = carStats.maxSpeed;
        steeringPower = carStats.steeringPower;

        normalGrip = carStats.normalGrip;
        driftGrip = carStats.driftGrip;
        driftTurnBoost = carStats.driftTurnBoost;
        minimumDriftSpeed = carStats.minimumDriftSpeed;
        minimumSidewaysSpeed = carStats.minimumSidewaysSpeed;

        downforce = carStats.downforce;
    }

    private void ReadKeyboard()
    {
        steerInput = 0f;
        accelerateInput = 0f;
        brakeInput = 0f;
        driftButtonHeld = false;
        nitroButtonHeld = false;

        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.aKey.isPressed)
        {
            steerInput -= 1f;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            steerInput += 1f;
        }

        if (Keyboard.current.wKey.isPressed)
        {
            accelerateInput = 1f;
        }

        if (Keyboard.current.sKey.isPressed)
        {
            brakeInput = 1f;
        }

        driftButtonHeld =
            Keyboard.current.spaceKey.isPressed;

        nitroButtonHeld =
            Keyboard.current.leftShiftKey.isPressed;
    }

    private void ReadController()
    {
        Gamepad gamepad = Gamepad.current;

        if (gamepad == null)
        {
            return;
        }

        float controllerSteer =
            gamepad.leftStick.x.ReadValue();

        float controllerAcceleration =
            gamepad.rightTrigger.ReadValue();

        float controllerBrake =
            gamepad.leftTrigger.ReadValue();

        if (Mathf.Abs(controllerSteer) > Mathf.Abs(steerInput))
        {
            steerInput = controllerSteer;
        }

        if (controllerAcceleration > accelerateInput)
        {
            accelerateInput = controllerAcceleration;
        }

        if (controllerBrake > brakeInput)
        {
            brakeInput = controllerBrake;
        }

        // B button = drift
        if (gamepad.buttonEast.isPressed)
        {
            driftButtonHeld = true;
        }

        // A button = nitro
        if (gamepad.buttonSouth.isPressed)
        {
            nitroButtonHeld = true;
        }
    }

    private void Drive()
    {
        float forwardSpeed =
            Vector3.Dot(
                rb.linearVelocity,
                transform.forward
            );

        if (accelerateInput > 0f)
        {
            rb.AddForce(
                transform.forward *
                acceleration *
                accelerateInput,
                ForceMode.Acceleration
            );
        }

        if (brakeInput > 0f)
        {
            if (forwardSpeed > 2f)
            {
                rb.AddForce(
                    -transform.forward *
                    brakeForce *
                    brakeInput,
                    ForceMode.Acceleration
                );
            }
            else
            {
                float reverseForce =
                    acceleration * 0.7f;

                rb.AddForce(
                    -transform.forward *
                    reverseForce *
                    brakeInput,
                    ForceMode.Acceleration
                );
            }
        }
    }

    private void Steer()
    {
        float speedFactor =
            Mathf.Clamp01(
                rb.linearVelocity.magnitude / 5f
            );

        float turnAmount =
            steerInput *
            steeringPower *
            speedFactor *
            Time.fixedDeltaTime;

        if (driftButtonHeld)
        {
            turnAmount *= driftTurnBoost;
        }

        transform.Rotate(
            0f,
            turnAmount,
            0f
        );
    }

    private void ApplyGrip()
    {
        Vector3 sidewaysVelocity =
            Vector3.Dot(
                rb.linearVelocity,
                transform.right
            ) * transform.right;

        float grip =
            driftButtonHeld
                ? driftGrip
                : normalGrip;

        rb.AddForce(
            -sidewaysVelocity * grip,
            ForceMode.Acceleration
        );

        float forwardSpeed =
            Mathf.Abs(
                Vector3.Dot(
                    rb.linearVelocity,
                    transform.forward
                )
            );

        IsDrifting =
            driftButtonHeld &&
            forwardSpeed >= minimumDriftSpeed &&
            sidewaysVelocity.magnitude >= minimumSidewaysSpeed;
    }

    private void ApplyDownforce()
    {
        rb.AddForce(
            -transform.up * downforce,
            ForceMode.Acceleration
        );
    }

    private void LimitSpeed()
    {
        float allowedSpeed = maxSpeed;

        NitroSystem nitroSystem =
            GetComponent<NitroSystem>();

        if (
            nitroSystem != null &&
            nitroSystem.IsBoosting
        )
        {
            float boostMultiplier = 1.21f;

            if (carStats != null)
            {
                boostMultiplier =
                    carStats.boostedSpeedMultiplier;
            }

            allowedSpeed =
                maxSpeed * boostMultiplier;
        }

        Vector3 flatVelocity =
            new Vector3(
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

        rb.linearVelocity =
            new Vector3(
                limitedVelocity.x,
                rb.linearVelocity.y,
                limitedVelocity.z
            );
    }

    public void ApplyNitroForce(float boostForce)
    {
        if (!drivingEnabled)
        {
            return;
        }

        rb.AddForce(
            transform.forward * boostForce,
            ForceMode.Acceleration
        );
    }

    public void SetDrivingEnabled(bool enabled)
    {
        drivingEnabled = enabled;

        if (!drivingEnabled)
        {
            steerInput = 0f;
            accelerateInput = 0f;
            brakeInput = 0f;
            driftButtonHeld = false;
            nitroButtonHeld = false;
            IsDrifting = false;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}