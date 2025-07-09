using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;


[RequireComponent(typeof(Rigidbody))]
public class SwipeController : MonoBehaviour
{
    public CameraShake cameraShake;
    
    public float swipeThreshold = 50f;
    public float moveSpeed = 5f;
    public Transform pickaxe;
    public GameObject attackEffectPrefab;

    private Vector2 startTouchPos;
    private bool isSwiping;
    private string currentDirection;

    private Block currentHitBlock;
    
    private Animator animator;
    private Rigidbody rb;

    private bool isAttacking = false;
    private float attackCooldown = 1.8f;
    private float lastAttackTime = -Mathf.Infinity;
    
    public float effectOffsetX = 5f;

    private Vector3 lastHitPoint;
    
    private Vector3 blockTopPoint;
    
    private Tween moveTween;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Vector2 inputPos = Vector2.zero;
        bool isTouching = false;

        // Mouse input
        if (Input.GetMouseButtonDown(0))
        {
            isSwiping = true;
            startTouchPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isSwiping = false;
            currentDirection = null;
            ResetAnimator();
            StopAttack();  // остановить атаку при отпускании свайпа
        }
        isTouching = Input.GetMouseButton(0);
        if (isTouching)
            inputPos = Input.mousePosition;

        // Touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            isTouching = true;

            if (touch.phase == TouchPhase.Began)
            {
                isSwiping = true;
                startTouchPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isSwiping = false;
                currentDirection = null;
                ResetAnimator();
                StopAttack();  // остановить атаку при отпускании свайпа
            }

