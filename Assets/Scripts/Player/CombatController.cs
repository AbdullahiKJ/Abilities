using UnityEngine;

public class CombatController : MonoBehaviour
{
    private InputReader input;
    private StateMachine state;
    private AnimationController anim;
    private Animator animator;
    private Camera cam;

    // Shoot settings
    [SerializeField] float fireRate;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float maxAimDistance = 50f;
    [SerializeField] Transform gunTip;
    private float lastFireTime;
    Vector3 screenCenter;

    private void Awake()
    {
        input = GetComponent<InputReader>();
        state = GetComponent<StateMachine>();
        anim = GetComponent<AnimationController>();
        animator = GetComponent<Animator>();
        cam = Camera.main;

        screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
    }

    private void OnEnable()
    {
        input.PunchPressed += OnPunch;
        input.ShootPressed += TryFire;
    }

    private void OnDisable()
    {
        input.PunchPressed -= OnPunch;
        input.ShootPressed -= TryFire;
    }

    private void OnPunch()
    {
        TriggerBasicPunch();
    }

    // Animation events
    public void OnPunchAnimationEvent()
    {

    }

    // Attack triggers
    private void TriggerBasicPunch()
    {
        if (state.CurrentState == StateMachine.PlayerState.Attacking)
            return;

        state.SetAttacking(true);
        anim.PlayPunchAnim();
        Invoke(nameof(EndAttack), 0.4f);
    }

    private void TryFire()
    {
        if (Time.time < lastFireTime + fireRate || input.AimInput < 1f || !input.canAim)
            return;

        lastFireTime = Time.time;
        state.SetAttacking(true);
        anim.PlayShootAnim();

        // Fire the projectile
        Ray aimRay = cam.ScreenPointToRay(screenCenter);
        Vector3 targetPoint;

        if (Physics.Raycast(aimRay, out RaycastHit hit, maxAimDistance))
            targetPoint = hit.point;
        else
            targetPoint = aimRay.origin + aimRay.direction * maxAimDistance;

        Vector3 fireDirection = (targetPoint - gunTip.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, gunTip.position, Quaternion.LookRotation(fireDirection));
        bullet.GetComponent<Bullet>().Fire(fireDirection);
    }


    private void EndAttack()
    {
        state.SetAttacking(false);
        animator.SetLayerWeight(1, 1f);
    }
}