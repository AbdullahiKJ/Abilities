using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float damage = 10f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        rb.useGravity = false;
        rb.linearVelocity = transform.forward * speed;

        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // todo: get this working properly
        // if (collision.gameObject.TryGetComponent(
        //     out HitDetection sandHit))
        // {
        //     Vector3 hitPoint = collision.GetContact(0).point;

        //     sandHit.RegisterProjectileHit(
        //         hitPoint,
        //         rb.linearVelocity
        //     );
        // }

        Destroy(gameObject);
    }
}