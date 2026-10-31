public class EnemyDeadState : EnemyState
{
    public EnemyDeadState(
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
        enemy.Combat.InterruptAttack();
        enemy.CloseHitWindow();
        enemy.Navigation?.Stop();

        enemy.Encounter?.Unregister(enemy);

        enemy.Animation.PlayDeath();
    }

    public override void Exit()
    {
    }
}