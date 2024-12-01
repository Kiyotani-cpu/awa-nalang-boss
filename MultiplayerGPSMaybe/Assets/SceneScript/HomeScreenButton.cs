using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeScreen : MonoBehaviour
{
    public void PlayGame()
    {
        Debug.Log("Open NextScene");
        // Load the game scene
        SceneManager.LoadScene("LevelSelectScene");
    }


    public void ExitGame()
    {
        // Quit the application
        Debug.Log("Game exited!");
        Application.Quit();
    }
}
