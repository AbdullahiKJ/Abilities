using DG.Tweening;
using UnityEngine;
using UnityEngine.VFX;

public class VFXController : MonoBehaviour
{
    [Header("VFX Pefabs")]
    [SerializeField] GameObject hitPrefab;
    [SerializeField] GameObject dodgePrefab;
    [SerializeField] GameObject dashPrefab;

    [Header("Mesh Renderers")]
    [SerializeField] SkinnedMeshRenderer surfaceRenderer;
    [SerializeField] SkinnedMeshRenderer jointRenderer;

    [Header("Base Character VFX")]
    [SerializeField] VisualEffect idleVFX;
    [SerializeField] VisualEffect shpereVFX;

    [Header("Zip Settings")]
    [SerializeField] float zipAttractionForce = 100f;
    [SerializeField] float zipInitialVelocity = 20f;
    [SerializeField] float zipBaseIntensity = 100f;

    [Header("Dash Settings")]
    GameObject dashPrefabInstance;
    [Tooltip("Duration of the reform effect after dashing")]
    [SerializeField] float dashReformDuration = 0.5f;
    [Tooltip("Proportion of the reform duration at which alpha starts decreasing")]
    [SerializeField] float dashAlphaDelayProp = 0.75f;

    [Header("Weapon Formation")]
    [SerializeField] GameObject formationPrefab;

    [Header("Hover Settings")]
    [SerializeField] GameObject propeller;

    [Header("Jump Charge")]
    [SerializeField] private VisualEffect sandVFX;

    [SerializeField] private float normalRadius;
    [SerializeField] private float chargedRadius = 2f;

    [SerializeField] private Color normalColor1;
    [SerializeField] private Color normalColor2;

    [SerializeField] private Color chargedColor1 = Color.red;
    [SerializeField] private Color chargedColor2 = Color.magenta;

    private static readonly int RadiusID = Shader.PropertyToID("radius");
    private static readonly int Color1ID = Shader.PropertyToID("minColor");
    private static readonly int Color2ID = Shader.PropertyToID("maxColor");

    private Tween chargeTween;
    private float currentCharge;
    private bool isUsingDash;
    Vector3 previousPosition;
    VisualEffect dashVFX;

    void Start()
    {
        // Enable the base vfx game objects
        if (idleVFX)
            idleVFX.gameObject.SetActive(true);
        if (shpereVFX)
            shpereVFX.gameObject.SetActive(true);

        // Play the idle vfx 
        if (idleVFX)
            idleVFX.Play();

        // Stop the sphere vfx
        if (shpereVFX)
            shpereVFX.Stop();

        // Disable the propeller game object
        if (propeller)
            propeller.SetActive(false);

        previousPosition = transform.position;
    }

    void LateUpdate()
    {
        if (isUsingDash)
        {
            Vector3 currentPosition = transform.position;

            Vector3 velocity =
                (currentPosition - previousPosition) / Time.deltaTime;

            dashVFX.SetVector3("targetVelocity", velocity);

            previousPosition = currentPosition;
        }
    }

    public void CreateHitVFX(float lifetime, float radius, Vector3 entryPoint, Vector3 exitPoint)
    {
        // Instantiate the vfx prefab and assign graph properties
        GameObject prefab = Instantiate(hitPrefab, Vector3.zero, Quaternion.identity);
        Destroy(prefab, lifetime);
        VisualEffect vfx = prefab.GetComponent<VisualEffect>();

        vfx.SetFloat("lifetime", lifetime);
        vfx.SetFloat("radius", radius);
        vfx.SetVector3("capsuleStart", entryPoint);
        vfx.SetVector3("capsuleEnd", exitPoint);
    }

    public void CreateDodgeVFX(float dodgeDuration, Vector3 direction, float dodgeDistance)
    {
        GameObject prefabInstance = Instantiate(dodgePrefab, transform.position, Quaternion.identity);
        Destroy(prefabInstance, dodgeDuration);
        VisualEffect vfx = prefabInstance.GetComponent<VisualEffect>();

        // Set the vfx properties
        vfx.SetFloat("lifetime", dodgeDuration);
        vfx.SetVector3("dashDirection", direction);
        vfx.SetFloat("dashDistance", dodgeDistance);
        vfx.SetSkinnedMeshRenderer("skinnedMesh", surfaceRenderer);
    }

    private void InitialiseDashProperties()
    {
        // Stop the idle vfx
        idleVFX.Stop();

        // Hide the skinned mesh renderers
        // todo: smoothly fade away before hiding
        surfaceRenderer.enabled = false;
        jointRenderer.enabled = false;

        // Instantiate the dash prefab and assign it's properties
        dashPrefabInstance = Instantiate(dashPrefab, this.transform);
        dashVFX = dashPrefabInstance.GetComponent<VisualEffect>();
        dashVFX.SetSkinnedMeshRenderer("skinnedMesh", surfaceRenderer);
        dashVFX.SetFloat("alphaReformDelay", dashAlphaDelayProp);

        // Update the is using dash flag
        isUsingDash = true;
    }

