using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletData : NetworkBehaviour
{
    private NetworkVariable<ulong> owner = new(999);
    private NetworkVariable<bool> isActiveSelf = new(true);

    public static event Action<(ulong from, ulong to)> OnHitPlayer;

    private const int MAX_FLY_TIME = 3;
    private const float DAMAGE_AMOUNT = 10f;

    public override void OnNetworkSpawn()
    {
        DeactivateSelfDelay();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetOwnershipServerRpc(ulong id)
    {
        this.owner.Value = id;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetBulletIsActiveServerRpc(bool isActive)
    {
        if (!GetComponent<NetworkObject>()) return;

        isActiveSelf.Value = isActive;

        if (isActive == false)
        {
            GetComponent<NetworkObject>().Despawn();
        }
        else
        {
            GetComponent<NetworkObject>().Spawn();
        }
    }

    public void DeactivateSelfDelay()
    {
        StartCoroutine(DeactivateSelfDelayCoroutine());
    }

    IEnumerator DeactivateSelfDelayCoroutine()
    {
        yield return new WaitForSeconds(MAX_FLY_TIME);
        SetBulletIsActiveServerRpc(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsServer)
        {
            if (collision.transform.TryGetComponent(out NetworkObject networkObject))
            {
                if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    Debug.Log("Bullet has Collision to player");
                    (ulong, ulong) fromShooterToHit = new(owner.Value, networkObject.OwnerClientId);
                    OnHitPlayer?.Invoke(fromShooterToHit);
                    SetBulletIsActiveServerRpc(false);
                    return;
                }
                else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    Debug.Log("Bullet has Collision with enemy");
                    if (collision.gameObject.TryGetComponent<AREnemyHealth>(out AREnemyHealth enemyHealth))
                    {
                        enemyHealth.TakeDamageServerRpc(DAMAGE_AMOUNT);
                        Debug.Log("Lagunos");
                    }
                    SetBulletIsActiveServerRpc(false);
                    return;
                }
            }
            SetBulletIsActiveServerRpc(false);
        }
    }
}