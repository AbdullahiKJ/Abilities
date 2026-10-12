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

    // External scripts/objects
    private CharacterController characterController;
    private InputReader input;
    private Camera cam;
    private VFXController vfx;
    private StateMachine state;

    // Movement parameters
    private Vector3 velocity;
    public float gravity = -9.81f;
    bool canMove = true;

    [Header("Dodge Settings")]
    public float dodgeDuration = 2f;
    public float dodgeDistance = 4f;

    [Header("Zip settings")]
    Tween zipTween;


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
    }

    // unsubscribe to dodge and dash inputs
    private void OnDisable()
    {
        input.ShiftPressed -= TriggerShift;
        input.ShiftReleased -= CancelShift;
    }

    private void Update()
    {
        // Move if allowed and the current state is free or aiming
        bool isValidState =
            state.CurrentActionState == StateMachine.ActionState.Free ||
            state.CurrentActionState == StateMachine.ActionState.Aiming;
        if (canMove && isValidState)
            Move();
        ApplyGravity();
    }

    private void Move()
    {
        Vector3 move = Quaternion.Euler(0f, cam.transform.eulerAngles.y, 0f) * new Vector3(input.MoveInput.x, 0, input.MoveInput.y);

        // Face the camera when aiming
        if (input.AimInput == 1f)
        {
            Vector3 camForward = cam.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            Quaternion targetRot = Quaternion.LookRotation(camForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
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

        characterController.Move(moveSpeed * Time.deltaTime * move);
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // apply a small downward force to keep grounded
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    public void TriggerDodge()
    {
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
        vfx.StartDashVFX();
        moveSpeed = dashSpeed;
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

    private void CancelShift()
    {
        vfx.StopDashVFX();
        moveSpeed = regularSpeed;
    }

    private void EnableMovement()
    {
        // Re-enable the can move flag
        canMove = true;
        // Return to the free state
        state.ExitState(StateMachine.ActionState.Dodging);
    }
}