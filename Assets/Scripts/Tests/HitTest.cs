using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class HitTest : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float maxAimDistance = 50f;
    [SerializeField] float radius = 0.2f;
    [SerializeField] LayerMask hitMask;

    void OnShoot()
    {
        // Get the mouse position
        Vector3 mousePos = Mouse.current.position.ReadValue();

        // Create a ray from the camera through the mouse position.
        Ray aimRay = cam.ScreenPointToRay(mousePos);

        Ray ray = new Ray(cam.transform.position, aimRay.direction);

        if (TryGetEntryAndExit(
            ray,
            maxAimDistance,
            hitMask,
            out Vector3 entryPoint,
            out Vector3 exitPoint,
            out HitSystem hitSystem))
        {
            // Debug exit and entry points
            // Debug.DrawLine(entryPoint, exitPoint, Color.red, 2f);

            hitSystem.OnExit(entryPoint, exitPoint, radius);
        }
    }

    public bool TryGetEntryAndExit(
       Ray ray,
       float maxDistance,
       LayerMask hitMask,
       out Vector3 entryPoint,
       out Vector3 exitPoint,
       out HitSystem hitSystem)
    {
        entryPoint = default;
        exitPoint = default;
        hitSystem = null;

        // Forward raycast
        RaycastHit[] forwardHits = Physics.RaycastAll(
            ray,
            maxDistance,
            hitMask,
            QueryTriggerInteraction.Ignore
        );

        // Backwards raycast
        Ray backRay = new Ray(ray.origin + ray.direction * maxAimDistance, -ray.direction);
        RaycastHit[] backwardHits = Physics.RaycastAll(
            backRay,
            maxDistance,
            hitMask,
            QueryTriggerInteraction.Ignore
        );

        RaycastHit[] hits = forwardHits.Concat(backwardHits).ToArray();

        if (hits.Length == 0)
        {
            Debug.Log("No hits");
            return false;
        }

        // Sort hits from closest to furthest.
        System.Array.Sort(
            hits,
            (a, b) => a.distance.CompareTo(b.distance)
        );

        // Look for a collider that we can enter AND exit.
        for (int i = 0; i < hits.Length - 1; i++)
        {
            HitSystem candidate = hits[i].collider.GetComponentInParent<HitSystem>();

            if (candidate == null)
                continue;

            // Look for the next hit belonging to the SAME collider.
            for (int j = i + 1; j < hits.Length; j++)
            {
                if (hits[j].collider != hits[i].collider)
                    continue;

                entryPoint = hits[i].point;
                exitPoint = hits[j].point;
                hitSystem = candidate;

                return true;
            }
        }

        return false;
    }
}
