using DG.Tweening;
using UnityEngine;

public class InputReader : MonoBehaviour
{
    private MovementController movementController;
    private AbilityController abilityController;

    // Player input actions
    private PlayerControls controls;

    // Store input values
    public Vector2 MoveInput { get; private set; }
    public float AimInput { get; private set; }
    private Tween aimTween;
    public float aimTransition = 0.2f;
    private bool aimCancelled;
    public float JumpHoldDuration { get; private set; }
    private float jumpStartTime;
    private bool isJumpHeld;

    // Events other systems can subscribe to
    public event System.Action JumpStarted;
    public event System.Action<float> JumpReleased;
    public event System.Action PrimaryPressed;
    public event System.Action SecondaryPressed;
    public event System.Action ConstructPressed;
    public event System.Action ShiftPressed;
    public event System.Action ShiftReleased;
    public event System.Action AimStarted;
    public event System.Action AimReleased;
    public event System.Action SpecialPressed;
    public event System.Action<int> ConstructSelectionPressed;

    private void Awake()
    {
        // Get movement and ability controllers
        movementController = GetComponent<MovementController>();
        abilityController = GetComponent<AbilityController>();

        controls = new PlayerControls();

        // Assign callbacks to player controls

        // Update the move input based on input and reset upon cancellation
        controls.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

        // Smoothly transition in and out of the aim state and trigger the event callback
        controls.Player.Aim.performed += ctx => StartAim();
        controls.Player.Aim.canceled += ctx => ReleaseAim();

        // Assign jump pressed and released events, as well as the hold duration
        controls.Player.Jump.started += ctx => StartJumpHold();
        controls.Player.Jump.canceled += ctx => ReleaseJumpHold();

        // Assign events to each input that can be subscribed to by other scripts
        controls.Player.Primary.performed += ctx => PrimaryPressed?.Invoke();
        controls.Player.Secondary.performed += ctx => SecondaryPressed?.Invoke();
        controls.Player.Construct.performed += ctx => ConstructPressed?.Invoke();
        controls.Player.Dodge.performed += ctx => HandleDodge();
        controls.Player.Special.performed += ctx => SpecialPressed?.Invoke();

        // Assign two events for pressing and releasing the dash input
        controls.Player.Shift.performed += ctx => ShiftPressed?.Invoke();
        controls.Player.Shift.canceled += ctx => ShiftReleased?.Invoke();

        // Assign the select construct input to an event and pass the value
        controls.Player.SelectConstruct.performed += ctx =>
        {
            float value = ctx.ReadValue<float>();

            int direction = value > 0f ? 1 : -1;

            ConstructSelectionPressed?.Invoke(direction);
        };

        // Lock and hide the mouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (isJumpHeld)
            JumpHoldDuration = Time.time - jumpStartTime;
    }

    private void HandleDodge()
    {
        // Dodge acts as Cancel while aiming.
        if (abilityController.IsAiming)
        {
            aimCancelled = true;

            abilityController.CancelZip();

            // Reset the visual aiming transition.
            StartAimTween(0f);

            return;
        }

        // Otherwise, perform the normal dodge.
        movementController.TriggerDodge();
    }

    private void StartAim()
    {
        aimCancelled = false;

        StartAimTween(1f);

        AimStarted?.Invoke();
    }

    private void ReleaseAim()
    {
        StartAimTween(0f);

        // If the current aim was cancelled, ignore its release.
        if (aimCancelled)
        {
            aimCancelled = false;
            return;
        }

        AimReleased?.Invoke();
    }

    private void StartAimTween(float target)
    {
        aimTween?.Kill();

        aimTween = DOTween.To(
            () => AimInput,
            x => AimInput = x,
            target,
            aimTransition
        );
    }

    private void StartJumpHold()
    {
        isJumpHeld = true;
        jumpStartTime = Time.time;
        JumpHoldDuration = 0f;

        JumpStarted?.Invoke();
    }

    private void ReleaseJumpHold()
    {
        if (!isJumpHeld)
            return;

        JumpHoldDuration = Time.time - jumpStartTime;
        isJumpHeld = false;

        JumpReleased?.Invoke(JumpHoldDuration);
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
}