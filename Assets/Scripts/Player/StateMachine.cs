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
}