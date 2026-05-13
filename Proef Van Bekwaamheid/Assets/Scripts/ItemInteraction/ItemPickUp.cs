using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickUp : NetworkBehaviour
{
    [SerializeField] private InputActionAsset _inputActions;
    [SerializeField] private Transform _pickUpPoint;
    [SerializeField] private LayerMask _pickUpLayer;
    [SerializeField] private Material _highlightMaterial;
    [SerializeField] private float _throwForce = 10f;
    [SerializeField] private float _throwHoldTime = 0.3f;
    [SerializeField] private float _pickUpRange = 2f;
    [SerializeField] private float _pickUpOffset = 1f;

    private readonly NetworkVariable<bool> _isHolding = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private NetworkObject _heldItem;
    private Renderer _currentHighlightedRenderer;
    private Material[] _originalMaterials;
    private InputAction _interactAction;
    private PickUpState _state;
    private float _holdTime;

    private void Awake()
    {
        _interactAction = _inputActions.FindAction("PickUp");
    }

    private void OnEnable()
    {
        _interactAction.Enable();
    }

    private void FixedUpdate()
    {
        if (!IsServer)
            return;

        if (_heldItem == null)
            return;

        _heldItem.transform.position = _pickUpPoint.position;
        _heldItem.transform.rotation = _pickUpPoint.rotation;
    }

    private void Update()
    {
        UpdateHighlight();

        if (!IsOwner)
            return;

        HandleInput();
    }

    private void OnDisable()
    {
        _interactAction.Disable();
        RemoveHighlight();
    }

    private void HandleInput()
    {
        switch (_state)
        {
            case PickUpState.Empty:
                if (_interactAction.WasPressedThisFrame())
                {
                    TryPickUpServerRpc();
                    // Move to a transition state to prevent immediate release if the button is held
                    _state = PickUpState.WaitingForRelease;
                }
                break;

            case PickUpState.WaitingForRelease:
                if (_interactAction.WasReleasedThisFrame())
                    _state = PickUpState.Holding;
                break;

            case PickUpState.Holding:
                if (_interactAction.IsPressed())
                    _holdTime += Time.deltaTime;

                if (_interactAction.WasReleasedThisFrame())
                {
                    // Determines if the action is a simple drop (0) or a physics throw based on hold duration
                    ReleaseItemServerRpc(_holdTime >= _throwHoldTime ? _holdTime : 0f);
                    _state = PickUpState.Empty;
                    _holdTime = 0f;
                }
                break;
        }
    }

    private void UpdateHighlight()
    {
        Vector3 center = transform.position + transform.forward * _pickUpOffset;
        Collider[] hits = Physics.OverlapSphere(center, _pickUpRange, _pickUpLayer);

        Renderer closestRenderer = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            if (!hit.TryGetComponent(out NetworkObject netObj))
                continue;

            if (netObj == _heldItem)
                continue;

            float dist = Vector3.Distance(transform.position, netObj.transform.position);

            if (dist >= closestDistance)
                continue;

            closestDistance = dist;
            closestRenderer = netObj.GetComponentInChildren<Renderer>();
        }

        if (closestRenderer == _currentHighlightedRenderer)
            return;

        RemoveHighlight();

        if (closestRenderer != null)
            ApplyHighlight(closestRenderer);
    }

    private void ApplyHighlight(Renderer targetRenderer)
    {
        _currentHighlightedRenderer = targetRenderer;
        _originalMaterials = _currentHighlightedRenderer.materials;

        // Creates a new array with an extra slot to append the highlight material without destroying original looks
        Material[] newMaterials = new Material[_originalMaterials.Length + 1];
        Array.Copy(_originalMaterials, newMaterials, _originalMaterials.Length);
        newMaterials[newMaterials.Length - 1] = _highlightMaterial;

        _currentHighlightedRenderer.materials = newMaterials;
    }

    private void RemoveHighlight()
    {
        if (_currentHighlightedRenderer == null)
            return;

        if (_originalMaterials == null)
            return;

        _currentHighlightedRenderer.materials = _originalMaterials;
        _currentHighlightedRenderer = null;
        _originalMaterials = null;
    }

    [ServerRpc(RequireOwnership = false)]
    private void TryPickUpServerRpc()
    {
        if (_heldItem != null)
            return;

        Vector3 center = transform.position + transform.forward * _pickUpOffset;
        Collider[] hits = Physics.OverlapSphere(center, _pickUpRange, _pickUpLayer);

        NetworkObject best = null;
        float closest = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            if (!hit.TryGetComponent(out NetworkObject netObj))
                continue;

            float dist = Vector3.Distance(transform.position, netObj.transform.position);

            if (dist >= closest)
                continue;

            closest = dist;
            best = netObj;
        }

        if (best == null)
            return;

        _heldItem = best;
        _isHolding.Value = true;
        SetHeldState(best, true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReleaseItemServerRpc(float holdTime)
    {
        if (_heldItem == null)
            return;

        Vector3 dir = transform.forward;
        _heldItem.transform.position = _pickUpPoint.position + dir * 0.5f;

        if (_heldItem.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = true;

            if (holdTime >= _throwHoldTime)
                rb.AddForce(dir.normalized * _throwForce, ForceMode.Impulse);
        }

        if (_heldItem.TryGetComponent(out Collider col))
            col.enabled = true;

        _isHolding.Value = false;
        _heldItem = null;
    }

    private void SetHeldState(NetworkObject netObj, bool isHeld)
    {
        if (netObj.TryGetComponent(out Rigidbody rb))
        {
            // Forces momentum to zero to prevent the object from flying out of the hand due to previous physics state
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = isHeld;
            rb.useGravity = !isHeld;
        }

        if (netObj.TryGetComponent(out Collider col))
            col.enabled = !isHeld;
    }

    private enum PickUpState
    {
        Empty,
        WaitingForRelease,
        Holding
    }
}