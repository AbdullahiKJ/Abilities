using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    [SerializeField] string keyboardTag = "Keyboard";
    [SerializeField] string controllerTag = "Controller";
    [SerializeField] PlayerInput input;
    [SerializeField] string controlerScheme = "Controller";

    void Start()
    {
        // Check if the controller scheme is active and set the corresponding UI elements
        List<GameObject> taggedChildren = new List<GameObject>();
        GetChildrenWithTag(gameObject, input.currentControlScheme == controlerScheme ? keyboardTag : controllerTag, taggedChildren);
        foreach (GameObject button in taggedChildren)
        {
            button.SetActive(false);
        }
    }

    void GetChildrenWithTag(GameObject parent, string tag, List<GameObject> childrenWithTag)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.CompareTag(tag))
            {
                childrenWithTag.Add(child.gameObject);
            }
            else
            {
                GetChildrenWithTag(child.gameObject, tag, childrenWithTag);
            }
        }
    }
}
