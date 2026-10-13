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

    private void Awake()
    {
        initialRotation = transform.localRotation;
    }

    private void LateUpdate()
    {
        UpdateTilt();
        UpdateSpin();
    }

    private void UpdateTilt()
    {
        Vector2 moveInput = movement.MoveInput;

        float pitch = moveInput.y * maxTiltAngle;
        float roll = -moveInput.x * maxTiltAngle;

        Quaternion targetRotation =
            initialRotation * Quaternion.Euler(pitch, roll, 0f);

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