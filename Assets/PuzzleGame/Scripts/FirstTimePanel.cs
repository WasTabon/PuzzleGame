using UnityEngine;
using UnityEngine.UI;

public class FirstTimePanel : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;
    public Button closeButton;

    private const string FirstLaunchKey = "FirstLaunchCompleted";

    void Start()
    {
        if (!PlayerPrefs.HasKey(FirstLaunchKey))
        {
            ShowPanel();
        }
        else
        {
            panel.SetActive(false);
        }

        closeButton.onClick.AddListener(OnCloseClicked);
    }

    void ShowPanel()
    {
        panel.SetActive(true);
    }

    void OnCloseClicked()
    {
        panel.SetActive(false);
        PlayerPrefs.SetInt(FirstLaunchKey, 1);
        PlayerPrefs.Save();
    }
}