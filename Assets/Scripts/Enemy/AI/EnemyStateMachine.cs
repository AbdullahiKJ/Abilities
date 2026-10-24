using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    public EnemyState CurrentState { get; private set; }

    public void ChangeState(EnemyState newState)
    {
        if (newState == null || newState == CurrentState)
            return;

        CurrentState?.Exit();

        CurrentState = newState;

        CurrentState.Enter();
    }

    private void Update()
    {
        CurrentState?.Tick(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        CurrentState?.FixedTick(Time.fixedDeltaTime);
    }

    private void OnDisable()
    {
        CurrentState?.Exit();
        CurrentState = null;
    }
}