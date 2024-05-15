using System;
using Sensor;
using UnityEngine;

namespace Character
{
    [Serializable]
    public class ClimbMovement
    {
        [Header("Climb Values")]
        [SerializeField, Range(-2f, 2f)] private float _startRayClimb = 1f;
        [SerializeField, Range(0f, 1f)] private float _climbRayLength;
        [SerializeField] private float _smoothnessClimb = 2f;
        [SerializeField] private float _climbSpeed;
        [SerializeField] private float _climbRotationSpeed = 10f;
        [SerializeField] private float _angleClimbRays;
        [SerializeField] private int _totalRaysClimb = 3;
        [SerializeField, Range(-360f, 360f)] private float _offsetClimbRays;
        [SerializeField] private LayerMask _climbLayer;

        private ISensor _climbSensor; 
        private Rigidbody _rigidbody;
        private Transform _myTransform;
        private AnimatorController _animatorController;
        private PlayerMovement _playerMovement;
        private PlayerInput _playerInput;

        public void Initialize(PlayerMovement playerMovement, AnimatorController animatorController, PlayerInput playerInput, Rigidbody rigidbody, Transform transform, float playerHeight)
        {
            _playerMovement = playerMovement;
            _myTransform = transform;
            _animatorController = animatorController;
            _rigidbody = rigidbody;
            _playerInput = playerInput;

            _climbSensor = new ClimbSensor(transform, playerHeight, _startRayClimb, _climbRayLength, _offsetClimbRays, _angleClimbRays, _totalRaysClimb, _climbLayer);
        }

        public void UpdateSensor()
        {
            _climbSensor.Execute();
        }

        public void EnterClimb()
        {
            _animatorController.Animator.applyRootMotion = false;
            _animatorController.SetBool("Climb", true);
            _playerInput.GetComponent<GroundIKController>().Enabled = false; // Pedirlo en el Awake
            _playerInput.GetComponent<ClimbIKController>().Enabled = true;   // Pedirlo en el Awake
            _rigidbody.useGravity = false;
        }

        public void ClimbWall()
        {
            Vector2 input = _playerInput.InputMovement;

            if (input.magnitude <= 0.01f)
            {
                _rigidbody.velocity = Vector3.zero;
                return;
            }

            Vector3 dir = (_myTransform.up * input.y + _myTransform.right * input.x).normalized;

            dir += _playerMovement.VelocityRootMotion();

            _rigidbody.position = Vector3.Lerp(_rigidbody.position, _climbSensor.Hit.point + _climbSensor.Hit.normal * 0.05f, _smoothnessClimb * Time.fixedDeltaTime);
            _myTransform.rotation = Quaternion.Slerp(_myTransform.rotation, Quaternion.LookRotation(-_climbSensor.Hit.normal), _climbRotationSpeed * Time.fixedDeltaTime);

            _rigidbody.velocity = dir * (_climbSpeed * 100f * Time.fixedDeltaTime);
        }

        public void ExitClimb()
        {
            _animatorController.Animator.applyRootMotion = true;
            _animatorController.SetBool("Climb", false);
            _playerInput.GetComponent<GroundIKController>().Enabled = true; // Pedirlo en el Awake
            _playerInput.GetComponent<ClimbIKController>().Enabled = false; // Pedirlo en el Awake
            _rigidbody.useGravity = true;
        }

        public void ApplyClimbValues()
        {
            Vector2 input = _playerInput.InputMovement;
            
            _animatorController.SetFloat("Vertical", input.y);
            _animatorController.SetFloat("Horizontal", input.x);
        }

        public ISensor GetClimbSensor() => _climbSensor;
        
        public void DrawClimb(Vector3 bodyPosition)
        {
            Vector3 forward = _myTransform.forward;
            Vector3 startPos = bodyPosition + Vector3.up * _startRayClimb;

            for (int i = 0; i < _totalRaysClimb; i++)
            {
                float angle = i * (_angleClimbRays / _totalRaysClimb);
                Vector3 rayDirection = Quaternion.Euler(0, angle + _offsetClimbRays, 0) * forward;

                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(startPos, rayDirection * _climbRayLength);
            }
        }
    }
}