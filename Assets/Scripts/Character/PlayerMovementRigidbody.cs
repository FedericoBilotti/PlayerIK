using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

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

        [Header("Slope Check")]
        [SerializeField] private float _slopeDistanceDown = 1.4f;
        [SerializeField] private float _speedOnSlope = 10f;
        [SerializeField] private float _downSpeedOnSlope = 5f;
        [SerializeField] private float _slopeAngleLimit = 85f;
        [SerializeField] private LayerMask _slopeDownLayer;
        private RaycastHit _slopeHit;

        [Header("Ground check")]
        [SerializeField] private float _distanceToGround = 0.5f;

        [SerializeField] private LayerMask _groundLayer;

        private AnimatorController _animatorController;
        private PlayerInput _playerInput;
        private Rigidbody _rigidbody;
        private Transform _cameraTransform;
        private Transform _myTransform;

        private bool _onGround;
        private bool _onSlope;
        private Vector3 _velocityRootMotion;

        private void Awake()
        {
            _animatorController = GetComponent<AnimatorController>();
            _playerInput = GetComponent<PlayerInput>();

            _cameraTransform = Camera.main.transform;
            _myTransform = transform;

            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _rigidbody.drag = 5;
        }

        private void Update()
        {
            _rigidbody.drag = _onGround ? 5 : 0;

            ApplyAnimationValues();
        }

        public void FixedUpdate()
        {
            _onGround = OnGround();
            _onSlope = OnSlope();
            MoveAndRotateCharacter(Time.fixedDeltaTime);
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

            if (_onSlope)
            {
                Vector3 direction = Vector3.ProjectOnPlane(rootTarget, _slopeHit.normal);
                _rigidbody.AddForce(direction.normalized * (_speedOnSlope * 100f * fixedDelta), ForceMode.Force);

                if (_rigidbody.velocity.y > 0)
                {
                    _rigidbody.AddForce(Vector3.down * (_downSpeedOnSlope * 100f * fixedDelta), ForceMode.Force);
                }
            }
            else
            {
                _rigidbody.AddForce(rootTarget.normalized * (_speed * 100f * fixedDelta), ForceMode.Force);
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

        private bool OnGround()
        {
            Vector3 from = _myTransform.position + Vector3.up * _playerHeight;
            return Physics.Raycast(from, Vector3.down, out _slopeHit, _distanceToGround, _groundLayer);
        }

        private bool OnSlope()
        {
            Vector3 from = _myTransform.position + Vector3.up * _playerHeight;
            
            if (Physics.Raycast(from, Vector3.down, out _slopeHit, _slopeDistanceDown, _slopeDownLayer))
            {
                float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
                
                if (angle < _slopeAngleLimit && angle != 0)
                {
                    return true;
                }
                
                _rigidbody.AddForce(-_slopeHit.transform.forward * (7f * 100f * Time.fixedDeltaTime), ForceMode.Force);
            }

            return false;
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

            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(from, Vector3.down * _distanceToGround);
        }

        #endregion
    }
}