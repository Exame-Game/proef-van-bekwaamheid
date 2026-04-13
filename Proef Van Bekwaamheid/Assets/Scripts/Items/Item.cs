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
}
