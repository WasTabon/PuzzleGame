using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RenameScrollViewItems : MonoBehaviour
{
    [SerializeField] private Transform contentTransform;
    [SerializeField] private GridLayoutGroup gridLayoutGroup; // Привяжи сюда компонент
    [SerializeField] private Canvas canvas; // Привяжи Canvas, если не найдём — найдём сами

    private void Start()
    {
        AdjustCellWidthToScreen();
        RenameLevels();
    }

    private void AdjustCellWidthToScreen()
    {
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        // Получаем ширину экрана в UI-пикселях
        float screenWidth = Screen.width;

        // Переводим в канвас-единицы (если Canvas в режиме Scale With Screen Size)
        float scaleFactor = canvas.scaleFactor;
        float canvasWidth = screenWidth / scaleFactor;

        // Устанавливаем ширину ячеек по ширине экрана
        Vector2 newCellSize = gridLayoutGroup.cellSize;
        newCellSize.x = canvasWidth - gridLayoutGroup.padding.left - gridLayoutGroup.padding.right;

        gridLayoutGroup.cellSize = newCellSize;
    }

    public void RenameLevels()
    {
        int childCount = contentTransform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = contentTransform.GetChild(i);
            TextMeshProUGUI tmp = child.GetComponentInChildren<TextMeshProUGUI>();

            if (tmp != null)
            {
                tmp.text = $"LEVEL {i + 1}";
            }
        }
    }
}