using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] _spawnPoints;
    [SerializeField] private LayerMask _itemLayer;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private RaritySpawnRule[] _rarityRules;

    public List<ItemSO> AvailableItems;

    private int _currentItems = 0;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        ResetItems();
        SpawnItems();
    }

    public void SpawnItems()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        Dictionary<Rarity, int> spawnedCounts = new Dictionary<Rarity, int>();
        Dictionary<Rarity, int> maxCounts = new Dictionary<Rarity, int>();

        foreach (RaritySpawnRule rule in _rarityRules)
        {
            spawnedCounts[rule.Rarity] = 0;
            maxCounts[rule.Rarity] = rule.MaxCount;
        }

        for (int i = 0; i < _spawnPoints.Length; i++)
        {
            List<ItemSO> affordable = AvailableItems.FindAll(item => spawnedCounts[item.Rarity] < maxCounts[item.Rarity]);
            if (affordable.Count == 0)
                break;

            GameObject spawnPoint = _spawnPoints[i];
            if (spawnPoint == null)
                continue;

            Collider[] hits = Physics.OverlapSphere(spawnPoint.transform.position, 0.5f, _itemLayer);
            if (hits.Length > 0)
                continue;

            int randomIndex = Random.Range(0, affordable.Count);
            ItemSO selectedItem = affordable[randomIndex];
            GameObject prefab = selectedItem.Prefab;
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
                spawnedCounts[selectedItem.Rarity]++;
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
    }
}

[System.Serializable]
public struct RaritySpawnRule
{
    public Rarity Rarity;
    public int MaxCount;
}