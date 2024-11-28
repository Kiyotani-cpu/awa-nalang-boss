using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class EnemyHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>();  // Initialize here
    [SerializeField] private float delay = 1.51f;
    [SerializeField] private int Damage = 10;
    // Health Bar Reference
    [SerializeField] private Image healthBarFill;  // Reference to the UI Image (fill)

    // Reference to the Animator
    [SerializeField] private Animator enemyAnimator;

    // Movement settings
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackRange = 2f;
    private Transform playerTransform;

    // Combat settings
    private bool isWalking = false;
    private bool isAttacking = false;
    private bool isDead = false;

    private const string WALKING_ANIMATION = "IsWalking";
    private const string ATTACKING_ANIMATION = "IsAttacking";
    private const string DIE_ANIMATION = "IsDead";
    private const string IDLE_ANIMATION = "IsIdle";

    // Attack settings
    [SerializeField] private float attackCooldown = 1f;
    private float lastAttackTime = -1f;

    // Minimum distance to stay when attacking
    [SerializeField] private float minimumAttackDistance = 2f;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;  // Initialize health for the server
        }

        // Ensure the animator reference is set in the inspector
        if (enemyAnimator == null)
        {
            Debug.LogError("Animator reference is not assigned! Please drag the Animator in the inspector.");
        }

        // Set up the player reference
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        UpdateAnimationState();  // Initialize animation state
    }

    private void Update()
    {
        if (isDead) return;  // Prevent further logic if the enemy is dead

        // Update health bar fill
        UpdateHealthBar();

        if (playerTransform != null)
        {
            FollowPlayer();

            if (Vector3.Distance(transform.position, playerTransform.position) <= attackRange &&
                Time.time >= lastAttackTime + attackCooldown)
            {
                if (!isAttacking)
                {
                    StartAttacking();
                }
            }
            else
            {
                StopAttacking();
            }
        }
    }

    private void FollowPlayer()
    {
        if (!isAttacking && !isDead)  // Prevent movement if the enemy is dead
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer > minimumAttackDistance)
            {
                transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
            }

            isWalking = true;
            UpdateAnimationState();
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;  // Prevent damage if the enemy is already dead

        if (IsServer)
        {
            currentHealth.Value -= damage;

            if (currentHealth.Value > 0)
            {
                UpdateAnimationState();
            }
            else
            {
                Die();
            }
        }
    }

    private void UpdateAnimationState()
    {
        if (enemyAnimator != null && !isDead)
        {
            enemyAnimator.SetBool(WALKING_ANIMATION, isWalking);
            enemyAnimator.SetBool(ATTACKING_ANIMATION, isAttacking);
            enemyAnimator.SetBool(IDLE_ANIMATION, !isWalking && !isAttacking && currentHealth.Value > 0);

            if (currentHealth.Value <= 0 && !isDead)
            {
                isDead = true;
                enemyAnimator.SetTrigger(DIE_ANIMATION);
            }
        }
    }

    private void Die()
    {
        Debug.Log("Enemy died!");

        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger(DIE_ANIMATION);  // Trigger death animation
        }

        isWalking = false;
        isAttacking = false;

        // Optionally disable colliders, rigidbody or other interactions
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        Destroy(gameObject, delay);  // Adjust the delay to match the length of your death animation
    }

    public void StartWalking()
    {
        if (!isDead)
        {
            isWalking = true;
            UpdateAnimationState();
        }
    }

    public void StopWalking()
    {
        if (!isDead)
        {
            isWalking = false;
            UpdateAnimationState();
        }
    }

    public void StartAttacking()
    {
        if (!isDead)  // Prevent attacking if dead
        {
            isAttacking = true;
            UpdateAnimationState();

            if (playerTransform != null)
            {
                if (Vector3.Distance(transform.position, playerTransform.position) <= attackRange)
                {
                    playerTransform.GetComponent<PlayerHealth>().TakeDamage(Damage);
                    lastAttackTime = Time.time;
                }
            }
        }
    }

    public void StopAttacking()
    {
        if (!isDead)
        {
            isAttacking = false;
            UpdateAnimationState();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth.Value / maxHealth;
        }
    }

    public override void OnNetworkDespawn()
    {
        // Handle any cleanup or network despawn logic
    }
}
