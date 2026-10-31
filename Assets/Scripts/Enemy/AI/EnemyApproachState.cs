public class EnemyApproachState : EnemyState
{
    public EnemyApproachState(
        EnemyController enemy,
        EnemyStateMachine stateMachine)
        : base(enemy, stateMachine)
    {
    }

    public override void Enter()
    {
        enemy.Movement.SetFastSpeed();
    }

    public override void Tick(float deltaTime)
    {
        // Return to patrol when out of range
        if (!enemy.IsPlayerInRange())
        {
            stateMachine.ChangeState(enemy.PatrolState);
            return;
        }

        // Enter the circling state when the enemy does not have permission
        if (enemy.Encounter != null && !enemy.Encounter.HasAttackPermission(enemy))
        {
            stateMachine.ChangeState(enemy.CircleState);
            return;
        }

        // Look at the player
        enemy.Rotation?.LookAt(enemy.Target.position, deltaTime);

        // Start the attack if in range
        if (enemy.Combat.HasAvailableAttack(enemy.Target))
        {
            stateMachine.ChangeState(enemy.MeleeAttackState);
            return;
        }

        // Move towards the player
        enemy.Navigation.SetDestination(enemy.Target.position);
    }

    public override void Exit()
    {
        enemy.Movement.SetNormalSpeed();
    }
}