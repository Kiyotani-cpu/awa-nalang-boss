using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerShootBullets : NetworkBehaviour
{
    private PlayerInputControls _playerInputControls;

    private const float BULLET_DELAY = .2f;
    private const float SHOOTING_DELAY = .2f;
    private const float BULLET_SPEED = 5f;
    private const float BULLET_ANGLE_AMPLIFY = .11f;
    private const float BULLET_SHOOT_ANGLE_MAX = 25;

    [SerializeField] private Transform bulletSpawnTransform;  // Drag the child object here in the Inspector
    [SerializeField] private GameObject bulletPrefab;

    private float bulletShootAngle;
    private Coroutine ShootAutoCoroutine;

    public override void OnNetworkSpawn()
    {
        if (GetComponent<NetworkObject>().IsOwner)
        {
            _playerInputControls = GetComponent<PlayerInputControls>();

            _playerInputControls.OnShootInput += StartShooting;
            _playerInputControls.OnShootInputCancelled += StopShooting;
            _playerInputControls.OnShootAnglePerformed += PlayerInputControlsOnShootAnglePerformed;
        }
    }

    private void PlayerInputControlsOnShootAnglePerformed(Vector2 angleValue)
    {
        float newAngle;

        if (angleValue == Vector2.zero)
        {
            newAngle = 0;
        }
        else
        {
            newAngle = bulletShootAngle + angleValue.y * -BULLET_ANGLE_AMPLIFY;
            newAngle = Mathf.Clamp(newAngle, -BULLET_SHOOT_ANGLE_MAX, BULLET_SHOOT_ANGLE_MAX);
        }

        bulletShootAngle = newAngle;
    }

    private void StopShooting()
    {
        if (ShootAutoCoroutine != null)
        {
            bulletShootAngle = 0f;
            StopCoroutine(ShootAutoCoroutine);
            ShootAutoCoroutine = null;
        }
    }

    private void StartShooting()
    {
        if (ShootAutoCoroutine == null)
        {
            ShootAutoCoroutine = StartCoroutine(ShootCoroutine());
        }
    }

    private IEnumerator ShootCoroutine()
    {
        yield return new WaitForSeconds(SHOOTING_DELAY);

        while (true)
        {
            StartShootBulletServerRpc(bulletShootAngle, NetworkManager.Singleton.LocalClientId);
            yield return new WaitForSeconds(BULLET_DELAY);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartShootBulletServerRpc(float bulletShootAngle, ulong callerID)
    {
        // Set the rotation angle for the bullet spawn transform
        Quaternion rotation = Quaternion.Euler(0, bulletShootAngle, 0);
        bulletSpawnTransform.localRotation = rotation;

        // Instantiate the bullet at the bulletSpawnTransform's position and rotation
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnTransform.position, bulletSpawnTransform.rotation);

        NetworkObject bulletNetworkObject = bullet.GetComponent<NetworkObject>();
        bulletNetworkObject.Spawn();  // Spawn the bullet on the network

        // Set ownership information on the bullet
        bullet.GetComponent<BulletData>().SetOwnershipServerRpc(callerID);

        // Apply velocity to move the bullet forward
        Rigidbody bulletRigidBody = bullet.GetComponent<Rigidbody>();
        bulletRigidBody.velocity = bulletSpawnTransform.forward * BULLET_SPEED;
    }

    public override void OnNetworkDespawn()
    {
        if (GetComponent<NetworkObject>().IsOwner)
        {
            _playerInputControls.OnShootInput -= StartShooting;
            _playerInputControls.OnShootInputCancelled -= StopShooting;
            _playerInputControls.OnShootAnglePerformed -= PlayerInputControlsOnShootAnglePerformed;
        }
    }
}
