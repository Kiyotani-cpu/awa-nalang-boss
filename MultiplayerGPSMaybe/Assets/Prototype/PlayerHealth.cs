using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    private bool isDefending = false;

    [SerializeField] private HealthBar healthBar; // Reference to UI health bar (optional)

    // NetworkVariable to synchronize health across clients
    private NetworkVariable<int> networkHealth = new NetworkVariable<int>(100);

    private PlayerCombatHandler _combatHandler;

    public override void OnNetworkSpawn()
    {
        if (IsOwner) // Only the owning player should manage their health
        {
            currentHealth = maxHealth;
            networkHealth.Value = currentHealth; // Sync health on spawn
            _combatHandler = GetComponent<PlayerCombatHandler>();
        }

        // Register to listen to health changes from the network
        networkHealth.OnValueChanged += OnHealthChanged;
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        // Update health on the UI for the player
        if (healthBar != null)
        {
            healthBar.SetHealth(newValue, maxHealth);
        }
    }

    public void TakeDamage(int damage)
    {
        if (!IsOwner) return; // Ensure only the owning player can take damage

        if (isDefending)
        {
            // If the player is defending, reduce the damage (shielding effect)
            damage = Mathf.FloorToInt(damage * 0.5f); // 50% damage reduction when defending
        }

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health doesn't go below 0

        // Sync health change with the network
        networkHealth.Value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Handle player death logic (e.g., respawn, game over, etc.)
        Debug.Log("Player died!");
        // Optionally, you can trigger a respawn or end the game
        // Example: respawn the player after a delay or display a Game Over screen
    }

    public void StartDefending()
    {
        isDefending = true;
    }

    public void StopDefending()
    {
        isDefending = false;
    }

    // You can also add a method to regenerate health over time (if needed)
    public void RegenerateHealth(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        // Sync health change with the network
        networkHealth.Value = currentHealth;
    }

    public override void OnNetworkDespawn()
    {
        // Handle cleanup if necessary
        networkHealth.OnValueChanged -= OnHealthChanged; // Unsubscribe from the event
    }
}
