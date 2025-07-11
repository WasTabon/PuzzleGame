using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using PuzzleGame.Scripts;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class SwipeController : MonoBehaviour
{
    [SerializeField] private GameObject pickaxeGameObject;
    
    [SerializeField] private GameObject ladderPrefab;
    
    [SerializeField] private List<AudioClip> _walkSoundsDirt;
    [SerializeField] private List<AudioClip> _walkSoundsRock;
    [SerializeField] private List<AudioClip> _walkSoundsMetal;
    
    [SerializeField] private AudioClip _landSoundDirt;
    [SerializeField] private AudioClip _landSoundRock;
    [SerializeField] private AudioClip _landSoundMetal;
    
    [SerializeField] private int _attackCount;
    
    [Header("Walk Step Particles")] public Transform stepPointLeft;
    public Transform stepPointRight;
    public GameObject stepParticlePrefab;

    public GameObject particle;
    public Transform spawnPoint;

    public Transform handTransform;
    public CameraShake cameraShake;

    public float swipeThreshold = 50f;
    public float moveSpeed = 5f;
    public Transform pickaxe;
    public GameObject attackEffectPrefab;

    private ParticlePool particlePool;
    
    private bool wasStandingOnBlock = false;

    private Vector2 startTouchPos;
    private bool isSwiping;
    private string currentDirection;

    private Block blockUnderPlayer;
    private Outline currentOutline;

    private Block currentHitBlock;

    private Animator animator;
    private Rigidbody rb;

    private bool isAttacking = false;
    private float attackCooldown = 1.5f;
    private float lastAttackTime = -Mathf.Infinity;

    private bool isGrounded = false;
    private bool isFalling = false;
    public float groundCheckDistance = 0.2f;

    public float effectOffsetX = 5f;

    private Tween moveTween;
    
    private bool isClimbingLadder = false;
    
    private bool _isNearLadder = false;
    
    private Tween rotationTween;
    private float ladderFaceY = 0f;
    private float normalFaceY = 180f;
    private float rotateDuration = 0.3f;

    private bool wasOnLadder = false;

    private int playerLayer;
    private int nonPlayerLayerMask;

    private float checkInterval = 0.1f;

    private Vector3 cachedAttackEffectOffset;
    
    private Vector3 ladderMoveDirection = Vector3.zero;

    private bool _isOnLadder;
    
    private bool attackEffectOffsetCached = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        particlePool = FindObjectOfType<ParticlePool>();

        playerLayer = LayerMask.NameToLayer("Player");
        nonPlayerLayerMask = ~(1 << playerLayer);

        InvokeRepeating(nameof(CheckBlockUnderPlayer), 0f, checkInterval);
        InvokeRepeating(nameof(CheckGrounded), 0f, checkInterval);
        
        UIController.Instance.SetAttacksText(_attackCount);
    }

    private void Update()
    {
        if (isFalling)
            return;

        if (_isOnLadder)
        {
            rb.useGravity = false;

            if (pickaxeGameObject != null && pickaxeGameObject.activeSelf)
                pickaxeGameObject.SetActive(false);

            // Движение по лестнице
            if (!string.IsNullOrEmpty(currentDirection))
            {
                ladderMoveDirection = Vector3.zero;

                switch (currentDirection)
                {
                    case "Up": ladderMoveDirection = Vector3.up; break;
                    case "Down": ladderMoveDirection = Vector3.down; break;
                    case "Left": ladderMoveDirection = Vector3.left; break;
                    case "Right": ladderMoveDirection = Vector3.right; break;
                }

                animator.SetBool("Ladder", false);
                animator.SetBool("LadderMove", true);

                // ✅ Только в момент начала движения вызываем поворот
                if (!isClimbingLadder && blockUnderPlayer == null)
                {
                    isClimbingLadder = true;
                    RotateToY(ladderFaceY);
                }
            }
            else
            {
                ladderMoveDirection = Vector3.zero;

                animator.SetBool("LadderMove", false);

                if (blockUnderPlayer == null)
                {
                    animator.SetBool("Ladder", true);
                }
                else
                {
                    animator.SetBool("Ladder", false);
                }
            }

            rb.velocity = ladderMoveDirection * (moveSpeed / 2);
        }
        else
        {
            rb.useGravity = true;

            if (pickaxeGameObject != null && !pickaxeGameObject.activeSelf)
                pickaxeGameObject.SetActive(true);

            animator.SetBool("Ladder", false);
            animator.SetBool("LadderMove", false);

            // ✅ Только в момент выхода с лестницы сбрасываем и разворачиваем
            if (isClimbingLadder)
            {
                isClimbingLadder = false;
                RotateToY(normalFaceY);
            }
        }
        
        bool isTouching = Input.touchCount > 0 || Input.GetMouseButton(0);
        Vector2 inputPos = Input.touchCount > 0 ? (Vector2)Input.GetTouch(0).position : (Vector2)Input.mousePosition;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
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
                StopAttack();
            }
        }
        else
        {
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
                StopAttack();
            }
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
                currentDirection = null;
                ResetAnimator();
                StopAttack();
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
            }
        }
        else
        {
            rb.velocity = new Vector3(rb.velocity.x * 0.9f, rb.velocity.y, 0);
            ResetAnimator();
            StopAttack();
        }
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.CompareTag("Ladder"))
        {
            _isNearLadder = true;
            // Можно показать UI, что можно начать лезть
            UIController.Instance.ShowLadderButton();
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.gameObject.CompareTag("Ladder"))
        {
            _isNearLadder = false;

            // Если игрок был на лестнице и вышел из зоны — сбросить состояние подъёма
            if (isClimbingLadder)
            {
                isClimbingLadder = false;
                // Возврат поворота при выходе из зоны лестницы
                RotateToY(normalFaceY);

                // Восстановить гравитацию и активировать пиксель
                rb.useGravity = true;

                if (pickaxeGameObject != null && !pickaxeGameObject.activeSelf)
                    pickaxeGameObject.SetActive(true);

                // Сброс анимаций лестницы
                animator.SetBool("Ladder", false);
                animator.SetBool("LadderMove", false);
            }

            // Скрыть UI
            UIController.Instance.HideLadderButton();
        }
    }
    
    private void RotateToY(float targetY)
    {
        if (rotationTween != null && rotationTween.IsActive())
            rotationTween.Kill();

        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(0f, targetY, 0f);

        rotationTween = DOTween.To(() => 0f, t =>
        {
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
        }, 1f, rotateDuration).SetEase(Ease.OutSine);
    }


    private string GetSwipeDirection(Vector2 delta)
    {
        return Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
            ? (delta.x > 0 ? "Right" : "Left")
            : (delta.y > 0 ? "Up" : "Down");
    }

    private void HandleSwipe(string direction)
    {
        if (_isOnLadder)
        {
            return;
        }

        if (direction == "Down")
        {
            TryAttackDownward();
            StopHorizontalMovement();
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
        if (Mathf.Approximately(rb.velocity.x, targetX))
            return;

        if (moveTween != null && moveTween.IsActive())
            moveTween.Kill();

        float startX = rb.velocity.x;
        float y = rb.velocity.y;

        moveTween = DOTween.To(() => startX, x =>
        {
            rb.velocity = new Vector3(x, y, 0);
            startX = x;
        }, targetX, 0.1f).SetEase(Ease.OutSine);
    }

    private void StopHorizontalMovement()
    {
        if (Mathf.Abs(rb.velocity.x) < 0.01f)
            return;

        if (moveTween != null && moveTween.IsActive())
            moveTween.Kill();

        float startX = rb.velocity.x;
        float y = rb.velocity.y;

        moveTween = DOTween.To(() => startX, x =>
        {
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

    public void PlaceLadder()
    {
        if (blockUnderPlayer != null)
        {
            // Получаем коллайдер блока
            Collider blockCollider = blockUnderPlayer.GetComponent<Collider>();
            if (blockCollider == null || ladderPrefab == null)
                return;

            // Верхняя точка блока по Y
            float blockTopY = blockCollider.bounds.max.y;

            // Центр блока по X
            float centerX = blockCollider.bounds.center.x;

            // Самая крайняя верхняя точка по Z (максимум Z)
            float frontZ = blockCollider.bounds.max.z;

            // Получаем высоту префаба лестницы
            Collider ladderCollider = ladderPrefab.GetComponent<Collider>();
            if (ladderCollider == null)
                return;

            float ladderHeight = ladderCollider.bounds.size.y;

            // Нижняя точка лестницы должна совпасть с верхом блока
            float ladderBottomY = blockTopY;
            float ladderCenterY = ladderBottomY + ladderHeight / 2f;

            Vector3 spawnPosition = new Vector3(centerX, ladderCenterY, frontZ);
            Quaternion spawnRotation = Quaternion.Euler(270f, 0f, 0f);

            Instantiate(ladderPrefab, spawnPosition, spawnRotation);
        }
        else if (_isOnLadder)
        {
            // Тут можно реализовать спуск или другой функционал
        }
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

        Vector3 rayOrigin = handTransform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 0.9f, nonPlayerLayerMask))
        {
            if (hit.collider.TryGetComponent(out Block block))
            {
                lastAttackTime = Time.time;

                if (!isAttacking)
                {
                    isAttacking = true;
                    animator.SetBool("Attack", true);
                }

                currentHitBlock = block;

                if (!attackEffectOffsetCached)
                {
                    Vector3 effectPos = block.transform.position;
                    cachedAttackEffectOffset = new Vector3(effectOffsetX, effectPos.y - pickaxe.position.y,
                        effectPos.z - pickaxe.position.z);
                    attackEffectOffsetCached = true;
                }
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
        if (pickaxe == null || currentHitBlock == null)
            return;

        if (!attackEffectOffsetCached)
        {
            // Получаем верхнюю точку блока по Y
            Collider blockCollider = currentHitBlock.GetComponent<Collider>();
            float blockTopY = currentHitBlock.transform.position.y;
            if (blockCollider != null)
                blockTopY += blockCollider.bounds.extents.y; // extents.y = половина высоты

            Vector3 blockTopPos = new Vector3(
                currentHitBlock.transform.position.x,
                blockTopY,
                currentHitBlock.transform.position.z);

            // Находим середину между pickaxe.position и верхней точкой блока
            Vector3 midPoint = (pickaxe.position + blockTopPos) * 0.5f;

            cachedAttackEffectOffset = new Vector3(
                midPoint.x - pickaxe.position.x,
                midPoint.y - pickaxe.position.y,
                midPoint.z - pickaxe.position.z);

            attackEffectOffsetCached = true;
        }

        transform.DOShakePosition(0.15f, new Vector3(0.1f, 0.1f, 0), 10, 90);

        Vector3 effectPos = pickaxe.position + cachedAttackEffectOffset;

        var effect = particlePool?.Spawn("attack", effectPos, Quaternion.identity);
        StartCoroutine(DoHitStop(0.05f));
        cameraShake.Shake(0.15f, 0.2f);

        if (currentHitBlock != null)
        {
            currentHitBlock.Attack();
            AnimateWaveFromBlock(currentHitBlock);
            currentHitBlock = null;
        }

        _attackCount--;
        CheckAttackCount();
    }

    public void CheckAttackCount()
    {
        UIController.Instance.SetAttacksText(_attackCount);
        if (_attackCount <= 0)
        {
            UIController.Instance.ShowPanel();
        }
    }

    private IEnumerator DoHitStop(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    public void SpawnParticleAtTransform()
    {
        if (spawnPoint != null)
        {
            Vector3 spawnPos = spawnPoint.position;
            Quaternion spawnRot = Quaternion.Euler(0, 0, -45);
            particlePool?.Spawn("swipeParticle", spawnPos, spawnRot);
        }
    }

    public void SpawnStepParticleLeft()
    {
        particlePool?.Spawn("stepLeft", stepPointLeft.position, stepPointLeft.rotation);
        
        PlayWalkSound();
    }

    public void SpawnStepParticleRight()
    {
        particlePool?.Spawn("stepRight", stepPointRight.position, stepPointRight.rotation);
        
        PlayWalkSound();
    }

    private void PlayWalkSound()
    {
        if (blockUnderPlayer == null)
            return;
        
        if (blockUnderPlayer._blockType == BlockType.Dirt)
        {
            int randomSound = Random.Range(0, _walkSoundsDirt.Count);
            MusicController.Instance.PlaySpecificSound(_walkSoundsDirt[randomSound]);
        }
        else if (blockUnderPlayer._blockType == BlockType.Rock)
        {
            int randomSound = Random.Range(0, _walkSoundsRock.Count);
            MusicController.Instance.PlaySpecificSound(_walkSoundsRock[randomSound]);
        }
        else if (blockUnderPlayer._blockType == BlockType.Metal)
        {
            int randomSound = Random.Range(0, _walkSoundsMetal.Count);
            MusicController.Instance.PlaySpecificSound(_walkSoundsMetal[randomSound]);
        }
    }

    private void CheckBlockUnderPlayer()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        bool isStandingNow = false;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 0.11f))
        {
            if (hit.collider.TryGetComponent(out Block block))
            {
                isStandingNow = true;

                if (block != blockUnderPlayer)
                {
                    DisableCurrentOutline();

                    blockUnderPlayer = block;
                    UIController.Instance.SetBlockText(block._blockType);
                    currentOutline = blockUnderPlayer.GetComponent<Outline>();
                    if (currentOutline != null)
                        currentOutline.enabled = true;
                }
            }
            else
            {
                DisableCurrentOutline();
                UIController.Instance.SetBlockText();
            }
        }
        else
        {
            DisableCurrentOutline();
            UIController.Instance.SetBlockText();
        }
        
        if (isStandingNow && !wasStandingOnBlock)
        {
            UIController.Instance.ShowLadderButton();
        }
        else if (!isStandingNow && wasStandingOnBlock)
        {
            UIController.Instance.HideLadderButton();
        }

        wasStandingOnBlock = isStandingNow;
    }


    private void DisableCurrentOutline()
    {
        if (blockUnderPlayer != null && currentOutline != null)
        {
            currentOutline.enabled = false;
            currentOutline = null;
            blockUnderPlayer = null;
        }
    }

    private void CheckGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        isGrounded = Physics.Raycast(origin, Vector3.down, groundCheckDistance, nonPlayerLayerMask);

        bool shouldFall = !isGrounded && rb.velocity.y < -0.1f;

        if (shouldFall && !isFalling)
        {
            isFalling = true;
            animator.SetBool("Fall", true);
            currentDirection = null;
            isSwiping = false;
            ResetAnimator();
            StopAttack();
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
        else if (!shouldFall && isFalling)
        {
            isFalling = false;
            animator.SetBool("Fall", false);
            PlayLandSound();
        }
    }

    private void AnimateWaveFromBlock(Block centerBlock)
    {
        Vector3 centerPos = centerBlock.transform.position;
        float spacing = 1f;
        float jumpHeight = 0.3f;
        float jumpDuration = 0.3f;
        float delayBetweenBlocks = 0.05f;

        List<Block> waveBlocks = new List<Block>();

        if (centerBlock != blockUnderPlayer)
            waveBlocks.Add(centerBlock);

        for (int i = 1; i <= 2; i++)
        {
            Vector3 leftPos = centerPos + Vector3.left * spacing * i;
            Block leftBlock = FindBlockAtPosition(leftPos);
            if (leftBlock != null && leftBlock != blockUnderPlayer)
                waveBlocks.Add(leftBlock);
            else if (leftBlock == null)
                break;
        }

        for (int i = 1; i <= 2; i++)
        {
            Vector3 rightPos = centerPos + Vector3.right * spacing * i;
            Block rightBlock = FindBlockAtPosition(rightPos);
            if (rightBlock != null && rightBlock != blockUnderPlayer)
                waveBlocks.Add(rightBlock);
            else if (rightBlock == null)
                break;
        }

        waveBlocks = waveBlocks.OrderBy(b => Vector3.Distance(centerPos, b.transform.position)).ToList();

        for (int i = 0; i < waveBlocks.Count; i++)
        {
            Block b = waveBlocks[i];
            float delay = i * delayBetweenBlocks;

            Transform t = b.transform;
            Vector3 originalPos = t.position;

            t.DOMoveY(originalPos.y + jumpHeight, jumpDuration / 2)
                .SetDelay(delay)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => { t.DOMoveY(originalPos.y, jumpDuration / 2).SetEase(Ease.InQuad); });
        }
    }
    
    private void PlayLandSound()
    {
        if (blockUnderPlayer == null)
            return;

        AudioClip clip = null;

        switch (blockUnderPlayer._blockType)
        {
            case BlockType.Dirt:
                clip = _landSoundDirt;
                break;
            case BlockType.Rock:
                clip = _landSoundRock;
                break;
            case BlockType.Metal:
                clip = _landSoundMetal;
                break;
        }

        if (clip != null)
            MusicController.Instance.PlaySpecificSound(clip);
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

    private void OnDrawGizmos()
    {
        if (handTransform != null)
        {
            float rayDistance = 0.9f;
            Vector3 rayDirection = Vector3.down;
            Vector3 rayOrigin = handTransform.position + Vector3.up * 0.1f;
        
            Gizmos.color = Color.green;
            Gizmos.DrawRay(rayOrigin, rayDirection * rayDistance);
        }

        {
            float checkDistance = 0.11f;
            Vector3 origin = transform.position + Vector3.up * 0.1f;

            Gizmos.color = Color.yellow; // Отличающийся цвет
            Gizmos.DrawRay(origin, Vector3.down * checkDistance);
        }
    }
}