using Sensor;
using UnityEngine;
using Utilities;

namespace Character
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Settings")]
        [Header("Player")]
        [SerializeField] private float _playerHeight = 0.5f;

        [Header("Velocity")]
        [SerializeField] private float _speed = 10f;
        [SerializeField] private float _maxSpeed = 10f;
        
        [Header("Jump")]
        [SerializeField] private float _jumpForce = 10f;
        [SerializeField] private float _jumpDelay = .4f;

        [Header("Rotations")]
        [SerializeField, Range(0f, 1f)] private float _allowRotation = 0.3f;
        [SerializeField] private float _smoothnessRotation = 10f;
        private float _actualSpeed;

        [Header("Ground check")]
        [SerializeField, Range(0.01f, 1f)] private float _groundRadius = 0.6f;
        [SerializeField, Range(0.01f, 2f)] private float _groundCheckDistance = 0.6f;
        [SerializeField] private LayerMask _groundLayer;
        
        [Header("Slope Check")]
        [SerializeField] private float _slopeRayLength = 1.4f;
        [SerializeField] private float _downSpeedOnSlope = 5f;
        [SerializeField] private float _slopeAngleLimit = 85f;
        [SerializeField] private LayerMask _slopeLayer;

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

        [Header("Stairs Values")]
        [SerializeField, Range(0f, 1f)] private float _stairHeight = 0.2f;
        [SerializeField, Range(0.01f, 100f)] private float _moveSmoothness = 2f;
        [SerializeField] private LayerMask _stairLayer;
        
        [Header("Rays Stairs Check")]
        [SerializeField, Range(0, 10)] private int _totalRays = 3;
        [SerializeField, Range(1f, 360f)] private float _angleRays;
        [SerializeField, Range(0f, 360f)] private float _offset;
        [SerializeField, Range(0f, 1f)] private float _stairRayUpperLength = 0.2f;
        [SerializeField, Range(-5f, 5f)] private float _startRayUpper;
        [SerializeField, Range(0f, 1f)] private float _stairRayLowerLength = 0.2f;
        [SerializeField, Range(-5f, 5f)] private float _startRayLower;

        private Timer _jumpTimer;
        private ISensor _groundSensor;
        private ISensor _slopeSensor;
        private ISensor _stairSensor;
        private ISensor _climbSensor;
        private ISensor[] _sensors;
        
        private Rigidbody _rigidbody;
        private AnimatorController _animatorController;
        private PlayerInput _playerInput;
        private Transform _cameraTransform;
        private Transform _myTransform;

        private Vector3 _velocityRootMotion;

        public void Initialize(AnimatorController animatorController, PlayerInput playerInput, Rigidbody rigidbody)
        {
            _playerInput = playerInput;
            _animatorController = animatorController;
            
            _rigidbody = rigidbody;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _rigidbody.drag = 5;
            
            _cameraTransform = Camera.main.transform;
            _myTransform = transform;
            
            CreateSensors();
            CreateTimer();
        }

        private void Update()
        {
            UpdateTimer();
            _rigidbody.drag = _groundSensor.OnCollision ? 5 : 0;
        }

        public void FixedUpdate()
        {
            UpdateSensors();
            RestrictVelocity();
        }

        private void OnAnimatorMove() => _velocityRootMotion += _animatorController.Animator.deltaPosition;

        public Vector3 GetDirectionTarget()
        {
            Vector3 forward = _cameraTransform.forward.NormalizeWithoutY();
            Vector3 right = _cameraTransform.right.NormalizeWithoutY();

            Vector3 positionTarget = _playerInput.InputMovement.x * right + _playerInput.InputMovement.y * forward;
            return positionTarget;
        }

        public void EnterClimb()
        {
            _animatorController.Animator.applyRootMotion = false;
            _animatorController.SetBool("Climb", true);
            GetComponent<GroundIKController>().Enabled = false; // Pedirlo en el Awake
            GetComponent<ClimbIKController>().Enabled = true;   // Pedirlo en el Awake
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

            dir += _velocityRootMotion;
            
            _rigidbody.position = Vector3.Lerp(_rigidbody.position, _climbSensor.Hit.point + _climbSensor.Hit.normal * 0.05f, _smoothnessClimb * Time.fixedDeltaTime);
            _myTransform.rotation = Quaternion.Slerp(_myTransform.rotation, Quaternion.LookRotation(-_climbSensor.Hit.normal), _climbRotationSpeed * Time.fixedDeltaTime);
            
            _rigidbody.velocity = dir * (_climbSpeed * 100f * Time.fixedDeltaTime);

            _velocityRootMotion = Vector3.zero;
        }

        public void ExitClimb()
        {
            _animatorController.Animator.applyRootMotion = true;
            _animatorController.SetBool("Climb", false);
            GetComponent<GroundIKController>().Enabled = true; // Pedirlo en el Awake
            GetComponent<ClimbIKController>().Enabled = false; // Pedirlo en el Awake
            _rigidbody.useGravity = true;
        }

        public void Jump()
        {
            _rigidbody.velocity = Vector3.up * (_jumpForce * 100f * Time.fixedDeltaTime);
        }

        public void JumpOutClimb()
        {
            Debug.Log("OutClimb");
            _rigidbody.velocity = -transform.TransformDirection(Vector3.forward) * (_jumpForce * 100f * Time.fixedDeltaTime);
        }

        public void MoveInSlope(Vector3 rootTarget)
        {
            MoveInGround(GetSlopeDirection(rootTarget).normalized);

            if (_rigidbody.velocity.y > 0)
            {
                MoveInGround(Vector3.down * _downSpeedOnSlope);
            }

            _velocityRootMotion = Vector3.zero;
        }

        private Vector3 GetSlopeDirection(Vector3 direction)
        {
            return Vector3.ProjectOnPlane(direction, _slopeSensor.Hit.normal);
        }

        public void MoveInGround(Vector3 direction)
        {
            _rigidbody.AddForce(direction * (_speed * 100f * Time.fixedDeltaTime), ForceMode.Force);
            _velocityRootMotion = Vector3.zero;
        }

        public void MoveInStairs()
        {
            _rigidbody.position += new Vector3(0, _moveSmoothness, 0) * Time.fixedDeltaTime;
            _velocityRootMotion = Vector3.zero;
        }

        public void Rotation(Vector3 positionTarget)
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
            var previousVelocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);

            if (previousVelocity.magnitude < _maxSpeed) return;
            Vector3 newVel = previousVelocity.normalized * _maxSpeed;
            _rigidbody.velocity = new Vector3(newVel.x, _rigidbody.velocity.y, newVel.z);
        }

        private void CreateTimer() => _jumpTimer = new CountdownTimer(_jumpDelay);
        private void UpdateTimer() => _jumpTimer.Tick(Time.deltaTime);

        private void CreateSensors()
        {
            _sensors = new[]
            {
                    _groundSensor = new GroundSensor(_myTransform, _playerHeight, _groundRadius, _groundCheckDistance, _groundLayer),
                    _slopeSensor = new SlopeSensor(this, _myTransform, _playerHeight, _slopeRayLength, _slopeAngleLimit, _slopeLayer),
                    _climbSensor = new ClimbSensor(_myTransform, _playerHeight, _startRayClimb, _climbRayLength, _offsetClimbRays, _angleClimbRays, _totalRaysClimb, _climbLayer),
                    _stairSensor = new StairSensor(_myTransform, _playerHeight, _stairHeight, _stairRayLowerLength, _stairRayUpperLength, _startRayLower, _startRayUpper,
                            _totalRays, _angleRays, _offset, _stairLayer)
            };
        }

        private void UpdateSensors()
        {
            foreach (ISensor sensor in _sensors)
            {
                sensor.Execute();
            }
        }

        public void ApplyWalkingValues()
        {
            Vector2 input = _playerInput.InputMovement;

            _animatorController.SetFloat("Vertical", input.y);
            _animatorController.SetFloat("Horizontal", input.x);

            _actualSpeed = _playerInput.InputMovement.sqrMagnitude;

            _animatorController.SetFloat("InputMagnitude", _actualSpeed);
        }

        public void ApplyClimbValues()
        {
            Vector2 input = _playerInput.InputMovement;
            
            _animatorController.SetFloat("Vertical", input.y);
            _animatorController.SetFloat("Horizontal", input.x);
        }

        public ISensor GetGroundSensor() => _groundSensor;
        public ISensor GetStairSensor() => _stairSensor;
        public ISensor GetSlopeSensor() => _slopeSensor;
        public ISensor GetClimbSensor() => _climbSensor;

        public Timer GetJumpTimer() => _jumpTimer;
        
        public bool CanMove() => _actualSpeed > _allowRotation;
        public Vector3 VelocityRootMotion() => _velocityRootMotion;

        #region Gizmos

        private void OnDrawGizmos()
        {
            Vector3 bodyPosition = transform.position + Vector3.up * _playerHeight;

            DrawSlope(bodyPosition);

            DrawGround(bodyPosition);
            
            DrawClimb(bodyPosition);

            DrawStairs();
        }

        private void DrawSlope(Vector3 bodyPosition)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(bodyPosition, Vector3.down * _slopeRayLength);
        }

        private void DrawGround(Vector3 bodyPosition)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(bodyPosition, Vector3.down * _groundCheckDistance);
            Gizmos.DrawWireSphere(bodyPosition, _groundRadius);
            Gizmos.DrawWireSphere(bodyPosition + Vector3.down * _groundCheckDistance, _groundRadius);
        }

        private void DrawClimb(Vector3 bodyPosition)
        {
            Vector3 forward = transform.forward;
            Vector3 startPos = bodyPosition + Vector3.up * _startRayClimb;

            for (int i = 0; i < _totalRaysClimb; i++)
            {
                float angle = i * (_angleClimbRays / _totalRaysClimb);
                Vector3 rayDirection = Quaternion.Euler(0, angle + _offsetClimbRays, 0) * forward;

                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(startPos, rayDirection * _climbRayLength);
            }
        }

        private void DrawStairs()
        {
            Vector3 pos = transform.position;
            Vector3 forward = transform.forward;
            Vector3 fromLower = pos + Vector3.up * (_playerHeight * _startRayLower);
            Vector3 fromUpper = pos + Vector3.up * (_playerHeight * _startRayUpper * _stairHeight);
            Gizmos.color = Color.blue;

            for (int i = 0; i < _totalRays; i++)
            {
                float angle = i * (_angleRays / _totalRays);
                Vector3 rayDirection = Quaternion.Euler(0, angle + _offset, 0) * forward;

                Gizmos.DrawRay(fromLower, rayDirection * _stairRayLowerLength);
                Gizmos.DrawRay(fromUpper, rayDirection * _stairRayUpperLength);
            }
        }

        #endregion
    }
}