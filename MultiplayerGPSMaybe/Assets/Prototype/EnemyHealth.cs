using Unity.Netcode;
using UnityEngine;

public class EnemyHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    // Reference to the Animator - you can drag and drop the Animator here
    [SerializeField] private Animator enemyAnimator;

    private bool isWalking = false;
    private bool isAttacking = false;

    private const string WALKING_ANIMATION = "IsWalking";  // Ensure this matches your parameter name in Animator
    private const string ATTACKING_ANIMATION = "IsAttacking";  // Ensure this matches your parameter name in Animator
    private const string DIE_ANIMATION = "IsDead";  // Ensure this matches your parameter name in Animator

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth = maxHealth;
        }

        // Ensure the animator reference is set in the inspector
        if (enemyAnimator == null)
        {
            Debug.LogError("Animator reference is not assigned! Please drag the Animator in the inspector.");
        }
    }

    public void TakeDamage(int damage)
    {
        if (IsServer)
        {
            currentHealth -= damage;

            // Trigger damage animations or transitions if required
            UpdateAnimationState();

            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    private void UpdateAnimationState()
    {
        // Only change the animation states if the animator is assigned
        if (enemyAnimator != null)
        {
            if (isWalking)
            {
                enemyAnimator.SetBool(WALKING_ANIMATION, true);
            }
            else
            {
                enemyAnimator.SetBool(WALKING_ANIMATION, false);
            }

            if (isAttacking)
            {
                enemyAnimator.SetTrigger(ATTACKING_ANIMATION);  // Assuming you are using a Trigger for attack
            }

            // You can also check for death and trigger the "Die" animation if needed
            if (currentHealth <= 0)
            {
                enemyAnimator.SetTrigger(DIE_ANIMATION);  // Trigger death animation
            }
        }
    }

    private void Die()
    {
        // Handle death logic (e.g., destroy the enemy, play death animation, etc.)
        Debug.Log("Enemy died!");
        Destroy(gameObject); // Destroy enemy or play death animation
    }

    public void StartWalking()
    {
        isWalking = true;
        UpdateAnimationState();
    }

    public void StopWalking()
    {
        isWalking = false;
        UpdateAnimationState();
    }

    public void StartAttacking()
    {
        isAttacking = true;
        UpdateAnimationState();
    }

    public void StopAttacking()
    {
        isAttacking = false;
        UpdateAnimationState();
    }

    public override void OnNetworkDespawn()
    {
        // Handle any cleanup or network despawn logic
    }
}
