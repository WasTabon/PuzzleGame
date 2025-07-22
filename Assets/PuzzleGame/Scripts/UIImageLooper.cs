using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class UIImageLooper : MonoBehaviour
{
    public bool moveLeft;
    public bool moveRight;
    public bool moveUp;
    public bool moveDown;

    public float moveDuration = 1f;

    private RectTransform rectTransform;
    private Vector2 startPosition;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;

        StartLoop();
    }

    private void StartLoop()
    {
        Vector2 targetPosition = startPosition;

        if (moveLeft)
            targetPosition += Vector2.left * rectTransform.rect.width;
        else if (moveRight)
            targetPosition += Vector2.right * rectTransform.rect.width;
        else if (moveUp)
            targetPosition += Vector2.up * rectTransform.rect.height;
        else if (moveDown)
            targetPosition += Vector2.down * rectTransform.rect.height;
        else
            return;

        LoopMove(targetPosition);
    }

    private void LoopMove(Vector2 target)
    {
        rectTransform.DOAnchorPos(target, moveDuration).SetEase(Ease.Linear).OnComplete(() =>
        {
            rectTransform.anchoredPosition = startPosition;
            LoopMove(target);
        });
    }
}
