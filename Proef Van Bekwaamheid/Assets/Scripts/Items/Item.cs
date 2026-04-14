using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private ItemSO _itemSO;
    private ItemData _data;
    private GameObject _prefab;

    private void Awake()
    {
        _data = new ItemData(_itemSO);
        _prefab = _itemSO.Prefab;
    }

    private void Start()
    {
        SpawnItem();
    }

    private void SpawnItem()
    {
        GameObject itemPrefab = Instantiate(_prefab);
        itemPrefab.transform.SetParent(transform);
        itemPrefab.transform.localPosition = Vector3.zero;
    }

    private void EnterToInventory(InventoryData inventoryData)
    {
        inventoryData.AddItem(_data);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Inventory")
        {
            Inventory inventory = other.GetComponent<Inventory>();
            inventory.InventoryData.AddItem(_data);
        }
    }
}
