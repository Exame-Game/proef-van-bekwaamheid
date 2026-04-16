using System.Collections.Generic;
using UnityEngine;
public class ItemManager : MonoBehaviour
{
    [SerializeField] private List<Item> _items = new List<Item>();
    [SerializeField] private GameObject[] _spawnPoints;
    [SerializeField] private GameObject _itemParent;

    [SerializeField] private LayerMask _itemLayer;
    [SerializeField] private LayerMask _groundLayer;

    [SerializeField] private int _maxItems = 10;

    public List<ItemSO> AvaliableItems;

    private int _currentItems = 0;

    public void Start()
    {
        InstantiateAndSpawnItems();
    }

    private void InstantiateAndSpawnItems()
    {
        for (int i = 0; i < _spawnPoints.Length; i++)
        {
            if (_currentItems >= _maxItems)
                break;

            GameObject spawnPoint = _spawnPoints[i];
            if (spawnPoint == null)
                continue;

            Collider[] colliders = Physics.OverlapSphere(spawnPoint.transform.position, 0.5f, _itemLayer);
            if (colliders.Length > 0)
                continue;

            int randomIndex = Random.Range(0, AvaliableItems.Count);
            GameObject itemPrefab = AvaliableItems[randomIndex].Prefab;
            if (itemPrefab == null)
                continue;

            Ray ray = new Ray(spawnPoint.transform.position, Vector3.down);
            Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 100);

            if (Physics.Raycast(ray, out RaycastHit hit, 10f, _groundLayer))
            {
                if (((1 << hit.collider.gameObject.layer) & _groundLayer.value) == 0)
                    continue;
                
                GameObject newItem = Instantiate(itemPrefab, hit.point, spawnPoint.transform.rotation);
                Item spawnedItem = newItem.GetComponent<Item>();
                float halfHeight = spawnedItem.Collider.bounds.extents.y;
                newItem.transform.position = hit.point + Vector3.up * halfHeight;
                AddItem(spawnedItem);
            }
        }
    }

    public void ResetItems()
    {
        foreach (Item item in _items)
        {
            if (item != null)
                Destroy(item.gameObject);
        }

        _items.Clear();
        _currentItems = 0;
        InstantiateAndSpawnItems();
    }

    public void AddItem(Item item)
    {
        if (_currentItems >= _maxItems)
            return;

        _items.Add(item);
        item.transform.parent = _itemParent.transform;
        _currentItems++;
    }

    public void RemoveItem(Item item)
    {
        if (_items.Contains(item))
        {
            _items.Remove(item);
            item.transform.parent = null;
            _currentItems--;
        }
    }
}