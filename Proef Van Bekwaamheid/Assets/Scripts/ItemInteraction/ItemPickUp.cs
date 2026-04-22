using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickUp : NetworkBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private Transform pickUpPoint;
    [SerializeField] private LayerMask pickUpLayer;

    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float throwHoldTime = 0.3f;
    [SerializeField] private float pickUpRange = 2f;
    [SerializeField] private float pickUpOffset = 1f;

    private InputAction interactAction;

    private enum PickUpState { Empty, WaitingForRelease, Holding }
    private PickUpState state = PickUpState.Empty;
    private float holdTime = 0f;

    private NetworkVariable<bool> isHolding = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private NetworkObject heldItem;

    private void Awake()
    {
        interactAction = inputActions.FindAction("PickUp");
    }

    private void Update()
    {
        if (!IsOwner) return;

        switch (state)
        {
            case PickUpState.Empty:
                if (interactAction.WasPressedThisFrame())
                {
                    TryPickUpServerRpc();
                    state = PickUpState.WaitingForRelease;
                }
                break;

            case PickUpState.WaitingForRelease:
                if (interactAction.WasReleasedThisFrame())
                {
                    state = PickUpState.Holding;
                }
                break;

            case PickUpState.Holding:
                if (interactAction.IsPressed())
                {
                    holdTime += Time.deltaTime;
                }

                if (interactAction.WasReleasedThisFrame())
                {
                    if (holdTime >= throwHoldTime)
                        ReleaseItemServerRpc(holdTime);
                    else
                        ReleaseItemServerRpc(0f);

                    state = PickUpState.Empty;
                    holdTime = 0f;
                }
                break;
        }
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        if (heldItem != null)
        {
            heldItem.transform.position = pickUpPoint.position;
            heldItem.transform.rotation = pickUpPoint.rotation;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TryPickUpServerRpc(ServerRpcParams rpcParams = default)
    {
        if (heldItem != null) return;

        Vector3 center = transform.position + transform.forward * pickUpOffset;
        Collider[] hits = Physics.OverlapSphere(center, pickUpRange, pickUpLayer);

        if (hits.Length == 0) return;

        NetworkObject best = null;
        float closest = Mathf.Infinity;

        foreach (var hit in hits)
        {
            NetworkObject netObj = hit.GetComponentInParent<NetworkObject>();
            if (netObj == null) continue;

            float dist = Vector3.Distance(transform.position, netObj.transform.position);
            if (dist < closest)
            {
                closest = dist;
                best = netObj;
            }
        }

        if (best == null) return;

        heldItem = best;
        isHolding.Value = true;
        SetHeldState(best, true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReleaseItemServerRpc(float holdTime)
    {
        if (heldItem == null) return;

        Vector3 dir = transform.forward;
        bool shouldThrow = holdTime >= throwHoldTime;

        Rigidbody rb = heldItem.GetComponent<Rigidbody>();
        Collider col = heldItem.GetComponent<Collider>();

        heldItem.transform.position = pickUpPoint.position + dir * 0.5f;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = true;

            if (shouldThrow)
                rb.AddForce(dir.normalized * throwForce, ForceMode.Impulse);
        }

        if (col != null)
            col.enabled = true;

        isHolding.Value = false;
        heldItem = null;
    }

    private void SetHeldState(NetworkObject netObj, bool isHeld)
    {
        Rigidbody rb = netObj.GetComponent<Rigidbody>();
        Collider col = netObj.GetComponent<Collider>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = isHeld;
            rb.useGravity = !isHeld;
        }
        if (col != null) col.enabled = !isHeld;
    }

    private void OnEnable() => interactAction.Enable();
    private void OnDisable() => interactAction.Disable();
}