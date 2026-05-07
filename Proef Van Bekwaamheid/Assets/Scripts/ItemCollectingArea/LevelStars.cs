using UnityEngine;
using UnityEngine.UI;

public class LevelStars : MonoBehaviour
{
    [SerializeField] private int _commonStarPoints = 1;
    [SerializeField] private int _uncommonStarPoints = 2;
    [SerializeField] private int _rareStarPoints = 3;
    [SerializeField] private int _epicStarPoints = 4;
    [SerializeField] private int _legendaryStarPoints = 5;

    [SerializeField] private Image _star1;
    [SerializeField] private Image _star2;
    [SerializeField] private Image _star3;

    [SerializeField] private int _pointsForStar1 = 10;
    [SerializeField] private int _pointsForStar2 = 25;
    [SerializeField] private int _pointsForStar3 = 50;

    private Image[] _stars;
    private int[] _starPointRequirements;
    private bool _hasReachedThreeStars = false;

    private void Awake()
    {
        _stars = new Image[] { _star1, _star2, _star3 };
        _starPointRequirements = new int[] { _pointsForStar1, _pointsForStar2, _pointsForStar3 };
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
        float totalStarPoints = GetTotalStarPoints();
        int cumulativePoints = 0;

        for (int i = 0; i < _stars.Length; i++)
        {
            int pointsNeededForThisStar = _starPointRequirements[i] - cumulativePoints;
            float pointsIntoThisStar = totalStarPoints - cumulativePoints;
            float fillAmount = Mathf.Clamp01(pointsIntoThisStar / pointsNeededForThisStar);

            _stars[i].fillAmount = fillAmount;

            cumulativePoints += pointsNeededForThisStar;
        }

        HasReachedThreeStars();
    }

    private bool HasReachedThreeStars()
    {
        float totalStarPoints = GetTotalStarPoints();
        int cumulativePoints = 0;
        int filledStars = 0;

        for (int i = 0; i < _starPointRequirements.Length; i++)
        {
            int pointsNeededForThisStar = _starPointRequirements[i] - cumulativePoints;
            float pointsIntoThisStar = totalStarPoints - cumulativePoints;
            float fillAmount = Mathf.Clamp01(pointsIntoThisStar / pointsNeededForThisStar);

            if (fillAmount >= 1f)
                filledStars++;

            cumulativePoints += pointsNeededForThisStar;
        }

        if (filledStars == 3 && !_hasReachedThreeStars)
        {
            _hasReachedThreeStars = true;
            return true;
        }

        return false;
    }

    private float GetTotalStarPoints()
    {
        float starPoints = 0f;
        var itemsByRarity = InventoryData.Instance.ItemsByRarity;

        if (itemsByRarity.ContainsKey(Rarity.Common))
            starPoints += itemsByRarity[Rarity.Common].Count * _commonStarPoints;
        if (itemsByRarity.ContainsKey(Rarity.Uncommon))
            starPoints += itemsByRarity[Rarity.Uncommon].Count * _uncommonStarPoints;
        if (itemsByRarity.ContainsKey(Rarity.Rare))
            starPoints += itemsByRarity[Rarity.Rare].Count * _rareStarPoints;
        if (itemsByRarity.ContainsKey(Rarity.Epic))
            starPoints += itemsByRarity[Rarity.Epic].Count * _epicStarPoints;
        if (itemsByRarity.ContainsKey(Rarity.Legendary))
            starPoints += itemsByRarity[Rarity.Legendary].Count * _legendaryStarPoints;

        return starPoints;
    }
}