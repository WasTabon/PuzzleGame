using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScrollViewSnapController : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private int totalItems = 5;
    [SerializeField] private int visibleIndex = 0;

    [SerializeField] private float scrollDuration = 0.3f; // длительность анимации

    private float stepSize;
    private Tween scrollTween;

    private void Start()
    {
        stepSize = 1f / (totalItems - 1);
        SnapToIndex(visibleIndex, true); // без анимации при старте
    }

    public void ScrollLeft()
    {
        if (visibleIndex > 0)
        {
            visibleIndex--;
            SnapToIndex(visibleIndex);
        }
    }

    public void ScrollRight()
    {
        if (visibleIndex < totalItems - 1)
        {
            visibleIndex++;
            SnapToIndex(visibleIndex);
        }
    }

    private void SnapToIndex(int index, bool instant = false)
    {
        float targetPosition = stepSize * index;

        // Отменяем предыдущую анимацию, если она есть
        if (scrollTween != null && scrollTween.IsActive())
            scrollTween.Kill();

        if (instant)
        {
            scrollRect.horizontalNormalizedPosition = targetPosition;
        }
        else
        {
            scrollTween = DOTween.To(
                () => scrollRect.horizontalNormalizedPosition,
                x => scrollRect.horizontalNormalizedPosition = x,
                targetPosition,
                scrollDuration
            ).SetEase(Ease.OutCubic);
        }
    }
}