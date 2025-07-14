using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuLoaderController : MonoBehaviour
{
    public static MenuLoaderController Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void LoadMenuScene()
    {
        SceneManager.LoadScene("Levels");
    }

    public void LoadWinMenu()
    {
        string level = PlayerPrefs.GetString("currentLevel");
        PlayerPrefs.SetString("winLevel", level);
        PlayerPrefs.Save();
        
        SceneManager.LoadScene("Levels");
    }
}
