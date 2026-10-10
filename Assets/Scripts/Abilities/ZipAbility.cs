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

    [Header("Targeting")]
    [SerializeField] float targetDistance = 30f;
    [SerializeField] float zipDuration = 1.5f;
    [SerializeField] LayerMask sandLayer;

    [Header("Target Colours")]
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

    private Vector3 currentTargetPosition;

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

        UpdateAiming();

        // todo: start the aim animation
    }

    public void UpdateAiming()
    {
        // Cast a ray from the camera forward.
        Ray ray = new Ray(
            Camera.main.transform.position,
            Camera.main.transform.forward
        );

        // Try to find a surface
        bool hitSurface = Physics.Raycast(
            ray,
            out RaycastHit hit,
            targetDistance,
            sandLayer
        );

        if (hitSurface)
        {
            // Target the surface.
            currentTargetPosition = hit.point;
        }
        else
        {
            // Target a point in the air.
            currentTargetPosition = ray.GetPoint(targetDistance);
        }

        // Both surface and air targets are valid.
        hasValidTarget = true;

        // Update target position.
        targetInstance.transform.position = currentTargetPosition;

        // Update target glow colour.
        UpdateTargetColour();

        // Update the ray between the hand and target.
        UpdateRay();
    }

    void UpdateTargetColour()
    {
        if (targetRenderer == null)
            return;

        Color colour = hasValidTarget
            ? validColour
            : invalidColour;

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
            state.ExitState(StateMachine.ActionState.Aiming);
            return;
        }

        // Leave the aim state before entering the zip state
        state.ExitState(StateMachine.ActionState.Aiming);

        if (!state.TryEnterState(StateMachine.ActionState.Zipping))
            return;

        // Pass the selected target to the movement controller.
        movementController.StartZip(currentTargetPosition, zipDuration);
        Debug.Log($"Zip to {currentTargetPosition}");
    }

    public void CancelAiming()
    {
        // Exit targeting mode without zipping.
        state.ExitState(StateMachine.ActionState.Aiming);
        hasValidTarget = false;

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
