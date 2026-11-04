using UnityEngine;

public class MovementController : MonoBehaviour
{
    public enum MovementMode
    {
        Normal,
        Shift,
        WallClimb,
        Hover,
        Dodge,
        Jump,
        Zip
    }

    // References
    private StateMachine state;
    private InputReader input;
    private PlayerMotor motor;
    private PlayerLocomotion locomotion;
    private PlayerJump jump;
    private PlayerWallClimb wallClimb;
    private PlayerHover hover;
    private PlayerDodge dodge;
    private PlayerZipMovement zip;
    private PlayerShift shift;

    [Header("Falling Detection")]
    [SerializeField, Min(0f)] private float fallAnimationDelay = 0.1f;
    [SerializeField, Min(0f)] private float minimumFallHeight = 0.5f;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private Transform groundProbe;

    private float airborneTime;

    private bool hoverRequested;
    private bool hoverExitRequested;

    public MovementMode CurrentMovementMode { get; private set; } = MovementMode.Normal;

    public Vector2 MoveInput => input.MoveInput;
    public bool IsGrounded => motor.IsGrounded;
    public bool IsShifting => CurrentMovementMode == MovementMode.Shift;
    public bool IsWallClimbing => CurrentMovementMode == MovementMode.WallClimb;
    public bool IsHovering => CurrentMovementMode == MovementMode.Hover;
    public bool IsShifted => IsShifting || IsWallClimbing;
    public bool IsFalling =>
        !motor.IsGrounded &&
        !IsWallClimbing &&
        !IsHovering &&
        CurrentMovementMode != MovementMode.Zip &&
        airborneTime >= fallAnimationDelay &&
        !IsGroundNearby() &&
        motor.VerticalVelocity < -0.5f;

    // Subscribe to dodge and dash inputs
    private void OnEnable()
    {
        input.ShiftPressed += ToggleShift;
        input.JumpStarted += HandleJumpStart;
        input.JumpReleased += HandleJumpRelease;
    }

    // unsubscribe to dodge and dash inputs
    private void OnDisable()
    {
        input.ShiftPressed -= ToggleShift;
        input.JumpStarted -= HandleJumpStart;
        input.JumpReleased -= HandleJumpRelease;
    }

    private void Awake()
    {
        state = GetComponent<StateMachine>();
        input = GetComponent<InputReader>();
        motor = GetComponent<PlayerMotor>();
        locomotion = GetComponent<PlayerLocomotion>();
        jump = GetComponent<PlayerJump>();
        wallClimb = GetComponent<PlayerWallClimb>();
        hover = GetComponent<PlayerHover>();
        dodge = GetComponent<PlayerDodge>();
        zip = GetComponent<PlayerZipMovement>();
        shift = GetComponent<PlayerShift>();
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // Reset frame-specific requests, not persistent velocity.
        motor.BeginFrame();

        UpdateTransitions();

        // Updates charging, queued jumps and related timers.
        jump.Tick(dt);

        switch (CurrentMovementMode)
        {
            case MovementMode.Normal:
                bool canWalk =
                    state.CurrentActionState == StateMachine.ActionState.Free ||
                    state.CurrentActionState == StateMachine.ActionState.Aiming;

                if (canWalk)
                    locomotion.Tick(input.MoveInput, false, dt);
                break;

            case MovementMode.Shift:
                locomotion.Tick(input.MoveInput, true, dt);
                break;

            case MovementMode.WallClimb:
                wallClimb.Tick(input.MoveInput, dt);
                break;

            case MovementMode.Hover:
                hover.Tick(input.MoveInput, dt);
                break;

            case MovementMode.Dodge:
                dodge.Tick(dt);
                break;

            case MovementMode.Zip:
                zip.Tick(dt);
                break;
        }

        bool isAttacking = state.CurrentActionState == StateMachine.ActionState.Attacking;

        if (!isAttacking &&
            (CurrentMovementMode == MovementMode.Normal ||
             CurrentMovementMode == MovementMode.Shift))
        {
            jump.ApplyMomentum(dt);
        }

        // Applies the selected movement, gravity and external forces.
        motor.Tick(dt);

        if (CurrentMovementMode == MovementMode.Dodge && !dodge.IsActive)
        {
            SetMode(MovementMode.Normal);
        }

        if (motor.IsGrounded || IsGroundNearby())
            airborneTime = 0f;
        else
            airborneTime += dt;

        if (CurrentMovementMode == MovementMode.WallClimb && !wallClimb.IsActive)
        {
            SetMode(MovementMode.Shift);
        }

        if (CurrentMovementMode == MovementMode.Zip && zip.Completed)
        {
            bool attach = false;

            if (zip.TargetIsWall &&
                wallClimb.TryFindWallInDirection(
                    -zip.TargetWallNormal,
                    out Vector3 normal))
            {
                bool matchingSurface =
                    Vector3.Dot(normal, zip.TargetWallNormal) > 0.7f;

                if (matchingSurface)
                    attach = wallClimb.Begin(normal);
            }

            zip.FinishEffects(attach);

            SetMode(
                attach
                    ? MovementMode.WallClimb
                    : MovementMode.Normal
            );
        }
    }

