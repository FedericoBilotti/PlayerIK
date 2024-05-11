using Sensor;
using UnityEngine;
using Utilities;

namespace Character
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Settings")]
        [Header("Player")]
        [SerializeField] private float _playerHeight = 0.5f;

        [Header("Velocity")]
        [SerializeField] private float _speed = 10f;
        [SerializeField] private float _maxSpeed = 10f;
        [SerializeField, Range(0f, 1f)] private float _dampSmoothness = .1f;

        [Header("Rotations")]
        [SerializeField, Range(0f, 1f)] private float _allowRotation = 0.3f;
        [SerializeField] private float _smoothnessRotation = 10f;
        private float _actualSpeed;

        [Header("Slope Check")]
        [SerializeField] private float _slopeRayLength = 1.4f;
        [SerializeField] private float _downSpeedOnSlope = 5f;
        [SerializeField] private float _slopeAngleLimit = 85f;
        [SerializeField] private LayerMask _slopeLayer;
        private ISensor _slopeSensor;

        [Header("Ground check")]
        [SerializeField] private float _groundRayLength = 0.6f;
        [SerializeField] private LayerMask _groundLayer;
        private ISensor _groundSensor;

        private AnimatorController _animatorController;
        private PlayerInput _playerInput;
        private Rigidbody _rigidbody;
        private Transform _cameraTransform;
        private Transform _myTransform;
        
        private Vector3 _velocityRootMotion;

        private void Awake()
        {
            _animatorController = GetComponent<AnimatorController>();
            _playerInput = GetComponent<PlayerInput>();

            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _rigidbody.drag = 5;
            
            _cameraTransform = Camera.main.transform;
            _myTransform = transform;
        }

        private void Start()
        {
            _groundSensor = new GroundSensor(_myTransform, _playerHeight, _groundRayLength, _groundLayer);
            _slopeSensor = new SlopeSensor(this, _myTransform, _playerHeight, _slopeRayLength, _slopeAngleLimit, _slopeLayer);
        }

        private void Update()
        {
            _rigidbody.drag = _groundSensor.OnCollision ? 5 : 0;

            ApplyAnimationValues();
        }

        public void FixedUpdate()
        {
            UpdateSensors();
            
            RestrictVelocity();
            MoveAndRotateCharacter(Time.fixedDeltaTime);
        }

        private void OnAnimatorMove() => _velocityRootMotion += _animatorController.Animator.deltaPosition;

        private void MoveAndRotateCharacter(float fixedDelta)
        {
            if (_actualSpeed < _allowRotation) return;

            Vector3 positionTarget = GetPositionTarget();

            Rotation(positionTarget);

            Vector3 rootTarget = _velocityRootMotion + positionTarget;

            if (_slopeSensor.OnCollision) // Move to FSM
            {
                Move(GetSlopeDirection(rootTarget).normalized, _speed, fixedDelta);

                if (_rigidbody.velocity.y > 0)
                {
                    Move(Vector3.down, _downSpeedOnSlope, fixedDelta);
                }
            }
            else
            {
                Move(rootTarget.normalized, _speed, fixedDelta);
            }

            _velocityRootMotion = Vector3.zero;
        }

        private Vector3 GetPositionTarget()
        {
            Vector3 forward = _cameraTransform.forward.NormalizeWithoutY();
            Vector3 right = _cameraTransform.right.NormalizeWithoutY();

            Vector3 positionTarget = _playerInput.InputMovement.x * right + _playerInput.InputMovement.y * forward;
            return positionTarget;
        }

        private Vector3 GetSlopeDirection(Vector3 direction)
        {
            return Vector3.ProjectOnPlane(direction, _slopeSensor.Hit.normal);
        }

        private void Move(Vector3 direction, float velocity, float fixedDelta)
        {
            _rigidbody.AddForce(direction * (velocity * 100f * fixedDelta), ForceMode.Force);
        }

        private void Rotation(Vector3 positionTarget)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(positionTarget.normalized);
            Quaternion newRotation = Quaternion.Slerp(_myTransform.rotation, desiredRotation, _smoothnessRotation * Time.fixedDeltaTime);
            _rigidbody.rotation = newRotation;
        }

        private void RestrictVelocity()
        {
            if (_slopeSensor.OnCollision)
            {
                RestrictVelocityOnSlope();
                return;
            }

            RestrictVelocityInGround();
        }

        private void RestrictVelocityOnSlope()
        {
            if (_rigidbody.velocity.magnitude < _maxSpeed) return;
            
            _rigidbody.velocity = _rigidbody.velocity.normalized * _maxSpeed;
        }

        private void RestrictVelocityInGround()
        {
            Vector3 previousVelocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);

            if (previousVelocity.magnitude < _maxSpeed) return;
            Vector3 newVel = previousVelocity.normalized * _maxSpeed;
            _rigidbody.velocity = new Vector3(newVel.x, _rigidbody.velocity.y, newVel.z);
        }

        private void UpdateSensors()
        {
            _groundSensor.Execute();
            _slopeSensor.Execute();
        }

        private void ApplyAnimationValues()
        {
            Vector3 input = _playerInput.InputMovement;

            _animatorController.SetFloat("Vertical", input.y, _dampSmoothness, Time.deltaTime);
            _animatorController.SetFloat("Horizontal", input.x, _dampSmoothness, Time.deltaTime);

            _actualSpeed = _playerInput.InputMovement.sqrMagnitude;

            _animatorController.SetFloat("InputMagnitude", _actualSpeed, _dampSmoothness, Time.deltaTime);
        }

        #region Gizmos

        private void OnDrawGizmos()
        {
            Vector3 from = transform.position + Vector3.up * _playerHeight;
            
            Gizmos.color = Color.red;
            Gizmos.DrawRay(from, Vector3.down * _slopeRayLength);

            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(from, Vector3.down * _groundRayLength);
        }

        #endregion
    }
}