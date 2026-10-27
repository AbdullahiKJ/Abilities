using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct SandHitJob : IJobParallelFor
{
    public NativeArray<SandParticle> particles;
    [ReadOnly] public SandHitData hit;
    public uint randomSeed;

    public void Execute(int index)
    {
        SandParticle particle = particles[index];

        if (hit.SegmentLengthSq < 0.0001f)
            return;

        float t =
            math.dot(
                particle.position - (float3)hit.Entry,
                hit.Segment
            )
            / hit.SegmentLengthSq;

        t = math.saturate(t);

        float3 closestPoint = (float3)hit.Entry + (float3)hit.Segment * t;

        float3 offset = particle.position - closestPoint;

        float distanceSq = math.lengthsq(offset);

        if (distanceSq > hit.Radius * hit.Radius)
            return;

        float distance = math.sqrt(distanceSq);

        float normalizedDistance = math.saturate(distance / hit.Radius);

        float strength = 1f - normalizedDistance;
        // Smooth the strength curve to make it more natural
        strength *= strength;

        float3 projectileDirection = hit.Direction;
        float3 radialDirection = math.normalizesafe(offset);

        // Random movement
        float groupSize = 0.25f;

        // Get the cell that the particle is in based on its position and the group size
        int3 cell =
            (int3)math.floor(
                particle.position / groupSize
            );

        uint groupSeed = math.hash(
            new int4(
                cell.x,
                cell.y,
                cell.z,
                (int)randomSeed
            )
        );

        // Calculate the random group direction
        var groupRandom =
            new Random(
                math.max(1u, groupSeed)
            );
        float3 groupDirection = groupRandom.NextFloat3Direction();

        // Calculate the random individual direction for the particle
        var particleRandom =
            new Random(
                math.max(
                    1u,
                    math.hash(
                        new uint2(
                            randomSeed,
                            (uint)index + 1
                        )
                    )
                )
            );
        float3 individualDirection = particleRandom.NextFloat3Direction();

        // Combine the group and individual directions to get a final random direction for the particle
        float3 randomDirection =
            math.normalizesafe(
                projectileDirection
                + groupDirection * 0.5f
                + individualDirection * 0.15f
            );
        float randomStrength = groupRandom.NextFloat(0.25f, 1f);

        // Combine all motion
        float3 impulse =
            projectileDirection
                * hit.ForwardImpulse
                * strength

            + radialDirection
                * hit.RadialImpulse
                * 0.5f
                * strength

            + randomDirection
                * hit.RandomImpulse
                * randomStrength
                * strength;

        particle.velocity += impulse;

        particle.state = SandParticleState.Displaced;

        float randomValue = groupRandom.NextFloat();
        particle.recoveryTimer = math.lerp(
            hit.MinRecoveryTime,
            hit.MaxRecoveryTime,
            randomValue
        );

        particles[index] = particle;
    }
}