using UnityEngine;

public class Item : MonoBehaviour
{
    private ItemData _data;
    private GameObject _prefab;
    [SerializeField] private ItemSO _itemSO;

    void Awake()
    {
        _data = new ItemData(_itemSO);
        _prefab = _itemSO.Prefab;
    }
}
