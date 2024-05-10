using UnityEngine;

namespace Character
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovementRigidbody : MonoBehaviour
    {
        [Header("Settings")]
        [Header("Player")]
        [SerializeField] private float _playerHeight = 0.5f;
        
        [Header("Velocity")]
        [SerializeField] private float _speed = 10f;
        [SerializeField, Range(0f, 1f)] private float _dampSmoothness = .1f;

        [Header("Rotations")]
        [SerializeField, Range(0f, 1f)] private float _allowRotation = 0.3f;
        [SerializeField] private float _smoothnessRotation = 10f;
        private float _actualSpeed;

        [Header("Slope")]
        [SerializeField] private float _lineDistance;
        [SerializeField, Range(1, 20)] private int _totalRays;
        [SerializeField] private float _slopeDistanceDown = 1.4f;
        [SerializeField] private float _slopeDistanceForward = 1.4f;
        [SerializeField] private float _speedOnSlope = 10f;
        [SerializeField] private float _slopeAngleLimit = 85f;
        [SerializeField] private LayerMask _slopeDownLayer;
        [SerializeField] private LayerMask _slopeForwardLayer;
        private RaycastHit _forwardHit;
        private RaycastHit _downHit;

        [Header("Ground check")]
        [SerializeField] private LayerMask _groundLayer;

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
            _cameraTransform = Camera.main.transform;
            _myTransform = transform;

            ConfigurateRigidbody();
        }

        private void Update()
        {
            ApplyAnimationValues();
        }

        public void FixedUpdate()
        {
            IsSlopeForward();
            MoveAndRotateCharacter(Time.fixedDeltaTime);
        }

        private void ConfigurateRigidbody()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _rigidbody.drag = 5;
        }

        private void OnAnimatorMove()
        {
            _velocityRootMotion += _animatorController.Animator.deltaPosition;
        }

        private void MoveAndRotateCharacter(float fixedDelta)
        {
            if (_actualSpeed < _allowRotation) return;

            Vector3 cameraForward = NormalizeVector3(_cameraTransform.forward);
            Vector3 cameraRight = NormalizeVector3(_cameraTransform.right);

            Vector3 positionTarget = _playerInput.InputMovement.x * cameraRight + _playerInput.InputMovement.y * cameraForward;
            positionTarget.y = 0;

            Rotation(positionTarget);

            Vector3 rootTarget = _velocityRootMotion + positionTarget;

            if (OnSlope())
            {
                rootTarget = Vector3.ProjectOnPlane(rootTarget, _downHit.normal);
                _rigidbody.AddForce(rootTarget.normalized * (_speedOnSlope * 100f * fixedDelta), ForceMode.Acceleration);
            }
            else
            {
                _rigidbody.AddForce(rootTarget.normalized * (_speed * 100f * fixedDelta), ForceMode.Acceleration);
            }

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
            Quaternion newRotation = Quaternion.Slerp(_myTransform.rotation, desiredRotation, _smoothnessRotation * Time.fixedDeltaTime);
            _rigidbody.rotation = newRotation;
        }

        private bool IsSlopeForward()
        {
            Vector3 from = _myTransform.position + Vector3.up * _playerHeight;
            return Physics.Raycast(from, _myTransform.forward, out _forwardHit, _slopeDistanceForward, _slopeForwardLayer);
        }

        private bool OnSlope()
        {
            Vector3 from = _myTransform.position + Vector3.up * _playerHeight;
            return Physics.Raycast(from, Vector3.down, out _downHit, _slopeDistanceDown, _slopeDownLayer);
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
            Gizmos.color = Color.red;
            var tr = transform;
            Vector3 position = tr.position;
            Vector3 from = position + Vector3.up * _playerHeight;
            Gizmos.DrawRay(from, Vector3.down * _slopeDistanceDown);
            Gizmos.DrawRay(from , tr.forward * _slopeDistanceForward);

            Gizmos.color = Color.green;
            Vector3 rightDirection = tr.right;
            Vector3 startRayPosition = from - rightDirection * (_lineDistance * (_totalRays - 1) / 2f);

            for (int i = 0; i < _totalRays; i++)
            {
                Vector3 from2 = startRayPosition + rightDirection * (_lineDistance * i);
                Gizmos.DrawRay(from2, tr.forward);
            }
        }
        
        #endregion
    }
}