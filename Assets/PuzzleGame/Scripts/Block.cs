using System;
using UnityEngine;

public enum BlockType
{
    Dirt,
    Rock,
    Metal
}

public class Block : MonoBehaviour
{
    [SerializeField] private BlockType _blockType;
    
    private Renderer rend;
    private Material originalMat;
    public Material whiteFlashMaterial;

    private int _health;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        originalMat = rend.material;
    }

    private void Start()
    {
        switch (_blockType)
        {
            case BlockType.Dirt:
                _health = 1;
                break;
            case BlockType.Rock:
                _health = 3;
                break;
            case BlockType.Metal:
                _health = 1000;
                break;
        }
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
        FlashWhite();
        _health--;
    }
}
