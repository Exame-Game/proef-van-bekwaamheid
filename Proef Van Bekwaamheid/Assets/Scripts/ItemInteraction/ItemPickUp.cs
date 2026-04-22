using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickUp : NetworkBehaviour
{
    [SerializeField] private InputActionAsset _inputActions;
    [SerializeField] private Transform _pickUpPoint;
    [SerializeField] private LayerMask _pickUpLayer;
    [SerializeField] private float _throwForce = 10f;
    [SerializeField] private float _throwHoldTime = 0.3f;
    [SerializeField] private float _pickUpRange = 2f;
    [SerializeField] private float _pickUpOffset = 1f;

    private InputAction _interactAction;
    private PickUpState _state;
    private float _holdTime;

    private NetworkVariable<bool> _isHolding = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    // Server-only — never assign or read this on a client
    private NetworkObject _heldItem;

    private void Awake()
    {
        _interactAction = _inputActions.FindAction("PickUp");
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;
        if (_heldItem == null) return;

        _heldItem.transform.position = _pickUpPoint.position;
        _heldItem.transform.rotation = _pickUpPoint.rotation;
    }

    private void Update()
    {
        if (!IsOwner) return;

        switch (_state)
        {
            case PickUpState.Empty:
                if (_interactAction.WasPressedThisFrame())
                {
                    TryPickUpServerRpc();
                    _state = PickUpState.WaitingForRelease;
                }
                break;

            // Waits for the button to be fully released before entering Holding.
            // This prevents the release that follows a pickup from immediately dropping the item.
            case PickUpState.WaitingForRelease:
                if (_interactAction.WasReleasedThisFrame())
                    _state = PickUpState.Holding;
                break;

            case PickUpState.Holding:
                if (_interactAction.IsPressed())
                    _holdTime += Time.deltaTime;

                if (_interactAction.WasReleasedThisFrame())
                {
                    // Pass holdTime if it meets the throw threshold, otherwise 0 to signal a plain drop
                    ReleaseItemServerRpc(_holdTime >= _throwHoldTime ? _holdTime : 0f);
                    _state = PickUpState.Empty;
                    _holdTime = 0f;
                }
                break;
        }
    }

    private void OnEnable() => _interactAction.Enable();
    private void OnDisable() => _interactAction.Disable();

    /// <summary>
    /// Finds the closest item within range and picks it up.
    /// Only runs on the server to keep physics and ownership authoritative.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void TryPickUpServerRpc(ServerRpcParams rpcParams = default)
    {
        if (_heldItem != null) return;

        Vector3 center = transform.position + transform.forward * _pickUpOffset;
        Collider[] hits = Physics.OverlapSphere(center, _pickUpRange, _pickUpLayer);

        if (hits.Length == 0) return;

        NetworkObject best = null;
        float closest = Mathf.Infinity;

        foreach (var hit in hits)
        {
            NetworkObject netObj = hit.GetComponentInParent<NetworkObject>();
            if (netObj == null) continue;

            float dist = Vector3.Distance(transform.position, netObj.transform.position);
            if (dist >= closest) continue;

            closest = dist;
            best = netObj;
        }

        if (best == null) return;

        _heldItem = best;
        _isHolding.Value = true;
        SetHeldState(best, true);
    }

    /// <summary>
    /// Releases the held item. Throws it if holdTime meets the throw threshold, otherwise drops it in place.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void ReleaseItemServerRpc(float holdTime)
    {
        if (_heldItem == null) return;

        Vector3 dir = transform.forward;
        Rigidbody rb = _heldItem.GetComponent<Rigidbody>();
        Collider col = _heldItem.GetComponent<Collider>();

        // Nudge the item forward slightly so it doesn't overlap the player collider on release
        _heldItem.transform.position = _pickUpPoint.position + dir * 0.5f;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = true;

            if (holdTime >= _throwHoldTime)
                rb.AddForce(dir.normalized * _throwForce, ForceMode.Impulse);
        }

        if (col != null)
            col.enabled = true;

        _isHolding.Value = false;
        _heldItem = null;
    }

    /// <summary>
    /// Toggles the item's physics and collision so it can be carried or released cleanly.
    /// </summary>
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

        if (col != null)
            col.enabled = !isHeld;
    }

    private enum PickUpState
    {
        Empty,
        WaitingForRelease,
        Holding
    }
}