public class EnemyRangedAttackState : EnemyState
{
    public EnemyRangedAttackState(
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
            stateMachine.ChangeState(enemy.OutOfCombatState);
            return;
        }

        enemy.Rotation?.LookAt(enemy.Target.position, deltaTime);

        if (enemy.Encounter == null)
        {
            enemy.Combat.TryAttack(enemy.Target);
            return;
        }

        if (!enemy.Encounter.HasAttackPermission(enemy))
            return;

        if (enemy.Combat.TryAttack(enemy.Target))
            enemy.Encounter.NotifyAttackStarted(enemy);
    }

    public override void Exit()
    {
        enemy.Combat.InterruptAttack();
    }
}