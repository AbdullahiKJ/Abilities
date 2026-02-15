using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 50f;
    public float lifeTime = 5f;
    private Vector3 direction;

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

    private void OnTriggerEnter(Collider other)
    {
        // Get the hit system script and handle interactions
        Destroy(gameObject);
    }
}