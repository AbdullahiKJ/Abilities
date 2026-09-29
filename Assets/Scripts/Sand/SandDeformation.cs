using UnityEngine;

public enum SandDeformationType
{
    Depression,
    Raise
}

public class SandDeformation
{
    public SandDeformationType type;

    public Vector3 worldPosition;

    public float radius;
    public float amount;

    public float duration;
    public float elapsed;

    public float PreviousProgress { get; private set; }

    public bool IsFinished => elapsed >= duration;

    public SandDeformation(
        SandDeformationType type,
        Vector3 worldPosition,
        float radius,
        float amount,
        float duration)
    {
        this.type = type;
        this.worldPosition = worldPosition;
        this.radius = radius;
        this.amount = amount;
        this.duration = duration;

        elapsed = 0f;
        PreviousProgress = 0f;
    }

    public float Advance(float deltaTime)
    {
        elapsed += deltaTime;

        float normalizedTime = Mathf.Clamp01(elapsed / duration);

        // SmoothStep without using Mathf.SmoothStep so it's
        // obvious what curve we're applying.
        float currentProgress =
            normalizedTime * normalizedTime *
            (3f - 2f * normalizedTime);

        float deltaProgress = currentProgress - PreviousProgress;

        PreviousProgress = currentProgress;

        return deltaProgress;
    }
}