using System.Collections.Generic;
using System.Linq;

public class InventoryData
{
    private static InventoryData _instance;
    private Dictionary<Rarity, List<ItemData>> _itemsByRarity;

    public Dictionary<Rarity, List<ItemData>> ItemsByRarity => _itemsByRarity;

    public static InventoryData Instance
    {
        get
        {
            if (_instance == null)
                _instance = new InventoryData();
            return _instance;
        }
    }

    private InventoryData()
    {
        _itemsByRarity = new Dictionary<Rarity, List<ItemData>>();

        // Initialize dictionary with all rarity types
        foreach (Rarity rarity in System.Enum.GetValues(typeof(Rarity)))
        {
            _itemsByRarity[rarity] = new List<ItemData>();
        }
    }

    public void AddItem(ItemData item)
    {
        if (item == null) return;

        if (!_itemsByRarity.ContainsKey(item.Rarity))
            _itemsByRarity[item.Rarity] = new List<ItemData>();

        _itemsByRarity[item.Rarity].Add(item);
    }

    public void RemoveItem(ItemData item)
    {
        if (item == null) return;

        if (_itemsByRarity.ContainsKey(item.Rarity))
            _itemsByRarity[item.Rarity].Remove(item);
    }
}