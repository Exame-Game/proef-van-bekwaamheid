using System;
using System.Collections.Generic;
using System.Linq;

public class InventoryData
{
    private static InventoryData s_Instance;

    private Dictionary<Rarity, List<ItemData>> _itemsByRarity;

    public event Action OnItemAdded;

    public Dictionary<Rarity, List<ItemData>> ItemsByRarity => _itemsByRarity;

    public static InventoryData Instance
    {
        get
        {
            if (s_Instance == null)
                s_Instance = new InventoryData();

            return s_Instance;
        }
    }

    private InventoryData()
    {
        _itemsByRarity = new Dictionary<Rarity, List<ItemData>>();

        foreach (Rarity rarity in Enum.GetValues(typeof(Rarity)))
            _itemsByRarity[rarity] = new List<ItemData>();
    }

    public void AddItem(ItemData item)
    {
        if (item == null)
            return;

        if (!_itemsByRarity.ContainsKey(item.Rarity))
            _itemsByRarity[item.Rarity] = new List<ItemData>();

        _itemsByRarity[item.Rarity].Add(item);
        OnItemAdded?.Invoke();
    }

    public void RemoveItem(ItemData item)
    {
        if (item == null)
            return;

        if (_itemsByRarity.ContainsKey(item.Rarity))
            _itemsByRarity[item.Rarity].Remove(item);
    }
    public void ResetInventory()
    {
        foreach (Rarity rarity in _itemsByRarity.Keys)
            _itemsByRarity[rarity].Clear();

        OnItemAdded?.Invoke();
    }
}
