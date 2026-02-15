using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public enum PlayerState
    {
        Grounded,
        Airborne,
        Attacking,
    }

    public PlayerState CurrentState { get; private set; }

    private CharacterController cc;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        CurrentState = PlayerState.Grounded;
    }

    private void Update()
    {
        if (!cc.isGrounded)
            CurrentState = PlayerState.Airborne;
        else if (CurrentState != PlayerState.Attacking)
            CurrentState = PlayerState.Grounded;
    }

    public void SetAttacking(bool attacking)
    {
        CurrentState = attacking ? PlayerState.Attacking : PlayerState.Grounded;
    }
}