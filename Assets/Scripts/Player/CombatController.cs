using UnityEngine;
using System.Collections.Generic;

public class CombatController : MonoBehaviour
{
    public float gatlingTimeWindow = 0.5f;
    public int gatlingThreshold = 3;

    private InputReader input;
    private StateMachine state;
    private AnimationController anim;
    private Animator animator;

    private List<float> recentPunchTimes = new List<float>();

    private void Awake()
    {
        input = GetComponent<InputReader>();
        state = GetComponent<StateMachine>();
        anim = GetComponent<AnimationController>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        input.PunchPressed += OnPunch;
        input.KickPressed += OnKick;
    }

    private void OnDisable()
    {
        input.PunchPressed -= OnPunch;
        input.KickPressed -= OnKick;
    }

    private void OnPunch()
    {
        if (state.CurrentState == StateMachine.PlayerState.Swinging)
            return;

        recentPunchTimes.Add(Time.time);

        // Remove old inputs
        recentPunchTimes.RemoveAll(t => Time.time - t > gatlingTimeWindow);

        if (recentPunchTimes.Count >= gatlingThreshold)
        {
            TriggerGatling();
        }
        else
        {
            TriggerBasicPunch();
        }
    }

    private void OnKick()
    {
        if (state.CurrentState == StateMachine.PlayerState.Swinging)
            return;

        TriggerKick();
    }

    // Animation events
    public void OnPunchAnimationEvent()
    {
        animator.SetLayerWeight(1, 0f);
    }
    public void OnGatlingAnimationEvent()
    {
    }
    public void OnKickAnimationEvent()
    {
    }

    // Attack triggers
    private void TriggerBasicPunch()
    {
        if (state.CurrentState == StateMachine.PlayerState.Attacking)
            return;

        state.SetAttacking(true);
        anim.PlayPunchAnim();
        Invoke(nameof(EndAttack), 0.4f);
    }

    private void TriggerGatling()
    {
        if (state.CurrentState == StateMachine.PlayerState.Attacking)
            return;

        state.SetAttacking(true);
        anim.PlayGatlingAnim();
        Invoke(nameof(EndAttack), 1.2f);
    }

    private void TriggerKick()
    {
        if (state.CurrentState == StateMachine.PlayerState.Attacking)
            return;

        state.SetAttacking(true);
        anim.PlayKickAnim();
        Invoke(nameof(EndAttack), 0.5f);
    }

    private void EndAttack()
    {
        state.SetAttacking(false);
        animator.SetLayerWeight(1, 1f);
    }
}