            inputPos = touch.position;
        }

        if (isSwiping && isTouching)
        {
            Vector2 swipeDelta = inputPos - startTouchPos;

            if (swipeDelta.magnitude > swipeThreshold)
            {
                string newDirection = GetSwipeDirection(swipeDelta);

                if (newDirection != currentDirection)
                {
                    currentDirection = newDirection;
                }

                HandleSwipe(currentDirection);
            }
            else
            {
                // Если свайп не превышает порог — сброс движения и атаки
                currentDirection = null;
                ResetAnimator();
                StopAttack();
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
            }
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0); // Остановить по X
            ResetAnimator();
            StopAttack();
        }
    }

    private string GetSwipeDirection(Vector2 delta)
    {
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            return delta.x > 0 ? "Right" : "Left";
        else
            return delta.y > 0 ? "Up" : "Down";
    }

    private void HandleSwipe(string direction)
    {
        if (direction == "Down")
        {
            TryAttackDownward();
            StopHorizontalMovement(); // плавно остановить движение
        }
        else
        {
            StopAttack();
            ResetAnimator();

            switch (direction)
            {
                case "Left":
                    animator.SetBool("MoveLeft", true);
                    AccelerateToDirection(-moveSpeed);
                    break;

                case "Right":
                    animator.SetBool("MoveRight", true);
                    AccelerateToDirection(moveSpeed);
                    break;

                default:
                    StopHorizontalMovement();
                    break;
            }
        }
    }
    
    private void AccelerateToDirection(float targetX)
    {
        if (moveTween != null && moveTween.IsActive())
            moveTween.Kill();

        float startX = rb.velocity.x;
        float y = rb.velocity.y;

        moveTween = DOTween.To(() => startX, x => {
            rb.velocity = new Vector3(x, y, 0);
            startX = x; // обновляем текущую скорость, чтобы tween продолжал корректно
        }, targetX, 0.1f).SetEase(Ease.OutSine);
    }

    private void StopHorizontalMovement()
    {
        if (moveTween != null && moveTween.IsActive())
            moveTween.Kill();

        float startX = rb.velocity.x;
        float y = rb.velocity.y;

        moveTween = DOTween.To(() => startX, x => {
            rb.velocity = new Vector3(x, y, 0);
            startX = x;
        }, 0f, 0.1f).SetEase(Ease.OutSine);
    }

    private void ResetAnimator()
    {
        animator.SetBool("MoveLeft", false);
        animator.SetBool("MoveRight", false);
        animator.SetBool("Attack", false);
    }

    private void TryAttackDownward()
    {
        if (Time.time < lastAttackTime + attackCooldown)
        {
            if (!isAttacking)
            {
                animator.SetBool("Attack", true);
                isAttacking = true;
            }
            return;
        }

        int playerLayer = LayerMask.NameToLayer("Player");
        int layerMask = ~(1 << playerLayer);
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 20f, layerMask))
        {
            if (hit.collider.TryGetComponent(out Block block))
            {
                lastAttackTime = Time.time;

                if (!isAttacking)
                {
                    isAttacking = true;
                    animator.SetBool("Attack", true);
                }

                // Сохраняем блок для последующей атаки в момент эффекта
                currentHitBlock = block;

                // Сохраняем верхнюю точку блока для позиционирования эффекта
                blockTopPoint = hit.collider.bounds.max;
            }
            else
            {
                StopAttack();
                currentHitBlock = null;
            }
        }
        else
        {
            StopAttack();
            currentHitBlock = null;
        }
    }

    private void StopAttack()
    {
        if (isAttacking)
        {
            isAttacking = false;
            animator.SetBool("Attack", false);
        }
    }

    public void SpawnAttackEffect()
    {
        if (attackEffectPrefab != null && pickaxe != null)
        {
            Vector3 effectPos = new Vector3(
                pickaxe.position.x + effectOffsetX,
                (pickaxe.position.y + blockTopPoint.y) / 2f,
                (pickaxe.position.z + blockTopPoint.z) / 2f
            );

            Debug.Log("Spawn effect at " + effectPos);
            Instantiate(attackEffectPrefab, effectPos, Quaternion.identity);

            cameraShake.Shake(0.15f, 0.2f);

            if (currentHitBlock != null)
            {
                currentHitBlock.Attack();
                AnimateWaveFromBlock(currentHitBlock);
                currentHitBlock = null; // сбрасываем после использования
            }
        }
    }

    private void OnDrawGizmos()
    {
        float rayDistance = 20f;
        Vector3 rayDirection = Vector3.down;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, rayDirection * rayDistance);
    }
    
    private void AnimateWaveFromBlock(Block centerBlock)
    {
        Vector3 centerPos = centerBlock.transform.position;
        float spacing = 1f; // предполагаем, что блоки расположены через 1 юнит по X
        float jumpHeight = 0.3f;
        float jumpDuration = 0.3f;
        float delayBetweenBlocks = 0.05f;

        List<Block> waveBlocks = new List<Block> { centerBlock };

        // Добавляем до 2 блоков влево
        for (int i = 1; i <= 2; i++)
        {
            Vector3 leftPos = centerPos + Vector3.left * spacing * i;
            Block leftBlock = FindBlockAtPosition(leftPos);
            if (leftBlock != null)
                waveBlocks.Add(leftBlock);
            else
                break; // если нет блока — дальше не ищем
        }

        // Добавляем до 2 блоков вправо
        for (int i = 1; i <= 2; i++)
        {
            Vector3 rightPos = centerPos + Vector3.right * spacing * i;
            Block rightBlock = FindBlockAtPosition(rightPos);
            if (rightBlock != null)
                waveBlocks.Add(rightBlock);
            else
                break;
        }

        // Сортируем блоки по расстоянию до центра (чтобы применить задержку)
        waveBlocks = waveBlocks.OrderBy(b => Vector3.Distance(centerPos, b.transform.position)).ToList();

        for (int i = 0; i < waveBlocks.Count; i++)
        {
            Block b = waveBlocks[i];
            float delay = i * delayBetweenBlocks;

            Transform t = b.transform;
            Vector3 originalPos = t.position;

            // Подпрыгивание
            t.DOMoveY(originalPos.y + jumpHeight, jumpDuration / 2)
                .SetDelay(delay)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    t.DOMoveY(originalPos.y, jumpDuration / 2).SetEase(Ease.InQuad);
                });
        }
    }
    
    private Block FindBlockAtPosition(Vector3 pos)
    {
        Collider[] hits = Physics.OverlapSphere(pos, 0.1f);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out Block block))
                return block;
        }
        return null;
    }
}
