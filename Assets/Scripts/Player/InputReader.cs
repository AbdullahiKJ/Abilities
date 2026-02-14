using UnityEngine;

public class InputReader : MonoBehaviour
{
    // Player input actions
    private PlayerControls controls;

    // Store input values
    public Vector2 MoveInput { get; private set; }
    public float AimInput { get; private set; }

    // Actions that the character can perform
    public bool canAim;

    // Events other systems can subscribe to
    public event System.Action JumpPressed;
    public event System.Action PunchPressed;
    public event System.Action KickPressed;
    public event System.Action ShootPressed;

    private void Awake()
    {
        controls = new PlayerControls();

        // Assign callbacks to player controls
        controls.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

        controls.Player.Aim.performed += ctx => AimInput = 1f;
        controls.Player.Aim.canceled += ctx => AimInput = 0f;

        controls.Player.Jump.performed += ctx => JumpPressed?.Invoke();
        controls.Player.Punch.performed += ctx => PunchPressed?.Invoke();
        controls.Player.Kick.performed += ctx => KickPressed?.Invoke();
        controls.Player.Shoot.performed += ctx => ShootPressed?.Invoke();
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