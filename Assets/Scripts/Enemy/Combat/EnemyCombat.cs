using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [Tooltip("Highest-priority attacks first.")]
    [SerializeField] private EnemyAttack[] attacks;

    public EnemyAttack ActiveAttack { get; private set; }
    public bool IsAttacking => ActiveAttack != null;

    public event System.Action AttackFinished;

    public bool TryAttack(Transform target)
    {
        if (target == null || IsAttacking)
            return false;

        foreach (EnemyAttack attack in attacks)
        {
            if (attack == null || !attack.isActiveAndEnabled)
                continue;

            if (!attack.CanAttack(target))
                continue;

            // Assign before starting: execution may finish immediately.
            ActiveAttack = attack;
            attack.Finished += OnAttackFinished;

            if (attack.TryAttack(target))
                return true;

            // Defensive cleanup if starting the attack was rejected.
            attack.Finished -= OnAttackFinished;
            ActiveAttack = null;
        }

        return false;
    }

    private void OnAttackFinished(EnemyAttack attack)
    {
        attack.Finished -= OnAttackFinished;

        if (ActiveAttack != attack)
            return;

        ActiveAttack = null;
        AttackFinished?.Invoke();
    }

    public void InterruptAttack()
    {
        if (ActiveAttack != null)
            ActiveAttack.CancelAttack();
    }

    public bool HasAvailableAttack(Transform target)
    {
        if (target == null || IsAttacking)
            return false;

        foreach (EnemyAttack attack in attacks)
        {
            if (attack != null
               && attack.isActiveAndEnabled
               && attack.CanAttack(target))
            {
                return true;
            }
        }

        return false;
    }

    private void OnDisable()
    {
        InterruptAttack();
    }
}