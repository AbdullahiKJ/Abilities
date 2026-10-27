using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyWeapon : MonoBehaviour
{
    [Header("Testing Settings")]
    [SerializeField] private SandParticleController targetController;
    [SerializeField] private bool hitWindowActive = true; // Enable for editor testing.

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
        }

        // Update the previois positions AFTER submitting the sweep.
        previousWeaponBase = currentBase;
        previousWeaponTip = currentTip;
    }

    public void BeginHitWindow()
    {
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