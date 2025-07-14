using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScrollViewSnapController : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private int totalItems = 5;
    [SerializeField] private float scrollDuration = 0.3f;
    [SerializeField] private float scrollDurationAnimated = 0.1f;

    private int visibleIndex = 0;
    private float stepSize;
    private Tween scrollTween;

    private const string PlayerPrefsKey = "ScrollViewVisibleIndex";

    private void Start()
    {
        stepSize = 1f / (totalItems - 1);

        // Загружаем сохранённый индекс
        visibleIndex = PlayerPrefs.GetInt(PlayerPrefsKey, 0);
        visibleIndex = Mathf.Clamp(visibleIndex, 0, totalItems - 1);

        // Стартовая быстрая анимация "пролистывания" к сохранённому индексу
        StartCoroutine(ScrollToIndexAnimated(visibleIndex));
    }

    public void ScrollLeft()
    {
        if (visibleIndex > 0)
        {
            visibleIndex--;
            SnapToIndex(visibleIndex);
            SaveIndex();
        }
    }

    public void ScrollRight()
    {
        if (visibleIndex < totalItems - 1)
        {
            visibleIndex++;
            SnapToIndex(visibleIndex);
            SaveIndex();
        }
    }

    private void SnapToIndex(int index, bool instant = false)
    {
        float targetPosition = stepSize * index;

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

    private void SaveIndex()
    {
        PlayerPrefs.SetInt(PlayerPrefsKey, visibleIndex);
        PlayerPrefs.Save();
    }

    private System.Collections.IEnumerator ScrollToIndexAnimated(int targetIndex)
    {
        for (int i = 0; i <= targetIndex; i++)
        {
            SnapToIndex(i);
            yield return new WaitForSeconds(scrollDurationAnimated * 0.75f); // немного быстрее
        }
    }
}
