using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyWeapon : MonoBehaviour
{
    [Header("Testing Settings")]
    [SerializeField] private SandParticleController targetController;
    [SerializeField] private bool hitWindowActive = true; // Enable for editor testing.
    private bool damageApplied;

    [Header("General Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Weapon Transforms")]
    [SerializeField] private Transform weaponBase;
    [SerializeField] private Transform weaponTip;
    private Vector3 previousWeaponBase;
    private Vector3 previousWeaponTip;

    [Header("Impulse Settings")]
    [SerializeField] private float radius = 8f;
    [SerializeField] private float forwardImpulse = 8f;
    [SerializeField] private float radialImpulse = 5f;

    [Header("Recovery Settings")]
    [SerializeField] private float minRecoveryTime = 1f;
    [SerializeField] private float maxRecoveryTime = 2f;

    private void OnEnable()
    {
        previousWeaponBase = weaponBase.position;
        previousWeaponTip = weaponTip.position;
    }

    private void LateUpdate()
    {
        Vector3 currentBase = weaponBase.position;
        Vector3 currentTip = weaponTip.position;

        if (hitWindowActive &&
            targetController != null &&
            Time.deltaTime > 0f)
        {
            targetController.RegisterWeaponHit(new SandWeaponHitData
            {
                previousBase = previousWeaponBase,
                previousTip = previousWeaponTip,
                currentBase = currentBase,
                currentTip = currentTip,

                radius = radius,
                forwardImpactMultiplier = forwardImpulse,
                radialImpactMultiplier = radialImpulse,

                minRecoveryTime = minRecoveryTime,
                maxRecoveryTime = maxRecoveryTime,

                deltaTime = Time.deltaTime
            });

            // Detect the player and apply damage
            ApplyDamage();
        }

        // Update the previous positions AFTER submitting the sweep.
        previousWeaponBase = currentBase;
        previousWeaponTip = currentTip;
    }

    private void ApplyDamage()
    {
        int sampleCount = 5;
        if (damageApplied)
            return;

        for (int i = 0; i < sampleCount; i++)
        {
            float bladeT = i / (float)(sampleCount - 1);

            // Get the current and previous positions for each sample along the blade
            Vector3 previous =
                math.lerp(
                    previousWeaponBase,
                    previousWeaponTip,
                    bladeT
                );

            Vector3 current =
                math.lerp(
                    weaponBase.position,
                    weaponTip.position,
                    bladeT
                );

            Vector3 direction = current - previous;
            float distance = direction.magnitude;

            // Perform a sphere cast between these two positions
            if (distance > 0.00001f &&
                Physics.SphereCast(
                    origin: previous,
                    radius: radius,
                    direction: direction,
                    hitInfo: out RaycastHit hit,
                    maxDistance: distance,
                    layerMask: playerLayer.value,
                    queryTriggerInteraction: QueryTriggerInteraction.Ignore))
            {
                // Get the health component and apply damage
                Health health = hit.collider.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                    damageApplied = true;
                    return;
                }
            }
        }
    }

    public void SetTarget(SandParticleController target)
    {
        targetController = target;
    }

    public void BeginHitWindow()
    {
        damageApplied = false;

        // Prevent a sweep from an old blade position.
        previousWeaponBase = weaponBase.position;
        previousWeaponTip = weaponTip.position;

        hitWindowActive = true;
    }

    public void EndHitWindow()
    {
        hitWindowActive = false;
    }

    private void OnDrawGizmos()
    {
        int count = 5;

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            Vector3 position = math.lerp(weaponBase.position, weaponTip.position, t);
            Gizmos.DrawWireSphere(position, radius);
        }
    }
}