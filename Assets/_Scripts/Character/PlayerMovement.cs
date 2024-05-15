using Sensor;
using UnityEngine;
using Utilities;

namespace Character
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private bool _enabledGizmos;
        
        [Header("Settings")]
        [Header("Player")]
        [SerializeField] private float _playerHeight = 0.5f;

        [Space(10f)]
        [SerializeField] private GroundMovement _groundMovement;
        [SerializeField] private JumpMovement _jumpMovement;
        [SerializeField] private SlopeMovement _slopeMovement;
        [SerializeField] private ClimbMovement _climbMovement;
        [SerializeField] private StairsMovement _stairsMovement;

        private Rigidbody _rigidbody;
        private AnimatorController _animatorController;
        private PlayerInput _playerInput;
        private Transform _cameraTransform;

        private Vector3 _velocityRootMotion;

        public void Initialize(AnimatorController animatorController, PlayerInput playerInput, Rigidbody rigidbody)
        {
            _playerInput = playerInput;
            _animatorController = animatorController;

            _rigidbody = rigidbody;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _rigidbody.drag = 5;

            InitializeMovements();

            _cameraTransform = Camera.main.transform;
            _enabledGizmos = true;
        }

        private void Update()
        {
            _groundMovement.OnUpdate();
            _jumpMovement.OnUpdate();
        }

        public void FixedUpdate()
        {
            UpdateSensors();
            _velocityRootMotion = Vector3.zero;
        }

        private void OnAnimatorMove() => _velocityRootMotion += _animatorController.Animator.deltaPosition;

        private void InitializeMovements()
        {
            _groundMovement.Initialize(_animatorController, _playerInput, _rigidbody, transform, _playerHeight);
            _jumpMovement.Initialize(_rigidbody);
            _stairsMovement.Initialize(_rigidbody, transform, _playerHeight);
            _climbMovement.Initialize(this, _animatorController, _playerInput, _rigidbody, transform, _playerHeight);
            _slopeMovement.Initialize(_rigidbody, _groundMovement, transform, _playerHeight);
        }

        public Vector3 GetInputTargetDirection()
        {
            Vector3 forward = _cameraTransform.forward.NormalizeWithoutY();
            Vector3 right = _cameraTransform.right.NormalizeWithoutY();

            Vector3 positionTarget = _playerInput.InputMovement.x * right + _playerInput.InputMovement.y * forward;
            return positionTarget;
        }

        public void ApplyWalkingValues() => _groundMovement.ApplyWalkingValues();
        public void Rotation(Vector3 direction) => _groundMovement.Rotation(direction);
        public void MoveInGround(Vector3 direction) => _groundMovement.MoveInGround(direction);
        public void RestrictVelocityInGround() => _groundMovement.RestrictVelocityInGround();

        public void Jump() => _jumpMovement.Jump();

        public void MoveInStairs() => _stairsMovement.MoveInStairs();

        public void MoveInSlope(Vector3 direction) => _slopeMovement.MoveInSlope(direction);
        public void RestrictVelocityInSlope() => _slopeMovement.RestrictVelocityOnSlope();

        public void EnterClimb() => _climbMovement.EnterClimb();
        public void ClimbWall() => _climbMovement.ClimbWall();
        public void ExitClimb() => _climbMovement.ExitClimb();
        public void ApplyClimbValues() => _climbMovement.ApplyClimbValues();

        private void UpdateSensors()
        {
            _groundMovement.UpdateSensor();
            _slopeMovement.UpdateSensor();
            _climbMovement.UpdateSensor();
            _stairsMovement.UpdateSensor();
        }

        public ISensor GetGroundSensor() => _groundMovement.GetGroundSensor();
        public ISensor GetStairSensor() => _stairsMovement.GetStairsSensor();
        public ISensor GetSlopeSensor() => _slopeMovement.GetSlopeSensor();
        public ISensor GetClimbSensor() => _climbMovement.GetClimbSensor();
        public Timer GetJumpTimer() => _jumpMovement.GetJumpTimer();

        public Vector3 VelocityRootMotion() => _velocityRootMotion;
        public bool CanMove() => _groundMovement.CanMove();

        #region Gizmos

        private void OnDrawGizmos()
        {
            if (!_enabledGizmos) return;
            
            Vector3 bodyPosition = transform.position + Vector3.up * _playerHeight;

            _slopeMovement.DrawSlope(bodyPosition);

            _groundMovement.DrawGround(bodyPosition);

            _climbMovement.DrawClimb(bodyPosition);

            _stairsMovement.DrawStairs();
        }

        #endregion
    }
}