using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ItemCollection : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _inventoryText;
    
    private DOTweenAnimation _tween;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("PickUp"))
            return;

        if (other.gameObject.GetComponent<Item>() == null)
            return;

        InventoryData.Instance.AddItem(other.gameObject.GetComponent<Item>().data);
        UpdateInventoryText();
        _tween = other.gameObject.GetComponent<DOTweenAnimation>();
        StartCoroutine(Tween());
        Destroy(other.gameObject.GetComponent<Item>());
    }

    private void UpdateInventoryText()
    {
        string text = "";

        foreach (KeyValuePair<Rarity, List<ItemData>> entry in InventoryData.Instance.ItemsByRarity.OrderBy(kvp => (int)kvp.Key))
        {
            if (entry.Value.Count == 0)
                continue;

            text += $"<b>{entry.Key}</b>\n";

            foreach (ItemData item in entry.Value)
                text += $"  {item.Name}\n";

            text += "\n";
        }

        _inventoryText.text = text;
    }

    private IEnumerator Tween()
    {
        _tween.DOPlay();
        yield return new WaitForSeconds(_tween.duration);
    }
}