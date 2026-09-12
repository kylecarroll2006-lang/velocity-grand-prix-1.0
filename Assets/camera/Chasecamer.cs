using UnityEngine;

public class ChaseCamera : MonoBehaviour
{
    public Transform target;

    public Vector3 offset = new Vector3(0f, 3f, -6f);

    public float followSpeed = 8f;
    public float rotationSpeed = 8f;

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = target.position + target.TransformDirection(offset);

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        Vector3 lookTarget = target.position + Vector3.up * 1f;

        Quaternion desiredRotation = Quaternion.LookRotation(
            lookTarget - transform.position
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}