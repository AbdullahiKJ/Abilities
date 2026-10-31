using UnityEngine;

[RequireComponent(typeof(EnemyStateMachine))]
public class EnemyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private EnemyWeapon[] weapons;

    [Header("Combat Detection")]
    [SerializeField, Min(0f)] private float engagementRange = 15f;

    [Header("Encounters")]
    [SerializeField] private EncounterManager encounter;

    [Header("Movement")]
    [SerializeField] float minRetreatDistance;
    [SerializeField] float maxRetreatDistance;
    [SerializeField] float circleDuration;
    [SerializeField] float holdDuration;

    [Header("Debug")]
    [SerializeField] private Color debugColour = Color.green;

    public enum EnemyBehaviour
    {
        StationaryRanged,
        MobileMelee
    }

    [SerializeField] private EnemyBehaviour behaviour;

    public EnemyState OutOfCombatState =>
        behaviour == EnemyBehaviour.StationaryRanged
            ? IdleState
            : PatrolState;

    public Transform Target => player;

    public float MinRetreatDistance => minRetreatDistance;
    public float MaxRetreatDistance => maxRetreatDistance;
    public float CircleDuration => circleDuration;
    public float HoldDuration => holdDuration;

    public EnemyStateMachine StateMachine { get; private set; }
    public EnemyCombat Combat { get; private set; }
    public EnemyMovement Movement { get; private set; }
    public EnemyRotation Rotation { get; private set; }
    public EnemyNavigation Navigation { get; private set; }
    public EnemyAnimationController Animation { get; private set; }

    public EncounterManager Encounter => encounter;

    public EnemyIdleState IdleState { get; private set; }
    public EnemyRangedAttackState RangedAttackState { get; private set; }
    public EnemyMeleeAttackState MeleeAttackState { get; private set; }
    public EnemyApproachState ApproachState { get; private set; }
    public EnemyCircleState CircleState { get; private set; }
    public EnemyRetreatState RetreatState { get; private set; }
    public EnemyPatrolState PatrolState { get; private set; }

    private void Awake()
    {
        StateMachine = GetComponent<EnemyStateMachine>();
        Combat = GetComponent<EnemyCombat>();
        Movement = GetComponent<EnemyMovement>();
        Rotation = GetComponent<EnemyRotation>();
        Navigation = GetComponent<EnemyNavigation>();
        Animation = GetComponent<EnemyAnimationController>();

        IdleState = new EnemyIdleState(this, StateMachine);
        RangedAttackState = new EnemyRangedAttackState(this, StateMachine);
        MeleeAttackState = new EnemyMeleeAttackState(this, StateMachine);
        ApproachState = new EnemyApproachState(this, StateMachine);
        CircleState = new EnemyCircleState(this, StateMachine);
        RetreatState = new EnemyRetreatState(this, StateMachine);
        PatrolState = new EnemyPatrolState(this, StateMachine);
    }

    private void Start()
    {
        Combat.AttackFinished += HandleAttackFinished;

        // Assign the enemy to it's encounter manager
        Encounter?.Register(this);

        StateMachine.ChangeState(OutOfCombatState);

        InitialiseWeapons();
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

    private void HandleAttackFinished()
    {
        encounter?.ReleaseAttackPermission(this);
    }

    private void InitialiseWeapons()
    {
        SandParticleController particleController;
        if (Target != null)
            particleController = Target.transform.root.GetComponent<SandParticleController>();
        else
            particleController = null;

        foreach (EnemyWeapon weapon in weapons)
        {
            weapon.SetTarget(particleController);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = debugColour;
        Gizmos.DrawWireSphere(transform.position, engagementRange);
    }

    private void OnDestroy()
    {
        if (Combat != null)
            Combat.AttackFinished -= HandleAttackFinished;

        if (Encounter != null)
            encounter.Unregister(this);
    }
}