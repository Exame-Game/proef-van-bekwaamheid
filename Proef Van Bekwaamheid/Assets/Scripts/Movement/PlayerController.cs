using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    public float speed = 7f;
    public float rotationSpeed = 10f;
    [Range(0f, 1f)] public float inputSmoothing = 0.1f;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private ItemPickUp itemPickUp;

    // ? Shared direction for pickup/throw
    public NetworkVariable<Vector3> MoveDirection =
        new NetworkVariable<Vector3>(Vector3.zero);

    private Vector2 _move;
    private Vector2 _smoothedMove;

    private void FixedUpdate()
    {
        if (!IsServer) return;

        _smoothedMove = Vector2.Lerp(_smoothedMove, _move, inputSmoothing);
        MovePlayer();
    }

    // ?? Client input
    public void OnMove(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        Vector2 input = context.ReadValue<Vector2>();
        SendMoveServerRpc(input);
    }

    // ?? Send input to server
    [ServerRpc(RequireOwnership = false)]
    private void SendMoveServerRpc(Vector2 input)
    {
        _move = input;
    }

    private void MovePlayer()
    {
        Vector3 movement = new Vector3(_smoothedMove.x, 0f, _smoothedMove.y);

        if (movement.sqrMagnitude < 0.001f)
            movement = Vector3.zero;

        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);

            rb.MoveRotation(Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            ));
        }

        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);

        // ? THIS is now synced correctly
        MoveDirection.Value = movement;
    }
}