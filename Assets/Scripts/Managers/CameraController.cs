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
    }

    void Update()
    {
        // Update the aim cam priority
        if (input.AimInput > 0.5f && state.CurrentActionState == StateMachine.ActionState.Aiming)
        {
            aimCam.Priority.Value = normalPriority + 1;
        }
        else
            aimCam.Priority.Value = aimPriority;
    }
}