using UnityEngine;
using UnityEngine.UI;

public class ButtonActivationScript : MonoBehaviour
{
    [SerializeField] private GameObject objectToActivate; // Object to be activated when the button is clicked
    [SerializeField] private GameObject canvasToRemove;   // The canvas that will be removed when the button is clicked

    [Header("Button References")]
    [SerializeField] private Button nextLevelButton;      // Next Level Button reference
    [SerializeField] private Button exitButton;           // Exit Button reference

    private void Start()
    {
        // Assign listeners to buttons
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }
    }

    // This method will be called when the "Next Level" button is clicked
    private void OnNextLevelButtonClicked()
    {
        // Activate the object
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);  // Activate the object
        }

        // Remove the canvas (deactivate it)
        if (canvasToRemove != null)
        {
            canvasToRemove.SetActive(false);  // Deactivate the canvas
        }
    }

    // This method will be called when the "Exit" button is clicked
    private void OnExitButtonClicked()
    {
        // Quit the application
        Debug.Log("Exiting the game...");
        Application.Quit();

        // In the editor, stop the play mode
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

