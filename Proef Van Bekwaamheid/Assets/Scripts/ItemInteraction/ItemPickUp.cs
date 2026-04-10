using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickUp : NetworkBehaviour
{
    public Vector3 Direction { get; set; }

    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private Transform PickUpPoint;
    [SerializeField] private LayerMask PickUpLayer;

    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float throwHoldTime = 0.3f;

    [SerializeField] private float pickUpRange = 2f;
    [SerializeField] private float pickUpOffset;

    private GameObject itemHolding;
    private InputAction interactAction;

    private float holdTime;
    private bool justPickedUp;

    private void Awake()
    {
        interactAction = inputActions.FindAction("PickUp");
    }

    void Update()
    {
        if (!IsOwner) return;

        if (interactAction.WasPressedThisFrame())
            OnInteractPressed();

        if (interactAction.IsPressed() && itemHolding && !justPickedUp)
            holdTime += Time.deltaTime;

        if (interactAction.WasReleasedThisFrame())
            OnInteractReleased();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        if (itemHolding && itemHolding.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.MovePosition(PickUpPoint.position);
            rb.MoveRotation(PickUpPoint.rotation);
        }
    }

    private void OnInteractPressed()
    {
        holdTime = 0f;

        if (itemHolding && !justPickedUp)
            return;
        
        if (!itemHolding)
            TryPickUp();
    }

    private void OnInteractReleased()
    {
        if (justPickedUp)
        {
            justPickedUp = false;
            holdTime = 0f;
            return;
        }

        if (itemHolding)
            if (holdTime >= throwHoldTime)
                ThrowItem();
            else
                DropItem();
        
        holdTime = 0f;
    }

    private void TryPickUp()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position + Direction * pickUpOffset, pickUpRange, PickUpLayer);
        if (colliders.Length > 0 && PickUpPoint.childCount == 0)
            PickUpItem(colliders[0].gameObject);
    }

    private void PickUpItem(GameObject item)
    {
        itemHolding = item;
        justPickedUp = true; // flag that this press was a pickup

        if (itemHolding.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.position = PickUpPoint.position;
            rb.rotation = PickUpPoint.rotation;
            rb.Sleep();
        }

        itemHolding.transform.parent = PickUpPoint;
        itemHolding.transform.localPosition = Vector3.zero;
        itemHolding.transform.localRotation = Quaternion.identity;
    }

    private void DropItem()
    {
        itemHolding.transform.parent = null;
        itemHolding.transform.position = transform.position + Direction;

        if (itemHolding.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        itemHolding = null;
    }

    private void ThrowItem()
    {
        itemHolding.transform.parent = null;

        if (itemHolding.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(Direction * throwForce, ForceMode.Impulse);
        }

        itemHolding = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + Direction * pickUpOffset, pickUpRange);
    }

    void OnEnable() { interactAction.Enable(); }
    void OnDisable() { interactAction.Disable(); }
}