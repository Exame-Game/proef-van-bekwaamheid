public class ItemData
{
    public Rarity Rarity;
    public float Value;
    public float Weight;
    public string Name;
    public ItemData(ItemSO itemSO)
    {
        Rarity = itemSO.Rarity;
        Value = itemSO.Value;
        Weight = itemSO.Weight;
        Name = itemSO.Name;
    }
}

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
