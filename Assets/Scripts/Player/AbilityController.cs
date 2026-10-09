using UnityEngine;

public class AbilityController : MonoBehaviour
{
    [Header("Abilities")]
    private ShieldAbility shieldAbility;
    private SpikeAbility spikesAbility;
    private SpecialAbility specialAbility;
    private ZipAbility zipAbility;

    private ConstructType selectedConstruct = ConstructType.Shield;
    private InputReader input;

    private bool isAiming = false;
    public bool IsAiming => isAiming;

    public enum ConstructType
    {
        Shield,
        Spikes
    }

    void Awake()
    {
        // Get input and ability scripts
        input = GetComponent<InputReader>();
        shieldAbility = GetComponent<ShieldAbility>();
        spikesAbility = GetComponent<SpikeAbility>();
        specialAbility = GetComponent<SpecialAbility>();
        zipAbility = GetComponent<ZipAbility>();
    }

    private void OnEnable()
    {
        input.ConstructPressed += ActivateConstruct;
        input.AimStarted += BeginZipAim;
        input.AimReleased += ExecuteZip;
        input.SpecialPressed += ActivateSpecial;
    }

    private void OnDisable()
    {
        input.ConstructPressed -= ActivateConstruct;
        input.AimStarted -= BeginZipAim;
        input.AimReleased -= ExecuteZip;
        input.SpecialPressed -= ActivateSpecial;
    }

    public void SelectShield()
    {
        selectedConstruct = ConstructType.Shield;
    }

    public void SelectSpikes()
    {
        selectedConstruct = ConstructType.Spikes;
    }

    public void ActivateConstruct()
    {
        switch (selectedConstruct)
        {
            case ConstructType.Shield:

                if (shieldAbility.CanActivate())
                    shieldAbility.Activate();

                break;

            case ConstructType.Spikes:

                if (spikesAbility.CanActivate())
                    spikesAbility.Activate();

                break;
        }
    }

    public void ActivateSpecial()
    {
        if (specialAbility.CanActivate())
            specialAbility.Activate();
    }

    public void BeginZipAim()
    {
        isAiming = true;
        zipAbility.StartAiming();
    }

    public void ExecuteZip()
    {
        isAiming = false;
        zipAbility.ExecuteZip();
    }

    public void CancelZip()
    {
        isAiming = false;
        zipAbility.CancelAiming();
    }
}

public interface IPlayerAbility
{
    bool CanActivate();

    void Activate();
}
