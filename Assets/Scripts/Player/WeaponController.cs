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

        // Assign the weapon material
        MeshRenderer rd = currentWeapon.GetComponent<MeshRenderer>();
        rd.material = data.sandMaterial;

        // Set the alpha to 0 and start the alpha fade in
        rd.material.SetFloat("_alpha", 0);
        DOTween.To(
            () => rd.material.GetFloat("_alpha"),
            x => rd.material.SetFloat("_alpha", x),
            1,
            data.formationDuration
        );
    }

    public void FinishWeaponAnimation()
    {
        // Start dissolve VFX

        // Get the mesh renderer
        MeshRenderer rd = currentWeapon.GetComponent<MeshRenderer>();

        // Start the alpha fade out
        DOTween.To(
            () => rd.material.GetFloat("_alpha"),
            x => rd.material.SetFloat("_alpha", x),
            0,
            currentWeaponData.formationDuration
        );
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