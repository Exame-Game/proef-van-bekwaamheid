// ToggleGameObject.cs
using UnityEngine;
using DG.Tweening;

public class ToggleGameObject : MonoBehaviour
{
    [Header("Tween Settings")]
    public float tweenDuration = 0.3f;
    public Ease easeIn = Ease.OutBack;
    public Ease easeOut = Ease.InBack;
    public Vector3 hiddenOffset = new Vector3(-500f, 0f, 0f);

    public void ToggleActive(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("ToggleActive called with a null GameObject reference.");
            return;
        }

        RectTransform rect = obj.GetComponent<RectTransform>();

        if (rect == null)
        {
            Debug.LogWarning("ToggleActive: GameObject has no RectTransform. Using simple SetActive fallback.");
            obj.SetActive(!obj.activeSelf);
            return;
        }

        if (obj.activeSelf)
            SlideOut(obj, rect);
        else
            SlideIn(obj, rect);
    }

    public void SlideIn(GameObject obj, RectTransform rect)
    {
        obj.SetActive(true);
        rect.anchoredPosition = hiddenOffset;
        rect.DOAnchorPos(Vector2.zero, tweenDuration)
            .SetEase(easeIn);
    }

    public void SlideOut(GameObject obj, RectTransform rect)
    {
        rect.DOAnchorPos(hiddenOffset, tweenDuration)
            .SetEase(easeOut)
            .OnComplete(() => obj.SetActive(false));
    }

    // Instantly hide with no tween — used for resetting state
    public void HideInstant(GameObject obj)
    {
        if (obj == null) return;
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect != null) rect.anchoredPosition = hiddenOffset;
        obj.SetActive(false);
    }
}