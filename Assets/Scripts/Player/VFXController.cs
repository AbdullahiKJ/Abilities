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
    [SerializeField] VisualEffect sphereVFX;
    [SerializeField] float sphereVFXRadius = 0.2f;

    [Header("Dash Settings")]
    GameObject dashPrefabChild;
    GameObject dashPrefabInstance;
    [Tooltip("Duration of the reform effect after dashing")]
    [SerializeField] float reformDuration = 0.5f;
    [Tooltip("Proportion of the reform duration at which alpha starts decreasing")]
    [SerializeField] float alphaDelayProp = 0.75f;

    void Start()
    {
        // Enable the base vfx game objects
        idleVFX.gameObject.SetActive(true);
        sphereVFX.gameObject.SetActive(true);

        // Play the idle vfx and Stop the sphere vfx
        idleVFX.Play();
        sphereVFX.Stop();

        // Set the float property on the sphere vfx
        sphereVFX.SetFloat("radius", sphereVFXRadius);
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
        // Play the sphere vfx and stop the idle vfx
        idleVFX.Stop();
        sphereVFX.Play();

        // Hide the skinned mesh renderers
        surfaceRenderer.enabled = false;
        jointRenderer.enabled = false;

        // Instantiate the dash prefab and assign it's properties
        dashPrefabInstance = Instantiate(dashPrefab, transform.position, Quaternion.identity);
        VisualEffect vfx = dashPrefabInstance.GetComponent<VisualEffect>();
        vfx.SetSkinnedMeshRenderer("skinnedMesh", surfaceRenderer);
        vfx.SetFloat("alphaReformDelay", alphaDelayProp);

        // Reparent the target game object to the sphere vfx
        dashPrefabChild = dashPrefabInstance.transform.GetChild(0).gameObject;
        dashPrefabChild.transform.SetParent(sphereVFX.transform, false);
        dashPrefabChild.transform.localPosition = Vector3.zero;
    }

    public void StopDashVFX()
    {
        // Exit early if the dash prefab instance doesn't exist
        if(dashPrefabInstance == null)
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
            surfaceRenderer.enabled = true;
            jointRenderer.enabled = true;

            // Stop the sphere vfx and play the idle vfx
            idleVFX.Play();
            sphereVFX.Stop();

            // Destroy the existing dash prefab instance and child
            if (dashPrefabInstance != null)
                Destroy(dashPrefabInstance);
            if (dashPrefabChild != null)
                Destroy(dashPrefabChild);
        });
    }
}
