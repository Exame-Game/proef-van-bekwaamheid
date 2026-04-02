using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb;
    public float speed = 7f;
    public float rotationSpeed = 10f;
    [Range(0f, 1f)]
    public float inputSmoothing = 0.1f;

    private Vector2 _move;
    private Vector2 _smoothedMove;

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        _smoothedMove = Vector2.Lerp(_smoothedMove, _move, inputSmoothing);
        MovePlayer();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        _move = context.ReadValue<Vector2>();
    }

    public void SetInput(Vector2 input)
    {
        if (!IsOwner) return;
        _move = input;
    }

    private void MovePlayer()
    {
        Vector3 movement = new Vector3(_smoothedMove.x, 0f, _smoothedMove.y);

        if (movement.magnitude >= 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));

            // Preserve current Y velocity so gravity is not overridden
            Vector3 newPosition = rb.position + movement * speed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
        }
    }
}
