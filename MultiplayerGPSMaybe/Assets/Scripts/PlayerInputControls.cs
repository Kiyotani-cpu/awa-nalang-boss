using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerInputControls : NetworkBehaviour
{
    private PlayerControlInputAction _playerControlsInputAction;
    private Vector3 movementVector;

    // Defend event
    public event Action<Vector3> OnMoveInput;
    public event Action OnMoveActionCancelled;
    public event Action OnShootInput;
    public event Action OnShootInputCancelled;
    public event Action OnAttackInput; // Event for attack
    public event Action OnAttackInputCancelled; // Event for attack cancellation

    public event Action<Vector2> OnShootAnglePerformed;
    public event Action OnDefendInput;
    public event Action OnDefendInputCancelled;

    

    public override void OnNetworkSpawn()
    {
        if (GetComponent<NetworkObject>().IsOwner)
        {
            _playerControlsInputAction = new PlayerControlInputAction();
            _playerControlsInputAction.Enable();

            _playerControlsInputAction.PlayerControlsMap.Move.performed += MoveActionPerformed;
            _playerControlsInputAction.PlayerControlsMap.Move.canceled += MoveActionCancelled;

            _playerControlsInputAction.PlayerControlsMap.Shoot.performed += ShootOnperformed;
            _playerControlsInputAction.PlayerControlsMap.Shoot.canceled += ShootOncanceled;

            _playerControlsInputAction.PlayerControlsMap.ShootAngle.performed += ShootAngleOnperformed;

            _playerControlsInputAction.PlayerControlsMap.Defend.performed += DefendPerformed;
            _playerControlsInputAction.PlayerControlsMap.Defend.canceled += DefendCancelled;

            // Bind the Attack button
            _playerControlsInputAction.PlayerControlsMap.Attack.performed += AttackPerformed;
            _playerControlsInputAction.PlayerControlsMap.Attack.canceled += AttackCancelled; // Bind attack cancel
        }
    }

    private void DefendPerformed(InputAction.CallbackContext context)
    {
        OnDefendInput?.Invoke();
    }

    private void DefendCancelled(InputAction.CallbackContext context)
    {
        OnDefendInputCancelled?.Invoke();
    }

    private void ShootAngleOnperformed(InputAction.CallbackContext context)
    {
        OnShootAnglePerformed?.Invoke(context.ReadValue<Vector2>());
    }

    private void ShootOncanceled(InputAction.CallbackContext obj)
    {
        OnShootInputCancelled?.Invoke();
       
    }

    private void ShootOnperformed(InputAction.CallbackContext obj)
    {
        
            OnShootInput?.Invoke();
        
    }

    // New method for the Attack button
    private void AttackPerformed(InputAction.CallbackContext context)
    {
        OnAttackInput?.Invoke(); // Trigger the attack event
    }

    // New method for the Attack cancellation
    private void AttackCancelled(InputAction.CallbackContext context)
    {
        OnAttackInputCancelled?.Invoke(); // Trigger the attack cancelled event
    }

    private void MoveActionCancelled(InputAction.CallbackContext context)
    {
        movementVector = Vector3.zero;
        OnMoveActionCancelled?.Invoke();
    }

    private void MoveActionPerformed(InputAction.CallbackContext context)
    {
        Vector2 v2Movement = context.ReadValue<Vector2>();
        movementVector = new Vector3(v2Movement.x, 0, v2Movement.y);
    }

    void Update()
    {
        if (movementVector != Vector3.zero)
        {
            OnMoveInput?.Invoke(movementVector);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (GetComponent<NetworkObject>().IsOwner)
        {
            _playerControlsInputAction.PlayerControlsMap.Move.performed -= MoveActionPerformed;
            _playerControlsInputAction.PlayerControlsMap.Move.canceled -= MoveActionCancelled;
            _playerControlsInputAction.PlayerControlsMap.Shoot.performed -= ShootOnperformed;
            _playerControlsInputAction.PlayerControlsMap.Shoot.canceled -= ShootOncanceled;
            _playerControlsInputAction.PlayerControlsMap.Defend.performed -= DefendPerformed;
            _playerControlsInputAction.PlayerControlsMap.Defend.canceled -= DefendCancelled;

            // Unbind the Attack button
            _playerControlsInputAction.PlayerControlsMap.Attack.performed -= AttackPerformed;
            _playerControlsInputAction.PlayerControlsMap.Attack.canceled -= AttackCancelled; // Unbind attack cancel
        }
    }
}
