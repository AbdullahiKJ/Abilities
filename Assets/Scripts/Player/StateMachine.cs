using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public enum ActionState
    {
        None,
        Attacking
    }

    public ActionState CurrentActionState { get; private set; }

    private void Awake()
    {
        CurrentActionState = ActionState.None;
    }

    public void SetAttacking(bool attacking)
    {
        CurrentActionState = attacking ? ActionState.Attacking : ActionState.None;
    }

    // todo: change to free and add other states
    public bool IsFree => CurrentActionState == ActionState.None;

    public bool TryEnterState(ActionState newState)
    {
        if (!IsFree)
            return false;

        CurrentActionState = newState;

        return true;
    }

    public void ExitState(ActionState state)
    {
        if (CurrentActionState == state)
        {
            CurrentActionState = ActionState.None;
        }
    }
}