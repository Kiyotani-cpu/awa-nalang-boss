using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoaderButton : MonoBehaviour
{
    [SerializeField] private Button sceneLoadButton;  // Reference to the button
    [SerializeField] private string sceneName;        // Name of the scene to load

    private void Start()
    {
        if (sceneLoadButton != null)
        {
            sceneLoadButton.onClick.AddListener(LoadScene);  // Add click listener to button
        }
    }

    private void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);  // Load the specified scene
        }
        else
        {
            Debug.LogWarning("Scene name is empty! Please set a valid scene name.");
        }
    }

    private void OnDestroy()
    {
        if (sceneLoadButton != null)
        {
            sceneLoadButton.onClick.RemoveListener(LoadScene);  // Remove listener on destroy
        }
    }
}
