using UnityEngine;
using UnityEngine.VFX;

[CreateAssetMenu(menuName = "Combat/Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;

    [Header("Visual")]
    public GameObject weaponPrefab;

    [Header("VFX")]
    public GameObject formationVFXPrefab;
    public GameObject dissolveVFXPrefab;

    [Header("Timing")]
    public float formationDuration = 0.2f;
    public float dissolveDuration = 0.15f;
}