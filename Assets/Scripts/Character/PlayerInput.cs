using UnityEngine;

namespace Character
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private PlayerInputController _playerInputController;

        public Vector2 InputMovement { get; private set; }

        private void OnEnable() => _playerInputController.OnMove += InputMove;
        private void OnDisable() => _playerInputController.OnMove -= InputMove;

        private void InputMove(Vector2 inputMovement)
        {
            InputMovement = inputMovement;
        }
    }
}