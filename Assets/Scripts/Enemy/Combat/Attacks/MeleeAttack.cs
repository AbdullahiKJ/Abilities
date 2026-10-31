using UnityEngine;

public class MeleeAttack : EnemyAttack
{
    [Header("References")]
    [SerializeField] private EnemyAnimationController animationController;

    public override void ExecuteAttack(Transform target)
    {
        animationController.PlayAttack(0);
        return;
    }
}