using DG.Tweening;
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

    [Header("Zip Camera")]
    [SerializeField] private float zipZoomMultiplier = 1.8f;
    [SerializeField] private float zoomOutFraction = 0.15f;
    [SerializeField] private float zoomHoldFraction = 0.4f;
    [SerializeField] private float zoomInFraction = 0.45f;
    private float originalTopRadius;
    private float originalCenterRadius;
    private float originalBottomRadius;
    private Tween zipCameraTween;

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

        // Store the initial orbit radii
        originalTopRadius = normalOrbit.Orbits.Top.Radius;
        originalCenterRadius = normalOrbit.Orbits.Center.Radius;
        originalBottomRadius = normalOrbit.Orbits.Bottom.Radius;
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

    public void PlayZipCameraEffect(float duration)
    {
        if (normalOrbit == null)
            return;

        // Initialise the zoom transition durations
        float zoomOutDuration = duration * zoomOutFraction;
        float zoomHoldDuration = duration * zoomHoldFraction;
        float zoomInDuration = duration * zoomInFraction;

        // Stop any previous zoom animation.
        zipCameraTween?.Kill();

        float zoom = 1f;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(
            DOTween.To(
                () => zoom,
                value =>
                {
                    zoom = value;

                    var orbits = normalOrbit.Orbits;

                    orbits.Top.Radius = originalTopRadius * zoom;
                    orbits.Center.Radius = originalCenterRadius * zoom;
                    orbits.Bottom.Radius = originalBottomRadius * zoom;

                    normalOrbit.Orbits = orbits;
                },
                zipZoomMultiplier,
                zoomOutDuration
            ).SetEase(Ease.OutCubic)
        );

        sequence.AppendInterval(zoomHoldDuration);

        sequence.Append(
            DOTween.To(
                () => zoom,
                value =>
                {
                    zoom = value;

                    var orbits = normalOrbit.Orbits;

                    orbits.Top.Radius = originalTopRadius * zoom;
                    orbits.Center.Radius = originalCenterRadius * zoom;
                    orbits.Bottom.Radius = originalBottomRadius * zoom;

                    normalOrbit.Orbits = orbits;
                },
                1f,
                zoomInDuration
            ).SetEase(Ease.InOutSine)
        );

        zipCameraTween = sequence;
    }
}