using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SaveSystemTest : MonoBehaviour
{
    [SerializeField] private TMP_Text receiptText;

    [SerializeField] private string _inventoryName;

    private InventoryData permanentInventory;

    private void Start()
    {
        permanentInventory = SaveSystem.Load<InventoryData>(_inventoryName);
        if (permanentInventory.InventoryName == null)
        {    
            permanentInventory = new InventoryData { InventoryName = _inventoryName };
            permanentInventory.Save();
        }
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

    private void TestAddItems()
    {
        permanentInventory.AddItem(new ItemData { Name = "Iron Sword",   Value = 10f, Weight = 5f,  Rarity = Rarity.Common    });
        permanentInventory.AddItem(new ItemData { Name = "Silver Bow",   Value = 50f, Weight = 3f,  Rarity = Rarity.Rare      });
        permanentInventory.AddItem(new ItemData { Name = "Dragon Staff", Value = 99f, Weight = 8f,  Rarity = Rarity.Legendary });
        permanentInventory.AddItem(new ItemData { Name = "Wooden Club",  Value = 5f,  Weight = 4f,  Rarity = Rarity.Common    });
    }

    private void TestMergeAndDestroy()
    {
        InventoryData tempInventory = new InventoryData { InventoryName = "temp" };
        tempInventory.AddItem(new ItemData { Name = "Epic Dagger", Value = 75f, Weight = 1f, Rarity = Rarity.Epic     });
        tempInventory.AddItem(new ItemData { Name = "Magic Ring",  Value = 60f, Weight = 0f, Rarity = Rarity.Uncommon });

        permanentInventory.AddDictionairy(tempInventory.ItemsByRarity);
        tempInventory.RemoveInventory();
        tempInventory = null;

    }

    private void TestSave()
    {
        permanentInventory.Save();
    }

    private void TestLoad()
    {
        permanentInventory = SaveSystem.Load<InventoryData>("permanent");
    }

    private void PrintReceipt()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("====== RECEIPT ======");

        // Sort rarities highest to lowest (Legendary -> Common)
        System.Collections.Generic.List<Rarity> sortedRarities = new System.Collections.Generic.List<Rarity>(permanentInventory.ItemsByRarity.Keys);
        sortedRarities.Sort((a, b) => b.CompareTo(a));

        foreach (Rarity rarity in sortedRarities)
        {
            sb.AppendLine($"  = {rarity} =");

            // Combine duplicates by name, summing count and value
            System.Collections.Generic.Dictionary<string, (int count, float value)> counts = new System.Collections.Generic.Dictionary<string, (int, float)>();
            foreach (ItemData item in permanentInventory.ItemsByRarity[rarity])
            {
                if (counts.ContainsKey(item.Name))
                    counts[item.Name] = (counts[item.Name].count + 1, counts[item.Name].value + item.Value);
                else
                    counts[item.Name] = (1, item.Value);
            }

            foreach (System.Collections.Generic.KeyValuePair<string, (int count, float value)> entry in counts)
                sb.AppendLine($"    {entry.Key} x{entry.Value.count} | Value: {entry.Value.value}");
        }

        sb.AppendLine("=====================");
        receiptText.text = sb.ToString();
    }

    private void DeleteInventory()
    {
        permanentInventory.RemoveInventory();
        permanentInventory = null;

        permanentInventory = SaveSystem.Load<InventoryData>(_inventoryName);
    }
}