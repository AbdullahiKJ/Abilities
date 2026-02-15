using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] CinemachineCamera normalCam;
    [SerializeField] CinemachineCamera aimCam;
    [SerializeField] PlayerManager playerManager;
    private InputReader input;
    [SerializeField] int normalPriority = 10;
    [SerializeField] int aimPriority = 5;
    public bool IsAiming { get; private set; }

    void Start()
    {
        // Set the camera priority
        normalCam.Priority.Value = normalPriority;
        aimCam.Priority.Value = aimPriority;

        // Get the current playable character
        input = playerManager.characterArray[playerManager.currentChar].GetComponent<InputReader>();
    }

    void Update()
    {
        IsAiming = input.AimInput > 0.5f;

        // Update the aim cam priority
        if (IsAiming)
        {
            aimCam.Priority.Value = normalPriority + 1;
        }
        else
            aimCam.Priority.Value = aimPriority;
    }
}