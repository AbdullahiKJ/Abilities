using System.Collections;
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

    // External scripts/objects
    private CharacterController characterController;
    private InputReader input;
    private Camera cam;
    private VFXController vfx;
    private StateMachine state;

    // Movement parameters
    private Vector3 velocity;
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

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        input = GetComponent<InputReader>();
        cam = Camera.main;
        vfx = GetComponent<VFXController>();
        state = GetComponent<StateMachine>();

        // Set move speed
        moveSpeed = regularSpeed;
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

        Vector3 horizontalVelocity = Vector3.zero;

        if (canMove && isValidState)
            horizontalVelocity = CalculateMovement();

        ApplyGravity();

        // Combine horizontal and vertical velocity.
        Vector3 totalVelocity = horizontalVelocity + Vector3.up * velocity.y;

        // Move the CharacterController ONCE.
        characterController.Move(totalVelocity * Time.deltaTime);

        // Update grounded state after moving.
        CheckGrounded();

        // Exit the hover state upon landing
        if (IsGrounded && IsHovering)
            ExitHover();
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
        if (!IsShifting || isPreparingJump)
            return;
        CurrentMovementMode = MovementMode.Normal;
        moveSpeed = regularSpeed;

        state.ExitState(StateMachine.ActionState.MovementAbility);

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

    private void PerformChargedJump(float duration)
    {
        if (!IsShifting)
            return;

        float chargeRatio = Mathf.Clamp01(duration / holdDuration);

        // Calculate jump velocity.
        float jumpForce = Mathf.Lerp(
            minJumpForce,
            maxJumpForce,
            chargeRatio
        );

        // Exit Shift mode.
        CurrentMovementMode = MovementMode.Normal;
        moveSpeed = regularSpeed;

        state.ExitState(StateMachine.ActionState.MovementAbility);

        // Apply the jump immediately.
        velocity.y = jumpForce;

        // Handle the visual transition.
        vfx.StopDashVFX();
        vfx.PlayJumpExplosion();
    }

    private void PerformWallJump(float duration)
    {

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
}