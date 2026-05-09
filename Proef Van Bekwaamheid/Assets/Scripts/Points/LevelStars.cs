using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[System.Serializable]
public class RarityStarPoints
{
    public Rarity rarity;
    public int starPoints;
}

public class LevelStars : MonoBehaviour
{
    [SerializeField] private RarityStarPoints[] _rarityStarPointsArray;

    [SerializeField] private Image[] _stars;

    [SerializeField] private int _pointsForStar1 = 10;
    [SerializeField] private int _pointsForStar2 = 25;
    [SerializeField] private int _pointsForStar3 = 50;

    private int[] _starPointRequirements;
    private Dictionary<Rarity, int> _rarityPointsMap;

    private bool _hasReachedThreeStars = false;

    private void Awake()
    {
        _starPointRequirements = new int[] { _pointsForStar1, _pointsForStar2, _pointsForStar3 };

        _rarityPointsMap = new Dictionary<Rarity, int>();
        foreach (var entry in _rarityStarPointsArray)
        {
            _rarityPointsMap[entry.rarity] = entry.starPoints;
        }
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
        UpdateStarVisuals(totalStarPoints);
        CheckIfReachedThreeStars(totalStarPoints);
    }

    private void UpdateStarVisuals(float totalStarPoints)
    {
        float[] fillAmounts = GetStarFillAmounts(totalStarPoints);

        for (int i = 0; i < _stars.Length; i++)
        {
            _stars[i].fillAmount = fillAmounts[i];
        }
    }

    private void CheckIfReachedThreeStars(float totalStarPoints)
    {
        float[] fillAmounts = GetStarFillAmounts(totalStarPoints);
        int filledStars = fillAmounts.Count(x => x >= 1f);

        if (filledStars == 3 && !_hasReachedThreeStars)
        {
            _hasReachedThreeStars = true;
            // TODO: Add any behavior for reaching 3 stars (unlock, sound, etc.)
        }
    }

    private float[] GetStarFillAmounts(float totalStarPoints)
    {
        float[] fillAmounts = new float[_stars.Length];
        int cumulativePoints = 0;

        for (int i = 0; i < _stars.Length; i++)
        {
            int pointsNeededForThisStar = _starPointRequirements[i] - cumulativePoints;
            float pointsIntoThisStar = totalStarPoints - cumulativePoints;
            fillAmounts[i] = Mathf.Clamp01(pointsIntoThisStar / pointsNeededForThisStar);
            cumulativePoints += pointsNeededForThisStar;
        }

        return fillAmounts;
    }

    private float GetTotalStarPoints()
    {
        var itemsByRarity = InventoryData.Instance.ItemsByRarity;

        return _rarityPointsMap
            .Where(x => itemsByRarity.ContainsKey(x.Key))
            .Sum(x => itemsByRarity[x.Key].Count * x.Value);
    }
}