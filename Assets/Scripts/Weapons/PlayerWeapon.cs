using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Testing Settings")]
    [SerializeField] private bool hitWindowActive = false; // Enable for editor testing.
    private HashSet<Health> enemiesHit = new();
    private float currentDamage;

    [Header("General Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float hitRadius;

    [Header("Weapon Transforms")]
    [SerializeField] private Transform weaponBase;
    [SerializeField] private Transform weaponTip;
    private Vector3 previousWeaponBase;
    private Vector3 previousWeaponTip;

    private int sampleCount = 5;

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
            Time.deltaTime > 0f)
        {
            // Detect the player and apply damage
            ApplyDamage();
        }

        // Update the previous positions AFTER submitting the sweep.
        previousWeaponBase = currentBase;
        previousWeaponTip = currentTip;
    }

    private void ApplyDamage()
    {
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
                    radius: hitRadius,
                    direction: direction,
                    hitInfo: out RaycastHit hit,
                    maxDistance: distance,
                    layerMask: enemyLayer.value,
                    queryTriggerInteraction: QueryTriggerInteraction.Ignore))
            {
                // Get the health component and apply damage
                Health health = hit.collider.GetComponentInParent<Health>();
                if (health != null)
                {
                    // Ignore if the enemy has already been hit
                    if (enemiesHit.Contains(health))
                        continue;

                    // Apply damage
                    health.TakeDamage(currentDamage);

                    // Add the enemy to the hash set
                    enemiesHit.Add(health);
                }
            }
        }
    }

    public void PrepareAttack(float damage)
    {
        CloseHitWindow();

        currentDamage = Mathf.Max(0f, damage);
        enemiesHit.Clear();
    }

    public void OpenHitWindow()
    {
        // Prevent a sweep from an old blade position.
        previousWeaponBase = weaponBase.position;
        previousWeaponTip = weaponTip.position;

        hitWindowActive = true;
    }

    public void CloseHitWindow()
    {
        hitWindowActive = false;
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / (sampleCount - 1);
            Vector3 position = math.lerp(weaponBase.position, weaponTip.position, t);
            Gizmos.DrawWireSphere(position, hitRadius);
        }
    }
}