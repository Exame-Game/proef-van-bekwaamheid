using UnityEngine;

public class DropIcon : MonoBehaviour
{
    [SerializeField] private Renderer _renderer;

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer != 9)
            return;
        ItemPickUp pickUp = other.GetComponent<ItemPickUp>();
        if (pickUp.State != ItemPickUp.PickUpState.Holding && pickUp.State != ItemPickUp.PickUpState.WaitingForRelease)
            return;
        _renderer.enabled = true;    
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != 9)
            return;
        _renderer.enabled = false;
    }
}
