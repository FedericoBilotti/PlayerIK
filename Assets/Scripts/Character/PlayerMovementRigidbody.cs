using System.Numerics;
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

        [Header("Wall Check")]
        [SerializeField, Range(1, 30)] private int _totalRays = 8;
        [SerializeField, Range(0.1f, 4f)] private float _wallDistanceForward = .25f;
        [SerializeField, Range(0.01f, 1f)] private float _lineDistance = 0.05f;
        [SerializeField] private LayerMask _wallForwardLayer;
        private RaycastHit[] _wallHits;

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

        private bool _wallForward;
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
            foreach (bool predicate in IsWallForward(out int index))
            {
                _wallForward = predicate;
            }

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

        private bool[] IsWallForward(out int index)
        {
            bool[] result = new bool[_totalRays];
            _wallHits = new RaycastHit[_totalRays];

            Vector3 from = _myTransform.position + Vector3.up;
            Vector3 upDirection = _myTransform.up;
            Vector3 startRayPosition = from + Vector3.up - upDirection * (_lineDistance * (_totalRays - 1) / 2f);
            index = 0;

            for (int i = 0; i < _totalRays; i++)
            {
                Vector3 origin = startRayPosition + upDirection * (_lineDistance * i);
                result[i] = Physics.Raycast(origin, _myTransform.forward, out RaycastHit hit, _wallDistanceForward, _wallForwardLayer);
                _wallHits[i] = hit;
                index = i;
            }

            return result;
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

            Gizmos.color = Color.blue;

            Vector3 upDirection = tr.up;
            Vector3 startRayPosition = position + Vector3.up - upDirection * (_lineDistance * (_totalRays - 1) / 2f);

            for (int i = 0; i < _totalRays; i++)
            {
                Vector3 from2 = startRayPosition + upDirection * (_lineDistance * i);
                Gizmos.DrawRay(from2, tr.forward * _wallDistanceForward);
            }
        }

        #endregion
    }
}