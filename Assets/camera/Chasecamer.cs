using UnityEngine;

public class ChaseCamera : MonoBehaviour
{
    public Transform target;


[Header("Position")]
    public float distance = 7f;
    public float height = 2.2f;
    public float positionSmooth = 15f;

    [Header("Rotation")]
    public float rotationSmooth = 10f;

    [Header("Look")]
    public float lookAhead = 2f;
    public float lookHeight = 0.8f;

    private Rigidbody targetRb;

    private void Start()
    {
        if (target != null)
            targetRb = target.GetComponent<Rigidbody>();

        if (targetRb != null)
            targetRb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        // Use the car's interpolated rotation
        Vector3 forward = target.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            return;

        forward.Normalize();

        // Desired camera position
        Vector3 desiredPosition =
            target.position
            - forward * distance
            + Vector3.up * height;

        // Smooth toward the position
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            positionSmooth * Time.deltaTime
        );

        // Force the camera to NEVER exceed the desired distance
        Vector3 flatOffset = transform.position - target.position;
        flatOffset.y = 0f;

        if (flatOffset.sqrMagnitude > 0.001f)
        {
            flatOffset =
                flatOffset.normalized * distance;

            transform.position =
                target.position
                + flatOffset
                + Vector3.up * height;
        }

        // Look ahead of the car
        Vector3 lookPosition =
            target.position
            + forward * lookAhead
            + Vector3.up * lookHeight;

        Vector3 direction =
            lookPosition - transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion desiredRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                rotationSmooth * Time.deltaTime
            );
        }
    }


}
