using DG.Tweening;
using UnityEngine;
using UnityEngine.VFX;

public class HitSystem : MonoBehaviour
{
    private InputReader input;
    private Camera cam;
    public Vector2 bulletRadiuses = new Vector2(0.03f, 0.1f);
    public float maxRayDistance = 50f;
    public GameObject vfxPrefab;
    public float lifetime = 5f;
    public float delayMultiplier = 0.2f;
    private Vector3 pos;
    private int layerMask;
    private int holeCount = 0;
    private int maxHole = 4;
    private MaterialPropertyBlock block;

    void Awake()
    {
        input = GetComponent<InputReader>();
        cam = Camera.main;
        pos = new Vector3(cam.scaledPixelWidth / 2, cam.scaledPixelHeight / 2, 0f);
        layerMask = LayerMask.GetMask("Sand");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        block = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        input.ShootPressed += OnShoot;
    }

    private void OnDisable()
    {
        input.ShootPressed -= OnShoot;
    }

    private void OnShoot()
    {
        Ray ray = cam.ScreenPointToRay(pos);
        RaycastHit hitEntry;
        RaycastHit hitExit;

        bool hitEntryExists = Physics.Raycast(ray, out hitEntry, maxRayDistance, layerMask);

        if (hitEntryExists)
        {
            Vector3 exitRayStart = hitEntry.point + ray.direction * 10f;
            bool hitExitExists = Physics.Raycast(exitRayStart, ray.direction * -1f, out hitExit, maxRayDistance, layerMask);

            if (hitExitExists)
            {
                Renderer renderer = hitEntry.transform.gameObject.GetComponent<Renderer>();
                renderer.GetPropertyBlock(block);

                float radius = Random.Range(bulletRadiuses.x, bulletRadiuses.y);
                block.SetVector($"_EntryPosition{holeCount}", hitEntry.point);
                block.SetVector($"_ExitPosition{holeCount}", hitExit.point);
                block.SetFloat($"_Radius{holeCount}", radius);
                renderer.SetPropertyBlock(block);

                // Instantiate the vfx prefab and assign graph properties
                GameObject prefab = Instantiate(vfxPrefab, Vector3.zero, Quaternion.identity);
                Destroy(prefab, lifetime);
                VisualEffect vfx = prefab.GetComponent<VisualEffect>();

                vfx.SetFloat("lifetime", lifetime);
                vfx.SetFloat("radius", radius);
                vfx.SetVector3("capsuleStart", hitEntry.point);
                vfx.SetVector3("capsuleEnd", hitExit.point);

                // Interpolate the radius size
                int index = holeCount;
                DOTween.To(
                    () => radius,
                    x =>
                    {
                        radius = x;
                        renderer.GetPropertyBlock(block);
                        block.SetFloat($"_Radius{index}", radius);
                        renderer.SetPropertyBlock(block);
                    },
                    0f,
                    lifetime * (1 - delayMultiplier)
                ).SetDelay(lifetime * delayMultiplier);

                // Update the hole count
                holeCount++;
                if (holeCount == maxHole)
                    holeCount = 0;
            }
        }
    }
}
