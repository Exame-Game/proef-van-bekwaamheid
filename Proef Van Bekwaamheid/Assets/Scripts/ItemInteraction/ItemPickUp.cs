using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickUp : NetworkBehaviour
{
    public InputActionAsset inputActions;
    public LayerMask PickUpLayer;
    public Vector3 Direction { get; set; }
    public Transform PickUpPoint;

    private GameObject ItemHolding;
    private InputAction interactAction;

    private void Awake()
    {
        interactAction = inputActions.FindAction("PickUp");
    }

    void Update()
    {
        if (!IsOwner) return;

        if (ItemHolding)
            CarryItem();

        if (interactAction.WasPressedThisFrame())
            OnInteractPressed();
    }

    private void CarryItem()
    {
        ItemHolding.transform.position = PickUpPoint.position;
        ItemHolding.transform.rotation = PickUpPoint.rotation;
    }

    private void OnInteractPressed()
    {
        if (ItemHolding)
            DropItem();
        else
            TryPickUp();
    }

    private void TryPickUp()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 2f, PickUpLayer);
        if (colliders.Length > 0 && PickUpPoint.childCount == 0)
            PickUpItem(colliders[0].gameObject);
    }

    private void PickUpItem(GameObject item)
    {
        ItemHolding = item;
        if (ItemHolding.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    private void DropItem()
    {
        ItemHolding.transform.position = transform.position + Direction;
        if (ItemHolding.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
        ItemHolding = null;
    }

    void OnEnable() { interactAction.Enable(); }
    void OnDisable() { interactAction.Disable(); }
}