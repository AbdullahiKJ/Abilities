using UnityEngine;

public class CombatController : MonoBehaviour
{
    private InputReader input;
    private StateMachine state;
    private Animator animator;
    private Camera cam;

    [Header("Wall Settings")]
    [SerializeField] GameObject wallPrefab;
    [SerializeField] GameObject wallPlacementPrefab;
    GameObject wallInstance;
    GameObject wallPlacementInstance;
    [SerializeField] float wallOffset = 5f;
    bool isAiming;

    [Header("Combo Settings")]
    [SerializeField] AttackData rootPrimaryAttack;
    [SerializeField] AttackData rootSecondaryAttack;
    [Tooltip("Duration after the attack animations ends during which the player can still buffer the next attack in the combo")]
    [SerializeField] float extraComboWindow = 0.5f;
    private float comboBufferTimer;
    private bool bufferActive = false;
    private AttackData currentAttack;
    private AttackData queuedAttack;
    private bool comboWindowOpen;

    private void Awake()
    {
        input = GetComponent<InputReader>();
        state = GetComponent<StateMachine>();
        animator = GetComponent<Animator>();
        cam = Camera.main;
    }

    void Update()
    {
        // Show the wall placement preview if aiming
        isAiming = input.AimInput > 0.5f;
        PlaceWallPreview(isAiming);

        if (currentAttack == null)
            return;

        if (bufferActive)
        {
            comboBufferTimer -= Time.deltaTime;
            CheckQueue();

            if (comboBufferTimer <= 0f)
            {
                bufferActive = false;
                comboWindowOpen = false;
                currentAttack = null;
            }
        }
    }

    private void OnEnable()
    {
        input.PrimaryPressed += OnPrimary;
        input.SecondaryPressed += OnSecondary;
        input.ShootPressed += TryFire;
    }

    private void OnDisable()
    {
        input.PrimaryPressed -= OnPrimary;
        input.SecondaryPressed -= OnSecondary;
        input.ShootPressed -= TryFire;
    }

    private void OnAttack(InputType inputType)
    {
        if (currentAttack == null)
        {
            StartAttack(inputType == InputType.Primary ? rootPrimaryAttack : rootSecondaryAttack); // first attack in combo
            return;
        }

        // Ignore the input if the combo window and buffer zone are inactive
        if (!comboWindowOpen && !bufferActive)
            return;

        QueueNextAttack(inputType);
    }

    private void OnPrimary()
    {
        OnAttack(InputType.Primary);
    }

    private void OnSecondary()
    {
        OnAttack(InputType.Secondary);
    }

    public void OpenComboWindow()
    {
        comboWindowOpen = true;
    }

    public void CloseComboWindow()
    {
        comboWindowOpen = false;
        CheckQueue();
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

        // Reset to the first attack in the combo
        StartAttack(input == InputType.Primary ? rootPrimaryAttack : rootSecondaryAttack);
        return;
    }

    private void StartAttack(AttackData attack)
    {
        // Update player state
        state.SetAttacking(true);

        currentAttack = attack;
        queuedAttack = null;
        comboWindowOpen = false;

        animator.CrossFade(
            attack.animationName,
            attack.crossFadeDuration
        );
    }

    private void TryFire()
    {
        // Place the wall instance
        if (wallInstance == null && isAiming)
        {
            Vector3 camForward = cam.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();
            Vector3 spawnPos = transform.position + camForward * wallOffset;
            wallInstance = Instantiate(wallPrefab, spawnPos, Quaternion.LookRotation(camForward));
        }
    }

    public void OnAttackFinished()
    {
        state.SetAttacking(false);

        bufferActive = true;
        comboBufferTimer = extraComboWindow;

        CheckQueue();
    }

    void CheckQueue()
    {
        if (queuedAttack != null)
        {
            StartAttack(queuedAttack);
            queuedAttack = null;
            bufferActive = false;
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