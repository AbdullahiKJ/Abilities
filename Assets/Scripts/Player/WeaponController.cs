using DG.Tweening;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private Transform leftHandSocket;

    [SerializeField] private Transform rightHandSocket;
    private GameObject currentWeapon;
    private WeaponData currentWeaponData;

    public void BeginWeaponAnimation(WeaponData data)
    {
        currentWeaponData = data;

        Equip(data);

        // Start sand formation VFX

        // Get all mesh renderers on the object and set the materials on each renderer
        MeshRenderer[] rdList = currentWeapon.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer rd in rdList)
        {
            Material[] materials = rd.materials;

            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];

                // Set the initial alpha to 0 and start the fade in
                material.SetFloat("_alpha", 0f);

                material.DOFloat(
                    1f,
                    "_alpha",
                    data.formationDuration
                );
            }
        }
    }

    public void FinishWeaponAnimation()
    {
        // Start dissolve VFX

        // Get all mesh renderers on the object
        MeshRenderer[] rdList = currentWeapon.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer rd in rdList)
        {
            Material[] materials = rd.materials;

            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];

                // Start the alpha fade out
                material.DOFloat(
                    0f,
                    "_alpha",
                    currentWeaponData.formationDuration
                );
            }
        }
    }

    private void Equip(WeaponData data)
    {
        if (currentWeapon != null)
            Destroy(currentWeapon);

        currentWeapon = Instantiate(
            data.weaponPrefab,
            rightHandSocket
        );

        currentWeapon.transform.localPosition = Vector3.zero;
        currentWeapon.transform.localRotation = Quaternion.identity;
    }
}