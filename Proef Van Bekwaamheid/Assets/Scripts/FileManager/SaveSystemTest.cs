using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SaveSystemTest : MonoBehaviour
{
    [SerializeField] private TMP_Text _receiptText;
    [SerializeField] private string _inventoryName;

    private InventoryData _permanentInventory;

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)  TestAddItems();
        if (Mouse.current.rightButton.wasPressedThisFrame) TestMergeAndDestroy();
        if (Keyboard.current.sKey.wasPressedThisFrame)     TestSave();
        if (Keyboard.current.lKey.wasPressedThisFrame)     TestLoad();
        if (Keyboard.current.pKey.wasPressedThisFrame)     PrintReceipt();
        if (Keyboard.current.dKey.wasPressedThisFrame)     DeleteInventory();
    }

    private void Initialize()
    {
        _permanentInventory = SaveSystem.Load<InventoryData>(_inventoryName);
        if (_permanentInventory.InventoryName != null)
            return;

        _permanentInventory = new InventoryData { InventoryName = _inventoryName };
        _permanentInventory.Save();
    }

    private void TestAddItems()
    {
        // permanentInventory.AddItem(new ItemData { Name = "Iron Sword",   Value = 10f, Weight = 5f,  Rarity = Rarity.Common    });
        // permanentInventory.AddItem(new ItemData { Name = "Silver Bow",   Value = 50f, Weight = 3f,  Rarity = Rarity.Rare      });
        // permanentInventory.AddItem(new ItemData { Name = "Dragon Staff", Value = 99f, Weight = 8f,  Rarity = Rarity.Legendary });
        // permanentInventory.AddItem(new ItemData { Name = "Wooden Club",  Value = 5f,  Weight = 4f,  Rarity = Rarity.Common    });
    }

    private void TestMergeAndDestroy()
    {
        InventoryData tempInventory = new InventoryData { InventoryName = "temp" };
        // tempInventory.AddItem(new ItemData { Name = "Epic Dagger", Value = 75f, Weight = 1f, Rarity = Rarity.Epic     });
        // tempInventory.AddItem(new ItemData { Name = "Magic Ring",  Value = 60f, Weight = 0f, Rarity = Rarity.Uncommon });

        _permanentInventory.AddDictionairy(tempInventory.ItemsByRarity);
        tempInventory.RemoveInventory();
        tempInventory = null;
    }

    private void TestSave()
    {
        _permanentInventory.Save();
    }

    private void TestLoad()
    {
        _permanentInventory = SaveSystem.Load<InventoryData>("permanent");
    }

    private void PrintReceipt()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("====== RECEIPT ======");

        // Sort rarities highest to lowest (Legendary -> Common)
        System.Collections.Generic.List<Rarity> sortedRarities = new System.Collections.Generic.List<Rarity>(_permanentInventory.ItemsByRarity.Keys);
        sortedRarities.Sort((a, b) => b.CompareTo(a));

        foreach (Rarity rarity in sortedRarities)
        {
            sb.AppendLine($"  = {rarity} =");

            // Combine duplicates by name, summing count and value
            System.Collections.Generic.Dictionary<string, (int count, float value)> counts = new System.Collections.Generic.Dictionary<string, (int, float)>();
            foreach (ItemData item in _permanentInventory.ItemsByRarity[rarity])            
                if (counts.ContainsKey(item.Name))
                    counts[item.Name] = (counts[item.Name].count + 1, counts[item.Name].value + item.Value);
                else
                    counts[item.Name] = (1, item.Value);
            

            foreach (System.Collections.Generic.KeyValuePair<string, (int count, float value)> entry in counts)
                sb.AppendLine($"    {entry.Key} x{entry.Value.count} | Value: {entry.Value.value}");
        }

        sb.AppendLine("=====================");
        _receiptText.text = sb.ToString();
    }

    private void DeleteInventory()
    {
        _permanentInventory.RemoveInventory();
        _permanentInventory = null;
        _permanentInventory = SaveSystem.Load<InventoryData>(_inventoryName);
    }
}