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

        public void OnMovement(InputAction.CallbackContext context)
        {
            OnMove?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnCameraMovement(InputAction.CallbackContext context)
        {
            OnMoveCamera?.Invoke(context.ReadValue<Vector2>());
        }
    }
}