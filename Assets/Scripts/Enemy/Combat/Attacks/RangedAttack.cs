using UnityEngine;

public class RangedAttack : EnemyAttack
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    [SerializeField] private EnemyAnimationController animationController;

    public override bool CanAttack(Transform target)
    {
        // Perform the shared range and cooldown checks.
        if (!base.CanAttack(target))
            return false;

        // Ranged-specific checks.
        if (projectilePrefab == null || projectileSpawnPoint == null)
            return false;

        return true;
    }

    public override void ExecuteAttack(Transform target)
    {
        currentTarget = target;
        animationController.PlayAttack(animationIndex);
    }

    // Called via animation events
    public void ReleaseProjectile()
    {
        if (!IsAttacking || currentTarget == null)
            return;

        Vector3 direction = currentTarget.position - projectileSpawnPoint.position;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        Instantiate(
            projectilePrefab,
            projectileSpawnPoint.position,
            Quaternion.LookRotation(direction)
        );
    }
}