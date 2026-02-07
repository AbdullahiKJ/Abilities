using UnityEngine;

public class JumpController : MonoBehaviour
{
    public float jumpForce = 6f;

    private MovementController movement;
    private InputReader input;
    private CharacterController characterController;
    private AnimationController anim;

    private void Awake()
    {
        movement = GetComponent<MovementController>();
        input = GetComponent<InputReader>();
        characterController = GetComponent<CharacterController>();
        anim = GetComponent<AnimationController>();
    }

    private void OnEnable()
    {
        input.JumpPressed += OnJump;
    }

    private void OnDisable()
    {
        input.JumpPressed -= OnJump;
    }

    private void OnJump()
    {
        if (characterController.isGrounded)
        {
            anim.PlayJumpAnim();
            movement.AddVerticalVelocity(jumpForce);
        }
    }
}