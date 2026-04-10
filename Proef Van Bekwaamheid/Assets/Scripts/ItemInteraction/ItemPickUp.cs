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
        {
            if (holdTime >= throwHoldTime)
                ThrowItem();
            else
                DropItem();
        }

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
        justPickedUp = true;

        if (itemHolding.TryGetComponent<NetworkObject>(out NetworkObject netObj))
            RequestOwnershipServerRpc(netObj.NetworkObjectId);

        if (itemHolding.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;   // zero BEFORE
            rb.angularVelocity = Vector3.zero;  // setting kinematic
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.position = PickUpPoint.position;
            rb.rotation = PickUpPoint.rotation;
            rb.Sleep();
        }

        if (itemHolding.TryGetComponent<Collider>(out Collider col))
            col.enabled = false;
    }

    private void DropItem()
    {
        if (itemHolding.TryGetComponent<NetworkObject>(out NetworkObject netObj))
            DropItemServerRpc(netObj.NetworkObjectId, transform.position + Direction);

        if (itemHolding.TryGetComponent<Collider>(out Collider col))
            col.enabled = true;

        itemHolding = null;
    }

    private void ThrowItem()
    {
        if (itemHolding.TryGetComponent<NetworkObject>(out NetworkObject netObj))
            ThrowItemServerRpc(netObj.NetworkObjectId, transform.position + Direction, Direction);

        if (itemHolding.TryGetComponent<Collider>(out Collider col))
            col.enabled = true;

        itemHolding = null;
    }

    [ServerRpc]
    private void DropItemServerRpc(ulong networkObjectId, Vector3 dropPosition)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
        {
            netObj.RemoveOwnership();

            if (netObj.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.position = dropPosition;
            }

            if (netObj.TryGetComponent<Collider>(out Collider col))
                col.enabled = true;
        }
    }

    [ServerRpc]
    private void ThrowItemServerRpc(ulong networkObjectId, Vector3 dropPosition, Vector3 throwDirection)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
        {
            netObj.RemoveOwnership();

            if (netObj.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.position = dropPosition;
                rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
            }

            if (netObj.TryGetComponent<Collider>(out Collider col))
                col.enabled = true;
        }
    }

    [ServerRpc]
    private void RequestOwnershipServerRpc(ulong networkObjectId)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
            netObj.ChangeOwnership(OwnerClientId);
    }

    [ServerRpc]
    private void ReleaseOwnershipServerRpc(ulong networkObjectId)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
            netObj.RemoveOwnership();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + Direction * pickUpOffset, pickUpRange);
    }

    void OnEnable() { interactAction.Enable(); }
    void OnDisable() { interactAction.Disable(); }
}