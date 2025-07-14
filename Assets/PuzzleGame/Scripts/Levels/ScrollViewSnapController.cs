using System.Collections.Generic;
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

    private HashSet<int> completedLevels = new HashSet<int>();
    private const string CompletedLevelsKey = "CompletedLevels";
    
    private int visibleIndex = 0;
    private float stepSize;
    private Tween scrollTween;

    private const string PlayerPrefsKey = "ScrollViewVisibleIndex";

    private bool _isWin;

    private void Start()
    {
        stepSize = 1f / (totalItems - 1);

        LoadCompletedLevels();

        visibleIndex = PlayerPrefs.GetInt(PlayerPrefsKey, 0);
        visibleIndex = Mathf.Clamp(visibleIndex, 0, totalItems - 1);

        LoadLevel();

        ShowCompletedStars(); // <-- ДОБАВЛЕНО
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
                PlayerPrefs.SetString("currentLevel", "123");
                PlayerPrefs.SetString("winLevel", "321");
            }
        }
    }

    private void AnimateLevelStars()
    {
        if (!completedLevels.Contains(visibleIndex))
        {
            completedLevels.Add(visibleIndex);
            SaveCompletedLevels();
        }

        Transform levelItem = content.GetChild(visibleIndex);
        Transform[] stars = levelItem.GetComponentsInChildren<Transform>(true);

        Sequence sequence = DOTween.Sequence();

        foreach (Transform star in stars)
        {
            if (star.CompareTag("Star"))
            {
                Image starImage = star.GetComponent<Image>();
                if (starImage != null)
                {
                    star.localScale = Vector3.zero;

                    sequence.Append(
                        star.DOScale(Vector3.one, 0.3f)
                            .SetEase(Ease.OutBack)
                            .OnStart(() =>
                            {
                                MusicController.Instance.PlaySpecificSound(starSound);
                            })
                    );

                    sequence.AppendInterval(0.1f);
                }
            }
        }
    }
    
    private void SaveCompletedLevels()
    {
        string save = string.Join(",", completedLevels);
        PlayerPrefs.SetString(CompletedLevelsKey, save);
        PlayerPrefs.Save();
    }
    
    private void LoadCompletedLevels()
    {
        completedLevels.Clear();

        if (PlayerPrefs.HasKey(CompletedLevelsKey))
        {
            string save = PlayerPrefs.GetString(CompletedLevelsKey);
            string[] levels = save.Split(',');

            foreach (string s in levels)
            {
                if (int.TryParse(s, out int levelIndex))
                {
                    completedLevels.Add(levelIndex);
                }
            }
        }
    }
    
    private void ShowCompletedStars()
    {
        Transform[] allChildren = content.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < totalItems; i++)
        {
            if (!completedLevels.Contains(i)) continue;

            Transform levelItem = content.GetChild(i);

            Transform[] stars = levelItem.GetComponentsInChildren<Transform>(true);
            foreach (Transform star in stars)
            {
                if (star.CompareTag("Star"))
                {
                    star.localScale = Vector3.one;
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
