using UnityEngine;

public class KnockbackAttack : EnemyAttack
{
    [Header("References")]
    [SerializeField] private EnemyAnimationController animationController;
    [SerializeField] private Transform hitOrigin;

    [Header("Hit Detection")]
    [SerializeField, Min(0f)] private float hitRange = 2f;
    [SerializeField, Range(0f, 180f)] private float hitAngle = 100f;

    [Header("Knockback")]
    [SerializeField, Min(0f)] private float knockbackSpeed = 10f;
    [SerializeField, Min(0f)] private float upwardSpeed = 2f;

    private bool impactUsed;

    public override void ExecuteAttack(Transform target)
    {
        impactUsed = false;
        animationController.PlayAttack(animationIndex);
    }

    // Called by an Animation Event at the impact frame.
    public void ApplyImpact()
    {
        if (!IsAttacking || impactUsed)
            return;

        // Consume this impact even if the player dodged.
        impactUsed = true;

        if (currentTarget == null)
            return;

        Vector3 origin = hitOrigin != null
            ? hitOrigin.position
            : transform.position;

        Vector3 offset = currentTarget.position - origin;

        // Recheck range when the hit actually lands.
        if (offset.sqrMagnitude > hitRange * hitRange)
            return;

        Vector3 horizontalDirection = offset;
        horizontalDirection.y = 0;
        horizontalDirection.Normalize();

        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();

        if (horizontalDirection.sqrMagnitude > 0.0001f)
        {
            float facing = Vector3.Dot(
                forward,
                horizontalDirection.normalized
            );

            float requiredFacing = Mathf.Cos(hitAngle * 0.5f * Mathf.Deg2Rad);

            if (facing < requiredFacing)
                return;
        }

        MovementController movement = currentTarget.GetComponentInParent<MovementController>();

        if (movement == null)
            return;

        Vector3 direction = horizontalDirection.sqrMagnitude > 0.0001f
            ? horizontalDirection.normalized
            : forward;

        Vector3 knockback =
            direction * knockbackSpeed +
            Vector3.up * upwardSpeed;

        movement.ApplyKnockback(knockback);

        // Call your player-health API here:
        // playerHealth.TakeDamage(damage);
    }

    protected override void OnAttackCancelled()
    {
        impactUsed = true;

        // Also transition out of the attack animation through
        // your animation controller when interruption is supported.
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = debugColour1;
        Vector3 origin = hitOrigin != null ? hitOrigin.position : transform.position;
        // Draw the hit range
        Gizmos.DrawWireSphere(origin, hitRange);

        // Draw the hit angle using two rays
        Vector3 leftDirection = Quaternion.Euler(0, -hitAngle / 2f, 0) * transform.forward;
        Vector3 rightDirection = Quaternion.Euler(0, hitAngle / 2f, 0) * transform.forward;

        Gizmos.color = debugColour2;
        Gizmos.DrawRay(origin, leftDirection * hitRange);
        Gizmos.DrawRay(origin, rightDirection * hitRange);
    }
}