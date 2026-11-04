using UnityEngine;

public class PlayerDodge : MonoBehaviour
{
    [Header("References")]
    private PlayerMotor motor;
    private Transform cameraTransform;
    private VFXController vfx;

    [Header("Settings")]
    [SerializeField, Min(0.01f)] private float duration = 0.25f;
    [SerializeField, Min(0f)] private float distance = 4f;

    private Vector3 dodgeDirection;
    private float remainingTime;
    private float dodgeSpeed;

    public bool IsActive { get; private set; }

    private void Awake()
    {
        motor = GetComponent<PlayerMotor>();
        cameraTransform = Camera.main.transform;
        vfx = GetComponent<VFXController>();
    }

    public bool Begin(Vector2 moveInput)
    {
        if (IsActive)
            return false;

        float yaw = cameraTransform != null
            ? cameraTransform.eulerAngles.y
            : transform.eulerAngles.y;

        Quaternion cameraYaw = Quaternion.Euler(0f, yaw, 0f);

        // Dodge backward relative to the camera if no input is held.
        Vector3 inputDirection = moveInput.sqrMagnitude > 0.01f
            ? new Vector3(moveInput.x, 0f, moveInput.y)
            : Vector3.back;

        dodgeDirection = (
            cameraYaw * inputDirection
        ).normalized;

        float safeDuration = Mathf.Max(0.01f, duration);

        remainingTime = safeDuration;
        dodgeSpeed = distance / safeDuration;
        IsActive = true;

        vfx.CreateDodgeVFX(
            safeDuration,
            dodgeDirection,
            distance
        );

        return true;
    }

    public void Tick(float deltaTime)
    {
        if (!IsActive || deltaTime <= 0f)
            return;

        // Limit the final frame so we don't exceed the intended distance.
        float activeTime = Mathf.Min(deltaTime, remainingTime);

        motor.SetMoveVelocity(
            dodgeDirection * dodgeSpeed * (activeTime / deltaTime)
        );

        remainingTime -= activeTime;

        if (remainingTime <= 0f)
            IsActive = false;
    }

    public void Cancel()
    {
        IsActive = false;
        remainingTime = 0f;
        dodgeDirection = Vector3.zero;
    }

    private void OnDisable()
    {
        Cancel();
    }
}