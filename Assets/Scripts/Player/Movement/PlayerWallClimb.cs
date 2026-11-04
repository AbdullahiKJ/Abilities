using UnityEngine;

public class PlayerWallClimb : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMotor motor;
    [SerializeField] private Transform wallProbe;
    [SerializeField] private Transform cameraTransform;
    private CharacterController controller;

    [Header("Detection")]
    [SerializeField] private LayerMask climbableLayers;
    [SerializeField, Min(0.01f)] private float probeRadius = 0.15f;
    [SerializeField, Min(0.01f)] private float probeDistance = 0.8f;
    [SerializeField, Range(0f, 1f)]
    private float maximumNormalY = 0.4f;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float climbSpeed = 8f;
    [SerializeField, Min(0f)] private float stickSpeed = 2f;
    [SerializeField, Min(0f)] private float lostWallGrace = 0.15f;
    [SerializeField, Min(0f)] private float reattachDelay = 0.3f;

    private float lostWallTime;
    private float nextAttachTime;

    public bool IsActive { get; private set; }
    public Vector3 WallNormal { get; private set; }

    private void Awake()
    {
        if (motor == null)
            motor = GetComponent<PlayerMotor>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        controller = GetComponent<CharacterController>();
    }

    public bool TryFindWall(
        Vector2 moveInput,
        out Vector3 normal)
    {
        normal = Vector3.zero;

        if (Time.time < nextAttachTime ||
            moveInput.sqrMagnitude < 0.01f)
        {
            return false;
        }

        float yaw = cameraTransform != null
            ? cameraTransform.eulerAngles.y
            : transform.eulerAngles.y;

        Vector3 direction = Quaternion.Euler(0f, yaw, 0f)
            * new Vector3(moveInput.x, 0f, moveInput.y);

        return TryFindWallInDirection(direction.normalized, out normal);
    }

    public bool TryFindWallInDirection(Vector3 direction, out Vector3 normal)
    {
        normal = Vector3.zero;

        if (direction.sqrMagnitude < 0.0001f)
            return false;

        if (!Probe(direction, out RaycastHit hit))
            return false;

        normal = hit.normal;
        return true;
    }

    public bool Begin(Vector3 normal)
    {
        if (normal.sqrMagnitude < 0.0001f)
            return false;

        WallNormal = normal.normalized;
        lostWallTime = 0f;
        IsActive = true;

        motor.SetVerticalVelocity(0f);
        return true;
    }

    public void Tick(Vector2 input, float deltaTime)
    {
        if (!IsActive)
            return;

        if (Probe(-WallNormal, out RaycastHit hit))
        {
            WallNormal = hit.normal;
            lostWallTime = 0f;
        }
        else
        {
            lostWallTime += deltaTime;

            if (lostWallTime >= lostWallGrace)
            {
                Debug.Log(
                    $"Wall lost. Probe: {wallProbe?.position}, " +
                    $"Normal: {WallNormal}",
                    this
                );

                End();
                return;
            }
        }

        Vector3 wallRight =
            Vector3.Cross(Vector3.up, WallNormal).normalized;

        Vector3 referenceRight = cameraTransform != null
            ? cameraTransform.right
            : transform.right;

        if (Vector3.Dot(wallRight, referenceRight) < 0f)
            wallRight = -wallRight;

        Vector3 wallUp =
            Vector3.ProjectOnPlane(Vector3.up, WallNormal).normalized;

        Vector3 movement = Vector3.ClampMagnitude(
            wallRight * input.x + wallUp * input.y,
            1f
        );

        motor.SuspendGravity();
        motor.SetMoveVelocity(
            movement * climbSpeed - WallNormal * stickSpeed
        );

        // Allow downward movement to leave the wall at ground level.
        if (input.y < -0.1f && motor.IsGrounded)
            End();
    }

    private bool Probe(Vector3 direction, out RaycastHit hit)
    {
        hit = default;

        if (direction.sqrMagnitude < 0.0001f)
            return false;

        direction.Normalize();

        Vector3 origin = transform.TransformPoint(
            controller.center
        );

        // Assumes an upright player root with unit world scale.
        float backoff =
            controller.radius + probeRadius + 0.1f;

        Vector3 castOrigin = origin - direction * backoff;

        bool found = Physics.SphereCast(
            castOrigin,
            probeRadius,
            direction,
            out hit,
            probeDistance + backoff,
            climbableLayers,
            QueryTriggerInteraction.Ignore
        );

        if (!found)
            return false;

        return Mathf.Abs(hit.normal.y) <= maximumNormalY;
    }

    public void End()
    {
        if (!IsActive)
            return;

        IsActive = false;
        nextAttachTime = Time.time + reattachDelay;
    }

    private void OnDisable()
    {
        End();
    }
}