using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class SandSurfaceMask : MonoBehaviour
{
    [Header("Mask Settings")]
    [SerializeField, Range(32, 1024)] private int resolution = 256;
    [SerializeField, Range(0, 7)] private int maskUVChannel = 2;
    [SerializeField, Min(1)] private int stampRadiusPixels = 3;
    [SerializeField] private string maskProperty = "_DisplacementMask";

    private Texture2D maskTexture;
    private Color32[] pixels; // Stores the current mask texture's pixel colours
    private Vector2Int[] particlePixels; // Stores each particle's original surface location on the texture.
    private Vector2Int[] stampOffsets; // Cached circular footprint.

    private SkinnedMeshRenderer targetRenderer;
    private MaterialPropertyBlock propertyBlock;
    private int maskPropertyID;
    private bool initialized;

    public void Initialize(
        SkinnedMeshRenderer renderer,
        NativeArray<int> triangles,
        NativeArray<SandSurfacePoint> surfacePoints)
    {
        if (initialized)
            throw new System.InvalidOperationException("SandSurfaceMask has already been initialized.");

        Mesh mesh = renderer.sharedMesh;

        var uvs = new List<Vector2>(mesh.vertexCount);
        mesh.GetUVs(maskUVChannel, uvs);

        if (uvs.Count != mesh.vertexCount)
        {
            throw new System.InvalidOperationException(
                $"Mesh needs valid UVs in channel {maskUVChannel}.");
        }

        // This implementation uses a single, non-repeating UV atlas.
        for (int i = 0; i < uvs.Count; i++)
        {
            Vector2 uv = uvs[i];

            if (!float.IsFinite(uv.x) || !float.IsFinite(uv.y) ||
                uv.x < 0f || uv.x > 1f ||
                uv.y < 0f || uv.y > 1f)
            {
                throw new System.InvalidOperationException(
                    "Mask UVs must be finite and inside the 0-1 range.");
            }
        }

        targetRenderer = renderer;
        particlePixels = new Vector2Int[surfacePoints.Length];

        // Step 2: map stable triangle samples to stable UV positions.
        for (int i = 0; i < surfacePoints.Length; i++)
        {
            SandSurfacePoint point = surfacePoints[i];
            int triangleStart = point.triangleIndex * 3;

            Vector2 a = uvs[triangles[triangleStart]];
            Vector2 b = uvs[triangles[triangleStart + 1]];
            Vector2 c = uvs[triangles[triangleStart + 2]];

            float3 barycentric = point.barycentric;

            Vector2 uv =
                a * barycentric.x +
                b * barycentric.y +
                c * barycentric.z;

            particlePixels[i] = new Vector2Int(
                Mathf.Clamp(
                    Mathf.FloorToInt(uv.x * resolution),
                    0, resolution - 1),
                Mathf.Clamp(
                    Mathf.FloorToInt(uv.y * resolution),
                    0, resolution - 1)
            );
        }

        BuildStampOffsets();

        pixels = new Color32[resolution * resolution];

        maskTexture = new Texture2D(
            resolution,
            resolution,
            TextureFormat.RGBA32,
            mipChain: false,
            linear: true
        )
        {
            name = "Sand Displacement Mask",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        maskTexture.SetPixels32(pixels);
        maskTexture.Apply(false, false);

        propertyBlock = new MaterialPropertyBlock();
        maskPropertyID = Shader.PropertyToID(maskProperty);

        // Preserve other renderer-level material properties.
        targetRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetTexture(maskPropertyID, maskTexture);
        targetRenderer.SetPropertyBlock(propertyBlock);

        initialized = true;
    }

    private void BuildStampOffsets()
    {
        var offsets = new List<Vector2Int>();

        int radius = stampRadiusPixels;
        int radiusSq = radius * radius;

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= radiusSq)
                    offsets.Add(new Vector2Int(x, y));
            }
        }

        stampOffsets = offsets.ToArray();
    }

    public void UpdateMask(NativeArray<SandParticle> particles)
    {
        if (!initialized)
            return;

        if (particles.Length != particlePixels.Length)
        {
            throw new System.ArgumentException(
                "Particle count changed after mask initialization.");
        }

        // Step 3: rebuild from current states.
        // Black = intact surface, white = hidden surface.
        System.Array.Clear(pixels, 0, pixels.Length);

        Color32 hidden = new Color32(255, 255, 255, 255);

        for (int i = 0; i < particles.Length; i++)
        {
            if (particles[i].state == SandParticleState.Attached)
                continue;

            Vector2Int centre = particlePixels[i];

            foreach (Vector2Int offset in stampOffsets)
            {
                int x = centre.x + offset.x;
                int y = centre.y + offset.y;

                if (x < 0 || x >= resolution ||
                    y < 0 || y >= resolution)
                    continue;

                pixels[y * resolution + x] = hidden;
            }
        }

        maskTexture.SetPixels32(pixels);

        // Keep the CPU copy because it changes every frame.
        maskTexture.Apply(false, false);
    }

    private void OnDestroy()
    {
        if (targetRenderer != null && propertyBlock != null)
        {
            targetRenderer.GetPropertyBlock(propertyBlock);
            // Black mask means the entire character surface is visible.
            propertyBlock.SetTexture(
                maskPropertyID,
                Texture2D.blackTexture
            );
            targetRenderer.SetPropertyBlock(propertyBlock);
        }

        if (maskTexture != null)
            Destroy(maskTexture);
    }
}