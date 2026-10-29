using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimationController : MonoBehaviour
{
    [SerializeField] private string attackProperty = "Attack";
    [SerializeField] private string attackLevelProperty = "AttackLevel";
    [SerializeField] private string inRangeProperty = "InRange";
    [SerializeField] private string hitProperty = "Hit";
    [SerializeField] private string deathProperty = "Death";
    [SerializeField] private string moveProperty = "MoveSpeed";

    private int attackHash;
    private int attackLevelHash;
    private int inRangeHash;
    private int hitHash;
    private int deathHash;
    private int moveHash;

    private Animator animator;

    public void Awake()
    {
        animator = GetComponent<Animator>();
        attackHash = Animator.StringToHash(attackProperty);
        attackLevelHash = Animator.StringToHash(attackLevelProperty);
        inRangeHash = Animator.StringToHash(inRangeProperty);
        hitHash = Animator.StringToHash(hitProperty);
        deathHash = Animator.StringToHash(deathProperty);
        moveHash = Animator.StringToHash(moveProperty);
    }

    public void SetMovementSpeed(float speed)
    {
        animator.SetFloat(moveHash, speed);
    }
    public void SetInRange(bool inRange)
    {
        animator.SetBool(inRangeHash, inRange);
    }
    public void PlayAttack(int attackIndex)
    {
        animator.SetInteger(attackLevelHash, attackIndex);
        animator.SetTrigger(attackHash);
    }
    public void PlayHit()
    {
        animator.SetTrigger(hitHash);
    }
    public void PlayDeath()
    {
        animator.SetTrigger(deathHash);
    }
}