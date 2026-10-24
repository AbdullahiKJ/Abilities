using UnityEngine;

public class RangedAttack : EnemyAttack
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

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
        Vector3 direction = (
            target.position - projectileSpawnPoint.position
        ).normalized;

        Quaternion rotation = Quaternion.LookRotation(direction);

        Instantiate(
            projectilePrefab,
            projectileSpawnPoint.position,
            rotation
        );
    }
}