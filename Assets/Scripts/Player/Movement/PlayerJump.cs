using UnityEngine;
public class PlayerJump : MonoBehaviour
{
    [Header("References")]
    private PlayerMotor motor;
    private InputReader input;
    private Transform cameraTransform;
    private VFXController vfx;
    private AnimationController animations;

    [Header("Charge")]
    [SerializeField, Min(0.01f)] private float chargeDuration = 2f;
    [SerializeField, Min(0f)] private float minimumJumpSpeed = 5f;
    [SerializeField, Min(0f)] private float maximumJumpSpeed = 15f;

    [Header("Launch Momentum")]
    [SerializeField, Min(0f)] private float momentumDuration = 0.3f;
    [SerializeField, Range(0f, 1f)] private float airControl = 0.3f;

    [Header("Wall Jump")]
    [SerializeField, Min(0f)] private float wallJumpUpwardAmount = 0.35f;

    private float chargeTime;
    private float momentumRemaining;
    private Vector3 horizontalLaunchVelocity;

    public bool IsCharging { get; private set; }
    private void Awake()
    {
        motor = GetComponent<PlayerMotor>();
        input = GetComponent<InputReader>();
        cameraTransform = Camera.main.transform;
        vfx = GetComponent<VFXController>();
        animations = GetComponent<AnimationController>();
    }

    public void BeginCharge(MovementController.MovementMode currentMode)
    {
        if (IsCharging)
            return;

        IsCharging = true;
        chargeTime = 0f;

        vfx.StartJumpCharge();
    }

    public void Tick(float deltaTime)
    {
        if (!IsCharging)
            return;

        chargeTime += deltaTime;

        float amount = Mathf.Clamp01(
            chargeTime / chargeDuration
        );

        vfx.UpdateJumpCharge(amount);
    }

    public void ReleaseChargedJump(float heldDuration)
    {
        if (!IsCharging)
            return;

        float yaw = cameraTransform != null
            ? cameraTransform.eulerAngles.y
            : transform.eulerAngles.y;

        Vector2 moveInput = input.MoveInput;

        Vector3 direction = Quaternion.Euler(0f, yaw, 0f)
            * new Vector3(moveInput.x, 0f, moveInput.y);

        direction.Normalize();

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector3.up;

        direction = Vector3.Slerp(
            direction,
            Vector3.up,
            0.5f
        ).normalized;

        float charge = Mathf.Clamp01(
            heldDuration / chargeDuration
        );

        float speed = Mathf.Lerp(
            minimumJumpSpeed,
            maximumJumpSpeed,
            charge
        );

        Launch(direction, speed);
    }

    public void ReleaseWallJump(Vector3 wallNormal)
    {
        if (!IsCharging)
            return;

        Vector3 direction = (
            wallNormal + Vector3.up * wallJumpUpwardAmount
        ).normalized;

        // Preserves your existing full-strength wall jump.
        Launch(direction, maximumJumpSpeed);
    }

    private void Launch(Vector3 direction, float speed)
    {
        CancelCharge();

        Vector3 launchVelocity = direction * speed;

        motor.SetVerticalVelocity(launchVelocity.y);

        horizontalLaunchVelocity = new Vector3(
            launchVelocity.x,
            0f,
            launchVelocity.z
        );

        momentumRemaining = momentumDuration;

        vfx.PlayJumpExplosion();
        animations.PlayJumpAnim();
    }

    // Call after the active movement behaviour has supplied velocity.
    public void ApplyMomentum(float deltaTime)
    {
        if (momentumRemaining <= 0f)
            return;

        float progress = 1f - Mathf.Clamp01(
            momentumRemaining / Mathf.Max(momentumDuration, 0.001f)
        );

        float blend = Mathf.SmoothStep(0f, 1f, progress);

        // Launch dominates initially; input gradually regains control.
        motor.BlendHorizontalVelocity(
            horizontalLaunchVelocity * (1f - blend),
            blend
        );

        momentumRemaining =
            Mathf.Max(0f, momentumRemaining - deltaTime);
    }

    public void CancelCharge()
    {
        if (!IsCharging)
            return;

        IsCharging = false;
        chargeTime = 0f;

        vfx.StopJumpCharge();
    }

    public void CancelMomentum()
    {
        momentumRemaining = 0f;
        horizontalLaunchVelocity = Vector3.zero;
    }

    private void OnDisable()
    {
        CancelCharge();
        CancelMomentum();
    }
}