using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Attack")]
public class AttackData : ScriptableObject
{
    [Header("Animation")]
    public string animationName;
    public float crossFadeDuration = 0.1f;

    [Header("Combo Window (normalized time 0-1)")]
    public float comboWindowOpen = 0.6f;
    public float comboWindowClose = 0.9f;

    [Header("Cancel")]
    public bool canBeInterrupted;

    [Header("Combo Branches")]
    public AttackLink[] nextAttacks;
}

[System.Serializable]
public struct AttackLink
{
    public InputType inputType;
    public AttackData nextAttack;
}

public enum InputType
{
    Primary,
    Secondary
}