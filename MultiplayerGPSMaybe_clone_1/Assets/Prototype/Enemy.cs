using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.XR.CoreUtils;
using UnityEngine;

public class EnemyHealth1 : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>();

    public event Action OnEnemyDeath;
    public event Action<float> OnHealthChanged;

    [SerializeField] private float spawnDistance = 2f; // Distance in front of camera to spawn
    [SerializeField] private GameObject deathEffect; // Reference to death effect prefab
    [SerializeField] private float effectDuration = 1f; // How long the effect should last

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;

            // Find XR Origin Camera
            var xrCamera = FindObjectOfType<XROrigin>();
            if (xrCamera != null)
            {
                // Calculate spawn position in front of camera
                Vector3 spawnPosition = xrCamera.transform.position + (xrCamera.transform.forward * spawnDistance);
                transform.position = spawnPosition;

                // Make enemy face the camera
                transform.LookAt(xrCamera.transform);
            }
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


