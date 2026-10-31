using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class EnemyNavigation : MonoBehaviour
{
    [SerializeField, Min(0f)] private float stoppingDistance = 0.5f;

    private EnemyMovement movement;
    private Vector3 destination;
    private bool hasDestination;

    public bool HasDestination => hasDestination;

    public bool HasArrived =>
        hasDestination &&
        HorizontalDistanceSquared(destination)
            <= stoppingDistance * stoppingDistance;

    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
    }

    public void SetDestination(Vector3 position)
    {
        destination = position;
        hasDestination = true;
    }

    public void Stop()
    {
        hasDestination = false;
        movement.SetDesiredDirection(Vector3.zero);
    }

    private void Update()
    {
        if (!hasDestination || HasArrived)
        {
            movement.SetDesiredDirection(Vector3.zero);
            return;
        }

        Vector3 direction = destination - transform.position;
        direction.y = 0f;

        movement.SetDesiredDirection(direction.normalized);
    }

    private float HorizontalDistanceSquared(Vector3 position)
    {
        Vector3 offset = position - transform.position;
        offset.y = 0f;

        return offset.sqrMagnitude;
    }

    private void OnDisable()
    {
        if (movement != null)
            Stop();

    }

    private void OnDrawGizmos()
    {
        if (!hasDestination)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(destination, stoppingDistance);
        Gizmos.DrawLine(transform.position, destination);
    }
}