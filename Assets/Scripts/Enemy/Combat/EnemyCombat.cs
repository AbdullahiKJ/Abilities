using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [Tooltip("Highest-priority attacks first.")]
    [SerializeField] private EnemyAttack[] attacks;

    public EnemyAttack ActiveAttack { get; private set; }
    public bool IsAttacking => ActiveAttack != null;

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

        if (ActiveAttack == attack)
            ActiveAttack = null;
    }

    public void InterruptAttack()
    {
        if (ActiveAttack != null)
            ActiveAttack.CancelAttack();
    }

    private void OnDisable()
    {
        InterruptAttack();
    }
}