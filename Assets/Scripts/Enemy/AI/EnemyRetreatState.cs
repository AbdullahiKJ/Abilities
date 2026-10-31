using UnityEngine;

public class EnemyRetreatState : EnemyState
{
    public EnemyRetreatState(
        EnemyController enemy,
        EnemyStateMachine stateMachine)
        : base(enemy, stateMachine)
    {
    }

    public override void Enter()
    {
        // Find a random position away from the player
        Vector3 away = enemy.transform.position - enemy.Target.position;
        away.y = 0f;
        away.Normalize();

        float distance = Random.Range(enemy.MinRetreatDistance, enemy.MaxRetreatDistance);
        Vector3 destination = enemy.transform.position + away * distance;

        enemy.Navigation.SetDestination(destination);
    }

    public override void Tick(float deltaTime)
    {
        // Return to the patrol state if out of range
        if (!enemy.IsPlayerInRange())
        {
            stateMachine.ChangeState(enemy.PatrolState);
            return;
        }

        // Switch to the circle state once the destination has been reached
        if (enemy.Navigation.HasArrived)
        {
            stateMachine.ChangeState(enemy.CircleState);
            return;
        }

        // Look at the target
        enemy.Rotation.LookAt(enemy.Target.position, deltaTime);
    }

    public override void Exit()
    {
    }
}