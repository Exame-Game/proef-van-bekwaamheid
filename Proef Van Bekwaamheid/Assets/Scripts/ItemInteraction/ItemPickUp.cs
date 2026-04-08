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
            OnInteractPressed();
    }
    void FixedUpdate()
    {
        if (!IsOwner) return;
        if (ItemHolding && ItemHolding.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.MovePosition(PickUpPoint.position);
            rb.MoveRotation(PickUpPoint.rotation);
        }
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
            rb.linearVelocity = Vector3.zero;      // clear velocity first
            rb.angularVelocity = Vector3.zero;     // then set kinematic
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.position = PickUpPoint.position;
            rb.rotation = PickUpPoint.rotation;
            rb.Sleep();
        }
        ItemHolding.transform.parent = PickUpPoint;
        ItemHolding.transform.localPosition = Vector3.zero;
        ItemHolding.transform.localRotation = Quaternion.identity;
    }

    private void DropItem()
    {
        ItemHolding.transform.parent = null;
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