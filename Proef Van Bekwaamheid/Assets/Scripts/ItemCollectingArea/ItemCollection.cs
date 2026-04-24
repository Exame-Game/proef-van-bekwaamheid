using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ItemCollection : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _inventoryText;
    [SerializeField] private Collider _collider;

    private InventoryData _inventoryData;

    private void Start()
    {
        _inventoryData = new InventoryData();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("PickUp")) 
            return;

        _inventoryData.AddItem(other.gameObject.GetComponent<Item>().data);
        UpdateInventoryText();
    }

    private void UpdateInventoryText()
    {
        string text = "";

        foreach (KeyValuePair<Rarity, List<ItemData>> entry in _inventoryData.ItemsByRarity.OrderBy(kvp => (int)kvp.Key))
        {
            text += $"<b>{entry.Key}</b>\n";

            foreach (ItemData item in entry.Value)
                text += $"  {item.Name}\n";

            text += "\n";
        }

        _inventoryText.text = text;
    }
}
