using UnityEngine;
using Unity.Netcode;

public class PlayerCameraSetup : NetworkBehaviour
{
    private Camera playerCamera;
    private GameObject cameraObject;
    private Vector3 positionOffset = new Vector3(0, 1, 0); // Adjust as needed for camera positioning
    private Vector3 rotationOffset = new Vector3(0, 0, 0); // Adjust as needed for camera rotation

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Only initialize for the local player
        if (IsOwner)
        {
            // Find the camera in the hierarchy (child of a child in your case)
            cameraObject = transform.Find("XROrigin/Main Camera").gameObject; // Adjust based on your hierarchy
            playerCamera = cameraObject.GetComponent<Camera>();
            playerCamera.enabled = true;

            // Optionally, disable other cameras in the scene for non-local players
            DisableOtherCameras();
        }
        else
        {
            // Disable the camera for non-local players
            cameraObject = transform.Find("XROrigin/Main Camera").gameObject; // Adjust based on your hierarchy
            cameraObject.SetActive(false);
        }
    }

    private void DisableOtherCameras()
    {
        Camera[] allCameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in allCameras)
        {
            if (cam.gameObject != playerCamera.gameObject)
            {
                cam.enabled = false;
            }
        }
    }

    void Update()
    {
        if (!IsOwner || playerCamera == null) return;

        // Update the local player's position to match the camera's position, with the offset
        Vector3 cameraPosition = playerCamera.transform.position + positionOffset;
        transform.position = cameraPosition;

        // Update the local player's rotation to match the camera's rotation, with the offset
        Vector3 cameraEuler = playerCamera.transform.eulerAngles;
        transform.eulerAngles = new Vector3(cameraEuler.x, cameraEuler.y, 0) + rotationOffset;
    }
}
