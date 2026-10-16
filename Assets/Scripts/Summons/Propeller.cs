using UnityEngine;

public class Propeller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MovementController movement;
    [SerializeField] private Transform blades;

    [Header("Tilt")]
    [SerializeField] private float maxTiltAngle = 20f;
    [SerializeField] private float tiltSmoothSpeed = 6f;

    [Header("Spin")]
    [SerializeField] private float rotationSpeed = 720f;

    private Quaternion initialRotation;
    private Transform cameraTransform;

    private void Awake()
    {
        initialRotation = transform.localRotation;
        cameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        UpdateTilt();
        UpdateSpin();
    }

    private void UpdateTilt()
    {
        if (!movement.IsHovering)
        {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                initialRotation,
                tiltSmoothSpeed * Time.deltaTime
            );

            return;
        }

        Vector2 moveInput = movement.MoveInput;

        // Calculate movement direction relative to the camera.
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 worldMoveDirection =
            cameraForward * moveInput.y +
            cameraRight * moveInput.x;

        worldMoveDirection = Vector3.ClampMagnitude(
            worldMoveDirection,
            1f
        );

        // Convert world movement into the propeller parent's local space.
        Vector3 localMoveDirection =
            transform.parent.InverseTransformDirection(worldMoveDirection);

        float pitch = localMoveDirection.z * maxTiltAngle;
        float roll = -localMoveDirection.x * maxTiltAngle;

        Quaternion targetRotation =
            initialRotation * Quaternion.Euler(pitch, 0f, roll);

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            tiltSmoothSpeed * Time.deltaTime
        );
    }

    private void UpdateSpin()
    {
        if (blades == null)
        {
            Debug.LogError("Blades transform is not assigned!");
            return;
        }
        blades.Rotate(
            Vector3.forward,
            rotationSpeed * Time.deltaTime,
            Space.Self
        );
    }
}