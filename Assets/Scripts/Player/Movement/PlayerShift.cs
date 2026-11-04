using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerShift : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VFXController vfx;

    [Header("Shifted Shape")]
    [SerializeField, Min(0.1f)] private float shiftedHeight = 0.6f;
    [SerializeField, Min(0.05f)] private float shiftedRadius = 0.25f;

    [Header("Standing Clearance")]
    [SerializeField] private LayerMask obstructionLayers;

    // Small tolerance prevents floor contact from blocking standing.
    [SerializeField, Range(0.001f, 0.03f)]
    private float clearanceTolerance = 0.01f;

    private CharacterController controller;

    private float standingHeight;
    private float standingRadius;
    private float standingStepOffset;
    private Vector3 standingCenter;

    private readonly Collider[] overlaps = new Collider[32];

    public bool IsActive { get; private set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (vfx == null)
            vfx = GetComponent<VFXController>();

        standingHeight = controller.height;
        standingRadius = controller.radius;
        standingCenter = controller.center;
        standingStepOffset = controller.stepOffset;
    }

    public void Begin()
    {
        if (IsActive)
            return;

        // Ensure the shifted shape actually fits inside the standing shape.
        float radius = Mathf.Min(shiftedRadius, standingRadius);
        float height = Mathf.Clamp(
            shiftedHeight,
            radius * 2f,
            standingHeight
        );

        // Keep the bottom of the capsule at the same height.
        float bottom = standingCenter.y - standingHeight * 0.5f;

        Vector3 center = standingCenter;
        center.y = bottom + height * 0.5f;

        controller.stepOffset =
            Mathf.Min(standingStepOffset, height * 0.5f);

        controller.radius = radius;
        controller.height = height;
        controller.center = center;

        IsActive = true;
        vfx.StartDashVFX();
    }

    public bool CanStand()
    {
        if (!IsActive)
            return true;

        Vector3 center = transform.TransformPoint(standingCenter);
        Vector3 up = transform.up;

        float sphereOffset = Mathf.Max(
            0f,
            standingHeight * 0.5f - standingRadius
        );

        Vector3 top = center + up * sphereOffset;
        Vector3 bottom = center - up * sphereOffset;

        float testRadius = Mathf.Max(
            0.001f,
            standingRadius - clearanceTolerance
        );

        int count = Physics.OverlapCapsuleNonAlloc(
            bottom,
            top,
            testRadius,
            overlaps,
            obstructionLayers,
            QueryTriggerInteraction.Ignore
        );

        // Treat a full buffer as blocked rather than risk missing obstacles.
        if (count == overlaps.Length)
            return false;

        for (int i = 0; i < count; i++)
        {
            Collider other = overlaps[i];

            // Ignore this character and its child colliders.
            if (other == controller ||
                other.transform.IsChildOf(transform))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    public bool TryEnd()
    {
        if (!IsActive)
            return true;

        if (!CanStand())
            return false;

        controller.height = standingHeight;
        controller.radius = standingRadius;
        controller.center = standingCenter;
        controller.stepOffset = standingStepOffset;

        IsActive = false;
        vfx.StopDashVFX();

        return true;
    }
}