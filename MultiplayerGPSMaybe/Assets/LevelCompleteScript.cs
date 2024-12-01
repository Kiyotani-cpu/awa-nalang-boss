using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelComplete : MonoBehaviour
{
    // This method will be called when the "Select Level" button is pressed
    public void OnSelectLevel()
    {
        SceneManager.LoadScene("LevelSelectScene"); // Go to the Level Select Scene
    }

    // This method will be called when the "Quit" button is pressed
    public void OnQuit()
    {
        GameManager.Instance.LoadSelectedLevel(); // Quit the game
    }
}
