using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private ItemSO _itemSO;

    public BoxCollider Collider;
    public ItemData data;

    private GameObject _prefab;

    private void Awake()
    {
        data = new ItemData(_itemSO);
        _prefab = _itemSO.Prefab;
    }
}
