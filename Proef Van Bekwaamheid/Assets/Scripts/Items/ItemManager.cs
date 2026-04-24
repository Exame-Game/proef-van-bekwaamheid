using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] _spawnPoints;
    [SerializeField] private LayerMask _itemLayer;
    [SerializeField] private LayerMask _groundLayer;

    [SerializeField] private int _maxItems = 10;

    public List<ItemSO> AvailableItems;

    private int _currentItems = 0;

    private void Start()
    {
        if (!NetworkManager.Singleton.IsServer) 
            return;

        SpawnItems();
    }

    private void SpawnItems()
    {
        for (int i = 0; i < _spawnPoints.Length; i++)
        {
            if (_currentItems >= _maxItems)
                break;

            GameObject spawnPoint = _spawnPoints[i];
            if (spawnPoint == null)
                continue;

            Collider[] hits = Physics.OverlapSphere(spawnPoint.transform.position, 0.5f, _itemLayer);
            if (hits.Length > 0)
                continue;

            int randomIndex = Random.Range(0, AvailableItems.Count);
            GameObject prefab = AvailableItems[randomIndex].Prefab;

            if (prefab == null)
                continue;

            Ray ray = new Ray(spawnPoint.transform.position, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit, 10f, _groundLayer))
            {
                GameObject obj = Instantiate(prefab, hit.point, Quaternion.identity);

                NetworkObject netObj = obj.GetComponent<NetworkObject>();
                if (netObj != null)
                    netObj.Spawn();

                Item item = obj.GetComponent<Item>();

                if (item != null && item.Collider != null)
                {
                    float halfHeight = item.Collider.bounds.extents.y;
                    obj.transform.position = hit.point + Vector3.up * halfHeight;
                }

                _currentItems++;
            }
        }
    }

    public void ResetItems()
    {
        if (!NetworkManager.Singleton.IsServer) 
            return;

        Item[] items = FindObjectsOfType<Item>();

        foreach (Item item in items)
        {
            if (item == null) 
                continue;

            NetworkObject netObj = item.GetComponent<NetworkObject>();

            if (netObj != null && netObj.IsSpawned)
                netObj.Despawn();

            Destroy(item.gameObject);
        }

        _currentItems = 0;

        SpawnItems();
    }
}
