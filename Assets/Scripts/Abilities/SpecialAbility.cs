using UnityEngine;

public class SpecialAbility : MonoBehaviour, IPlayerAbility
{
    public bool CanActivate()
    {
        return true;
    }

    public void Activate()
    {

    }
}
