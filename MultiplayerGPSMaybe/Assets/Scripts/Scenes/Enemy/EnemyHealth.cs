using UnityEngine;
using Unity.Netcode;

public class EnemyHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;   // Set maximum health in the Inspector
    private NetworkVariable<int> currentHealth;     // Sync health across the network

    private void Awake()
    {
        // Initialize the health NetworkVariable with the max health value
        currentHealth = new NetworkVariable<int>(maxHealth);
    }

    public override void OnNetworkSpawn()
    {
        // Reset health to max if this enemy is respawning
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }

        // Subscribe to health changes for updating on all clients
        currentHealth.OnValueChanged += OnHealthChanged;
    }

    private void OnHealthChanged(int oldHealth, int newHealth)
    {
        if (newHealth <= 0)
        {
            // Enemy dies when health reaches zero
            Die();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        // Only reduce health if the enemy is alive and the server owns this
        if (currentHealth.Value > 0)
        {
            currentHealth.Value = Mathf.Max(currentHealth.Value - damage, 0);  // Ensure health does not go below zero
        }
    }

    private void Die()
    {
        // Logic for when the enemy dies, e.g., play animations, disable components, etc.
        Debug.Log("Enemy died");

        // Example: Destroy the enemy object
        if (IsServer)
        {
            NetworkObject.Despawn();   // Despawn the object across the network
        }
    }

    // Method to handle bullet collisions
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            // Assume bullet has a BulletData script with a damage property
            int damage = other.GetComponent<BulletData>().GetDamage();

            // Call the ServerRpc to apply damage
            TakeDamageServerRpc(damage);

            // Destroy bullet on contact
            Destroy(other.gameObject);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from health changes to avoid memory leaks
        currentHealth.OnValueChanged -= OnHealthChanged;
    }
}
