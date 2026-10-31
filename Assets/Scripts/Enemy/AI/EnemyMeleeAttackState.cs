public class EnemyMeleeAttackState : EnemyState
{
    private bool attackStarted;

    public EnemyMeleeAttackState(
        EnemyController enemy,
        EnemyStateMachine stateMachine)
        : base(enemy, stateMachine)
    {
    }

    public override void Enter()
    {
        attackStarted = false;

        enemy.Navigation.Stop();
        enemy.Animation.SetInRange(true);

        // Encounter-managed enemies must hold permission.
        if (enemy.Encounter != null &&
            !enemy.Encounter.HasAttackPermission(enemy))
        {
            return;
        }

        // Enemies without an encounter can attack freely.
        attackStarted = enemy.Combat.TryAttack(enemy.Target);

        if (attackStarted)
            enemy.Encounter?.NotifyAttackStarted(enemy);
    }

    public override void Tick(float deltaTime)
    {
        if (!attackStarted)
        {
            // Permission, range, or cooldown prevented the attack.
            enemy.Encounter?.ReleaseAttackPermission(enemy);

            stateMachine.ChangeState(
                enemy.IsPlayerInRange()
                    ? enemy.CircleState
                    : enemy.OutOfCombatState
            );

            return;
        }

        // Let the committed attack finish even if the player moves away.
        // The attack's impact check determines whether it connects.
        if (enemy.Combat.IsAttacking)
            return;

        stateMachine.ChangeState(
            enemy.IsPlayerInRange()
                ? enemy.RetreatState
                : enemy.OutOfCombatState
        );
    }

    public override void Exit()
    {
        enemy.Combat.InterruptAttack();
        enemy.Encounter?.ReleaseAttackPermission(enemy);
    }
}