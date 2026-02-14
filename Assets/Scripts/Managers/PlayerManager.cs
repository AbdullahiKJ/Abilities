using Unity.Cinemachine;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject[] characterArray;
    public int currentChar = 0;
    public CinemachineCamera cam;
    public string camTargetName = "CameraTarget";

    void Start()
    {
        // Update the tracking and look at target of the cinemachine camera
        Transform target = characterArray[currentChar].transform.Find(camTargetName);
        cam.Follow = target;
        cam.LookAt = target;
    }
}
