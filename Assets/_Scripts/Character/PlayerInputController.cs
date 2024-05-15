using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Character
{
    [CreateAssetMenu(menuName = "PlayerInputController", fileName = "PlayerInputController", order = 0)] 
    public class PlayerInputController : ScriptableObject, PlayerController.IMovementActions
    {
        private PlayerController _playerController;

        private void OnEnable()
        {
            if (_playerController != null) return;
            
            _playerController = new PlayerController();
            
            _playerController.Movement.SetCallbacks(this);
            
            _playerController.Enable();
        }
        
        public event Action<Vector2> OnMove;
        public event Action<Vector2> OnMoveCamera;
        public event Action<bool> OnJump;

        public void OnMovement(InputAction.CallbackContext context)
        {
            OnMove?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnCameraMovement(InputAction.CallbackContext context)
        {
            OnMoveCamera?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnJumping(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                OnJump?.Invoke(true);
            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                OnJump?.Invoke(false);
            }
        }
    }
}