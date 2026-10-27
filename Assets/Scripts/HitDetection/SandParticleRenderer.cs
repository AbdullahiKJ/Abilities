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

    private int BatchSize = 511;

    private void Awake()
    {
        // Enable instancing on the particle material
        particleMaterial.enableInstancing = true;
    }

    public void Render(NativeArray<Matrix4x4> matrices)
    {
        var renderParams = new RenderParams(particleMaterial);

        for (int start = 0; start < matrices.Length; start += BatchSize)
        {
            int count = Mathf.Min(BatchSize, matrices.Length - start);

            Graphics.RenderMeshInstanced(
                renderParams,
                particleMesh,
                0,
                matrices,
                instanceCount: count,
                startInstance: start
            );
        }
    }
}