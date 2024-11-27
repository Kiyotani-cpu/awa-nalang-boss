using System;
using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerCombatHandler : NetworkBehaviour
{
    private PlayerInputControls _playerInputControls;

    private const float ATTACK_DELAY = 0.5f; // Time between attacks
    private Coroutine _attackCoroutine;

    private Animator _animator;

    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int DefendHash = Animator.StringToHash("Defend");
    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int ShieldLockHash = Animator.StringToHash("ShieldLock");

    private bool _isDefending;
    private Transform _attackTransform; // Reference to attack origin (e.g., hand, weapon)
    private bool _isAttacking;

    [SerializeField] private Transform graphicsTransform; // Reference to the Graphics transform
    [SerializeField] private int attackDamage = 20; // Basic attack damage
    [SerializeField] private float attackRange = 1.5f; // Range within which the attack can hit enemies
    [SerializeField] private LayerMask enemyLayer; // Specify the enemy layer for collision detection
    [SerializeField] private Transform attackPoint; // Transform for where the attack originates (e.g., hand or weapon)

    public override void OnNetworkSpawn()
    {
        if (GetComponent<NetworkObject>().IsOwner)
        {
            _playerInputControls = GetComponent<PlayerInputControls>();

            _playerInputControls.OnAttackInput += StartAttacking;
            _playerInputControls.OnAttackInputCancelled += StopAttacking; // Handle attack cancellation

            _playerInputControls.OnDefendInput += StartDefending;
            _playerInputControls.OnDefendInputCancelled += StopDefending;
        }

        // Find the Animator on the Graphics object
        if (graphicsTransform == null)
        {
            graphicsTransform = transform.Find("Camera Offset/Graphics");
        }

        _animator = graphicsTransform.GetComponent<Animator>();

        // Set attack transform to the specified attack point (can be a hand or weapon)
        if (attackPoint == null)
        {
            // Default to the player’s position if no attack point is specified
            _attackTransform = transform;
        }
        else
        {
            _attackTransform = attackPoint;
        }
    }

    private void StartAttacking()
    {
        if (!_isDefending && !_isAttacking)  // Check if not already attacking or defending
        {
            _attackCoroutine = StartCoroutine(AttackCoroutine());  // Start the attack coroutine
        }
        else if (_isAttacking)
        {
            // Optionally reset the attack coroutine if already attacking
            StopCoroutine(_attackCoroutine);
            _attackCoroutine = StartCoroutine(AttackCoroutine()); // Restart the attack coroutine
        }
    }

    private void StopAttacking()
    {
        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);  // Stop the attack coroutine
            _attackCoroutine = null;
            _isAttacking = false;  // Reset the attacking flag
            SetIdleState();  // Return to idle state
        }
    }

    private IEnumerator AttackCoroutine()
    {
        _isAttacking = true;
        while (_isAttacking)  // Keep attacking as long as the flag is true
        {
            _animator.SetTrigger(AttackHash);  // Trigger attack animation
            PerformAttackServerRpc();  // Perform the attack on the server
            DetectEnemiesWithinRange();  // Check for enemies within range
            yield return new WaitForSeconds(ATTACK_DELAY);  // Wait for the attack delay before attacking again
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void PerformAttackServerRpc()
    {
        // Perform the logic for applying damage to enemies or handling hit detection.
        Debug.Log("Attack performed on the server.");
    }

    private void DetectEnemiesWithinRange()
    {
        // Use the attack transform (hand or weapon) for detecting enemies within range
        Collider[] enemiesHit = Physics.OverlapSphere(_attackTransform.position, attackRange, enemyLayer);
        foreach (var enemy in enemiesHit)
        {
            if (enemy.CompareTag("Enemy")) // Ensure it's an enemy object
            {
                var enemyHealth = enemy.GetComponent<EnemyHealth>(); // Reference to enemy health
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(attackDamage); // Apply damage to enemy
                }
            }
        }
    }

    private void StartDefending()
    {
        if (_isDefending) return;

        _isDefending = true;

        // Play the shield lock animation
        _animator.SetTrigger(ShieldLockHash);

        StopAttacking(); // Ensure no attacks while defending
    }

    private void StopDefending()
    {
        if (!_isDefending) return;

        _isDefending = false;

        // Transition back to idle when shield is released
        SetIdleState();
    }

    private void SetIdleState()
    {
        if (!_isDefending && _attackCoroutine == null)
        {
            _animator.SetTrigger(IdleHash);  // Play the idle animation if not attacking or defending
        }
    }

    public override void OnNetworkDespawn()
    {
        if (GetComponent<NetworkObject>().IsOwner)
        {
            _playerInputControls.OnAttackInput -= StartAttacking;
            _playerInputControls.OnAttackInputCancelled -= StopAttacking;
            _playerInputControls.OnDefendInput -= StartDefending;
            _playerInputControls.OnDefendInputCancelled -= StopDefending;
        }
    }
}
