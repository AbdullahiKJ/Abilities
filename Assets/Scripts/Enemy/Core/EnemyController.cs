using UnityEngine;

[RequireComponent(typeof(EnemyStateMachine))]
public class EnemyController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    public Transform Target => player;

    public EnemyStateMachine StateMachine { get; private set; }
    public EnemyAttack Attack { get; private set; }
    public EnemyRotation Rotation { get; private set; }

    public EnemyIdleState IdleState { get; private set; }
    public EnemyAttackState AttackState { get; private set; }

    private void Awake()
    {
        StateMachine = GetComponent<EnemyStateMachine>();
        Attack = GetComponent<EnemyAttack>();
        Rotation = GetComponent<EnemyRotation>();

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

        return distance <= Attack.AttackRange;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, Attack.AttackRange);
        }
    }
}