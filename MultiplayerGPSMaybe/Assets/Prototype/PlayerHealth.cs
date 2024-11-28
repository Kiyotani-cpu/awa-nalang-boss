using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Canvas RestartQuitCanvas;
    private int currentHealth;

    private bool isDefending = false;

    [SerializeField] private HealthBar healthBar; // Reference to UI health bar (optional)

    // NetworkVariable to synchronize health across clients
    private NetworkVariable<int> networkHealth = new NetworkVariable<int>(100);

    private PlayerCombatHandler _combatHandler;

    // Event to notify when the player is killed
    public static event Action<ulong> OnKillPlayer;

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

        // Subscribe to the defend input events
        PlayerInputControls inputControls = GetComponent<PlayerInputControls>();
        if (inputControls != null)
        {
            inputControls.OnDefendInput += StartDefending;
            inputControls.OnDefendInputCancelled += StopDefending;
        }
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

        Debug.Log($"Original damage: {damage}");

        if (isDefending)
        {
            // If the player is defending, reduce the damage (shielding effect)
            damage = Mathf.FloorToInt(damage * 0.5f); // 50% damage reduction when defending
            Debug.Log($"Shielding active! Reduced damage: {damage}");
        }
        else
        {
            Debug.Log("No shield active, damage remains the same.");
        }

        // Apply the damage to current health
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health doesn't go below 0

        // Sync health change with the network
        networkHealth.Value = currentHealth;

        Debug.Log($"Current health after damage: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Handle player death logic (e.g., respawn, game over, etc.)
        Debug.Log("Player died!");

        RestartQuitCanvas.gameObject.SetActive(true);
    }

    public void StartDefending()
    {
        isDefending = true;
        Debug.Log("Player is defending: " + isDefending); // Debugging the defense state
    }

    public void StopDefending()
    {
        isDefending = false;
        Debug.Log("Player stopped defending: " + isDefending); // Debugging the defense state
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
