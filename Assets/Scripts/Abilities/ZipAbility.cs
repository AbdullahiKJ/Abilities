using UnityEngine;

public class ZipAbility : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] GameObject targetPrefab;
    [SerializeField] GameObject rayPrefab;

    [Header("References")]
    [SerializeField] Transform handTransform;
    Camera cam;
    private MovementController movementController;
    private StateMachine state;
    [SerializeField] AimRig aimRig;

    [Header("Targeting")]
    [SerializeField] float targetDistance = 30f;
    [SerializeField] float zipDuration = 1.5f;
    [SerializeField] LayerMask targetableLayer;
    [SerializeField] LayerMask obstacleLayer;

    [Header("Target Colours")]
    [ColorUsage(true, true)]
    [SerializeField] private Color baseColour = Color.blue;

    [ColorUsage(true, true)]
    [SerializeField] private Color validColour = Color.blue;

    [ColorUsage(true, true)]
    [SerializeField] private Color invalidColour = Color.red;
    [SerializeField] private string glowProperty = "_GlowColor";

    GameObject targetInstance;
    GameObject rayInstance;

    private Renderer targetRenderer;
    private MaterialPropertyBlock propertyBlock;
    private LineRenderer lineRenderer;

    private int glowPropertyID;

    private bool hasValidTarget;
    private bool hitSurface;

    private Vector3 currentTargetPosition;

    [Header("Wall Climbing")]
    [SerializeField] private LayerMask climbableWallLayer;
    private bool targetIsClimbableWall;
    private Vector3 targetWallNormal;

    void Awake()
    {
        if (cam == null)
            cam = Camera.main;

        propertyBlock = new MaterialPropertyBlock();
        glowPropertyID = Shader.PropertyToID(glowProperty);

        movementController = GetComponent<MovementController>();
        state = GetComponent<StateMachine>();
    }

    void Update()
    {
        if (state.CurrentActionState == StateMachine.ActionState.Aiming)
            UpdateAiming();
    }

    public void StartAiming()
    {
        // Enter targeting mode and create the targeting and ray prefabs.
        if (!state.TryEnterState(StateMachine.ActionState.Aiming))
            return;

        if (targetInstance == null)
        {
            targetInstance = Instantiate(targetPrefab);
            targetRenderer = targetInstance.GetComponentInChildren<Renderer>();
        }
        if (rayInstance == null)
        {
            rayInstance = Instantiate(rayPrefab);
            lineRenderer = rayInstance.GetComponentInChildren<LineRenderer>();
        }

        targetInstance.SetActive(true);
        rayInstance.SetActive(true);

        // Change move speed
        movementController.ToggleAimMovement(true);

        // Start aiming with the arm rig
        aimRig.StartAiming();

        UpdateAiming();
    }

    public void UpdateAiming()
    {
        // Cast a ray from the camera forward.
        Ray ray = new Ray(
            Camera.main.transform.position,
            Camera.main.transform.forward
        );

        // Try to find a surface
        hitSurface = Physics.Raycast(
            ray,
            out RaycastHit hit,
            targetDistance,
            obstacleLayer,
            QueryTriggerInteraction.Ignore
        );

        targetIsClimbableWall = false;
        targetWallNormal = Vector3.zero;

        if (hitSurface)
        {
            // Target the surface hit
            currentTargetPosition = hit.point;

            int hitLayerMask = 1 << hit.collider.gameObject.layer;

            // Check if the hit surface is valid
            hasValidTarget = (targetableLayer.value & hitLayerMask) != 0;

            // Check if the target is a climbable wall
            targetIsClimbableWall =
                hasValidTarget &&
                (climbableWallLayer.value & hitLayerMask) != 0;

            if (targetIsClimbableWall)
                targetWallNormal = hit.normal;
        }
        else
        {
            // Target a point in the air 
            currentTargetPosition = ray.GetPoint(targetDistance);

            // Air zipping is allowed.
            hasValidTarget = true;
        }

        // Update target position.
        targetInstance.transform.position = currentTargetPosition;

        // Update the right aim
        aimRig.UpdateAimPosition(currentTargetPosition);

        // Update target glow colour.
        UpdateTargetColour();

        // Update the ray between the hand and target.
        UpdateRay();
    }

    void UpdateTargetColour()
    {
        if (targetRenderer == null)
            return;

        Color colour;

        if (!hitSurface)
            colour = baseColour;
        else if (hasValidTarget)
            colour = validColour;
        else
            colour = invalidColour;

        targetRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(glowPropertyID, colour);
        targetRenderer.SetPropertyBlock(propertyBlock);
    }

    void UpdateRay()
    {
        if (lineRenderer == null || handTransform == null)
            return;

        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;

        lineRenderer.SetPosition(0, handTransform.position);
        lineRenderer.SetPosition(1, currentTargetPosition);
    }

    public void ExecuteZip()
    {
        // Begin movement toward the selected target.
        // Exit early if not in the aiming state
        if (state.CurrentActionState != StateMachine.ActionState.Aiming)
            return;

        HideTargetingVisuals();

        // Exit the aim state if there is no valid target
        if (!hasValidTarget)
        {
            CancelAiming();
            return;
        }

        // Leave the aim state before entering the zip state
        state.ExitState(StateMachine.ActionState.Aiming);

        if (!state.TryEnterState(StateMachine.ActionState.Zipping))
            return;

        // Pass the selected target to the movement controller.
        movementController.StartZip(currentTargetPosition, zipDuration, targetIsClimbableWall, targetWallNormal);

        // Change move speed
        movementController.ToggleAimMovement(false);

        // Stop aiming with the arm rig
        aimRig.StopAiming();

        Debug.Log($"Zip to {currentTargetPosition}");
    }

    public void CancelAiming()
    {
        // Exit targeting mode without zipping.
        state.ExitState(StateMachine.ActionState.Aiming);
        hasValidTarget = false;

        // Change move speed
        movementController.ToggleAimMovement(false);

        // Stop aiming with the arm rig
        aimRig.StopAiming();

        HideTargetingVisuals();
    }

    private void HideTargetingVisuals()
    {
        if (targetInstance != null)
            targetInstance.SetActive(false);

        if (rayInstance != null)
            rayInstance.SetActive(false);
    }

    private void OnDestroy()
    {
        if (targetInstance != null)
            Destroy(targetInstance);

        if (rayInstance != null)
            Destroy(rayInstance);
    }
}
