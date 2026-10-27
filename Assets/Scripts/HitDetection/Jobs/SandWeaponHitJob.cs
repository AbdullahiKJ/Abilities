using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct SandWeaponHitJob : IJobParallelFor
{
    public NativeArray<SandParticle> particles;

    public SandWeaponHitData hit;

    public void Execute(int index)
    {
        if (hit.radius <= 0f || hit.deltaTime <= 0f)
            return;

        SandParticle particle = particles[index];

        const int sampleCount = 5;

        float closestDistanceSq = float.MaxValue;

        float3 bestMovement = float3.zero;
        float3 bestOffset = float3.zero;

        for (int i = 0; i < sampleCount; i++)
        {
            float bladeT = i / (float)(sampleCount - 1);

            float3 previous =
                math.lerp(
                    hit.previousBase,
                    hit.previousTip,
                    bladeT
                );

            float3 current =
                math.lerp(
                    hit.currentBase,
                    hit.currentTip,
                    bladeT
                );

            float3 movement = current - previous;
            float movementLengthSq = math.lengthsq(movement);

            if (movementLengthSq < 0.0001f)
                continue;

            float t =
                math.dot(
                    particle.position - previous,
                    movement
                ) / movementLengthSq;

            t = math.saturate(t);

            float3 closestPoint = previous + movement * t;

            float3 offset = particle.position - closestPoint;

            float distanceSq = math.lengthsq(offset);

            if (distanceSq < closestDistanceSq)
            {
                closestDistanceSq = distanceSq;

                bestMovement = movement;
                bestOffset = offset;
            }
        }

        // Particle wasn't inside any blade sweep.
        if (closestDistanceSq > hit.radius * hit.radius)
            return;

        // How close was the particle to the centre of the weapon's path
        float distance = math.sqrt(closestDistanceSq);

        float normalizedDistance = math.saturate(distance / hit.radius);

        float strength = 1f - normalizedDistance;
        strength *= strength;

        // Velocity of the closest part of the blade.
        float speed =
            math.length(bestMovement)
            / math.max(hit.deltaTime, 0.0001f);

        float3 weaponDirection = math.normalizesafe(bestMovement);

        float3 radialDirection = math.normalizesafe(bestOffset);

        float forwardImpulse = speed * hit.forwardImpactMultiplier;

        float radialImpulse = speed * hit.radialImpactMultiplier;

        float3 impulse =
              weaponDirection
                * forwardImpulse
                * strength

            + radialDirection
                * radialImpulse
                * strength;

        particle.velocity += impulse;

        particle.state = SandParticleState.Displaced;
        particle.recoveryTimer = math.lerp(
            0.25f,
            0.65f,
            particle.randomValue
        );

        particles[index] = particle;
    }
}