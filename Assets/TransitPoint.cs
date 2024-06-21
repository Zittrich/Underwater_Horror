using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitPoint : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerSetPoint target;
    public void Interact()
    {
        Manager.Instance.Get<PlayerManager>().
            PlayerObject.transform.SetPositionAndRotation(target.transform.position, target.transform.rotation);
    }
}