    private void ResetDashProperties(bool playSphere)
    {
        // Exit early if the dash prefab instance doesn't exist
        if (dashPrefabInstance == null)
            return;

        // Increase the reform amount over time
        VisualEffect vfx = dashPrefabInstance.GetComponent<VisualEffect>();
        DOTween.To(
            () => vfx.GetFloat("reformAmount"),
            (x) => vfx.SetFloat("reformAmount", x),
            1f,
            dashReformDuration
        ).OnComplete(() =>
        {
            // Show the skinned mesh renderers
            // todo: smoothly fade in
            surfaceRenderer.enabled = true;
            jointRenderer.enabled = true;

            // Play the idle vfx
            idleVFX.Play();

            if (playSphere)
            {
                // Stop the sphere vfx and disable the game object
                shpereVFX.Stop();
                shpereVFX.gameObject.SetActive(false);
            }

            // Destroy the existing dash prefab instance
            if (dashPrefabInstance != null)
                Destroy(dashPrefabInstance);

            // Update the is using dash flag
            isUsingDash = false;
        });
    }

    public void StartDashVFX()
    {
        // Initialise the dash vfx
        InitialiseDashProperties();

        // Enable and start the sphere vfx
        shpereVFX.gameObject.SetActive(true);
        shpereVFX.Play();
    }

    public void StopDashVFX()
    {
        ResetDashProperties(true);
    }

    public void StartZipVFX()
    {
        // Initialise the dash vfx
        InitialiseDashProperties();

        // Assign additional properties for the zip vfx
        dashVFX.SetFloat("attractionForce", zipAttractionForce);
        dashVFX.SetFloat("initialVelocity", zipInitialVelocity);
        dashVFX.SetFloat("baseIntensity", zipBaseIntensity);
    }

    public void StopZipVFX()
    {
        ResetDashProperties(false);
    }

    public void StartJumpCharge()
    {
        chargeTween?.Kill();

        currentCharge = 0f;
        UpdateJumpCharge(0f);
    }

    public void UpdateJumpCharge(float chargePercent)
    {
        currentCharge = Mathf.Clamp01(chargePercent);

        // Increase VFX Graph radius.
        float radius = Mathf.Lerp(
            normalRadius,
            chargedRadius,
            currentCharge
        );

        sandVFX.SetFloat("radius", radius);

        // Transition Shader Graph colours.
        Color color1 = Color.Lerp(
            normalColor1,
            chargedColor1,
            currentCharge
        );

        Color color2 = Color.Lerp(
            normalColor2,
            chargedColor2,
            currentCharge
        );

        sandVFX.SetFloat(RadiusID, radius);
        sandVFX.SetVector4(Color1ID, color1);
        sandVFX.SetVector4(Color2ID, color2);
    }

    public void StopJumpCharge()
    {
        chargeTween?.Kill();

        chargeTween = DOTween.To(
            () => currentCharge,
            value => UpdateJumpCharge(value),
            0f,
            0.2f
        );
    }

    public void PlayJumpExplosion()
    {
        // Play the sand explosion.
        // todo: implement the explosion
        // sandExplosion.Play();
    }

    public void StartHoverVFX()
    {
        // Stop the idle vfx
        idleVFX.Stop();

        // Hide the skinned mesh renderers
        // todo: smoothly fade away before hiding
        surfaceRenderer.enabled = false;
        jointRenderer.enabled = false;

        // Show the propeller mesh
        propeller.SetActive(true);
    }

    public void StopHoverVFX()
    {
        // Hide the propeller mesh
        propeller.SetActive(false);

        // Show the skinned mesh renderers
        // todo: smoothly fade away before hiding
        surfaceRenderer.enabled = true;
        jointRenderer.enabled = true;

        // Play the idle vfx
        idleVFX.Play();
    }

    public void PlayWeaponFormationVFX(Transform parent, WeaponData data)
    {
        // Create the vfx instance
        GameObject instance = Instantiate(formationPrefab, parent);

        // Assign the vfx settings
        VisualEffect vfx = instance.GetComponent<VisualEffect>();
        vfx.SetFloat("formationDuration", data.formationDuration);
        Mesh mesh = parent.gameObject.GetComponentInChildren<Mesh>();
        vfx.SetMesh("mesh", mesh);
        Vector3 spawnPos = parent.position - Vector3.one * 5f;
        vfx.SetVector3("position", spawnPos);

        // Destroy the instance at the end of its lifetime
        Destroy(instance, data.formationDuration);
    }
}
