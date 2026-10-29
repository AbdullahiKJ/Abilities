using UnityEngine;

public class EnemyAttackState : EnemyState
{
    public EnemyAttackState(
        EnemyController enemy,
        EnemyStateMachine stateMachine)
        : base(enemy, stateMachine)
    {
    }

    public override void Enter()
    {
        // Start the in range animation
        enemy.Animation.SetInRange(true);
    }

    public override void Tick(float deltaTime)
    {
        if (!enemy.IsPlayerInRange())
        {
            stateMachine.ChangeState(enemy.IdleState);
            return;
        }

        enemy.Rotation?.LookAt(
            enemy.Target.position,
            deltaTime
        );

        enemy.Combat.TryAttack(enemy.Target);
    }

    public override void Exit()
    {
        // Stop the in range animation
        enemy.Animation.SetInRange(false);
    }
}