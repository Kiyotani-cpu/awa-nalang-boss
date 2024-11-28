using UnityEngine;

public class EnemyBillboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // Get the main camera at the start
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Ensure the enemy always faces the camera (ignore vertical rotation)
        Vector3 directionToCamera = mainCamera.transform.position - transform.position;
        directionToCamera.y = 0; // Ignore vertical axis to prevent tilting

        // Rotate the enemy to look at the camera
        transform.rotation = Quaternion.LookRotation(directionToCamera);
    }
}
