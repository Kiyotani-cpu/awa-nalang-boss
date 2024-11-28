using UnityEngine;
using UnityEngine.UI;

public class ToggleCanvasVisibility : MonoBehaviour
{
    [SerializeField] private Canvas QuitRestartButtons; // Reference to the canvas
    [SerializeField] private Button toggleButton;      // Reference to the button

    private bool isCanvasVisible = false;             // Track the canvas visibility state

    private void Start()
    {
        if (toggleButton != null)
        {
            // Add a listener to the button click event
            toggleButton.onClick.AddListener(ToggleCanvas);
        }

        if (QuitRestartButtons != null)
        {
            QuitRestartButtons.gameObject.SetActive(isCanvasVisible);
        }
    }

    private void ToggleCanvas()
    {
        isCanvasVisible = !isCanvasVisible; // Toggle the visibility state
        QuitRestartButtons.gameObject.SetActive(isCanvasVisible); // Set canvas active or inactive based on state
    }

    private void OnDestroy()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveListener(ToggleCanvas); // Cleanup listener to avoid memory leaks
        }
    }
}
