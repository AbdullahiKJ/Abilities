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
    }

    public override void Tick(float deltaTime)
    {
        if (enemy.IsPlayerInRange())
        {
            stateMachine.ChangeState(enemy.AttackState);
        }

        enemy.Rotation?.Scan(deltaTime);
    }

    public override void Exit()
    {
    }
}