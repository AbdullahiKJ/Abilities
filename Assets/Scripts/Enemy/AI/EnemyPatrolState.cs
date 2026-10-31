using UnityEngine;

public class EnemyPatrolState : EnemyState
{
    public EnemyPatrolState(
        EnemyController enemy,
        EnemyStateMachine stateMachine)
        : base(enemy, stateMachine)
    {
    }

    public override void Enter()
    {
        // Stop the in range animation
        enemy.Animation.SetInRange(false);
    }

    public override void Tick(float deltaTime)
    {
        if (enemy.IsPlayerInRange())
        {
            stateMachine.ChangeState(enemy.CircleState);
        }

        // todo: navigate through set points
        enemy.Navigation?.SetDestination(Vector3.zero);
    }

    public override void Exit()
    {
        // Start the in range animation
        enemy.Animation.SetInRange(true);
    }
}