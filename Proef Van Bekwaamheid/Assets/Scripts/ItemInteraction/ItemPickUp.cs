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

        if (interactAction.WasPressedThisFrame())
        {
            if (ItemHolding)
            {
                ItemHolding.transform.parent = null;
                ItemHolding.transform.position = transform.position + Direction;
                if (ItemHolding.TryGetComponent<Rigidbody>(out Rigidbody heldRb))
                {
                    heldRb.isKinematic = false;
                    heldRb.useGravity = true;
                    heldRb.detectCollisions = true;
                }
                ItemHolding = null;
            }
            else
            {
                Collider[] colliders = Physics.OverlapSphere(transform.position, 2f, PickUpLayer);
                if (colliders.Length > 0 && PickUpPoint.childCount == 0)
                {
                    ItemHolding = colliders[0].gameObject;
                    if (ItemHolding.TryGetComponent<Rigidbody>(out Rigidbody rb))
                    {
                        rb.isKinematic = true;
                        rb.useGravity = false;
                        rb.detectCollisions = false;
                    }
                    ItemHolding.transform.parent = PickUpPoint;
                    ItemHolding.transform.localPosition = Vector3.zero;
                    ItemHolding.transform.localRotation = Quaternion.identity;
                }
            }
        }
    }

    void OnEnable() { interactAction.Enable(); }
    void OnDisable() { interactAction.Disable(); }
}