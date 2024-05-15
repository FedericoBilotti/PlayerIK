using System;
using Sensor;
using UnityEngine;

namespace Character
{
    [Serializable]
    public class GroundMovement
    {
        [Header("Velocity")]
        [SerializeField] private float _speed = 10f;
        [SerializeField] private float _maxSpeed = 5f;

        [Header("Rotations")]
        [SerializeField, Range(0f, 1f)] private float _allowRotation = 0.3f;

        [SerializeField] private float _smoothnessRotation = 10f;
        private float _actualSpeed;

        [Header("Ground check")]
        [SerializeField, Range(0.01f, 1f)] private float _groundRadius = 0.6f;

        [SerializeField, Range(0.01f, 2f)] private float _groundCheckDistance = 0.6f;
        [SerializeField] private LayerMask _groundLayer;

        private AnimatorController _animatorController;
        private PlayerInput _playerInput;
        private Transform _myTransform;
        private Rigidbody _rigidbody;
        private ISensor _groundSensor;
        
        public void Initialize(AnimatorController animatorController, PlayerInput playerInput, Rigidbody rigidbody, Transform transform, float playerHeight)
        {
            _animatorController = animatorController;
            _playerInput = playerInput;
            _rigidbody = rigidbody;
            _myTransform = transform;
            
            _groundSensor = new GroundSensor(transform, playerHeight, _groundRadius, _groundCheckDistance, _groundLayer);
        }

        public void OnUpdate()
        {
            _rigidbody.drag = _groundSensor.OnCollision ? 5 : 0;
        }

        public void UpdateSensor()
        {
            _groundSensor.Execute();
        }

        public void MoveInGround(Vector3 direction)
        {
            _rigidbody.AddForce(direction * (_speed * 100f * Time.fixedDeltaTime), ForceMode.Force);
        }

        public void Rotation(Vector3 positionTarget)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(positionTarget.normalized);
            Quaternion newRotation = Quaternion.Slerp(_myTransform.rotation, desiredRotation, _smoothnessRotation * Time.fixedDeltaTime);
            _rigidbody.rotation = newRotation;
        }
        
        public void RestrictVelocityInGround()
        {
            var previousVelocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);

            if (previousVelocity.magnitude < _maxSpeed) return;
            Vector3 newVel = previousVelocity.normalized * _maxSpeed;
            _rigidbody.velocity = new Vector3(newVel.x, _rigidbody.velocity.y, newVel.z);
        }

        public void ApplyWalkingValues()
        {
            Vector2 input = _playerInput.InputMovement;

            _animatorController.SetFloat("Vertical", input.y);
            _animatorController.SetFloat("Horizontal", input.x);

            _actualSpeed = _playerInput.InputMovement.sqrMagnitude;

            _animatorController.SetFloat("InputMagnitude", _actualSpeed);
        }

        public bool CanMove() => _actualSpeed > _allowRotation;

        public ISensor GetGroundSensor() => _groundSensor;

        public void DrawGround(Vector3 bodyPosition)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(bodyPosition, Vector3.down * _groundCheckDistance);
            Gizmos.DrawWireSphere(bodyPosition, _groundRadius);
            Gizmos.DrawWireSphere(bodyPosition + Vector3.down * _groundCheckDistance, _groundRadius);
        }

    }
}