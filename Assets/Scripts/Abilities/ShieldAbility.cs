using UnityEngine;

public class ShieldAbility : MonoBehaviour, IPlayerAbility
{
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private float wallOffset = 2f;
    GameObject wallInstance;
    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }
    public bool CanActivate()
    {
        if (wallInstance == null)
            return true;
        else
            return false;
    }

    public void Activate()
    {
        // Place the wall instance
        Vector3 camForward = cam.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();
        Vector3 spawnPos = transform.position + camForward * wallOffset;
        wallInstance = Instantiate(wallPrefab, spawnPos, Quaternion.LookRotation(camForward));

        // todo: play an animation
    }
}
