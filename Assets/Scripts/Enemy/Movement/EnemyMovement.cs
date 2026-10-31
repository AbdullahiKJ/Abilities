using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyAnimationController animationController;
    private CharacterController charController;
    private Vector3 desiredDirection;

    [Header("References")]
    [SerializeField] private float normalSpeed;
    [SerializeField] private float fastSpeed;
    public float moveSpeed;

    private void Awake()
    {
        moveSpeed = normalSpeed;
        charController = GetComponent<CharacterController>();
    }

    public void SetDesiredDirection(Vector3 direction)
    {
        desiredDirection = Vector3.ClampMagnitude(direction, 1f);
    }

    public void SetNormalSpeed()
    {
        moveSpeed = normalSpeed;
    }

    public void SetFastSpeed()
    {
        moveSpeed = fastSpeed;
    }

    private void Update()
    {
        // Get the move direction relative to the forward direction
        Vector3 localDirection = transform.InverseTransformDirection(desiredDirection);

        // Assign the movement parameters in the animator
        animationController.SetMovement(localDirection);

        // Move the character controller
        charController.Move(desiredDirection * moveSpeed * Time.deltaTime);
    }
}