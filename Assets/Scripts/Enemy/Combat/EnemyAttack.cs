using System;
using UnityEngine;

public abstract class EnemyAttack : MonoBehaviour
{
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float cooldown = 2f;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected int animationIndex = 1;
    [SerializeField] protected Color debugColour1;
    [SerializeField] protected Color debugColour2;

    protected float lastAttackTime = -Mathf.Infinity;
    protected Transform currentTarget;

    public float AttackRange => attackRange;
    public bool IsAttacking { get; private set; }

    public event Action<EnemyAttack> Finished;

    public virtual bool CanAttack(Transform target)
    {
        if (target == null || IsAttacking || !isActiveAndEnabled)
            return false;

        if (Time.time < lastAttackTime + cooldown)
            return false;

        return Vector3.Distance(
            transform.position,
            target.position
        ) <= attackRange;
    }

    public bool TryAttack(Transform target)
    {
        if (!CanAttack(target))
            return false;

        currentTarget = target;
        IsAttacking = true;
        lastAttackTime = Time.time;

        ExecuteAttack(target);
        return true;
    }

    public abstract void ExecuteAttack(Transform target);

    // Call when animation recovery has finished.
    public void CompleteAttack()
    {
        if (!IsAttacking)
            return;

        IsAttacking = false;
        currentTarget = null;

        Finished?.Invoke(this);
    }

    public void CancelAttack()
    {
        if (!IsAttacking)
            return;

        OnAttackCancelled();
        CompleteAttack();
    }

    protected virtual void OnAttackCancelled()
    {
        // Subclasses close damage windows or cancel pending actions.
    }

    protected virtual void OnDisable()
    {
        CancelAttack();
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = debugColour1;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}