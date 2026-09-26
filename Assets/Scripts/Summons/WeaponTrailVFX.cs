using UnityEngine;
using UnityEngine.VFX;

public class WeaponTrailVFX : MonoBehaviour
{
    [SerializeField] private VisualEffect vfx;
    [SerializeField] private Transform bladeBase;
    [SerializeField] private Transform bladeTip;

    private Vector3 previousBase;
    private Vector3 previousTip;

    private void OnEnable()
    {
        previousBase = bladeBase.localPosition;
        previousTip = bladeTip.localPosition;
    }

    private void LateUpdate()
    {
        Vector3 currentBase = bladeBase.localPosition;
        Vector3 currentTip = bladeTip.localPosition;

        vfx.SetVector3("previousBase", previousBase);
        vfx.SetVector3("previousTip", previousTip);

        vfx.SetVector3("currentBase", currentBase);
        vfx.SetVector3("currentTip", currentTip);

        previousBase = currentBase;
        previousTip = currentTip;
    }
}