using UnityEngine;
using UnityEngine.Animations.Rigging;
using DG.Tweening;

public class AimRig : MonoBehaviour
{
    [Header("Rig")]
    [SerializeField] private Rig aimRig;
    [SerializeField] private Transform rightHandTarget;
    [SerializeField] private Transform rightShoulder;

    [Header("Settings")]
    [SerializeField] private float armReach = 0.85f;
    [SerializeField] private float rightOffset = 0.1f;
    [SerializeField] private float heightOffset = 0f;
    [SerializeField] private float transitionDuration = 0.2f;
    [SerializeField] private float targetSmoothSpeed = 15f;

    [Header("Hand Orientation")]
    [SerializeField] private Vector3 handRotationOffset;

    private Vector3 aimPosition;
    private bool isAiming;

    private Tween weightTween;

    private void Awake()
    {
        aimRig.weight = 0f;
    }

    private void LateUpdate()
    {
        if (!isAiming)
            return;

        Vector3 direction = (aimPosition - rightShoulder.position).normalized;

        Vector3 desiredPosition =
            rightShoulder.position
            + direction * armReach
            + transform.right * rightOffset
            + transform.up * heightOffset;

        rightHandTarget.position = Vector3.Lerp(
            rightHandTarget.position,
            desiredPosition,
            Time.deltaTime * targetSmoothSpeed
        );

        // Orient the hand toward the zip target.
        Vector3 handAimDirection = (aimPosition - desiredPosition).normalized;

        Quaternion desiredRotation =
            Quaternion.LookRotation(handAimDirection, transform.up) *
            Quaternion.Euler(handRotationOffset);

        rightHandTarget.rotation = Quaternion.Slerp(
            rightHandTarget.rotation,
            desiredRotation,
            Time.deltaTime * targetSmoothSpeed
        );
    }

    public void StartAiming()
    {
        isAiming = true;

        weightTween?.Kill();

        aimRig.weight = 0f;

        weightTween = DOTween.To(
            () => aimRig.weight,
            value => aimRig.weight = value,
            1f,
            transitionDuration
        );
    }

    public void UpdateAimPosition(Vector3 targetPosition)
    {
        aimPosition = targetPosition;
    }

    public void StopAiming()
    {
        isAiming = false;

        weightTween?.Kill();

        weightTween = DOTween.To(
            () => aimRig.weight,
            value => aimRig.weight = value,
            0f,
            transitionDuration
        );
    }

    private void OnDestroy()
    {
        weightTween?.Kill();
    }
}