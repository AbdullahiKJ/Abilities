using UnityEngine;

public class EnemyRotation : MonoBehaviour
{
    [Header("Target Tracking")]
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Idle Scanning")]
    [SerializeField] private float scanAngle = 60f;
    [SerializeField] private float scanSpeed = 1f;

    private Quaternion initialRotation;
    private float scanTimer;

    private void Awake()
    {
        initialRotation = transform.rotation;
    }

    public void Scan(float deltaTime)
    {
        scanTimer += deltaTime;

        // Oscillate between -scanAngle and +scanAngle.
        float angle = Mathf.Sin(scanTimer * scanSpeed) * scanAngle;

        Quaternion targetRotation = initialRotation * Quaternion.Euler(0f, angle, 0f);

        RotateTowards(targetRotation, deltaTime);
    }

    public void LookAt(Vector3 targetPosition, float deltaTime)
    {
        Vector3 direction = targetPosition - transform.position;

        // Only rotate around the Y axis.
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        RotateTowards(targetRotation, deltaTime);
    }

    private void RotateTowards(Quaternion targetRotation, float deltaTime)
    {
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * deltaTime
        );
    }
}