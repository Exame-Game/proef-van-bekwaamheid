using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;

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
    [SerializeField] private float _fillDuration = 0.5f;
    [SerializeField] private float _delayBetweenStars = 0.1f;
    [SerializeField] private Ease _fillEase = Ease.OutCubic;

    private int[] _starPointRequirements;
    private Dictionary<Rarity, int> _rarityPointsMap;
    private bool _hasReachedThreeStars = false;
    private Sequence _fillSequence;

    private void Awake()
    {
        _starPointRequirements = new int[] { _pointsForStar1, _pointsForStar2, _pointsForStar3 };
        _rarityPointsMap = new Dictionary<Rarity, int>();
        foreach (RarityStarPoints entry in _rarityStarPointsArray)
            _rarityPointsMap[entry.rarity] = entry.starPoints;
    }

    private void OnEnable()
    {
        InventoryData.Instance.OnItemAdded += UpdateStars;
        UpdateStars();
    }

    private void OnDisable()
    {
        InventoryData.Instance.OnItemAdded -= UpdateStars;
        _fillSequence?.Kill();
    }

    public void ResetStars()
    {
        _hasReachedThreeStars = false;
        foreach (Image star in _stars)
            star.fillAmount = 0f;
    }

    private void UpdateStars()
    {
        float totalStarPoints = GetTotalStarPoints();
        AnimateStarVisuals(totalStarPoints);
        CheckIfReachedThreeStars(totalStarPoints);
    }

    private float GetTotalStarPoints()
    {
        Dictionary<Rarity, List<ItemData>> itemsByRarity = InventoryData.Instance.ItemsByRarity;
        return _rarityPointsMap
            .Where(x => itemsByRarity.ContainsKey(x.Key))
            .Sum(x => itemsByRarity[x.Key].Count * x.Value);
    }

    private void AnimateStarVisuals(float totalStarPoints)
    {
        float[] targetFills = GetStarFillAmounts(totalStarPoints);

        _fillSequence?.Kill();
        _fillSequence = DOTween.Sequence();

        for (int i = 0; i < _stars.Length; i++)
        {
            Image star = _stars[i];
            float target = targetFills[i];

            if (Mathf.Approximately(star.fillAmount, target)) 
                continue;

            float duration = _fillDuration * Mathf.Abs(target - star.fillAmount);

            _fillSequence
                .Append(star.DOFillAmount(target, duration).SetEase(_fillEase))
                .AppendInterval(_delayBetweenStars);
        }
    }

    private void CheckIfReachedThreeStars(float totalStarPoints)
    {
        float[] fillAmounts = GetStarFillAmounts(totalStarPoints);
        int filledStars = fillAmounts.Count(x => x >= 1f);
        if (filledStars == 3 && !_hasReachedThreeStars)
            _hasReachedThreeStars = true;
        // TODO: Add any behavior for reaching 3 stars (unlock, sound, etc.)
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
}