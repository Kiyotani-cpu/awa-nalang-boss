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

    [Header("UI Components")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Text healthText; // Optional: Display health values

    [Header("Audio Settings")]
    [SerializeField] private AudioClip hurtSound; // Sound to play when player gets hurt
    private AudioSource audioSource;

    // NetworkVariable to synchronize health across clients
    private NetworkVariable<int> networkHealth = new NetworkVariable<int>(100);

    private PlayerCombatHandler _combatHandler;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            currentHealth = maxHealth;
            networkHealth.Value = currentHealth;
            _combatHandler = GetComponent<PlayerCombatHandler>();
            UpdateHealthUI();

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("AudioSource not found. Adding one.");
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        networkHealth.OnValueChanged += OnHealthChanged;

        PlayerInputControls inputControls = GetComponent<PlayerInputControls>();
        if (inputControls != null)
        {
            inputControls.OnDefendInput += StartDefending;
            inputControls.OnDefendInputCancelled += StopDefending;
        }
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        if (IsOwner)
        {
            currentHealth = newValue;
            UpdateHealthUI();
        }
    }

    public void TakeDamage(int damage)
    {
        if (!IsOwner) return;

        if (isDefending)
        {
            damage = Mathf.FloorToInt(damage * 0.5f);
        }

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        networkHealth.Value = currentHealth;

        PlayHurtSound(); // Play hurt sound when taking damage

        if (currentHealth <= 0)
        {
            Die();
        }

        UpdateHealthUI();
    }

    private void PlayHurtSound()
    {
        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }
    }

    private void UpdateHealthUI()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }

    private void Die()
    {
        Debug.Log("Player died!");
        RestartQuitCanvas.gameObject.SetActive(true);
    }

    public void StartDefending()
    {
        isDefending = true;
    }

    public void StopDefending()
    {
        isDefending = false;
    }

    public void RegenerateHealth(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        networkHealth.Value = currentHealth;
        UpdateHealthUI();
    }

    public override void OnNetworkDespawn()
    {
        networkHealth.OnValueChanged -= OnHealthChanged;
    }
}
