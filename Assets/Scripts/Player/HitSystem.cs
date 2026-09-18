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
    private Tween[] holeTweens = new Tween[4];
    private int[] entryIDs = new int[4];
    private int[] exitIDs = new int[4];
    private int[] radiusIDs = new int[4];

    void Awake()
    {
        rd = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
        vfx = GetComponent<VFXController>();

        for (int i = 0; i < maxHole; i++)
        {
            entryIDs[i] = Shader.PropertyToID($"_EntryPosition{i}");
            exitIDs[i] = Shader.PropertyToID($"_ExitPosition{i}");
            radiusIDs[i] = Shader.PropertyToID($"_Radius{i}");
        }
    }

    public void OnExit(Vector3 entryPoint, Vector3 exitPoint, float bulletRadius)
    {
        int index = holeCount;

        // Kill whatever was previously controlling this slot.
        holeTweens[index]?.Kill();

        rd.GetPropertyBlock(block);

        float radius = bulletRadius * Random.Range(radiusMultiplier.x, radiusMultiplier.y);

        block.SetVector(entryIDs[index], entryPoint);
        block.SetVector(exitIDs[index], exitPoint);
        block.SetFloat(radiusIDs[index], radius);
        rd.SetPropertyBlock(block);

        // Instantiate the vfx prefab and assign graph properties
        vfx.CreateHitVFX(lifetime, radius, entryPoint, exitPoint);

        // Interpolate the radius size
        holeTweens[index] = DOTween.To(
            () => radius,
            x =>
            {
                radius = x;
                rd.GetPropertyBlock(block);
                block.SetFloat(radiusIDs[index], radius);
                rd.SetPropertyBlock(block);
            },
            0f,
            lifetime * (1 - delayMultiplier)
        ).SetDelay(lifetime * delayMultiplier);

        // Update the hole count
        holeCount = (holeCount + 1) % maxHole;
    }
}
