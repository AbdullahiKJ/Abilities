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

    [Header("Weapon Data")]
    public WeaponData weapon;

    [Header("Combo Branches")]
    public AttackLink[] nextAttacks;

    [Header("Terrain Deformation")]
    public bool createDepression;
    public float depth;
    public float radius;
    public float duration;
    public bool createOnEnd;

    [Header("VFX")]
    public bool playFormationVFX;
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