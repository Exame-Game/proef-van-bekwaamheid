using UnityEngine;

public class ResetZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        other.transform.position = new Vector3(0, 10, 0);
    }
}