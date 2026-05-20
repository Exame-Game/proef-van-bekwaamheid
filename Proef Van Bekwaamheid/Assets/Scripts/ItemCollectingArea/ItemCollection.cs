using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ItemCollection : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _inventoryText;
    [SerializeField] private Collider _collider;
    private DOTweenAnimation _tween;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("PickUp"))
            return;

        Item itemComponent = other.gameObject.GetComponent<Item>();
        if (itemComponent == null)
        {
            Debug.LogWarning($"Item collider entered but no Item component found on {other.gameObject.name}");
            return;
        }

        ItemData data = itemComponent.data;
        if (data == null)
            return;

        InventoryData.Instance.AddItem(data);
        UpdateInventoryText();

        DOTweenAnimation tween = other.gameObject.GetComponent<DOTweenAnimation>();
        if (tween != null)
        {
            StartCoroutine(Tween(tween, other.gameObject));
        }
        else
        {
            other.gameObject.SetActive(false);
        }

        Destroy(itemComponent);
    }

    public void UpdateInventoryText()
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

    private IEnumerator Tween(DOTweenAnimation tween, GameObject targetObject)
    {
        tween.DOPlay();
        Debug.Log(tween.duration);
        yield return new WaitForSeconds(tween.duration);
        if (targetObject != null)
            targetObject.SetActive(false);
    }

    public void ResetInventory()
    {
        InventoryData.Instance.ResetInventory();
        UpdateInventoryText();
    }
}