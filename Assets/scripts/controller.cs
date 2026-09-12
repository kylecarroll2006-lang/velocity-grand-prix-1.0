using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class ControllerCarController : MonoBehaviour
{
    [Header("Input")]
    public InputActionAsset controls;

    private InputAction steerAction;
    private InputAction accelerateAction;
    private InputAction brakeAction;

    [Header("Movement")]
    public float acceleration = 35f;
    public float reverseSpeed = 20f;
    public float steering = 100f;
    public float maxSpeed = 25f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.centerOfMass = new Vector3(0f, -0.4f, 0f);
        rb.linearDamping = 1f;
        rb.angularDamping = 3f;

        var driving = controls.FindActionMap("Driving");

        steerAction = driving.FindAction("Steer");
        accelerateAction = driving.FindAction("Accelerate");
        brakeAction = driving.FindAction("Brake");
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void FixedUpdate()
    {
        float steer = steerAction.ReadValue<float>();
        float throttle = accelerateAction.ReadValue<float>();
        float brake = brakeAction.ReadValue<float>();

        // Forward
        rb.AddForce(transform.forward * throttle * acceleration, ForceMode.Acceleration);

        // Reverse / Brake
        rb.AddForce(-transform.forward * brake * reverseSpeed, ForceMode.Acceleration);

        // Steering
        if (rb.linearVelocity.magnitude > 0.5f)
        {
            transform.Rotate(0f, steer * steering * Time.fixedDeltaTime, 0f);
        }

        // Speed Limiter
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }
}