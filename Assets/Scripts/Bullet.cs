using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 50f;
    public float lifeTime = 5f;
    public float radius = 0.2f;
    private Vector3 direction;
    private Vector3 entryPoint;

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    // Set the movement direction and start lifetime timer
    public void Fire(Vector3 dir)
    {
        direction = dir;
        Destroy(gameObject, lifeTime);
    }

    // Get the hit system script, pass entry/exit points and the bullet to determine what kind of interaction occurs
    private void OnTriggerEnter(Collider other)
    {
        entryPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
        HitSystem hit = other.GetComponent<HitSystem>();
        if (hit != null)
            hit.OnEnter(this);
    }

    private void OnTriggerExit(Collider other)
    {
        Vector3 hitPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
        HitSystem hit = other.GetComponent<HitSystem>();
        if (hit != null)
            hit.OnExit(entryPoint, hitPoint, radius);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, radius);
    }
}