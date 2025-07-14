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
    
    
}
