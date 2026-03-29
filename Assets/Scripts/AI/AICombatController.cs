using UnityEngine;

public class AICombatController : MonoBehaviour
{
    private AnimationController anim;
    private Camera cam;

    [Header("Shoot settings")]
    [SerializeField] float fireRate;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float maxAimDistance = 50f;
    [SerializeField] Transform gunTip;
    private float lastFireTime;
    Vector3 screenCenter;
    float aimInput;

    void Awake()
    {
        anim = GetComponent<AnimationController>();
        cam = Camera.main;
    }

    void TryFire()
    {
        if (Time.time < lastFireTime + fireRate || aimInput < 1f)
            return;
        else
        {
            // Shooting logic
            lastFireTime = Time.time;
            anim.PlayShootAnim();

            // Fire the projectile
            Ray aimRay = cam.ScreenPointToRay(screenCenter);
            Vector3 targetPoint;

            if (Physics.Raycast(aimRay, out RaycastHit hit, maxAimDistance))
                targetPoint = hit.point;
            else
                targetPoint = aimRay.origin + aimRay.direction * maxAimDistance;

            Vector3 fireDirection = (targetPoint - gunTip.position).normalized;
            GameObject bullet = Instantiate(bulletPrefab, gunTip.position, Quaternion.LookRotation(fireDirection));
            bullet.GetComponent<Bullet>().Fire(fireDirection);
        }
    }
}
