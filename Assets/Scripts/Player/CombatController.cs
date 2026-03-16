using UnityEngine;

public class CombatController : MonoBehaviour
{
    private InputReader input;
    private StateMachine state;
    private AnimationController anim;
    private Animator animator;
    private Camera cam;

    [Header("Shoot settings")]
    [SerializeField] float fireRate;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float maxAimDistance = 50f;
    [SerializeField] Transform gunTip;
    private float lastFireTime;
    Vector3 screenCenter;
    bool isAiming;

    [Header("Wall Settings")]
    [SerializeField] GameObject wallPrefab;
    [SerializeField] GameObject wallPlacementPrefab;
    GameObject wallInstance;
    GameObject wallPlacementInstance;
    [SerializeField] float wallOffset = 5f;

    [Header("Combo Settings")]
    [SerializeField] AttackData rootPrimaryAttack;
    private AttackData currentAttack;
    private AttackData queuedAttack;
    private bool comboWindowOpen;

    private void Awake()
    {
        input = GetComponent<InputReader>();
        state = GetComponent<StateMachine>();
        anim = GetComponent<AnimationController>();
        animator = GetComponent<Animator>();
        cam = Camera.main;

        screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
    }

    void Update()
    {
        isAiming = input.AimInput > 0.5f;
        PlaceWallPreview(isAiming);

        if (currentAttack == null)
            return;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        float normalizedTime = state.normalizedTime % 1f;

        comboWindowOpen =
            normalizedTime >= currentAttack.comboWindowOpen &&
            normalizedTime <= currentAttack.comboWindowClose;

        if (normalizedTime >= 1f)
        {
            OnAttackFinished();
        }
    }

    private void OnEnable()
    {
        input.PrimaryPressed += OnPrimary;
        input.ShootPressed += TryFire;
    }

    private void OnDisable()
    {
        input.PrimaryPressed -= OnPrimary;
        input.ShootPressed -= TryFire;
    }

    private void OnPrimary()
    {
        // Update player state
        state.SetAttacking(true);

        if (currentAttack == null)
        {
            StartAttack(rootPrimaryAttack); // first attack in combo
            return;
        }

        if (!comboWindowOpen)
            return;

        QueueNextAttack(InputType.Primary);
    }


    public void OpenComboWindow()
    {
        comboWindowOpen = true;
    }

    public void CloseComboWindow()
    {
        comboWindowOpen = false;
    }

    private void QueueNextAttack(InputType input)
    {
        foreach (var link in currentAttack.nextAttacks)
        {
            if (link.inputType == input)
            {
                queuedAttack = link.nextAttack;
                return;
            }
        }
    }

    private void StartAttack(AttackData attack)
    {
        currentAttack = attack;
        comboWindowOpen = false;
        animator.CrossFade(
            attack.animationName,
            attack.crossFadeDuration
        );
    }

    private void TryFire()
    {
        // Placing Wall logic
        if (!input.canAim)
        {
            if (wallInstance == null && isAiming)
            {
                Vector3 camForward = cam.transform.forward;
                camForward.y = 0f;
                camForward.Normalize();
                Vector3 spawnPos = transform.position + camForward * wallOffset;
                wallInstance = Instantiate(wallPrefab, spawnPos, Quaternion.LookRotation(camForward));
            }
        }
        else if (Time.time < lastFireTime + fireRate || input.AimInput < 1f)
            return;
        // Shooting logic
        else
        {
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
    }

    public void OnAttackFinished()
    {
        state.SetAttacking(false);
        comboWindowOpen = false;
        if (queuedAttack != null)
        {
            StartAttack(queuedAttack);
            queuedAttack = null;
        }
        else
        {
            currentAttack = null;
        }
    }

    void PlaceWallPreview(bool isAiming)
    {
        if (isAiming)
        {
            // Calculate wall placement position
            Vector3 camForward = cam.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();
            Vector3 wallPos = transform.position + camForward * wallOffset;

            // Create the wall placement prefab if it doesn't exist
            if (wallPlacementInstance == null)
            {
                wallPlacementInstance = Instantiate(wallPlacementPrefab, wallPos, Quaternion.LookRotation(camForward));
            }
            // Move the wall placement prefab to the new position and rotate it to face the camera
            else
            {
                wallPlacementInstance.transform.position = wallPos;
                wallPlacementInstance.transform.rotation = Quaternion.LookRotation(camForward);
            }
        }
        else
        {
            // Destroy the wall placement prefab if it exists
            if (wallPlacementInstance != null)
            {
                Destroy(wallPlacementInstance);
            }
        }
    }
}