using UnityEngine;

public class PlayerHover : MonoBehaviour
{
    [Header("References")]
    private PlayerMotor motor;
    private Transform cameraTransform;
    private VFXController vfx;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 8f;
    [SerializeField, Min(0f)] private float gravityMultiplier = 0.05f;
    [SerializeField, Min(0f)] private float maximumFallSpeed = 2f;

    [Header("Activation")]
    [SerializeField, Min(0f)] private float minimumHeight = 3f;
    [SerializeField] private LayerMask groundLayers;

    public bool IsActive { get; private set; }

    private void Awake()
    {
        motor = GetComponent<PlayerMotor>();
        cameraTransform = Camera.main.transform;
        vfx = GetComponent<VFXController>();
    }

    public bool CanEnter()
    {
        if (motor.IsGrounded)
            return false;

        Vector3 origin = transform.position + Vector3.up * 0.1f;

        return !Physics.Raycast(
            origin,
            Vector3.down,
            minimumHeight,
            groundLayers,
            QueryTriggerInteraction.Ignore
        );
    }

    public void Begin()
    {
        if (IsActive)
            return;

        IsActive = true;

        // Enter hover without carrying previous falling speed.
        motor.SetVerticalVelocity(0f);
        vfx.StartHoverVFX();
    }

    public void Tick(Vector2 moveInput, float deltaTime)
    {
        if (!IsActive)
            return;

        moveInput = Vector2.ClampMagnitude(moveInput, 1f);

        float yaw = cameraTransform != null
            ? cameraTransform.eulerAngles.y
            : transform.eulerAngles.y;

        Vector3 direction = Quaternion.Euler(0f, yaw, 0f)
            * new Vector3(moveInput.x, 0f, moveInput.y);

        motor.SetMoveVelocity(direction * moveSpeed);
        motor.SetGravityMultiplier(gravityMultiplier);
        motor.LimitFallingSpeed(maximumFallSpeed);
    }

    public void End()
    {
        if (!IsActive)
            return;

        IsActive = false;
        vfx.StopHoverVFX();
    }

    private void OnDisable()
    {
        End();
    }
}