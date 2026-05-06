using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Rigidbody _rb;
    //[SerializeField] private ItemPickUp _itemPickUp;
    
    public NetworkVariable<Vector3> MoveDirection =
        new NetworkVariable<Vector3>(Vector3.zero);

    public float Speed = 7f;
    public float RotationSpeed = 10f;
    [Range(0f, 1f)] public float InputSmoothing = 0.1f;

    private Vector2 _move;
    private Vector2 _smoothedMove;

    private void FixedUpdate()
    {
        if (!IsServer) 
            return;

        _smoothedMove = Vector2.Lerp(_smoothedMove, _move, InputSmoothing);
        MovePlayer();
    }

    private void MovePlayer()
    {
        Vector3 movement = new Vector3(_smoothedMove.x, 0f, _smoothedMove.y);

        if (movement.sqrMagnitude < 0.001f)
            movement = Vector3.zero;

        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);

            _rb.MoveRotation(Quaternion.Slerp(
                _rb.rotation,
                targetRotation,
                RotationSpeed * Time.fixedDeltaTime
            ));
        }

        _rb.MovePosition(_rb.position + movement * Speed * Time.fixedDeltaTime);

        MoveDirection.Value = movement;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!IsOwner) 
            return;

        Vector2 input = context.ReadValue<Vector2>();
        SendMoveServerRpc(input);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendMoveServerRpc(Vector2 input)
    {
        _move = input;
    }
}
