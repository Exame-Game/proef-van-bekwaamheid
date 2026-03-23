using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : NetworkBehaviour
{
    public float speed = 1f;
    public float rotationSpeed = 0.15f;
    private Vector2 _move;

    private void Update()
    {
        if (!IsOwner)
            return;

        MovePlayer();
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        _move = context.ReadValue<Vector2>();
    }

    public void MovePlayer()
    {
        MoveServerRpc();
    }

    [ServerRpc]
    public void MoveServerRpc()
    {
        Vector3 movement = new Vector3(_move.x, 0f, _move.y);

        if (movement == Vector3.zero)
            return;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), rotationSpeed);
        transform.Translate(movement * speed * Time.deltaTime, Space.World);
    }
}
