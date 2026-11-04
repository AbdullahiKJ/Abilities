using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    [Header("Gravity")]
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float groundedVelocity = -2f;
    [SerializeField] private float terminalVelocity = -40f;

    [Header("External Forces")]
    [SerializeField, Min(0f)] private float impulseDrag = 8f;

    private CharacterController controller;

    // Persistent motion.
    private float verticalVelocity;
    private Vector3 externalVelocity;
    private float frameMaximumFallSpeed;

    // Requests reset at the beginning of each frame.
    private Vector3 requestedVelocity;
    private bool gravityEnabled;
    private float gravityMultiplier;

    public bool IsGrounded => controller != null && controller.isGrounded;
    public bool IsFalling => !IsGrounded && verticalVelocity < -0.1f;
    public Vector3 ActualVelocity => controller != null ? controller.velocity : Vector3.zero;
    public float VerticalVelocity => verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void BeginFrame()
    {
        requestedVelocity = Vector3.zero;
        gravityEnabled = true;
        gravityMultiplier = 1f;

        frameMaximumFallSpeed = Mathf.Abs(terminalVelocity);
    }

    // Full world-space velocity requested by the active movement mode.
    public void SetMoveVelocity(Vector3 velocity)
    {
        requestedVelocity = velocity;
    }

    public void SetGravityMultiplier(float multiplier)
    {
        gravityMultiplier = Mathf.Max(0f, multiplier);
    }

    public void SuspendGravity()
    {
        gravityEnabled = false;

        // Prevent old falling velocity returning after wall climbing.
        verticalVelocity = 0f;
    }

    public void SetVerticalVelocity(float velocity)
    {
        verticalVelocity = velocity;
    }

    public void AddImpulse(Vector3 velocity)
    {
        externalVelocity += velocity;
    }

    public void Tick(float deltaTime)
    {
        if (deltaTime <= 0f || !controller.enabled)
            return;

        if (gravityEnabled)
        {
            if (IsGrounded && verticalVelocity < 0f)
                verticalVelocity = groundedVelocity;

            verticalVelocity += gravity * gravityMultiplier * deltaTime;

            verticalVelocity = Mathf.Max(
                verticalVelocity,
                -frameMaximumFallSpeed
            );
        }

        Vector3 totalVelocity =
            requestedVelocity
            + Vector3.up * verticalVelocity
            + externalVelocity;

        CollisionFlags collisions = controller.Move(totalVelocity * deltaTime);

        // Stop upward motion when hitting a ceiling.
        if ((collisions & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
        {
            verticalVelocity = 0f;
        }

        // Stop downward motion when hitting the ground.
        if ((collisions & CollisionFlags.Below) != 0 && verticalVelocity < 0f)
        {
            verticalVelocity = groundedVelocity;
        }

        // Apply drag to external forces, so that they decay over time.
        externalVelocity *= Mathf.Exp(-impulseDrag * deltaTime);
    }

    public void ClearMotion()
    {
        requestedVelocity = Vector3.zero;
        verticalVelocity = 0f;
        externalVelocity = Vector3.zero;
    }

    public void BlendHorizontalVelocity(Vector3 launchVelocity, float inputMultiplier)
    {
        requestedVelocity.x = launchVelocity.x + requestedVelocity.x * inputMultiplier;

        requestedVelocity.z = launchVelocity.z + requestedVelocity.z * inputMultiplier;
    }

    public void LimitFallingSpeed(float maximumSpeed)
    {
        frameMaximumFallSpeed = Mathf.Max(0f, maximumSpeed);
    }
}