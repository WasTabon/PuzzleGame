using System;
using PuzzleGame.Scripts;
using UnityEngine;

public enum BlockType
{
    Dirt,
    Rock,
    Metal
}

public class Block : MonoBehaviour
{
    [SerializeField] private GameObject _particle;
    public BlockType _blockType;
    [SerializeField] private int _health;
    [SerializeField] private AudioClip _hitSound;
    
    private Renderer rend;
    private Material originalMat;
    public Material whiteFlashMaterial;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        originalMat = rend.material;
    }

    public void FlashWhite(float duration = 0.05f)
    {
        if (whiteFlashMaterial == null) return;

        rend.material = whiteFlashMaterial;
        Invoke(nameof(RestoreOriginalMaterial), duration);
    }

    private void RestoreOriginalMaterial()
    {
        rend.material = originalMat;
    }

    public void Attack()
    {
        MusicController.Instance.PlaySpecificSound(_hitSound);
        FlashWhite();
        _health--;
        CheckBlockHealth();
    }

    private void CheckBlockHealth()
    {
        if (_health <= 0)
        {
            Instantiate(_particle, transform.position, Quaternion.identity);
            gameObject.SetActive(false);
        }
    }
}
