using UnityEngine;
using UnityEngine.UI;

public class AudioToggleButton : MonoBehaviour
{
    [SerializeField] private Button toggleButton;   // Reference to the button
    [SerializeField] private Sprite muteIcon;       // Mute icon sprite
    [SerializeField] private Sprite unmuteIcon;     // Unmute icon sprite
    [SerializeField] private Image buttonImage;     // Image component to change icon

    private bool isMuted = false;                   // Track current mute state

    private void Start()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleAudio);  // Add button click listener
        }

        // Initialize the button icon and audio state
        isMuted = AudioListener.volume == 0;
        UpdateButtonIcon();
    }

    private void ToggleAudio()
    {
        isMuted = !isMuted;  // Toggle mute state

        // Mute or unmute all audio in the game
        AudioListener.volume = isMuted ? 0 : 1;

        UpdateButtonIcon();  // Update button sprite
    }

    private void UpdateButtonIcon()
    {
        if (buttonImage != null)
        {
            buttonImage.sprite = isMuted ? muteIcon : unmuteIcon;  // Change the sprite based on state
        }
    }

    private void OnDestroy()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveListener(ToggleAudio);  // Remove listener on destroy
        }
    }
}
