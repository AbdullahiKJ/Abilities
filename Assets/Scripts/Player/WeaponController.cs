using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.VFX;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private Transform leftHandSocket;

    [SerializeField] private Transform rightHandSocket;
    private List<GameObject> currentWeapons = new List<GameObject>();
    private WeaponData currentWeaponData;
    [Header("Weapon Prefabs")]
    [SerializeField] GameObject swordLeftPrefab;
    [SerializeField] GameObject swordRightPrefab;
    [SerializeField] GameObject longswordPrefab;
    [SerializeField] GameObject shieldPrefab;
    [SerializeField] GameObject spearPrefab;
    private GameObject swordLeftInstance;
    private GameObject swordRightInstance;
    private GameObject longswordInstance;
    private GameObject shieldInstance;
    private GameObject spearInstance;

    // Tweem dictionary
    private readonly Dictionary<GameObject, Tween> weaponTweens = new();

    void Awake()
    {
        // Instantiate all weapon prefabs
        swordLeftInstance = Instantiate(swordLeftPrefab, leftHandSocket);
        swordRightInstance = Instantiate(swordRightPrefab, rightHandSocket);
        longswordInstance = Instantiate(longswordPrefab, rightHandSocket);
        shieldInstance = Instantiate(shieldPrefab, leftHandSocket);
        spearInstance = Instantiate(spearPrefab, rightHandSocket);

        // Set all weapon instances to inactive
        SetWeaponStates(false);
    }

    public void BeginWeaponAnimation(WeaponData data)
    {
        // Kill all tweens before starting a new attack
        foreach (GameObject weapon in currentWeapons)
        {
            KillWeaponTweens(weapon);
        }

        currentWeaponData = data;

        // Set the active weapon instance
        Equip(data);

        // Start sand formation VFX
        StartVFX(currentWeaponData, true);

        // Get all mesh renderers on the weapons and set the materials on each renderer
        foreach (GameObject weapon in currentWeapons)
        {
            // Cancel previous animation for this weapon
            KillWeaponTweens(weapon);

            Sequence sequence = DOTween.Sequence();

            MeshRenderer[] rdList = weapon.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer rd in rdList)
            {
                Material[] materials = rd.materials;

                for (int i = 0; i < materials.Length; i++)
                {
                    Material material = materials[i];

                    // Set the initial alpha to 0 and start the fade in
                    material.SetFloat("_alpha", 0f);

                    sequence.Join(
                        material.DOFloat(
                            1f,
                            "_alpha",
                            data.formationDuration
                        )
                    );
                }
            }

            // Add the sequence to the weapon dictionary
            weaponTweens[weapon] = sequence;

            sequence.OnComplete(() =>
            {
                // Only remove this particular tween.
                if (weaponTweens.TryGetValue(weapon, out Tween currentTween) &&
                    currentTween == sequence)
                {
                    weaponTweens.Remove(weapon);
                }
            });
        }
    }

    public void FinishWeaponAnimation()
    {
        // Start dissolve VFX
        StartVFX(currentWeaponData, false);

        // Get all mesh renderers on the weapons
        foreach (GameObject weapon in currentWeapons)
        {
            // Cancel previous animation for this weapon
            KillWeaponTweens(weapon);

            Sequence sequence = DOTween.Sequence();

            MeshRenderer[] rdList = weapon.GetComponentsInChildren<MeshRenderer>();

            foreach (MeshRenderer rd in rdList)
            {
                Material[] materials = rd.materials;

                for (int i = 0; i < materials.Length; i++)
                {
                    Material material = materials[i];

                    // Start the alpha fade out then disable the weapon
                    // Add this tween to the weapon sequence
                    sequence.Join(
                        material.DOFloat(
                        0f,
                        "_alpha",
                        currentWeaponData.formationDuration
                        )
                    );
                }
            }

            // Add the tween to the dictionary
            weaponTweens[weapon] = sequence;

            // Once the sequence is completed, disable the weapon and remove this specific tween from the dictionary
            sequence.OnComplete(() =>
            {
                if (weaponTweens.TryGetValue(weapon, out Tween currentTween) &&
                    currentTween == sequence)
                {
                    weaponTweens.Remove(weapon);
                    weapon.SetActive(false);
                }
            });
        }
    }

    void KillWeaponTweens(GameObject weapon)
    {
        if (weaponTweens.TryGetValue(weapon, out Tween existingTween))
        {
            existingTween.Kill();
        }
    }

    private void Equip(WeaponData data)
    {
        // Clear the current weapons list
        currentWeapons.Clear();

        // Set all weapons to inactive
        SetWeaponStates(false);

        // Update the active weapons and assign them to the current weapons list
        switch (data.weaponName)
        {
            case "Sword":
                swordRightInstance.SetActive(true);
                currentWeapons.Add(swordRightInstance);
                break;
            case "DualSwords":
                swordLeftInstance.SetActive(true);
                swordRightInstance.SetActive(true);
                currentWeapons.Add(swordLeftInstance);
                currentWeapons.Add(swordRightInstance);
                break;
            case "Longsword":
                longswordInstance.SetActive(true);
                currentWeapons.Add(longswordInstance);
                break;
            case "Shield":
                shieldInstance.SetActive(true);
                currentWeapons.Add(shieldInstance);
                break;
            case "Spear":
                spearInstance.SetActive(true);
                currentWeapons.Add(spearInstance);
                break;
        }
    }

    void SetWeaponStates(bool active)
    {
        swordLeftInstance.SetActive(active);
        swordRightInstance.SetActive(active);
        longswordInstance.SetActive(active);
        shieldInstance.SetActive(active);
        spearInstance.SetActive(active);
    }

    List<Transform> GetWeaponPos(WeaponData data)
    {
        List<Transform> weaponTrans = new List<Transform>();
        switch (data.name)
        {
            case "Sword":
                weaponTrans.Add(rightHandSocket);
                break;
            case "DualSwords":
                weaponTrans.Add(leftHandSocket);
                weaponTrans.Add(rightHandSocket);
                break;
            case "Longsword":
                weaponTrans.Add(rightHandSocket);
                break;
            case "Shield":
                weaponTrans.Add(leftHandSocket);
                break;
            case "Spear":
                weaponTrans.Add(rightHandSocket);
                break;
        }
        return weaponTrans;
    }

    void StartVFX(WeaponData data, bool isForming)
    {
        // todo: remove once implemented
        return;

        List<Transform> weaponTrans = GetWeaponPos(currentWeaponData);
        // Create vfx at each transform
        foreach (Transform transform in weaponTrans)
        {
            GameObject vfxPrefab = isForming
                ? data.formationVFXPrefab
                : data.dissolveVFXPrefab;

            GameObject instance = Instantiate(vfxPrefab, transform);
            VisualEffect vfx = instance.GetComponent<VisualEffect>();

            // todo: assign necessary information such as meshes and transforms
        }
    }
}