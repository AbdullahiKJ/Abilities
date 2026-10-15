using UnityEngine;

public class CombatController : MonoBehaviour
{
    private InputReader input;
    private StateMachine state;
    private Animator animator;
    private WeaponController weapon;

    [Header("Combo Settings")]
    [SerializeField] AttackData rootPrimaryAttack;
    [SerializeField] AttackData rootSecondaryAttack;
    [Tooltip("Duration after the attack animations ends during which the player can still buffer the next attack in the combo")]
    [SerializeField] float extraComboWindow = 0.5f;
    private AttackData currentAttack;
    private bool comboWindowOpen;
    private bool bufferActive = false;
    private float comboBufferTimer;

    private void Awake()
    {
        input = GetComponent<InputReader>();
        state = GetComponent<StateMachine>();
        animator = GetComponent<Animator>();
        weapon = GetComponent<WeaponController>();
    }

    void Update()
    {
        if (!bufferActive)
            return;

        comboBufferTimer -= Time.deltaTime;

        if (comboBufferTimer <= 0f)
        {
            bufferActive = false;
            comboWindowOpen = false;
            currentAttack = null;
            comboBufferTimer = extraComboWindow;
        }

    }

    private void OnEnable()
    {
        input.PrimaryPressed += OnPrimary;
        input.SecondaryPressed += OnSecondary;
    }

    private void OnDisable()
    {
        input.PrimaryPressed -= OnPrimary;
        input.SecondaryPressed -= OnSecondary;
    }

    private void OnAttack(InputType inputType)
    {
        // incompatible action states.
        if (!state.CanAttack)
            return;

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

    // Animation callback for opening the combo window
    public void OpenComboWindow()
    {
        comboWindowOpen = true;
    }

    // Animation callback for closing the combo window
    public void CloseComboWindow()
    {
        comboWindowOpen = false;
    }

    // Animation callback for ending the animation and activating the buffer
    public void OnAttackFinished()
    {
        state.SetAttacking(false);

        bufferActive = true;
        comboBufferTimer = extraComboWindow;
    }

    // Animation callback for adding weapons
    public void ShowWeapon()
    {
        weapon.BeginWeaponAnimation(currentAttack.weapon, currentAttack);
    }

    // Animation callback for removing weapons
    public void HideWeapon()
    {
        weapon.FinishWeaponAnimation();
    }

    private void QueueNextAttack(InputType input)
    {
        foreach (var link in currentAttack.nextAttacks)
        {
            if (link.inputType == input)
            {
                StartAttack(link.nextAttack);
                return;
            }
        }

        // Reset to the first attack in the combo
        StartAttack(input == InputType.Primary ? rootPrimaryAttack : rootSecondaryAttack);
        return;
    }

    private void StartAttack(AttackData attack)
    {
        if (!state.CanAttack)
            return;

        // Update player state
        state.SetAttacking(true);

        currentAttack = attack;

        comboWindowOpen = false;
        bufferActive = false;
        comboBufferTimer = 0f;

        animator.CrossFade(
            attack.animationName,
            attack.crossFadeDuration
        );
    }
}