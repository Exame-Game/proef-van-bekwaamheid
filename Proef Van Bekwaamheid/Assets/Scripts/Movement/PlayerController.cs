using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb;
    public float speed = 1f;
    public float rotationSpeed = 0.15f;
    private Vector2 _move;
    private bool _pcAssigned;

    private void Start()
    {
        Debug.Log(OwnerClientId);
        InvokeRepeating(nameof(AssignPlayerController), 0.1f, 0.1f);
    }

    private void Update()
    {
        Debug.Log(IsOwner);

        if (!IsOwner)
            return;

        MovePlayer();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!IsOwner)
            return;

        _move = context.ReadValue<Vector2>();
    }

    public void MovePlayer()
    {
        if (!IsOwner && !_pcAssigned)
            return;

        Vector3 movement = new Vector3(_move.x, 0f, _move.y);

        if (movement == Vector3.zero)
            return;

        rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, Quaternion.LookRotation(movement), rotationSpeed);
        rb.transform.Translate(movement * speed * Time.deltaTime, Space.World);
    }

    private void AssignPlayerController()
    {   
        _pcAssigned = true;
        CancelInvoke();
    }

    public void ResetController()
    {
        _pcAssigned = false;
        InvokeRepeating(nameof(AssignPlayerController), 0.1f, 0.1f);
    }
}
