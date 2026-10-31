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
    [SerializeField] private string moveXProperty = "MoveX";
    [SerializeField] private string moveYProperty = "MoveY";

    private int attackHash;
    private int attackLevelHash;
    private int inRangeHash;
    private int hitHash;
    private int deathHash;
    private int moveHash;
    private int moveXHash;
    private int moveYHash;

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
        moveXHash = Animator.StringToHash(moveXProperty);
        moveYHash = Animator.StringToHash(moveYProperty);
    }

    public void SetMovement(Vector3 direction)
    {
        float magnitude = direction.magnitude;
        animator.SetFloat(moveHash, magnitude);
        animator.SetFloat(moveXHash, direction.x);
        animator.SetFloat(moveYHash, direction.z);
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