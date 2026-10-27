using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Projectile Impacts")]
    [SerializeField] private float projectilePenetrationDepth = 0.5f;
    [SerializeField] private float projectileRadius = 0.3f;
    [SerializeField] private float projectileForwardImpulse = 8f;
    [SerializeField] private float projectileRadialImpulse = 5f;
    [SerializeField] private float projectileRandomImpulse = 2f;

    [Header("Recovery Settings")]
    [SerializeField] private float minRecoveryTime = 1f;
    [SerializeField] private float maxRecoveryTime = 2f;

    private Rigidbody rb;
    private Vector3 moveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        rb.useGravity = false;
        rb.linearVelocity = transform.forward * speed;
        moveDirection = transform.forward;

        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // todo: get this working properly
        // Vfx based hit detection system
        // if (collision.gameObject.TryGetComponent(
        //     out HitDetection sandHit))
        // {
        //     Vector3 hitPoint = collision.GetContact(0).point;

        //     sandHit.RegisterProjectileHit(
        //         hitPoint,
        //         rb.linearVelocity
        //     );
        // }

        // Burst/Jobs based hit detection system
        if (collision.gameObject.TryGetComponent(
            out SandParticleController controller))
        {
            Vector3 entry = collision.GetContact(0).point;

            // Approximate penetration through the body.
            Vector3 exit = entry + moveDirection * projectilePenetrationDepth;

            SandHitData hitData = new SandHitData(
                entry,
                exit,
                moveDirection,
                projectileRadius,
                projectileForwardImpulse,
                projectileRadialImpulse,
                projectileRandomImpulse,
                minRecoveryTime,
                maxRecoveryTime
            );

            controller.RegisterHit(hitData);
        }

        // Only destroy the particle if it hits a non-player game object
        bool isPlayer = (playerLayer.value & (1 << collision.collider.gameObject.layer)) != 0;

        if (isPlayer)
        {
            // The sand hit has already been registered above.
            Physics.IgnoreCollision(
                collision.thisCollider, // Projectile collider
                collision.collider,      // Player collider
                true
            );

            // Restore movement after the initial collision response.
            rb.linearVelocity = moveDirection * speed;
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            // todo: play hit vfx
            Destroy(gameObject);
        }
    }
}