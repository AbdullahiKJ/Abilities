using UnityEngine;

public class HitDetection : MonoBehaviour
{
    [Header("Projectile Impacts")]
    [SerializeField] private float projectilePenetrationDepth = 0.5f;
    [SerializeField] private float projectileRadius = 0.3f;
    [SerializeField] private float projectileForwardImpulse = 8f;
    [SerializeField] private float projectileRadialImpulse = 5f;
    [SerializeField] private float projectileRandomImpulse = 2f;

    [Header("Recovery Settings")]
    [SerializeField] private float minRecoveryTime = 2f;
    [SerializeField] private float maxRecoveryTime = 2f;

    [Header("References")]
    private VFXController vfxController;

    private void Awake()
    {
        vfxController = GetComponent<VFXController>();
    }

    public void RegisterProjectileHit(
        Vector3 hitPosition,
        Vector3 projectileVelocity)
    {
        if (projectileVelocity.sqrMagnitude < 0.0001f)
            return;

        Vector3 direction = projectileVelocity.normalized;

        Vector3 entry = hitPosition;

        // Approximate penetration through the body.
        Vector3 exit = entry + direction * projectilePenetrationDepth;

        SandHitData hitData = new SandHitData(
            entry,
            exit,
            direction,
            projectileRadius,
            projectileForwardImpulse,
            projectileRadialImpulse,
            projectileRandomImpulse,
            minRecoveryTime,
            maxRecoveryTime
        );

        vfxController.PlaySandHit(hitData);
    }
}