using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct SandRendererMatrixJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<SandParticle> particles;
    [ReadOnly] public NativeArray<float> particleSizes;

    [WriteOnly] public NativeArray<Matrix4x4> matrices;

    public void Execute(int index)
    {
        float3 position = particles[index].position;
        float size = particleSizes[index];

        // Identity rotation and uniform scale.
        matrices[index] = new Matrix4x4(
            new Vector4(size, 0f, 0f, 0f),
            new Vector4(0f, size, 0f, 0f),
            new Vector4(0f, 0f, size, 0f),
            new Vector4(position.x, position.y, position.z, 1f)
        );
    }
}