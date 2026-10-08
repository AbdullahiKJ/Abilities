using DG.Tweening;
using UnityEngine;

public class InputReader : MonoBehaviour
{
    // Player input actions
    private PlayerControls controls;

    // Store input values
    public Vector2 MoveInput { get; private set; }
    public float AimInput { get; private set; }
    public float aimTransition = 0.2f;

    // Events other systems can subscribe to
    public event System.Action JumpPressed;
    public event System.Action PrimaryPressed;
    public event System.Action SecondaryPressed;
    public event System.Action ConstructPressed;
    public event System.Action DodgePressed;
    public event System.Action ShiftPressed;
    public event System.Action ShiftReleased;

    private void Awake()
    {
        controls = new PlayerControls();

        // Assign callbacks to player controls

        // Update the move input based on input and reset upon cancellation
        controls.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

        // Smoothly transition in and out of the aim state
        controls.Player.Aim.performed += ctx => DOTween.To(() => AimInput, (x) => AimInput = x, 1f, aimTransition);
        controls.Player.Aim.canceled += ctx => DOTween.To(() => AimInput, (x) => AimInput = x, 0f, aimTransition);

        // Assign events to each input that can be subscribed to by other scripts
        controls.Player.Jump.performed += ctx => JumpPressed?.Invoke();
        controls.Player.Primary.performed += ctx => PrimaryPressed?.Invoke();
        controls.Player.Secondary.performed += ctx => SecondaryPressed?.Invoke();
        controls.Player.Construct.performed += ctx => ConstructPressed?.Invoke();
        controls.Player.Dodge.performed += ctx => DodgePressed?.Invoke();

        // Assign two events for pressing and releasing the dash input
        controls.Player.Shift.performed += ctx => ShiftPressed?.Invoke();
        controls.Player.Shift.canceled += ctx => ShiftReleased?.Invoke();

        // Lock and hide the mouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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