    private void UpdateTransitions()
    {
        if (IsShifting && wallClimb.TryFindWall(input.MoveInput, out Vector3 normal))
        {
            if (wallClimb.Begin(normal))
                SetMode(MovementMode.WallClimb);
        }
        else if (IsHovering && IsGrounded)
        {
            SetMode(MovementMode.Normal);
        }
    }

    private void ToggleShift()
    {
        if (IsShifted)
        {
            // SetMode will reject standing if space is blocked.
            SetMode(MovementMode.Normal);
            return;
        }

        if (!CanStartMovementAbility())
            return;

        if (!state.TryEnterState(StateMachine.ActionState.MovementAbility))
            return;

        SetMode(MovementMode.Shift);
    }

    private void HandleJumpStart()
    {
        hoverRequested = false;
        hoverExitRequested = false;

        if (IsHovering)
        {
            hoverExitRequested = true;
            return;
        }

        // Normal airborne movement can activate hover.
        if (CurrentMovementMode == MovementMode.Normal &&
            state.IsFree &&
            motor.VerticalVelocity < 0f &&
            hover.CanEnter())
        {
            hoverRequested = true;
            return;
        }

        // Actual jump launches remain restricted to sand form.
        if (IsShifted &&
            state.CurrentActionState == StateMachine.ActionState.MovementAbility)
        {
            jump.BeginCharge(CurrentMovementMode);
        }
    }

    private void HandleJumpRelease(float holdDuration)
    {
        if (hoverExitRequested)
        {
            hoverExitRequested = false;

            if (IsHovering)
                SetMode(MovementMode.Normal);

            return;
        }

        if (hoverRequested)
        {
            hoverRequested = false;

            // Recheck: conditions may have changed while holding the button.
            if (CurrentMovementMode != MovementMode.Normal ||
                motor.VerticalVelocity >= 0f ||
                !hover.CanEnter())
            {
                return;
            }

            if (!state.TryEnterState(StateMachine.ActionState.MovementAbility))
                return;

            if (!SetMode(MovementMode.Hover))
            {
                state.ExitState(StateMachine.ActionState.MovementAbility);
            }

            return;
        }

        // Existing shift-only jump release checks and launch code follow.
        if (!IsShifted || state.CurrentActionState != StateMachine.ActionState.MovementAbility)
        {
            jump.CancelCharge();
            return;
        }

        if (IsWallClimbing)
        {
            Vector3 normal = wallClimb.WallNormal;

            // Leave climbing before launching, otherwise its gravity
            // suspension can erase the jump's vertical velocity.
            SetMode(MovementMode.Shift);

            jump.BeginCharge(CurrentMovementMode);
            jump.ReleaseWallJump(normal);
            return;
        }
        jump.ReleaseChargedJump(holdDuration);
    }

