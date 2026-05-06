using UnityEngine;
using UnityEngine.UI;

public class LevelStars : MonoBehaviour
{
    private const int k_StarCount = 3;
    private const float k_StarsPerLevel = 3f;

    [SerializeField] private int _itemsForFullStars = 15;
    [SerializeField] private Image _star1;
    [SerializeField] private Image _star2;
    [SerializeField] private Image _star3;
    [SerializeField] private Color _filledColor;

    private Image[] _stars;

    private void Awake()
    {
        _stars = new Image[] { _star1, _star2, _star3 };
    }

    private void OnEnable()
    {
        InventoryData.Instance.OnItemAdded += UpdateStars;
        UpdateStars();
    }

    private void OnDisable()
    {
        InventoryData.Instance.OnItemAdded -= UpdateStars;
    }

    private void UpdateStars()
    {
        int totalItems = GetTotalItemsCollected();
        float fillAmount = Mathf.Min((float)totalItems / _itemsForFullStars * k_StarsPerLevel, k_StarCount);

        for (int i = 0; i < _stars.Length; i++)
        {
            _stars[i].fillAmount = Mathf.Clamp01(fillAmount - i);
            _stars[i].color = _filledColor;
        }
    }

    private int GetTotalItemsCollected()
    {
        int total = 0;

        foreach (var itemList in InventoryData.Instance.ItemsByRarity.Values)
            total += itemList.Count;

        return total;
    }
}