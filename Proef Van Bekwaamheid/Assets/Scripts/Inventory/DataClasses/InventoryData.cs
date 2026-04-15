using System.Collections.Generic;

public class InventoryData
{
    public Dictionary<Rarity, List<ItemData>> ItemsByRarity = new Dictionary<Rarity, List<ItemData>>();
    public string InventoryName;

    public void AddDictionairy(Dictionary<Rarity, List<ItemData>> other)
    {
        foreach (KeyValuePair<Rarity, List<ItemData>> entry in other)
            if (ItemsByRarity.ContainsKey(entry.Key))
                ItemsByRarity[entry.Key].AddRange(entry.Value);
            else
                ItemsByRarity[entry.Key] = new List<ItemData>(entry.Value);
    }

    public void Save()
    {
        SaveSystem.Save<InventoryData>(this, InventoryName);
    }

    public void RemoveSave()
    {
        SaveSystem.Delete(InventoryName);
    }

    public void RemoveInventory()
    {
        RemoveSave();
        ItemsByRarity.Clear();
    }

    public void AddItem(ItemData item)
    {
        if (ItemsByRarity.ContainsKey(item.Rarity))
            ItemsByRarity[item.Rarity].Add(item);
        else
            ItemsByRarity[item.Rarity] = new List<ItemData> { item };
    }
}
