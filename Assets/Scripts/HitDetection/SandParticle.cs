using Unity.Mathematics;

public enum SandParticleState : byte
{
    Attached = 0,
    Displaced = 1,
    Recovering = 2
}

public struct SandParticle
{
    public float3 position;
    public float3 velocity;
    public float recoveryTimer;
    public SandParticleState state;
    public float randomValue;
}

public struct SandSurfacePoint
{
    public int triangleIndex;
    public float3 barycentric;
}
