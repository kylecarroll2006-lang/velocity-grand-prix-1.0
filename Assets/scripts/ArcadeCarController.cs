using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class ArcadeCarController : MonoBehaviour
{
    public float acceleration = 60f;
    public float reverseAcceleration = 45f;
    public float brakeForce = 75f;
    public float maxSpeed = 85f;
    public float maxReverseSpeed = 25f;
    public float steeringPower = 80f;

    public float normalGrip = 14f;
    public float driftGrip = 4f;
    public float driftTurnBoost = 1.15f;

    public float minimumDriftSpeed = 8f;
    public float minimumSidewaysSpeed = 2f;

    public float downforce = 28f;

    private Rigidbody rb;

    private float steerInput;
    private float accelerateInput;
    private float brakeInput;

    private bool driftButtonHeld;
    private bool nitroButtonHeld;
    private bool drivingEnabled = true;

    public bool IsDrifting { get; private set; }

    public bool IsNitroPressed
    {
        get { return nitroButtonHeld; }
    }

    public float CurrentSpeed
    {
        get
        {
            if (rb == null)
                return 0f;

            return rb.linearVelocity.magnitude;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!drivingEnabled)
            return;

        ReadKeyboard();
        ReadController();
    }

    private void FixedUpdate()
    {
        if (!drivingEnabled)
            return;

        Drive();
        Steer();
        ApplyGrip();
        ApplyDownforce();
        LimitSpeed();
    }

    private void ReadKeyboard()
    {
        steerInput = 0f;
        accelerateInput = 0f;
        brakeInput = 0f;

        driftButtonHeld = false;
        nitroButtonHeld = false;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.aKey.isPressed)
            steerInput = -1f;

        if (Keyboard.current.dKey.isPressed)
            steerInput = 1f;

        if (Keyboard.current.wKey.isPressed)
            accelerateInput = 1f;

        if (Keyboard.current.sKey.isPressed)
            brakeInput = 1f;

        if (Keyboard.current.spaceKey.isPressed)
            driftButtonHeld = true;

        if (Keyboard.current.leftShiftKey.isPressed)
            nitroButtonHeld = true;
    }

    private void ReadController()
    {
        Gamepad gamepad = Gamepad.current;

        if (gamepad == null)
            return;

        float steering = gamepad.leftStick.x.ReadValue();
        float throttle = gamepad.rightTrigger.ReadValue();
        float brake = gamepad.leftTrigger.ReadValue();

        if (Mathf.Abs(steering) > Mathf.Abs(steerInput))
            steerInput = steering;

        if (throttle > accelerateInput)
            accelerateInput = throttle;

        if (brake > brakeInput)
            brakeInput = brake;

        if (gamepad.buttonEast.isPressed)
            driftButtonHeld = true;

        if (gamepad.buttonSouth.isPressed)
            nitroButtonHeld = true;
    }

    private void Drive()
    {
        float forwardSpeed = Vector3.Dot(
            rb.linearVelocity,
            transform.forward
        );

        // FORWARD
        if (accelerateInput > 0f)
        {
            if (forwardSpeed < maxSpeed)
            {
                rb.AddForce(
                    transform.forward *
                    acceleration *
                    accelerateInput,
                    ForceMode.Acceleration
                );
            }
        }

        // REVERSE
        if (brakeInput > 0f)
        {
            if (forwardSpeed > 0.5f)
            {
                // Brake the car while moving forward
                rb.AddForce(
                    -transform.forward *
                    brakeForce *
                    brakeInput,
                    ForceMode.Acceleration
                );
            }
            else
            {
                // Reverse
                rb.AddForce(
                    -transform.forward *
                    reverseAcceleration *
                    brakeInput,
                    ForceMode.Acceleration
                );
            }
        }
    }
    private void Steer()
    {
        if (Mathf.Abs(steerInput) < 0.01f)
            return;


float forwardSpeed = Vector3.Dot(
    rb.linearVelocity,
    transform.forward
);

        if (Mathf.Abs(forwardSpeed) < 0.1f)
            return;

        float direction = forwardSpeed >= 0f ? 1f : -1f;

        // Strong steering at low speed, slightly controlled at high speed
        float speedFactor = Mathf.Lerp(
            1.0f,
            0.75f,
            Mathf.Clamp01(Mathf.Abs(forwardSpeed) / maxSpeed)
        );

        float turnSpeed =
            steeringPower * speedFactor;

        Quaternion turnRotation = Quaternion.Euler(
            0f,
            steerInput * turnSpeed * direction * Time.fixedDeltaTime,
            0f
        );

        rb.MoveRotation(
            rb.rotation * turnRotation
        );


}


    private void ApplyGrip()
    {
        Vector3 sidewaysVelocity =
            Vector3.Dot(
                rb.linearVelocity,
                transform.right
            ) * transform.right;

        float grip = normalGrip;

        if (driftButtonHeld)
            grip = driftGrip;

        rb.AddForce(
            -sidewaysVelocity *
            grip,
            ForceMode.Acceleration
        );

        float forwardSpeed = Mathf.Abs(
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
            -transform.up *
            downforce,
            ForceMode.Acceleration
        );
    }

    private void LimitSpeed()
    {
        Vector3 velocity = rb.linearVelocity;

        float forwardSpeed = Vector3.Dot(
            velocity,
            transform.forward
        );

        if (forwardSpeed > maxSpeed)
        {
            Vector3 forwardVelocity =
                transform.forward *
                maxSpeed;

            Vector3 sidewaysVelocity =
                transform.right *
                Vector3.Dot(
                    velocity,
                    transform.right
                );

            rb.linearVelocity =
                forwardVelocity +
                sidewaysVelocity +
                Vector3.up * velocity.y;
        }

        if (forwardSpeed < -maxReverseSpeed)
        {
            Vector3 reverseVelocity =
                -transform.forward *
                maxReverseSpeed;

            Vector3 sidewaysVelocity =
                transform.right *
                Vector3.Dot(
                    velocity,
                    transform.right
                );

            rb.linearVelocity =
                reverseVelocity +
                sidewaysVelocity +
                Vector3.up * velocity.y;
        }
    }

    public void ApplyNitroForce(float boostForce)
    {
        if (!drivingEnabled)
            return;

        rb.AddForce(
            transform.forward *
            boostForce,
            ForceMode.Acceleration
        );
    }

    public void SetDrivingEnabled(bool enabled)
    {
        drivingEnabled = enabled;

        if (!enabled)
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