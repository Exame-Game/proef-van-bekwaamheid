public class ItemData
{
    public Rarity Rarity;
    public string Name;
    public float Value;
    public float Weight;
    public ItemData(ItemSO itemSO)
    {
        Rarity = itemSO.Rarity;
        Name = itemSO.Name;
        Value = itemSO.Value;
        Weight = itemSO.Weight;
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
