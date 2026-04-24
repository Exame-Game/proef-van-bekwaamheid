using Unity.VisualScripting;
using UnityEngine;

public class ResetZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);

        if (other.gameObject.layer == LayerMask.NameToLayer("PickUp"))
        {
            Destroy(other.gameObject);
            return;
        }

        int x = Random.Range(0, 3);
        int y = Random.Range(0, 3);

        other.transform.position = new Vector3(x, 10, y);
    }
}
