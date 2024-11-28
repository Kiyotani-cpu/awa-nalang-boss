using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class EnemyHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>();

    [SerializeField] private float delay = 1.51f;
    [SerializeField] private int damage = 10;
    [SerializeField] private string canvaName;
    [SerializeField] private Image healthBarFill;

    [SerializeField] private Animator enemyAnimator;

    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackRange = 2f;
    private Transform playerTransform;

    [SerializeField] private float attackCooldown = 1f;
    private float lastAttackTime = -1f;

    [SerializeField] private float minimumAttackDistance = 2f;

    private bool isWalking = false;
    private bool isAttacking = false;
    private bool isDead = false;

    private const string WALKING_ANIMATION = "IsWalking";
    private const string ATTACKING_ANIMATION = "IsAttacking";
    private const string DIE_ANIMATION = "IsDead";
    private const string IDLE_ANIMATION = "IsIdle";

    // Audio clips for sound effects
    [Header("Audio Clips")]
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip hurtClip;
    [SerializeField] private AudioClip dieClip;

    // Reference to the enemy spawners
    private GameObject deathCanvas; // Reference to the UI Canvas or GameObject to activate on death

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Find the death canvas in the scene
        deathCanvas = GameObject.Find(canvaName); // You can change "DeathCanvas" to whatever name the Canvas has in your scene
        if (deathCanvas == null)
        {
            Debug.LogError("DeathCanvas not found in the scene.");
        }

        UpdateAnimationState();
    }

    private void Update()
    {
        if (isDead) return;

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
        if (!isAttacking && !isDead)
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
        if (isDead) return;

        if (IsServer)
        {
            currentHealth.Value -= damage;

            // Play hurt sound
            if (hurtClip != null)
            {
                PlaySound(hurtClip);
            }

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

        // Play die sound
        if (dieClip != null)
        {
            PlaySound(dieClip);
        }

        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger(DIE_ANIMATION);
        }

        isWalking = false;
        isAttacking = false;

        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // Activate the death UI (Canvas or GameObject)
        ShowDeathUI();

        Destroy(gameObject, delay);
    }

    public void StartAttacking()
    {
        if (!isDead)
        {
            isAttacking = true;
            UpdateAnimationState();

            // Play attack sound
            if (attackClip != null)
            {
                PlaySound(attackClip);
            }

            if (playerTransform != null)
            {
                if (Vector3.Distance(transform.position, playerTransform.position) <= attackRange)
                {
                    playerTransform.GetComponent<PlayerHealth>().TakeDamage(damage);
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

    private void PlaySound(AudioClip clip)
    {
        // Use PlayClipAtPoint for 3D sound placement
        AudioSource.PlayClipAtPoint(clip, transform.position);
    }

    // Method to activate the death UI (Canvas or GameObject)
    private void ShowDeathUI()
    {
        if (deathCanvas != null)
        {
            // Set the death canvas or GameObject active
            deathCanvas.SetActive(true);
        }
    }
}
