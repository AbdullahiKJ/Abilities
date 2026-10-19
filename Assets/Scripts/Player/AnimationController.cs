using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    private Animator animator;
    private MovementController movement;
    private StateMachine state;
    private CombatController combat;
    private InputReader input;
    private List<string> parameterNames = new List<string>();

    // Parameter names
    public string moveInput = "MoveInput";
    public string moveX = "MoveX";
    public string moveY = "MoveY";
    public string isAiming = "IsAiming";
    public string isAttacking = "IsAttacking";
    public string shoot = "Shoot";
    public string jump = "Jump";
    public string isFalling = "IsFalling";
    public string isGrounded = "IsGrounded";

    // Parameter hashes
    private int moveInputHash;
    private int moveXHash;
    private int moveYHash;
    private int isAimingHash;
    private int isAttackingHash;
    private int shootHash;
    private int jumpHash;
    private int isFallingHash;
    private int isGroundedHash;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<MovementController>();
        state = GetComponent<StateMachine>();
        combat = GetComponent<CombatController>();
        input = GetComponent<InputReader>();

        ValidateParameters();
    }

    private void Update()
    {
        UpdateLocomotion();
        UpdateAttackState();
    }

    private void LateUpdate()
    {
        UpdateJumpState();
    }

    private void UpdateLocomotion()
    {
        // Movement blend
        Vector2 move = movement != null ? input.MoveInput : Vector2.zero;
        animator.SetFloat(moveInputHash, move.sqrMagnitude);
        animator.SetFloat(moveXHash, move.x);
        animator.SetFloat(moveYHash, move.y);

        // Aim check
        float aim = input.AimInput;
        animator.SetFloat(isAimingHash, aim);
    }

    private void UpdateAttackState()
    {
        animator.SetBool(isAttackingHash, state.CurrentActionState == StateMachine.ActionState.Attacking);
    }

    private void UpdateJumpState()
    {
        if (movement == null)
            return;

        animator.SetBool(
            isGroundedHash,
            movement.IsGrounded
        );

        animator.SetBool(
            isFallingHash,
            movement.IsFalling
        );
    }

    // These are called by CombatController and other scripts for animation events
    public void PlayShootAnim()
    {
        animator.SetTrigger(shootHash);
    }

    public void PlayJumpAnim()
    {
        animator.SetBool(isGroundedHash, false);
        animator.SetTrigger(jumpHash);
    }

    // Create parameter hashes and check if they exist within the animator
    private void ValidateParameters()
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
            parameterNames.Add(param.name);

        moveInputHash = Animator.StringToHash(moveInput);
        moveXHash = Animator.StringToHash(moveX);
        moveYHash = Animator.StringToHash(moveY);
        isAimingHash = Animator.StringToHash(isAiming);
        isAttackingHash = Animator.StringToHash(isAttacking);
        shootHash = Animator.StringToHash(shoot);
        jumpHash = Animator.StringToHash(jump);
        isFallingHash = Animator.StringToHash(isFalling);
        isGroundedHash = Animator.StringToHash(isGrounded);

        // Debug missing parameters
        if (!parameterNames.Contains(moveInput))
            Debug.LogWarning($"Animator missing parameter: {moveInput} on {this.gameObject.name}", this);
        if (!parameterNames.Contains(moveX))
            Debug.LogWarning($"Animator missing parameter: {moveX} on {this.gameObject.name}", this);
        if (!parameterNames.Contains(moveY))
            Debug.LogWarning($"Animator missing parameter: {moveY} on {this.gameObject.name}", this);
        if (!parameterNames.Contains(isAiming))
            Debug.LogWarning($"Animator missing parameter: {isAiming} on {this.gameObject.name}", this);
        if (!parameterNames.Contains(isAttacking))
            Debug.LogWarning($"Animator missing parameter: {isAttacking} on {this.gameObject.name}", this);
        if (!parameterNames.Contains(shoot))
            Debug.LogWarning($"Animator missing parameter: {shoot} on {this.gameObject.name}", this);
        if (!parameterNames.Contains(jump))
            Debug.LogWarning($"Animator missing parameter: {jump} on {this.gameObject.name}", this);
        if (!parameterNames.Contains(isFalling))
            Debug.LogWarning($"Animator missing parameter: {isFalling} on {this.gameObject.name}", this);
        if (!parameterNames.Contains(isGrounded))
            Debug.LogWarning($"Animator missing parameter: {isGrounded} on {this.gameObject.name}", this);
    }
}