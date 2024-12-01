using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int selectedLevel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // Prevent duplicate GameManagers.
        }
    }

    public void SelectLevel(int level)
    {
        selectedLevel = level;
        LoadLobbyForLevel(level);
    }

    private void LoadLobbyForLevel(int level)
    {
        SceneManager.LoadScene($"GameSceneLevel{level}"); // Loads lobby specific to the selected level
    }

    public void LoadSelectedLevel()
    {
        SceneManager.LoadScene("HomeScene"); // Load gameplay scene
    }

    public void LoadLevelCompleteScene()
    {
        SceneManager.LoadScene("Victory");
    }

    public void LoadLevelSelectScene()
    {
        SceneManager.LoadScene("HomeScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
