using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using PuzzleGame.Scripts;

public class ScrollViewSnapController : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private int totalItems = 5;
    [SerializeField] private float scrollDuration = 0.3f;
    [SerializeField] private float scrollDurationAnimated = 0.1f;
    [SerializeField] private AudioClip starSound;

    private int visibleIndex = 0;
    private float stepSize;
    private Tween scrollTween;

    private const string PlayerPrefsKey = "ScrollViewVisibleIndex";

    private bool _isWin;

    private void Start()
    {
        stepSize = 1f / (totalItems - 1);

        visibleIndex = PlayerPrefs.GetInt(PlayerPrefsKey, 0);
        visibleIndex = Mathf.Clamp(visibleIndex, 0, totalItems - 1);

        LoadLevel();
        
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

    private void LoadLevel()
    {
        string level = PlayerPrefs.GetString("currentLevel");
        if (PlayerPrefs.HasKey("winLevel"))
        {
            string winLevel = PlayerPrefs.GetString("winLevel");
            if (level == winLevel)
            {
                _isWin = true;
            }
        }
    }

    private void AnimateLevelStars()
    {
        Transform[] allChildren = content.GetComponentsInChildren<Transform>(true);
        Sequence sequence = DOTween.Sequence();

        foreach (Transform child in allChildren)
        {
            if (child.CompareTag("Star"))
            {
                Image starImage = child.GetComponent<Image>();
                if (starImage != null)
                {
                    // Ставим начальный размер в 0
                    child.localScale = Vector3.zero;

                    // Добавляем анимацию в последовательность
                    sequence.Append(
                        child.DOScale(Vector3.one, 0.3f)
                            .SetEase(Ease.OutBack)
                            .OnStart(() =>
                            {
                                MusicController.Instance.PlaySpecificSound(starSound);
                            })
                    );

                    // Можно добавить небольшую паузу между звездами
                    sequence.AppendInterval(0.1f);
                }
            }
        }
    }
    
    private System.Collections.IEnumerator ScrollToIndexAnimated(int targetIndex)
    {
        for (int i = 0; i <= targetIndex; i++)
        {
            SnapToIndex(i);
            yield return new WaitForSeconds(scrollDurationAnimated * 0.75f);
        }

        if (_isWin)
        {
            AnimateLevelStars();
        }
    }
}
