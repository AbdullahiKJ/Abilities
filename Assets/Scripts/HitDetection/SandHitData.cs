using Unity.Mathematics;
using UnityEngine;

public readonly struct SandHitData
{
    public readonly Vector3 Entry;
    public readonly Vector3 Exit;
    public readonly Vector3 Direction;

    public readonly float Radius;
    public readonly float ForwardImpulse;
    public readonly float RadialImpulse;
    public readonly float RandomImpulse;

    public readonly float MinRecoveryTime;
    public readonly float MaxRecoveryTime;

    // Job variables
    public readonly Vector3 Segment;
    public readonly float SegmentLengthSq;
    public readonly float RadiusSq;


    public SandHitData(
        Vector3 entry,
        Vector3 exit,
        Vector3 direction,
        float radius,
        float forwardImpulse,
        float radialImpulse,
        float randomImpulse,
        float minRecoveryTime,
        float maxRecoveryTime)
    {
        Entry = entry;
        Exit = exit;
        Direction = direction.normalized;

        Radius = radius;
        ForwardImpulse = forwardImpulse;
        RadialImpulse = radialImpulse;
        RandomImpulse = randomImpulse;

        MinRecoveryTime = minRecoveryTime;
        MaxRecoveryTime = maxRecoveryTime;

        Segment = exit - entry;
        SegmentLengthSq = math.lengthsq(exit - entry);
        RadiusSq = radius * radius;
    }
}

public struct SandWeaponHitData
{
    public float3 previousBase;
    public float3 previousTip;
    public float3 currentBase;
    public float3 currentTip;

    public float radius;
    public float forwardImpactMultiplier;
    public float radialImpactMultiplier;

    public float minRecoveryTime;
    public float maxRecoveryTime;

    // Time between the previous and current blade samples.
    public float deltaTime;
}