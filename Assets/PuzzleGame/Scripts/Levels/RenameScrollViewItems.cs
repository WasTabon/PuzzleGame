using UnityEngine;
using TMPro;

public class RenameScrollViewItems : MonoBehaviour
{
    [SerializeField] private Transform contentTransform;

    private void Start()
    {
        RenameLevels();
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
                tmp.text = $"Level {i + 1}";
            }
        }
    }
}