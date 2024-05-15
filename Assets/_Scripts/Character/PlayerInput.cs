using UnityEngine;

namespace Character
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private PlayerInputController _playerInputController;

        public Vector2 InputMovement { get; private set; }
        public bool InputJump { get; private set; }

        private void OnEnable()
        {
            _playerInputController.OnMove += GetInputMovement;
            _playerInputController.OnJump += GetJumpInput;
        }

        private void OnDisable()
        {
            _playerInputController.OnMove -= GetInputMovement;
            _playerInputController.OnJump -= GetJumpInput;
        }

        private void GetInputMovement(Vector2 inputMovement)
        {
            InputMovement = inputMovement;
        }
        
        private void GetJumpInput(bool predicate)
        {
            InputJump = predicate;
        }
    }
}