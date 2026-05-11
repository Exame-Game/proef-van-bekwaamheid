using UnityEngine;

public class PlayerShadow : MonoBehaviour
{
    public Transform player;
    public LayerMask groundLayer;
    public Vector3 shadowOffset = Vector3.zero;

    [Range(0f, 1f)]
    public float maxOpacity = 0.5f;
    public float maxHeight = 5f;

    private MeshRenderer _shadowRenderer;

    void Start()
    {
        _shadowRenderer = GetComponent<MeshRenderer>();
    }

    void LateUpdate()
    {
        if (Physics.Raycast(player.position, Vector3.down, out RaycastHit hit, maxHeight, groundLayer))
        {
            transform.position = hit.point + new Vector3(shadowOffset.x, 0.01f, shadowOffset.z);
            transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            float distanceFromGround = player.position.y - hit.point.y;
            float alpha = Mathf.Lerp(maxOpacity, 0f, distanceFromGround / maxHeight);

            Color c = _shadowRenderer.material.color;
            c.a = alpha;
            _shadowRenderer.material.color = new Color(0, 0, 0, alpha);

        }
    }
}