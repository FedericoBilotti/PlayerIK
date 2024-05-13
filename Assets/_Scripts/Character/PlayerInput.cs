using UnityEngine;

namespace Character
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private PlayerInputController _playerInputController;

        public Vector2 InputMovement { get; private set; }

        private void OnEnable() => _playerInputController.OnMove += GetInputMovement;
        private void OnDisable() => _playerInputController.OnMove -= GetInputMovement;

        private void GetInputMovement(Vector2 inputMovement)
        {
            InputMovement = inputMovement;
        }
    }
}