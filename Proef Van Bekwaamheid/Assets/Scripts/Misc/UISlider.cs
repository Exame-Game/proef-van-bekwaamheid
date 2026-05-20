using UnityEngine;
using DG.Tweening;

public class UISlider : MonoBehaviour
{
    [Header("Slide Settings")]
    public float slideDuration = 0.4f;
    public Ease slideEase = Ease.OutCubic;

    [Header("Positions")]
    public Vector2 hiddenPosition;  
    public Vector2 visiblePosition;  

    private RectTransform _rect;
    private bool _isVisible = false;

    void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _rect.anchoredPosition = hiddenPosition;
    }

    public void SlideIn()
    {
        _rect.DOAnchorPos(visiblePosition, slideDuration)
             .SetEase(slideEase)
             .SetUpdate(true); 

        _isVisible = true;
    }

    public void SlideOut()
    {
        _rect.DOAnchorPos(hiddenPosition, slideDuration)
             .SetEase(slideEase)
             .SetUpdate(true);

        _isVisible = false;
    }

    public void Toggle()
    {
        if (_isVisible) SlideOut();
        else SlideIn();
    }
}