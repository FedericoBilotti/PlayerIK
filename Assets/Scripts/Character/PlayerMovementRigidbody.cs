using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Character
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovementRigidbody : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _speed = 10f;
        [SerializeField] private float _slerpSmoothness = 10f;
        [SerializeField, Range(0f, 1f)] private float _dampSmoothnessRun = .1f;
        [SerializeField, Range(0f, 1f)] private float _dampSmoothnessIdle = .1f;
        
        [Header("Slope")]
        [SerializeField] private float _speedOnSlope = 10f;
        [SerializeField] private float _slopeAngleLimit = 85f;
        [SerializeField] private LayerMask _slopeLayer;

        [Header("Ground check")]
        [SerializeField] private float _distance = 1.4f;
        [SerializeField] private LayerMask _groundLayer;

        private AnimatorController _animatorController;
        private PlayerInput _playerInput;
        private Rigidbody _rigidbody;
        private Transform _cameraTransform;

        private Vector3 _velocityRootMotion;

        private void Awake()
        {
            _animatorController = GetComponent<AnimatorController>();
            _playerInput = GetComponent<PlayerInput>();
            _rigidbody = GetComponent<Rigidbody>();

            _cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            AnimationMovement();
        }

        public void FixedUpdate()
        {
            MoveAndRotateCharacter();
        }

        private void OnAnimatorMove()
        {
            _velocityRootMotion += _animatorController.Animator.deltaPosition;
        }

        private void MoveAndRotateCharacter()
        {
            if (_playerInput.InputMovement == Vector2.zero) return;

            Vector3 cameraForward = NormalizeVector3(_cameraTransform.forward);
            Vector3 cameraRight = NormalizeVector3(_cameraTransform.right);

            Vector3 positionTarget = _playerInput.InputMovement.x * cameraRight + _playerInput.InputMovement.y * cameraForward;
            Vector3 rootTarget = _velocityRootMotion + positionTarget;
            positionTarget.y = 0;
            rootTarget.y = 0;

            Rotation(positionTarget);

            if (OnSlope(out RaycastHit hit))
            {
                rootTarget = Vector3.ProjectOnPlane(rootTarget, hit.normal);
                _rigidbody.AddForce(rootTarget.normalized * (_speedOnSlope * 100f * Time.fixedDeltaTime));
                _velocityRootMotion = Vector3.zero;
                return;
            }

            _rigidbody.AddForce(rootTarget.normalized * (_speed * 100f * Time.fixedDeltaTime));
            _velocityRootMotion = Vector3.zero;
        }

        private Vector3 NormalizeVector3(Vector3 vector)
        {
            Vector3 newVector = vector;
            newVector.y = 0;
            return newVector.normalized;
        }

        private void Rotation(Vector3 positionTarget)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(positionTarget.normalized);
            Quaternion newRotation = Quaternion.Slerp(transform.rotation, desiredRotation, _slerpSmoothness * Time.fixedDeltaTime);
            _rigidbody.MoveRotation(newRotation);
        }

        private bool OnSlope(out RaycastHit hit)
        {
            return Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, _distance, _slopeLayer);
        }

        private void AnimationMovement()
        {
            SmoothVelocity(_playerInput.InputMovement != Vector2.zero);
        }

        private void SmoothVelocity(bool isMoving)
        {
            Vector3 input = _playerInput.InputMovement;
            input.Normalize();
            if (isMoving)
            {
                _animatorController.SetFloat("Vertical", input.y, _dampSmoothnessRun, Time.deltaTime);
                _animatorController.SetFloat("Horizontal", input.x, _dampSmoothnessRun, Time.deltaTime);
                _animatorController.SetFloat("InputMagnitude", input.magnitude, _dampSmoothnessRun, Time.deltaTime);
                return;
            }

            _animatorController.SetFloat("InputMagnitude", 0, _dampSmoothnessRun, Time.deltaTime);
            _animatorController.SetFloat("Vertical", 0, _dampSmoothnessIdle, Time.deltaTime);
            _animatorController.SetFloat("Horizontal", 0, _dampSmoothnessIdle, Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            var from = transform.position + Vector3.up;
            Gizmos.DrawLine(from, from + Vector3.down * _distance);
        }
    }
}