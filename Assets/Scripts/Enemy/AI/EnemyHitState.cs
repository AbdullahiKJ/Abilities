public class EnemyHitState : EnemyState
{
    private float remainingTime;

    public EnemyHitState(
        EnemyController enemy,
        EnemyStateMachine stateMachine)
        : base(enemy, stateMachine)
    {
    }

    public void ResetDuration()
    {
        remainingTime = 0.5f;
    }

    public override void Enter()
    {
        enemy.Combat.InterruptAttack();
        enemy.CloseHitWindow();
        enemy.Encounter?.ReleaseAttackPermission(enemy);
        enemy.Navigation?.Stop();

        enemy.Animation.PlayHit();
    }

    public override void Tick(float deltaTime)
    {
        remainingTime -= deltaTime;

        if (remainingTime > 0f)
            return;

        if (!enemy.IsPlayerInRange())
        {
            stateMachine.ChangeState(enemy.OutOfCombatState);
            return;
        }

        // Preserve the different ranged and melee flows.
        stateMachine.ChangeState(
            enemy.OutOfCombatState == enemy.IdleState
                ? enemy.RangedAttackState
                : enemy.CircleState
        );
    }

    public override void Exit()
    {
    }
}