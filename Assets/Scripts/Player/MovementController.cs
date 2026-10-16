using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementController : MonoBehaviour
{
    [Header("Movement")]
    public float regularSpeed = 6f;
    public float aimSpeed = 4f;
    public float dashSpeed = 12f;
    private float moveSpeed;
    public float rotationSpeed = 10f;

    [Header("Jump Settings")]
    [SerializeField] float holdDuration = 2f;
    [SerializeField] float minJumpForce;
    [SerializeField] float maxJumpForce;
    private bool isPreparingJump;
    // Wall jump settings
    [SerializeField, Range(0f, 1f)] private float wallJumpUpwardAmount = 0.35f;
    [SerializeField] private float wallJumpControlDelay = 0.2f;
    [SerializeField] private float wallJumpAirControl = 0.3f;
    private Vector3 jumpHorizontalVelocity;
    private float jumpControlTimer;

    // External scripts/objects
    private CharacterController characterController;
    private InputReader input;
    private Camera cam;
    private VFXController vfx;
    private StateMachine state;
    private AnimationController anim;

    // Movement parameters
    private Vector3 velocity;
    public float VerticalSpeed => characterController.velocity.y;
    [SerializeField] float gravity = -9.81f;
    bool canMove = true;
    public Vector2 MoveInput => input.MoveInput;

    [Header("Dodge Settings")]
    public float dodgeDuration = 2f;
    public float dodgeDistance = 4f;

    [Header("Zip settings")]
    Tween zipTween;
    public enum MovementMode
    {
        Normal,

        Shift,
        WallClimbing,
        Hover
    }
    [Header("Player state")]
    public MovementMode CurrentMovementMode { get; private set; } = MovementMode.Normal;
    public bool IsGrounded => isGrounded;
    public bool IsHovering => CurrentMovementMode == MovementMode.Hover;
    public bool IsShifting => CurrentMovementMode == MovementMode.Shift;
    public bool IsWallClimbing => CurrentMovementMode == MovementMode.WallClimbing;

    [Header("Wall Climbing")]
    [SerializeField] private LayerMask climbableLayer;
    [SerializeField] private Transform wallDetectionPoint;
    [SerializeField] private float wallCastRadius = 0.15f;
    [SerializeField] private float wallDetectionDistance = 1f;
    [SerializeField] private float wallClimbSpeed = 8f;
    [SerializeField] private float wallStickSpeed = 2f;
    private Vector3 wallNormal;
    [SerializeField] private float wallLostGraceTime = 0.15f;
    private float wallLostTimer;

    [Header("Wall Top")]
    [SerializeField] private float ledgeForwardDistance = 0.8f;
    [SerializeField] private float ledgeProbeHeight = 1.5f;
    [SerializeField] private float ledgeCastDistance = 3f;

    [Header("Hover")]
    [SerializeField] private float minimumHoverHeight = 3f;
    [SerializeField] float hoverGravity = -1f;
    [SerializeField] private float hoverMaxFallSpeed = 2f;
    public float hoverMoveSpeed = 8f;

    private enum PendingJumpType
    {
        None,
        ChargedJump,
        WallJump,
        Hover,
        ExitHover
    }
    private PendingJumpType pendingJump;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.15f;
    [SerializeField] private float groundCheckRadiusMultiplier = 0.9f;
    private bool isGrounded;

    [Header("Tail")]
    [SerializeField] private Transform tail;
    [SerializeField] private LayerMask surfaceLayer;
    [SerializeField] private float surfaceDetectionDistance = 2f;
    [SerializeField] private float tailSubmergeDepth = 0.15f;
    [SerializeField] private float tailPositionSpeed = 15f;
    [SerializeField] private float tailRotationSpeed = 12f;
    private Vector3 tailOriginalLocalPosition;
    private Quaternion tailOriginalLocalRotation;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        input = GetComponent<InputReader>();
        cam = Camera.main;
        vfx = GetComponent<VFXController>();
        state = GetComponent<StateMachine>();
        anim = GetComponent<AnimationController>();

        // Set move speed
        moveSpeed = regularSpeed;

        tailOriginalLocalPosition = tail.localPosition;
        tailOriginalLocalRotation = tail.localRotation;
    }

    // Subscribe to dodge and dash inputs
    private void OnEnable()
    {
        input.ShiftPressed += TriggerShift;
        input.ShiftReleased += CancelShift;
        input.JumpStarted += HandleJumpStart;
        input.JumpReleased += HandleJumpRelease;
    }

    // unsubscribe to dodge and dash inputs
    private void OnDisable()
    {
        input.ShiftPressed -= TriggerShift;
        input.ShiftReleased -= CancelShift;
        input.JumpStarted -= HandleJumpStart;
        input.JumpReleased -= HandleJumpRelease;
    }

    private void Update()
    {
        bool isZipping = state.CurrentActionState == StateMachine.ActionState.Zipping;

        // Ignore movements and gravity if zipping
        if (isZipping)
            return;

        // Move if allowed and the current state is free or aiming
        bool isValidState =
            state.CurrentActionState == StateMachine.ActionState.Free ||
            state.CurrentActionState == StateMachine.ActionState.Aiming ||
            state.CurrentActionState == StateMachine.ActionState.MovementAbility;

        Vector3 movement = Vector3.zero;

        // Detect walls while shifting.
        if (IsShifting && TryDetectWall(out RaycastHit wallHit))
        {
            EnterWallClimb(wallHit);
        }

        if (IsWallClimbing)
        {
            bool wallDetected = IsWallStillValid();

            if (wallDetected)
            {
                wallLostTimer = 0f;
            }
            else
            {
                wallLostTimer += Time.deltaTime;

                if (wallLostTimer >= wallLostGraceTime)
                {
                    if (input.MoveInput.y > 0.1f &&
                        TryDetectWallTop(out Vector3 targetPosition))
                    {
                        TransitionToWallTop(targetPosition);
                    }
                    else
                    {
                        Debug.Log("Exiting wall climb: wall lost");
                        ExitSandMovement();
                    }

                    return;
                }
            }

            if (canMove && isValidState)
                movement = CalculateWallMovement();

            // No gravity while climbing.
            velocity.y = 0f;

            characterController.Move(movement * Time.deltaTime);

            CheckGrounded();

            // Transition back to ground Shift when descending.
            if (input.MoveInput.y < -0.1f && IsGrounded)
            {
                ExitWallClimb();
            }

            return;
        }

        if (canMove && isValidState)
            movement = CalculateMovement();

        ApplyGravity();

        // Preserve horizontal wall jump momentum briefly.
        if (jumpControlTimer > 0f)
        {
            jumpControlTimer -= Time.deltaTime;

            movement = jumpHorizontalVelocity + movement * wallJumpAirControl;
        }
        else
        {
            jumpHorizontalVelocity = Vector3.zero;
        }

        // Combine horizontal and vertical velocity.
        Vector3 totalVelocity = movement + Vector3.up * velocity.y;

        // Move the CharacterController ONCE.
        characterController.Move(totalVelocity * Time.deltaTime);

        // Update grounded state after moving.
        CheckGrounded();

        // Exit the hover state upon landing
        if (IsGrounded && IsHovering)
            ExitHover();
    }

    private void LateUpdate()
    {
        if (IsShifting || IsWallClimbing)
        {
            AlignTailToSurface();
        }
    }

    private Vector3 CalculateMovement()
    {
        Vector3 move = Quaternion.Euler(0f, cam.transform.eulerAngles.y, 0f) * new Vector3(input.MoveInput.x, 0, input.MoveInput.y);

        // Prevent diagonal movement from being faster.
        move = Vector3.ClampMagnitude(move, 1f);

        if (IsHovering)
        {
            // Don't rotate if hovering
        }
        // Face the camera when aiming
        else if (input.AimInput >= 0.99f)
        {
            Vector3 camForward = cam.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            if (camForward.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(camForward);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }
        // Rotate towards the move direction otherwise
        else
        {
            if (move.sqrMagnitude > 0.01f)
            {
                // Rotate toward movement direction relative to the camera
                Quaternion targetRot = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }

        return move * moveSpeed;
    }

    private void ApplyGravity()
    {
        if (IsGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // apply a small downward force to keep grounded
        }
        else if (IsHovering)
        {
            velocity.y += hoverGravity * Time.deltaTime;
            // Prevent falling faster than the hover limit.
            velocity.y = Mathf.Max(
                velocity.y,
                -hoverMaxFallSpeed
            );
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
    }

    public void TriggerDodge()
    {
        // Ignore if the player is not in the normal state
        if (CurrentMovementMode != MovementMode.Normal)
            return;

        // Determine the dash direction (dash backwards if there is no move input)
        Vector3 direction;
        if (input.MoveInput.magnitude > 0.05f)
            direction = Quaternion.Euler(0f, cam.transform.eulerAngles.y, 0f) * new Vector3(input.MoveInput.x, 0, input.MoveInput.y);
        else
            direction = Quaternion.Euler(0f, cam.transform.eulerAngles.y, 0f) * new Vector3(0, 0, -1);

        // Instantiate the prefab, destroy after it's duration has elapsed
        vfx.CreateDodgeVFX(dodgeDuration, direction, dodgeDistance);

        // Move the player to the position
        characterController.Move(direction * dodgeDistance);

        // Temporarily disable movement by entering the dodge state
        state.TryEnterState(StateMachine.ActionState.Dodging);
        Invoke(nameof(EnableMovement), dodgeDuration);
    }

    private void TriggerShift()
    {
        // Exit early if not grounded
        if (!IsGrounded || isPreparingJump)
        {
            return;
        }

        // Prevent shift activation while the jump sequence is running
        if (CurrentMovementMode != MovementMode.Normal)
        {
            return;
        }

        if (!state.TryEnterState(StateMachine.ActionState.MovementAbility))
            return;

        CurrentMovementMode = MovementMode.Shift;
        moveSpeed = dashSpeed;

        vfx.StartDashVFX();
    }

    private void CancelShift()
    {
        if ((!IsShifting && !IsWallClimbing) || isPreparingJump)
            return;
        CurrentMovementMode = MovementMode.Normal;
        moveSpeed = regularSpeed;
        velocity.y = -2f;

        state.ExitState(StateMachine.ActionState.MovementAbility);

        ResetTailTransform();

        vfx.StopDashVFX();
    }

    public void ToggleAimMovement(bool isAiming)
    {
        if (isAiming)
            moveSpeed = aimSpeed;
        else
            moveSpeed = regularSpeed;
    }

    public void StartZip(Vector3 target, float duration, bool onSurface = false)
    {
        // Kill the current tween
        zipTween?.Kill();

        // todo: play animation

        // todo: Enter the zip state

        // todo: play vfx and hide mesh

        // Start the movement tween 
        Vector3 previousPosition = transform.position;
        Vector3 tweenPosition = previousPosition;
        zipTween = DOTween.To(
            () => tweenPosition,
            value =>
            {
                tweenPosition = value;

                // Calculate the movement since the previous tween update.
                Vector3 delta = tweenPosition - previousPosition;

                // Move through the CharacterController.
                characterController.Move(delta);

                // Update the previous position
                previousPosition = transform.position;
            },
            target,
            duration
        )
        .SetEase(Ease.OutQuad)
        .OnComplete(() => state.ExitState(StateMachine.ActionState.Zipping));

        // todo: transition to the shift state if target is a surface
        if (onSurface)
            return;
        // todo: otherwise, end vfx and show mesh
        else
            return;
    }

    private void HandleJumpStart()
    {
        pendingJump = PendingJumpType.None;

        if (state.CurrentActionState == StateMachine.ActionState.Zipping)
            return;

        if (IsHovering)
        {
            pendingJump = PendingJumpType.ExitHover;
            return;
        }

        if (IsWallClimbing)
        {
            // todo: add charge indicator
            pendingJump = PendingJumpType.WallJump;
            return;
        }

        if (IsShifting)
        {
            // todo: add charge indicator
            pendingJump = PendingJumpType.ChargedJump;
            return;
        }

        if (!IsGrounded && IsHighAboveGround())
        {
            pendingJump = PendingJumpType.Hover;
        }
    }

    private void HandleJumpRelease(float holdDuration)
    {
        // Actions such as zipping should take priority.
        if (state.CurrentActionState == StateMachine.ActionState.Zipping)
            return;

        switch (pendingJump)
        {
            case PendingJumpType.ChargedJump:
                if (IsShifting)
                    PerformChargedJump(holdDuration);
                break;

            case PendingJumpType.WallJump:
                if (IsWallClimbing)
                    PerformWallJump(holdDuration);
                break;

            case PendingJumpType.Hover:
                if (!IsGrounded && IsHighAboveGround())
                    EnterHover();
                break;

            case PendingJumpType.ExitHover:
                ExitHover();
                break;
        }

        pendingJump = PendingJumpType.None;
    }

    private float CalculateJumpForce(float duration)
    {
        float chargeRatio = Mathf.Clamp01(duration / holdDuration);

        return Mathf.Lerp(
            minJumpForce,
            maxJumpForce,
            chargeRatio
        );
    }

    private void ExecuteChargedJump(
    float duration,
    Vector3 direction,
    bool preserveHorizontalMomentum = false)
    {
        float jumpForce = CalculateJumpForce(duration);

        direction.Normalize();

        // Exit sand movement.
        ExitSandMovement();

        // Apply vertical jump velocity.
        velocity.y = direction.y * jumpForce;

        // Apply horizontal jump velocity.
        jumpHorizontalVelocity = new Vector3(
            direction.x,
            0f,
            direction.z
        ) * jumpForce;

        // Temporarily preserve wall jump momentum.
        jumpControlTimer = preserveHorizontalMomentum
            ? wallJumpControlDelay
            : 0f;

        // Handle the visual transition.
        vfx.PlayJumpExplosion();

        // Play the jump start animation
        anim.PlayJumpAnim();
    }

    private void PerformChargedJump(float duration)
    {
        if (!IsShifting)
            return;

        // Get the player's current movement direction.
        Vector3 moveDirection =
            Quaternion.Euler(0f, cam.transform.eulerAngles.y, 0f) *
            new Vector3(input.MoveInput.x, 0f, input.MoveInput.y);

        // Prevent diagonal input from affecting the direction.
        moveDirection = moveDirection.normalized;

        // If there's no movement input, jump straight up.
        if (moveDirection.sqrMagnitude < 0.01f)
        {
            moveDirection = Vector3.up;
        }

        // Calculate the direction halfway between movement and upward.
        Vector3 jumpDirection = Vector3.Slerp(
            moveDirection,
            Vector3.up,
            0.5f
        ).normalized;

        ExecuteChargedJump(
            duration,
            Vector3.up
        );
    }

    private void PerformWallJump(float duration)
    {
        if (!IsWallClimbing)
            return;

        Vector3 direction = (
            wallNormal +
            Vector3.up * wallJumpUpwardAmount
        ).normalized;

        // Always perform the maximum jump force
        ExecuteChargedJump(
            holdDuration,
            direction,
            preserveHorizontalMomentum: true
        );
    }

    private void EnterWallClimb(RaycastHit hit)
    {
        if (!IsShifting)
            return;

        wallNormal = hit.normal;
        wallLostTimer = 0f;

        CurrentMovementMode = MovementMode.WallClimbing;

        moveSpeed = wallClimbSpeed;

        // Prevent existing falling velocity from carrying into wall climbing.
        velocity.y = 0f;

        Debug.Log("Entered Wall Climb");
    }

    private void ExitWallClimb()
    {
        if (!IsWallClimbing)
            return;

        CheckGrounded();

        if (!IsGrounded)
        {
            ExitSandMovement();
            return;
        }

        CurrentMovementMode = MovementMode.Shift;
        moveSpeed = dashSpeed;
        velocity.y = -2f;

        ResetTailTransform();

        Debug.Log("Exited Wall Climb");
    }

    private void ExitSandMovement()
    {
        if (!IsShifting && !IsWallClimbing)
            return;

        CurrentMovementMode = MovementMode.Normal;
        moveSpeed = regularSpeed;

        ResetTailTransform();

        state.ExitState(StateMachine.ActionState.MovementAbility);

        vfx.StopDashVFX();
    }

    private Vector3 CalculateWallMovement()
    {
        Vector2 moveInput = input.MoveInput;

        // Horizontal direction along the wall.
        Vector3 wallRight = Vector3.Cross(Vector3.up, wallNormal).normalized;

        // Keep horizontal movement consistent with camera orientation.
        if (Vector3.Dot(wallRight, cam.transform.right) < 0f)
            wallRight = -wallRight;

        Vector3 movement =
            wallRight * moveInput.x +
            Vector3.up * moveInput.y;

        movement = Vector3.ClampMagnitude(movement, 1f);

        // Apply a small force toward the wall to maintain contact.
        movement += -wallNormal * wallStickSpeed / wallClimbSpeed;

        return movement * wallClimbSpeed;
    }

    private void AlignTailToSurface()
    {
        Vector3 surfaceNormal = IsWallClimbing
            ? wallNormal
            : Vector3.up;

        Vector3 castOrigin = tail.parent.TransformPoint(
            tailOriginalLocalPosition
        ) + surfaceNormal * 1f;

        Vector3 castDirection = -surfaceNormal;

        if (!Physics.SphereCast(
            castOrigin,
            0.1f,
            castDirection,
            out RaycastHit hit,
            surfaceDetectionDistance,
            surfaceLayer,
            QueryTriggerInteraction.Ignore))
        {
            return;
        }

        surfaceNormal = hit.normal;

        // Place the tail slightly beneath the surface.
        Vector3 targetPosition =
            hit.point - surfaceNormal * tailSubmergeDepth;

        tail.position = Vector3.Lerp(
            tail.position,
            targetPosition,
            Mathf.Clamp01(tailPositionSpeed * Time.deltaTime)
        );

        // Get movement direction relative to the camera.
        Vector3 moveDirection = Quaternion.Euler(
            0f,
            cam.transform.eulerAngles.y,
            0f
        ) * new Vector3(
            input.MoveInput.x,
            0f,
            input.MoveInput.y
        );

        if (IsWallClimbing)
        {
            Vector3 wallRight = Vector3.Cross(
                Vector3.up,
                wallNormal
            ).normalized;

            if (Vector3.Dot(wallRight, cam.transform.right) < 0f)
                wallRight = -wallRight;

            moveDirection =
                wallRight * input.MoveInput.x +
                Vector3.up * input.MoveInput.y;
        }

        Vector3 surfaceForward = Vector3.ProjectOnPlane(
            moveDirection,
            surfaceNormal
        );

        if (surfaceForward.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(
                surfaceForward.normalized,
                surfaceNormal
            );

            tail.rotation = Quaternion.Slerp(
                tail.rotation,
                targetRotation,
                Mathf.Clamp01(tailRotationSpeed * Time.deltaTime)
            );
        }
    }

    private void ResetTailTransform()
    {
        tail.localPosition = tailOriginalLocalPosition;
        tail.localRotation = tailOriginalLocalRotation;
    }

    private bool IsWallStillValid()
    {
        Vector3 origin = wallDetectionPoint.position;

        float detectionDistance = characterController.radius + wallDetectionDistance;

        // First check for walls when the detection sphere
        // is already overlapping the surface.
        Collider[] overlaps = Physics.OverlapSphere(
            origin,
            wallCastRadius,
            climbableLayer,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider col in overlaps)
        {
            Vector3 closestPoint = col.ClosestPoint(origin);

            Vector3 direction = origin - closestPoint;

            // When the point is inside the collider, ClosestPoint
            // may return the original point.
            if (direction.sqrMagnitude < 0.0001f)
            {
                return true;
            }

            Vector3 normal = direction.normalized;

            if (Mathf.Abs(normal.y) < 0.2f &&
                Vector3.Dot(normal, wallNormal) > 0.5f)
            {
                wallNormal = normal;
                return true;
            }
        }

        // Otherwise, check for a wall in front of the sphere.
        if (Physics.SphereCast(
            origin,
            wallCastRadius,
            -wallNormal,
            out RaycastHit hit,
            detectionDistance,
            climbableLayer,
            QueryTriggerInteraction.Ignore))
        {
            if (Mathf.Abs(hit.normal.y) < 0.2f)
            {
                wallNormal = hit.normal;
                return true;
            }
        }

        return false;
    }

    private bool TryDetectWall(out RaycastHit hit)
    {
        Vector3 moveDirection =
            Quaternion.Euler(0f, cam.transform.eulerAngles.y, 0f) *
            new Vector3(input.MoveInput.x, 0f, input.MoveInput.y);

        moveDirection.y = 0f;

        if (moveDirection.sqrMagnitude < 0.01f)
        {
            hit = default;
            return false;
        }

        moveDirection.Normalize();

        return Physics.SphereCast(
            wallDetectionPoint.position,
            wallCastRadius,
            moveDirection,
            out hit,
            characterController.radius + wallDetectionDistance,
            climbableLayer,
            QueryTriggerInteraction.Ignore
        )
        && Mathf.Abs(hit.normal.y) < 0.2f;
    }

    private bool TryDetectWallTop(out Vector3 targetPosition)
    {
        targetPosition = Vector3.zero;

        Vector3 intoWall = -wallNormal;

        Vector3 origin =
            transform.position +
            Vector3.up * ledgeProbeHeight +
            intoWall * ledgeForwardDistance;

        if (!Physics.SphereCast(
            origin,
            wallCastRadius,
            Vector3.down,
            out RaycastHit hit,
            ledgeCastDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

        if (slopeAngle > characterController.slopeLimit)
            return false;

        // Assumes transform.position is at the player's feet.
        targetPosition = hit.point + Vector3.up * 0.05f;

        return true;
    }

    private void TransitionToWallTop(Vector3 targetPosition)
    {
        // Move upward first so the capsule clears the ledge.
        Vector3 verticalDelta = Vector3.up *
            Mathf.Max(0f, targetPosition.y - transform.position.y);

        characterController.Move(verticalDelta);

        // Then move horizontally onto the upper surface.
        Vector3 horizontalDelta = targetPosition - transform.position;
        horizontalDelta.y = 0f;

        characterController.Move(horizontalDelta);

        CheckGrounded();

        if (IsGrounded)
        {
            ExitWallClimb();
        }
        else
        {
            ExitSandMovement();
        }
    }

    private bool IsHighAboveGround()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        return !Physics.Raycast(
            origin,
            Vector3.down,
            minimumHoverHeight
        );
    }

    private void EnterHover()
    {
        if (IsGrounded)
            return;

        SetMovementMode(MovementMode.Hover);

        // Remove accumulated falling speed.
        velocity.y = 0f;

        // Change the movement speed
        moveSpeed = hoverMoveSpeed;

        // todo: play animation

        // Play vfx
        vfx.StartHoverVFX();
    }

    private void ExitHover()
    {
        if (!IsHovering)
            return;

        SetMovementMode(MovementMode.Normal);

        // Change the movement speed
        moveSpeed = regularSpeed;

        // todo: play animation

        // Play vfx
        vfx.StopHoverVFX();
    }

    private void SetMovementMode(MovementMode newMode)
    {
        if (CurrentMovementMode == newMode)
            return;

        MovementMode previousMode = CurrentMovementMode;

        CurrentMovementMode = newMode;

        // Handle visual changes, movement settings, etc.
    }

    private void EnableMovement()
    {
        // Re-enable the can move flag
        canMove = true;
        // Return to the free state
        state.ExitState(StateMachine.ActionState.Dodging);
    }

    private void CheckGrounded()
    {
        Vector3 center = transform.TransformPoint(characterController.center);

        float radius = characterController.radius * groundCheckRadiusMultiplier;
        float halfHeight = Mathf.Max(characterController.height * 0.5f, radius);

        // Start at the centre of the capsule's lower hemisphere.
        Vector3 origin = center + Vector3.down * (halfHeight - radius);

        isGrounded = Physics.SphereCast(
            origin,
            radius,
            Vector3.down,
            out RaycastHit hit,
            groundCheckDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        // Optional: reject surfaces that are too steep to stand on.
        if (isGrounded)
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            if (slopeAngle > characterController.slopeLimit)
                isGrounded = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (wallDetectionPoint == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            wallDetectionPoint.position,
            wallCastRadius
        );

        if (Application.isPlaying && IsWallClimbing)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                wallDetectionPoint.position,
                wallDetectionPoint.position
                    - wallNormal *
                    (characterController.radius + wallDetectionDistance)
            );
        }
    }
}