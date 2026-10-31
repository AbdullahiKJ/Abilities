public class EnemyIdleState : EnemyState
{
    public EnemyIdleState(
        EnemyController enemy,
        EnemyStateMachine stateMachine)
        : base(enemy, stateMachine)
    {
    }

    public override void Enter()
    {
        // Set the in range animation property
        enemy.Animation.SetInRange(false);
    }

    public override void Tick(float deltaTime)
    {
        if (enemy.IsPlayerInRange())
        {
            stateMachine.ChangeState(enemy.RangedAttackState);
        }

        enemy.Rotation?.Scan(deltaTime);
    }

    public override void Exit()
    {
    }
}