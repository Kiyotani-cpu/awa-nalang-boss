using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class EnemyHealth : NetworkBehaviour
{
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>();

    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int damage = 10;
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

    private void Awake()
    {
        // No need for death canvas logic, just remove the related code
    }

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

        // No death canvas logic now, just destroy the object after a delay
        Destroy(gameObject, 1.51f); // Adjust delay if needed
        GameManager.Instance.LoadLevelCompleteScene();
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
}
