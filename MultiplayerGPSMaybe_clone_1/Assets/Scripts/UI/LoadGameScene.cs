using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadGameScene : MonoBehaviour
{
    void Awake()
    {
        // Disable this object's components first
        foreach (var component in GetComponents<MonoBehaviour>())
        {
            if (component != this)
            {
                component.enabled = false;
            }
        }
    }

    void Start()
    {
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        // Wait one frame to let everything settle
        yield return null;

        // Shutdown network if it exists
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        yield return null;

        // Find and destroy all NetworkObjects
        var networkObjects = FindObjectsOfType<NetworkObject>();
        foreach (var obj in networkObjects)
        {
            if (obj != null)
            {
                Destroy(obj.gameObject);
            }
        }

        // Find and destroy StartGameAR
        var startGameAR = FindObjectOfType<StartGameAR>();
        if (startGameAR != null)
        {
            Destroy(startGameAR.gameObject);
        }

        // Find and destroy NetworkManager
        var networkManager = FindObjectOfType<NetworkManager>();
        if (networkManager != null)
        {
            Destroy(networkManager.gameObject);
        }

        // Load the game scene
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}