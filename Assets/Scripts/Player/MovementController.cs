using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 10f;

    private CharacterController characterController;
    private InputReader input;
    private Camera cam;

    private Vector3 velocity;
    public float gravity = -9.81f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        input = GetComponent<InputReader>();
        cam = Camera.main;
    }

    private void Update()
    {
        Move();
        ApplyGravity();
    }

    private void Move()
    {
        Vector3 move = Quaternion.Euler(0f, cam.transform.eulerAngles.y, 0f) * new Vector3(input.MoveInput.x, 0, input.MoveInput.y);

        // Face the camera when aiming
        if (input.canAim && input.AimInput == 1f)
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
            velocity.y = -2f; // small downward force to keep grounded
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    // Called by JumpController
    public void AddVerticalVelocity(float amount)
    {
        velocity.y = amount;
    }
}