using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject _tutorialPanel;

    private void Start()
    {
        if (PlayerPrefs.HasKey("tutorial"))
        {
            int tutorialState = PlayerPrefs.GetInt("tutorial", 0);
            if (tutorialState == 1)
            {
                _tutorialPanel.SetActive(false);
            }
            else
            {
                _tutorialPanel.SetActive(true);
            }
        }
        else
        {
            _tutorialPanel.SetActive(true);
        }
    }

    public void CloseTutorial()
    {
        PlayerPrefs.SetInt("tutorial", 1);
        PlayerPrefs.Save();
    }
}
