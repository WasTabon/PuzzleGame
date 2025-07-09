using UnityEngine;

public class Block : MonoBehaviour
{
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
        FlashWhite(); // <- вставим сюда, если хочешь миг при любом ударе
        // Логика разрушения и т.п.
    }
}
