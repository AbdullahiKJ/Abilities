using UnityEngine;

public class EnemyCircleState : EnemyState
{
    private readonly float circleDistance;
    private readonly float distanceCorrection;
    private readonly float lookAheadDistance;

    private float circleDirection;

    private float phaseTimer;
    private bool isHolding;
    public EnemyCircleState(
        EnemyController enemy,
        EnemyStateMachine stateMachine,
        float circleDistance = 4f,
        float distanceCorrection = 1f,
        float lookAheadDistance = 1f)
        : base(enemy, stateMachine)
    {
        this.circleDistance = circleDistance;
        this.distanceCorrection = distanceCorrection;
        this.lookAheadDistance = lookAheadDistance;
    }

    public override void Enter()
    {
        // Choose a move direction and keep it until the next time we enter this state.
        circleDirection = Random.value < 0.5f ? -1f : 1f;

        isHolding = false;
        phaseTimer = enemy.CircleDuration;

        enemy.Animation.SetInRange(true);
    }

    public override void Tick(float deltaTime)
    {
        // Return to the patrol state if out of range
        if (!enemy.IsPlayerInRange())
        {
            stateMachine.ChangeState(enemy.PatrolState);
            return;
        }

        // Attack once granted permission
        if (enemy.Encounter != null && enemy.Encounter.HasAttackPermission(enemy))
            stateMachine.ChangeState(enemy.ApproachState);

        // Constantly look at the player target
        enemy.Rotation?.LookAt(enemy.Target.position, deltaTime);

        // Hold and circling timers
        phaseTimer -= deltaTime;

        if (isHolding)
        {
            if (phaseTimer > 0f)
                return;

            // Hold finished: circle in the opposite direction.
            isHolding = false;
            circleDirection *= -1f;
            phaseTimer = enemy.CircleDuration;
        }
        else if (phaseTimer <= 0f)
        {
            // Circling finished: stop and hold.
            isHolding = true;
            phaseTimer = enemy.HoldDuration;

            enemy.Navigation.Stop();
            return;
        }


        Vector3 away = enemy.transform.position - enemy.Target.position;
        away.y = 0f;
        float distance = away.magnitude;

        // Use a fallback when the enemy and player are too close.
        Vector3 radialDirection = distance > 0.001f
            ? away / distance
            : Vector3.forward;

        Vector3 tangent = Vector3.Cross(Vector3.up, radialDirection) * circleDirection;

        // Positive when too far away; negative when too close.
        float distanceError = distance - circleDistance;

        Vector3 correction = -radialDirection * distanceError * distanceCorrection;

        Vector3 moveDirection = Vector3.ClampMagnitude(tangent + correction, 1f);

        Vector3 destination =
            enemy.transform.position
            + moveDirection * lookAheadDistance;

        enemy.Navigation.SetDestination(destination);
    }

    public override void Exit()
    {
    }
}