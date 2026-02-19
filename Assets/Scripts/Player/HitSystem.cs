using DG.Tweening;
using UnityEngine;

public class HitSystem : MonoBehaviour
{
    public Vector2 radiusMultiplier = new Vector2(1.1f, 1.5f);
    public float lifetime = 5f;
    public float delayMultiplier = 0.2f;
    private int holeCount = 0;
    private int maxHole = 4;
    private Renderer rd;
    private MaterialPropertyBlock block;
    private VFXController vfx;

    void Start()
    {
        rd = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
        vfx = GetComponent<VFXController>();
    }

    public void OnEnter(Bullet bullet)
    {
        // todo: check if this passes through or hits
    }

    public void OnExit(Vector3 entryPoint, Vector3 exitPoint, float bulletRadius)
    {
        rd.GetPropertyBlock(block);

        float radius = bulletRadius * Random.Range(radiusMultiplier.x, radiusMultiplier.y);

        block.SetVector($"_EntryPosition{holeCount}", entryPoint);
        block.SetVector($"_ExitPosition{holeCount}", exitPoint);
        block.SetFloat($"_Radius{holeCount}", radius);
        rd.SetPropertyBlock(block);

        // Instantiate the vfx prefab and assign graph properties
        vfx.CreateHitVFX(lifetime, radius, entryPoint, exitPoint);

        // Interpolate the radius size
        int index = holeCount;
        DOTween.To(
            () => radius,
            x =>
            {
                radius = x;
                rd.GetPropertyBlock(block);
                block.SetFloat($"_Radius{index}", radius);
                rd.SetPropertyBlock(block);
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
