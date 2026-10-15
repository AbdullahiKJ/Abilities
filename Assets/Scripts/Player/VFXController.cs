using System.Collections;
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
    [SerializeField] GameObject scorpionTail;

    [Header("Dash Settings")]
    GameObject dashPrefabChild;
    GameObject dashPrefabInstance;
    [Tooltip("Duration of the reform effect after dashing")]
    [SerializeField] float reformDuration = 0.5f;
    [Tooltip("Proportion of the reform duration at which alpha starts decreasing")]
    [SerializeField] float alphaDelayProp = 0.75f;
    [SerializeField] Vector3 defaultTailPos = new Vector3(0f, -0.15f, 0f);
    [SerializeField] private float tailRetractDistance = 1f;
    [SerializeField] private float tailRetractDuration = 0.2f;

    [Header("Weapon Formation")]
    [SerializeField] GameObject formationPrefab;

    [Header("Hover Settings")]
    [SerializeField] GameObject propeller;

    void Start()
    {
        // Enable the base vfx game objects
        if (idleVFX)
            idleVFX.gameObject.SetActive(true);

        // Hide the scorpion tail
        scorpionTail.SetActive(false);

        // Play the idle vfx 
        if (idleVFX)
            idleVFX.Play();

        // Disable the propeller game object
        if (propeller)
            propeller.SetActive(false);
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

    public void StartDashVFX()
    {
        // Stop the idle vfx
        idleVFX.Stop();

        // Hide the skinned mesh renderers
        // todo: smoothly fade away before hiding
        surfaceRenderer.enabled = false;
        jointRenderer.enabled = false;

        // Show the scorpion tail and set it's position
        // todo: create vfx to transition from player to tail
        scorpionTail.SetActive(true);
        scorpionTail.transform.localPosition = defaultTailPos;

        // Instantiate the dash prefab and assign it's properties
        dashPrefabInstance = Instantiate(dashPrefab, transform.position, Quaternion.identity);
        VisualEffect vfx = dashPrefabInstance.GetComponent<VisualEffect>();
        vfx.SetSkinnedMeshRenderer("skinnedMesh", surfaceRenderer);
        vfx.SetFloat("alphaReformDelay", alphaDelayProp);

        // Reparent the target game object to the sphere vfx
        dashPrefabChild = dashPrefabInstance.transform.GetChild(0).gameObject;
        dashPrefabChild.transform.SetParent(scorpionTail.transform, false);
        dashPrefabChild.transform.localPosition = Vector3.zero;
    }

    public void StopDashVFX()
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
            reformDuration
        ).OnComplete(() =>
        {
            // Show the skinned mesh renderers
            // todo: smoothly fade in
            surfaceRenderer.enabled = true;
            jointRenderer.enabled = true;

            // Play the idle vfx
            idleVFX.Play();

            // Hide the scorpion tail
            scorpionTail.gameObject.SetActive(false);

            // Destroy the existing dash prefab instance and child
            if (dashPrefabInstance != null)
                Destroy(dashPrefabInstance);
            if (dashPrefabChild != null)
                Destroy(dashPrefabChild);
        });
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
