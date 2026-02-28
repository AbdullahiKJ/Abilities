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

    [Header("Wall Destruction Settings")]
    [SerializeField] VisualEffect destructionVFX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke(nameof(RaiseVFX), 1f);
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
        // Move the wall mesh to the start position and animate it to the end position
        wallMesh.transform.localPosition = startPosition;
        wallMesh.transform.DOLocalMove(endPosition, riseDuration);
    }
}
