using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public enum ActionState
    {
        Free,
        Attacking,
        Dodging,
        Aiming,
        Zipping,
        MovementAbility
    }

    // States       Movement    Attack      Dodge 
    // Free         Yes         Yes         Yes
    // Attacking    No          X           No
    // Dodging      No          No          X
    // Aiming       Yes         No          No
    // Zipping      No          No          No
    // Movement     Yes         No          No

    public ActionState CurrentActionState { get; private set; }
    public bool CanAttack =>
        CurrentActionState == ActionState.Free ||
        CurrentActionState == ActionState.Attacking;

    private void Awake()
    {
        CurrentActionState = ActionState.Free;
    }

    public void SetAttacking(bool attacking)
    {
        CurrentActionState = attacking ? ActionState.Attacking : ActionState.Free;
    }

    public bool IsFree => CurrentActionState == ActionState.Free;

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
            CurrentActionState = ActionState.Free;
        }
    }
}