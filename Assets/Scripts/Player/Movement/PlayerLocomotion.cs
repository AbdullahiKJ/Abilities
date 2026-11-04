using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    [Header("References")]
    private PlayerMotor motor;
    private Transform cameraTransform;

    [Header("Speed")]
    [SerializeField, Min(0f)] private float regularSpeed = 6f;
    [SerializeField, Min(0f)] private float aimSpeed = 4f;
    [SerializeField, Min(0f)] private float shiftSpeed = 12f;

    [Header("Rotation")]
    [SerializeField, Min(0f)] private float rotationSpeed = 10f;

    private bool isAiming;

    private void Awake()
    {
        motor = GetComponent<PlayerMotor>();
        cameraTransform = Camera.main.transform;
    }

    public void SetAiming(bool aiming)
    {
        isAiming = aiming;
    }

    public void Tick(Vector3 moveInput, bool isShifting, float deltaTime)
    {
        moveInput = Vector2.ClampMagnitude(moveInput, 1f);

        Transform view = cameraTransform != null
            ? cameraTransform
            : transform;

        // Use camera yaw so looking up/down doesn't reduce movement.
        Quaternion cameraYaw = Quaternion.Euler(
            0f,
            view.eulerAngles.y,
            0f
        );

        Vector3 movement = cameraYaw * new Vector3(moveInput.x, 0f, moveInput.y);

        float speed = isShifting
            ? shiftSpeed
            : isAiming ? aimSpeed : regularSpeed;

        motor.SetMoveVelocity(movement * speed);

        Vector3 facingDirection = isAiming
            ? cameraYaw * Vector3.forward
            : movement;

        if (facingDirection.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(facingDirection, Vector3.up);

        float rotationBlend = 1f - Mathf.Exp(-rotationSpeed * deltaTime);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationBlend
        );
    }
}