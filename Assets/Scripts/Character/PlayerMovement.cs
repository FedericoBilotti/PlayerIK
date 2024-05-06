using UnityEngine;

namespace Character
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float _speed;
        [SerializeField] private float _slerpSmoothness = 10f;

        private PlayerInput _playerInput;
        private Rigidbody _rigidbody;
        private Transform _cameraTransform;
        private Transform _myTransform;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _playerInput = GetComponent<PlayerInput>();

            _cameraTransform = Camera.main.transform;
            _myTransform = transform;
        }

        public void FixedUpdate()
        {
            MoveAndRotateCharacter();
        }

        private void MoveAndRotateCharacter()
        {
            if (_playerInput.InputMovement == Vector2.zero) return;

            Vector3 positionTarget = _playerInput.InputMovement.x * _cameraTransform.right;
            positionTarget += _playerInput.InputMovement.y * _cameraTransform.forward;
            positionTarget.y = 0;

            _rigidbody.AddForce(positionTarget.normalized * (_speed * 100f * Time.fixedDeltaTime));
            _rigidbody.MoveRotation(Quaternion.Slerp(_myTransform.rotation, Quaternion.LookRotation(positionTarget), _slerpSmoothness * Time.fixedDeltaTime));
        }
    }
}