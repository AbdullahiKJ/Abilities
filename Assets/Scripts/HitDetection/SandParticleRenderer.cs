using Unity.Collections;
using UnityEngine;

public class SandParticleRenderer : MonoBehaviour
{
    [SerializeField] Material particleMaterial;
    [SerializeField] Mesh particleMesh;
    [SerializeField] private float minParticleSize = 0.005f;
    [SerializeField] private float maxParticleSize = 0.015f;
    public float MinParticleSize => minParticleSize;
    public float MaxParticleSize => maxParticleSize;
    private NativeArray<Matrix4x4> visibleMatrices;

    private int BatchSize = 511;

    private void Awake()
    {
        // Enable instancing on the particle material
        particleMaterial.enableInstancing = true;
    }

    public void Render(NativeArray<Matrix4x4> matrices, NativeArray<SandParticle> particles)
    {
        // Reset the visible matrices if it does not exist or does not match the particles length
        if (!visibleMatrices.IsCreated || visibleMatrices.Length != particles.Length)
        {
            if (visibleMatrices.IsCreated)
                visibleMatrices.Dispose();

            visibleMatrices = new NativeArray<Matrix4x4>(
                particles.Length,
                Allocator.Persistent
            );
        }

        int visibleCount = 0;

        for (int i = 0; i < particles.Length; i++)
        {
            if (particles[i].state == SandParticleState.Attached)
                continue;

            visibleMatrices[visibleCount++] = matrices[i];
        }

        var renderParams = new RenderParams(particleMaterial);

        for (int start = 0; start < visibleCount; start += BatchSize)
        {
            int count = Mathf.Min(BatchSize, visibleCount - start);

            Graphics.RenderMeshInstanced(
                renderParams,
                particleMesh,
                0,
                visibleMatrices,
                instanceCount: count,
                startInstance: start
            );
        }
    }

    private void OnDestroy()
    {
        if (visibleMatrices.IsCreated)
            visibleMatrices.Dispose();
    }
}