    public void ToggleAimMovement(bool isAiming)
    {
        locomotion.SetAiming(isAiming);
    }

    public void ApplyKnockback(Vector3 velocity)
    {
        jump.CancelCharge();
        jump.CancelMomentum();

        // May fail under a low ceiling; keep the compact collider.
        SetMode(MovementMode.Normal);

        motor.AddImpulse(velocity);
    }

    public void StartZip(
    Vector3 target,
    float duration,
    bool isClimbableWall,
    Vector3 normal)
    {
        jump.CancelCharge();
        jump.CancelMomentum();

        if (!SetMode(MovementMode.Zip))
        {
            // ZipAbility already entered this action state.
            state.ExitState(StateMachine.ActionState.Zipping);
            return;
        }

        zip.Begin(target, duration, isClimbableWall, normal);
    }

    public void TriggerDodge()
    {
        if (CurrentMovementMode != MovementMode.Normal)
            return;

        // Reserve the action before starting the movement.
        if (!state.TryEnterState(StateMachine.ActionState.Dodging))
            return;

        jump.CancelCharge();
        jump.CancelMomentum();

        if (!dodge.Begin(input.MoveInput))
        {
            state.ExitState(StateMachine.ActionState.Dodging);
            return;
        }

        SetMode(MovementMode.Dodge);
    }

    private void CancelActiveMovement()
    {
        jump.CancelCharge();
        hoverRequested = false;
        hoverExitRequested = false;
        SetMode(MovementMode.Normal);
    }

    private bool SetMode(MovementMode next)
    {
        if (next == CurrentMovementMode)
            return true;

        bool wasShifted = UsesShiftForm(CurrentMovementMode);
        bool willBeShifted = UsesShiftForm(next);

        if (shift.IsActive && !willBeShifted && !shift.TryEnd())
            return false;

        jump.CancelCharge();

        ExitMode(CurrentMovementMode);
        CurrentMovementMode = next;

        if (wasShifted && !willBeShifted)
        {
            state.ExitState(StateMachine.ActionState.MovementAbility);
        }

        if (willBeShifted)
        {
            shift.Begin();

            // ToggleShift already reserves this state.
            // Zip-to-wall attachment needs to reserve it after exiting Zipping.
            if (state.IsFree)
            {
                state.TryEnterState(
                    StateMachine.ActionState.MovementAbility
                );
            }
        }

        EnterMode(next);
        return true;
    }

    private bool CanStartMovementAbility()
    {
        return CurrentMovementMode == MovementMode.Normal;
    }

    private static bool UsesShiftForm(MovementMode mode)
    {
        return mode == MovementMode.Shift ||
               mode == MovementMode.WallClimb;
    }

    private void EnterMode(MovementMode mode)
    {
        // Start the relevant module and its effects.
        switch (mode)
        {
            case MovementMode.Hover:
                jump.CancelMomentum();
                hover.Begin();
                break;
        }
    }

    private bool IsGroundNearby()
    {
        Vector3 origin = groundProbe != null
            ? groundProbe.position
            : transform.position + Vector3.up * 0.1f;

        return Physics.Raycast(
            origin,
            Vector3.down,
            minimumFallHeight + 0.1f,
            groundLayers,
            QueryTriggerInteraction.Ignore
        );
    }

    private void ExitMode(MovementMode mode)
    {
        switch (mode)
        {
            case MovementMode.WallClimb:
                wallClimb.End();
                break;

            case MovementMode.Hover:
                hover.End();
                state.ExitState(StateMachine.ActionState.MovementAbility);
                break;

            case MovementMode.Dodge:
                dodge.Cancel();
                state.ExitState(StateMachine.ActionState.Dodging);
                break;

            case MovementMode.Zip:
                zip.Cancel();
                state.ExitState(StateMachine.ActionState.Zipping);
                break;
        }
    }
}