using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] CinemachineCamera normalCam;
    [SerializeField] CinemachineCamera aimCam;
    private InputReader input;
    [SerializeField] int normalPriority = 10;
    [SerializeField] int aimPriority = 5;
    public bool IsAiming { get; private set; }
    [SerializeField] string camTargetName = "CameraTarget";
    [SerializeField] GameObject player;
    [SerializeField] CinemachineCamera[] cams;
    private StateMachine state;
    private CinemachineOrbitalFollow normalOrbit;
    private CinemachineOrbitalFollow aimOrbit;

    void Start()
    {
        // Set the camera priority
        normalCam.Priority.Value = normalPriority;
        aimCam.Priority.Value = aimPriority;

        // Get the input reader script
        input = player.GetComponent<InputReader>();
        state = player.GetComponent<StateMachine>();

        // Assign the look and follow target
        Transform target = player.transform.Find(camTargetName);
        foreach (var cam in cams)
        {
            cam.Follow = target;
            cam.LookAt = target;
        }

        // Get the camera orbital follows
        normalOrbit = normalCam.GetComponent<CinemachineOrbitalFollow>();
        aimOrbit = aimCam.GetComponent<CinemachineOrbitalFollow>();
    }

    void Update()
    {
        bool shouldAim =
            input.AimInput > 0.5f &&
            state.CurrentActionState == StateMachine.ActionState.Aiming;

        if (shouldAim && !IsAiming)
        {
            SyncAimCamera();

            aimCam.Priority.Value = normalPriority + 1;
            IsAiming = true;
        }
        else if (!shouldAim && IsAiming)
        {
            SyncNormalCamera();

            aimCam.Priority.Value = aimPriority;
            IsAiming = false;
        }
    }

    private void SyncAimCamera()
    {
        aimOrbit.HorizontalAxis.Value = normalOrbit.HorizontalAxis.Value;
        aimOrbit.VerticalAxis.Value = normalOrbit.VerticalAxis.Value;
    }

    private void SyncNormalCamera()
    {
        normalOrbit.HorizontalAxis.Value = aimOrbit.HorizontalAxis.Value;
        normalOrbit.VerticalAxis.Value = aimOrbit.VerticalAxis.Value;
    }
}