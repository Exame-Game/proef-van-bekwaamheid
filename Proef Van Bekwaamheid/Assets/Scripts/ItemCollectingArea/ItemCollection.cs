using UnityEngine;

public class ItemCollection : MonoBehaviour
{
    private Collider _collider;
    [SerializeField] private InventoryData _inventoryData;

    private void Start()
    {
        _collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PickUp"))
        {
            _inventoryData.AddItem(other.gameObject.GetComponent<ItemData>());

            Debug.Log(_inventoryData.ItemsByRarity.Values);
        }
    }
}