using DG.Tweening;
using UnityEngine;
using UnityEngine.VFX;

public class SandWall : MonoBehaviour
{
    [Header("Wall Rise Settings")]
    [SerializeField] float riseDuration = 1f;
    [SerializeField] Vector3 startPosition;
    [SerializeField] Vector3 endPosition;
    [SerializeField] GameObject riseVFXPrefab;
    [SerializeField] float riseVFXLifetime = 3f;
    [SerializeField] GameObject wallMesh;

    [Header("Wall Lifetime Settings")]
    [SerializeField] float wallLifetime = 5f;
    [SerializeField] GameObject hitVFXPrefab;
    [SerializeField] GameObject fallVFXPrefab;
    GameObject fallVFXInstance;

    [Header("Wall Destruction Settings")]
    [SerializeField] GameObject destructionVFXPrefab;
    [SerializeField] float destructionVFXLifetime = 3f;

    void OnEnable()
    {
        // Ensure the wall mesh starts at the correct position
        wallMesh.transform.localPosition = startPosition;
        RaiseVFX();
    }

    void RaiseVFX()
    {
        // Instantiate the rise VFX at the wall's position
        GameObject riseVFXInstance = Instantiate(riseVFXPrefab, transform.position, Quaternion.identity);
        Destroy(riseVFXInstance, riseVFXLifetime);

        // Raise the wall
        Invoke(nameof(RaiseWall), 0.2f);
    }

    void RaiseWall()
    {
        // Start the fall VFX
        fallVFXInstance = Instantiate(fallVFXPrefab, transform.position, Quaternion.identity);

        // Animate the wall transform to the end position
        wallMesh.transform.DOLocalMove(endPosition, riseDuration).OnComplete(() =>
        {
            // After the wall has risen, start the lifetime timer
            Invoke(nameof(DestroyWall), wallLifetime);
        });
    }

    void DestroyWall()
    {
        // Hide the wall mesh
        wallMesh.SetActive(false);

        // Destroy the fall VFX if it exists
        if (fallVFXInstance != null)
        {
            Destroy(fallVFXInstance);
        }

        // Play the destruction VFX
        GameObject destructionVFXInstance = Instantiate(destructionVFXPrefab, transform.position, Quaternion.identity);
        destructionVFXInstance.GetComponent<VisualEffect>().SetFloat("lifetime", destructionVFXLifetime);

        // Destroy the wall after the VFX has played
        Destroy(destructionVFXInstance, destructionVFXLifetime);
        Destroy(gameObject, destructionVFXLifetime);
    }
}
