using UnityEngine;
using UnityEngine.VFX;

public class VFXController : MonoBehaviour
{
    [SerializeField] GameObject hitPrefab;
    [SerializeField] GameObject dashPrefab;
    [SerializeField] SkinnedMeshRenderer skinnedMeshRenderer;
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

    public void CreateDashVFX(float dashDuration, Vector3 direction, float dashDistance)
    {
        GameObject prefabInstance = Instantiate(dashPrefab, transform.position, Quaternion.identity);
        Destroy(prefabInstance, dashDuration);
        VisualEffect vfx = prefabInstance.GetComponent<VisualEffect>();

        // Set the vfx properties
        vfx.SetFloat("lifetime", dashDuration);
        vfx.SetVector3("dashDirection", direction);
        vfx.SetFloat("dashDistance", dashDistance);
        vfx.SetSkinnedMeshRenderer("skinnedMesh", skinnedMeshRenderer);
    }
}
