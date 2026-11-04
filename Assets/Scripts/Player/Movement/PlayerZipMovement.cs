using UnityEngine;

public class PlayerZipMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMotor motor;
    [SerializeField] private VFXController vfx;
    [SerializeField] private CameraController cameraController;

    [Header("Arrival")]
    [SerializeField, Min(0f)] private float arrivalTolerance = 0.2f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float elapsed;
    private float duration;

    public bool IsActive { get; private set; }
    public bool Completed { get; private set; }
    public bool TargetIsWall { get; private set; }
    public Vector3 TargetWallNormal { get; private set; }

    public bool ReachedTarget =>
        Completed &&
        Vector3.Distance(transform.position, targetPosition)
            <= arrivalTolerance;

    private void Awake()
    {
        if (motor == null)
            motor = GetComponent<PlayerMotor>();

        if (vfx == null)
            vfx = GetComponent<VFXController>();
    }

    public void Begin(
        Vector3 target,
        float travelDuration,
        bool isClimbableWall,
        Vector3 wallNormal)
    {
        Cancel();

        startPosition = transform.position;
        targetPosition = target;
        duration = Mathf.Max(0.01f, travelDuration);
        elapsed = 0f;

        TargetIsWall = isClimbableWall;
        TargetWallNormal = wallNormal.normalized;

        Completed = false;
        IsActive = true;

        motor.ClearMotion();

        vfx.StartZipVFX();
        cameraController?.PlayZipCameraEffect(duration);
    }

    public void Tick(float deltaTime)
    {
        if (!IsActive || deltaTime <= 0f)
            return;

        elapsed = Mathf.Min(elapsed + deltaTime, duration);

        float t = elapsed / duration;

        // Equivalent to an OutQuad travel curve.
        float eased = 1f - (1f - t) * (1f - t);

        Vector3 desiredPosition =
            Vector3.Lerp(startPosition, targetPosition, eased);

        motor.SuspendGravity();

        // Motor multiplies this by deltaTime.
        motor.SetMoveVelocity(
            (desiredPosition - transform.position) / deltaTime
        );

        if (elapsed >= duration)
        {
            IsActive = false;
            Completed = true;
        }
    }

    // Call after the motor has applied the final movement.
    public void FinishEffects(bool attachedToWall)
    {
        vfx.StopZipVFX(attachedToWall);
        Completed = false;
    }

    public void Cancel()
    {
        if (IsActive || Completed)
            vfx.StopZipVFX(false);

        IsActive = false;
        Completed = false;
    }

    private void OnDisable()
    {
        Cancel();
    }
}