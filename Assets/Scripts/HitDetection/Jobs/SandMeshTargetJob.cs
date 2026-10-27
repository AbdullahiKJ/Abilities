using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct SandMeshTargetJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> vertices;
    [ReadOnly] public NativeArray<int> triangles;
    [ReadOnly] public NativeArray<SandSurfacePoint> surfacePoints;

    [WriteOnly] public NativeArray<float3> meshTargets;

    public float4x4 localToWorld;

    public void Execute(int index)
    {
        SandSurfacePoint point = surfacePoints[index];

        int triangleStart = point.triangleIndex * 3;

        int i0 = triangles[triangleStart];
        int i1 = triangles[triangleStart + 1];
        int i2 = triangles[triangleStart + 2];

        float3 a = vertices[i0];
        float3 b = vertices[i1];
        float3 c = vertices[i2];

        float3 localPosition =
            a * point.barycentric.x +
            b * point.barycentric.y +
            c * point.barycentric.z;

        meshTargets[index] = math.transform(localToWorld, localPosition);
    }
}