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

    // Actions that the character can perform
    public bool canAim;

    // Events other systems can subscribe to
    public event System.Action JumpPressed;
    public event System.Action PunchPressed;
    public event System.Action KickPressed;
    public event System.Action ShootPressed;
    public event System.Action DodgePressed;
    public event System.Action DashPressed;
    public event System.Action DashReleased;

    private void Awake()
    {
        controls = new PlayerControls();

        // Assign callbacks to player controls
        controls.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

        controls.Player.Aim.performed += ctx => DOTween.To(() => AimInput, (x) => AimInput = x, 1f, aimTransition);
        controls.Player.Aim.canceled += ctx => DOTween.To(() => AimInput, (x) => AimInput = x, 0f, aimTransition);

        controls.Player.Jump.performed += ctx => JumpPressed?.Invoke();
        controls.Player.Punch.performed += ctx => PunchPressed?.Invoke();
        controls.Player.Kick.performed += ctx => KickPressed?.Invoke();
        controls.Player.Shoot.performed += ctx => ShootPressed?.Invoke();
        controls.Player.Dodge.performed += ctx => DodgePressed?.Invoke();

        controls.Player.Dash.performed += ctx => DashPressed?.Invoke();
        controls.Player.Dash.canceled += ctx => DashReleased?.Invoke();

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