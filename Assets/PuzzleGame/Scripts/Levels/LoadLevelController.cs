using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevelController : MonoBehaviour
{
    public void LoadLevel(string level)
    {
        PlayerPrefs.SetString("currentLevel", level);
        PlayerPrefs.Save();
        SceneManager.LoadScene(level);
    }
}
