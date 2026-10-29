using UnityEngine;

[RequireComponent(typeof(EnemyStateMachine))]
public class EnemyController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Combat Detection")]
    [SerializeField, Min(0f)] private float engagementRange = 15f;

    [Header("Debug")]
    [SerializeField] private Color debugColour = Color.green;

    public Transform Target => player;

    public EnemyStateMachine StateMachine { get; private set; }
    public EnemyCombat Combat { get; private set; }
    public EnemyRotation Rotation { get; private set; }
    public EnemyAnimationController Animation { get; private set; }

    public EnemyIdleState IdleState { get; private set; }
    public EnemyAttackState AttackState { get; private set; }

    private void Awake()
    {
        StateMachine = GetComponent<EnemyStateMachine>();
        Combat = GetComponent<EnemyCombat>();
        Rotation = GetComponent<EnemyRotation>();
        Animation = GetComponent<EnemyAnimationController>();

        IdleState = new EnemyIdleState(this, StateMachine);
        AttackState = new EnemyAttackState(this, StateMachine);
    }

    private void Start()
    {
        StateMachine.ChangeState(IdleState);
    }

    public bool IsPlayerInRange()
    {
        if (player == null)
            return false;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        return distance <= engagementRange;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = debugColour;
        Gizmos.DrawWireSphere(transform.position, engagementRange);
    }
}