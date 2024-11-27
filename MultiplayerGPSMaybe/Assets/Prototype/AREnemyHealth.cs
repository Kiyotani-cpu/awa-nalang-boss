using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.XR.CoreUtils;
using UnityEngine;

public class AREnemyHealth : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>();

    public event Action OnEnemyDeath;
    public event Action<float> OnHealthChanged;

    [SerializeField] private GameObject deathEffect; // Reference to death effect prefab
    [SerializeField] private float effectDuration = 1f; // How long the effect should last

    private XROrigin xrCamera;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;

            // Find XR Origin Camera
            xrCamera = FindObjectOfType<XROrigin>();
            if (xrCamera != null)
            {
                // Make the enemy face the camera without changing its position
                FaceCamera();
            }
        }
    }

    private void FaceCamera()
    {
        if (xrCamera != null)
        {
            // Get the direction from the enemy to the camera
            Vector3 directionToCamera = xrCamera.transform.position - transform.position;

            // Remove any vertical difference to avoid tilting up/down (keeping the rotation only on Y-axis)
            directionToCamera.y = 0;

            // Calculate the rotation to look at the camera
            Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);

            // Apply the rotation to the enemy
            transform.rotation = targetRotation;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage)
    {
        if (!IsServer) return;

        currentHealth.Value -= damage;
        OnHealthChanged?.Invoke(currentHealth.Value);

        if (currentHealth.Value <= 0)
        {
            OnEnemyDeath?.Invoke();
            SpawnDeathEffectClientRpc();
            GetComponent<NetworkObject>().Despawn();
        }
    }

    [ClientRpc]
    private void SpawnDeathEffectClientRpc()
    {
        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, transform.position, transform.rotation);
            Destroy(effect, effectDuration);
        }
    }
}
