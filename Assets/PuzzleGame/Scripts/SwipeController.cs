using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SwipeController : MonoBehaviour
{
    public float swipeThreshold = 50f;
    public float moveSpeed = 5f;
    public Transform pickaxe;
    public GameObject attackEffectPrefab;

    private Vector2 startTouchPos;
    private bool isSwiping;
    private string currentDirection;

    private Animator animator;
    private Rigidbody rb;

    private bool isAttacking = false;
    private float attackCooldown = 1.8f;
    private float lastAttackTime = -Mathf.Infinity;
    
    public float effectOffsetX = 5f;

    private Vector3 lastHitPoint;
    
    private Vector3 blockTopPoint;

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
            // Запускаем или продолжаем атаку вниз
            TryAttackDownward();
            rb.velocity = new Vector3(0, rb.velocity.y, 0); // Остановить движение по X во время атаки
        }
        else
        {
            // Сбросим атаку, если был свайп вниз, а теперь движение в другую сторону
            StopAttack();

            ResetAnimator();

            switch (direction)
            {
                case "Left":
                    animator.SetBool("MoveLeft", true);
                    rb.velocity = new Vector3(-moveSpeed, rb.velocity.y, 0);
                    break;

                case "Right":
                    animator.SetBool("MoveRight", true);
                    rb.velocity = new Vector3(moveSpeed, rb.velocity.y, 0);
                    break;

                default:
                    rb.velocity = new Vector3(0, rb.velocity.y, 0);
                    break;
            }
        }
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

                block.Attack();

                // Сохраняем верхнюю точку блока для спавна эффекта
                blockTopPoint = hit.collider.bounds.max;

                // Сохраняем позицию по X и Z для расчета в SpawnAttackEffect
                // lastHitPoint больше не нужен, можно удалить или не использовать
            }
            else
            {
                StopAttack();
            }
        }
        else
        {
            StopAttack();
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

    // Этот метод вызывается из Animation Event (без параметров!)
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
        }
    }

    private void OnDrawGizmos()
    {
        float rayDistance = 20f;
        Vector3 rayDirection = Vector3.down;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, rayDirection * rayDistance);
    }
}
