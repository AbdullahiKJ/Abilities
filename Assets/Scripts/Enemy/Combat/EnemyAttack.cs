using UnityEngine;

public abstract class EnemyAttack : MonoBehaviour
{
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float cooldown = 2f;
    [SerializeField] protected float attackRange = 2f;

    protected float lastAttackTime = -Mathf.Infinity;

    public float AttackRange => attackRange;

    public virtual bool CanAttack(Transform target)
    {
        if (target == null)
            return false;

        if (Time.time < lastAttackTime + cooldown)
            return false;

        return Vector3.Distance(
            transform.position,
            target.position
        ) <= attackRange;
    }

    public abstract void ExecuteAttack(Transform target);

    public bool TryAttack(Transform target)
    {
        if (!CanAttack(target))
            return false;

        ExecuteAttack(target);

        lastAttackTime = Time.time;

        return true;
    }
}