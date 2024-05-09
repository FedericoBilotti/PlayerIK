using System;
using UnityEngine;

namespace Character
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovementCharacterController : MonoBehaviour
    {
        [SerializeField] private float _characterHeight = 2f;
        [SerializeField] private float _dampTime = 0.2f;
        [SerializeField] private Vector3 _desiredMoveDirection;
        [SerializeField] private bool _isGrounded;
        [SerializeField] private float _desiredRotationSpeed;
        [SerializeField] private float _speed;
        [SerializeField] private float _allowPlayerRotation;
        [SerializeField] private float _verticalVel;
        [SerializeField] private Vector3 _moveVector;
        [SerializeField] private LayerMask _groundLayer;

        private Transform _cameraTransform;
        private PlayerInput _playerInput;
        private AnimatorController _animatorController;
        private CharacterController _characterController;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _animatorController = GetComponent<AnimatorController>();
            _characterController = GetComponent<CharacterController>();
            _cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            InputMagnitude();

            Gravity();
        }

        private void FixedUpdate()
        {
            _isGrounded = IsGrounded();
        }

        private bool IsGrounded() => Physics.Raycast(transform.position, Vector3.down, _characterHeight * 2f + 0.5f, _groundLayer);
        
        private void Gravity()
        {
            if (_isGrounded)
            {
                _verticalVel = 0;
            }
            else
            {
                _verticalVel -= 2;
            }

            _moveVector = Vector3.up * (_verticalVel * Time.deltaTime);
            _characterController.Move(_moveVector);
        }

        private void MoveAndRotation()
        {
            float horizontal = _playerInput.InputMovement.x;
            float vertical = _playerInput.InputMovement.y;

            Vector3 forward = _cameraTransform.forward;
            Vector3 right = _cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            _desiredMoveDirection = forward * vertical + right * horizontal;

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_desiredMoveDirection), _desiredRotationSpeed * Time.deltaTime);
        }

        private void InputMagnitude()
        {
            float horizontal = _playerInput.InputMovement.x;
            float vertical = _playerInput.InputMovement.y;

            _animatorController.SetFloat("Horizontal", horizontal, _dampTime, Time.deltaTime);
            _animatorController.SetFloat("Vertical", vertical, _dampTime, Time.deltaTime);

            _speed = new Vector2(horizontal, vertical).sqrMagnitude;

            _animatorController.SetFloat("InputMagnitude", _speed, _dampTime, Time.deltaTime);

            if (_speed >= _allowPlayerRotation)
            {
                MoveAndRotation();
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            var from = transform.position + Vector3.up;
            Gizmos.DrawLine(from, from + Vector3.down * (_characterHeight * 2f + 0.5f));
        }
    }
}