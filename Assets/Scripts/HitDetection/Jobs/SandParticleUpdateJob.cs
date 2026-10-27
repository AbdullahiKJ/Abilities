using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct SandParticleUpdateJob : IJobParallelFor
{
    public NativeArray<SandParticle> particles;

    [ReadOnly] public NativeArray<float3> meshTargets;
    public NativeArray<float3> previousMeshTargets;

    public float deltaTime;
    public float springStrength;
    public float damping;
    public float displacedDragFactor;
    public float recoveryDistance;

    public void Execute(int index)
    {
        SandParticle particle = particles[index];
        float3 target = meshTargets[index];

        float3 targetDelta = target - previousMeshTargets[index];
        previousMeshTargets[index] = target;

        switch (particle.state)
        {
            case SandParticleState.Attached:
                {
                    particle.position = target;
                    particle.velocity = float3.zero;
                    break;
                }

            case SandParticleState.Displaced:
                {
                    particle.position += particle.velocity * deltaTime;

                    particle.velocity *= displacedDragFactor;

                    particle.recoveryTimer -= deltaTime;

                    if (particle.recoveryTimer <= 0f)
                        particle.state = SandParticleState.Recovering;

                    break;
                }

            case SandParticleState.Recovering:
                {
                    // Follow the animated target's movement directly.
                    particle.position += targetDelta;

                    // Recover the remaining displacement relative to that target.
                    float3 offset = target - particle.position;

                    float3 acceleration =
                        offset * springStrength -
                        particle.velocity * damping;

                    particle.velocity += acceleration * deltaTime;
                    particle.position += particle.velocity * deltaTime;

                    if (math.distancesq(particle.position, target) < recoveryDistance * recoveryDistance)
                    {
                        particle.position = target;
                        particle.velocity = float3.zero;
                        particle.state = SandParticleState.Attached;
                    }

                    break;
                }
        }

        particles[index] = particle;
    }
}