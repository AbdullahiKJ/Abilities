using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    private Animator animator;
    private MovementController movement;
    private StateMachine state;
    private CombatController combat;
    private InputReader input;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<MovementController>();
        state = GetComponent<StateMachine>();
        combat = GetComponent<CombatController>();
        input = GetComponent<InputReader>();
    }

    private void Update()
    {
        UpdateLocomotion();
        UpdateState();
    }

    private void UpdateLocomotion()
    {
        // Movement blend
        Vector2 move = movement != null ? input.MoveInput : Vector2.zero;
        animator.SetFloat("MoveInput", move.sqrMagnitude);

        // Grounded / airborne
        animator.SetBool("IsGrounded", state.CurrentState == StateMachine.PlayerState.Grounded);
    }

    private void UpdateState()
    {
        animator.SetBool("IsAttacking", state.CurrentState == StateMachine.PlayerState.Attacking);
        animator.SetBool("IsSwinging", state.CurrentState == StateMachine.PlayerState.Swinging);
    }

    // These can be called by CombatController if you want animation events
    public void PlayPunchAnim()
    {
        animator.SetTrigger("Punch");
    }

    public void PlayKickAnim()
    {
        animator.SetTrigger("Kick");
    }

    public void PlayGatlingAnim()
    {
        animator.SetTrigger("Gatling");
    }

    public void PlayJumpAnim()
    {
        animator.SetTrigger("Jump");
    }
}