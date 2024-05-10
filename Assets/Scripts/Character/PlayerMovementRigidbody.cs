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
        [SerializeField] private float _jump = 0.2f;
        [SerializeField, Range(0.01f, 1f)] private float _distanceUp = 0.17f;
        [SerializeField, Range(0.01f, 1f)] private float _distanceForward = 0.4f;
        [SerializeField] private float _distanceToDownInSlopeForward = .7f;
        [SerializeField, Range(0.01f, 0.2f)] private float _lineDistance;
        [SerializeField, Range(1, 30)] private int _totalRays;
        [SerializeField] private float _slopeDistanceDown = 1.4f;
        [SerializeField] private float _slopeDistanceForward = 1.4f;
        [SerializeField] private float _speedOnSlope = 10f;
        [SerializeField] private float _slopeAngleLimit = 85f;
        [SerializeField] private LayerMask _slopeDownLayer;
        [SerializeField] private LayerMask _slopeForwardLayer;
        private Vector3[] _rayPositions;
        private RaycastHit[] _forwardHit;
        private RaycastHit _downHit;
        private bool _onSlope;

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
            _rayPositions = new Vector3[_totalRays];
            _forwardHit = new RaycastHit[_totalRays];
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
            _onSlope = OnSlope();
            MoveToSlopeForward();
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

            if (_onSlope)
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

        private void MoveToSlopeForward()
        {
            if (_playerInput.InputMovement.sqrMagnitude <= 0f) return;
            if (_onSlope)
            {
                return;
            }

            foreach (var predicate in IsSlopeForward(out int index))
            {
                if (!predicate)
                {
                    continue;
                }

                Vector3 origin = _forwardHit[index].point + Vector3.up * _distanceUp + _myTransform.forward * _distanceForward;
                bool desiredPosition = Physics.Raycast(origin, Vector3.down, out RaycastHit hit, _distanceToDownInSlopeForward, _slopeDownLayer);

                if (!desiredPosition)
                {
                    Debug.DrawRay(origin, Vector3.down * _distanceToDownInSlopeForward, Color.red);
                    continue;
                }
                
                Debug.DrawRay(origin, Vector3.down * _distanceToDownInSlopeForward, Color.green);
                _rigidbody.velocity = Vector3.up * _jump;
            }
        }

        private bool[] IsSlopeForward(out int index)
        {
            bool[] result = new bool[_totalRays];
            _forwardHit = new RaycastHit[_totalRays];

            Vector3 from = _myTransform.position + Vector3.up * _playerHeight;
            Vector3 rightDirection = _myTransform.right;
            Vector3 startRayPosition = from - rightDirection * (_lineDistance * (_totalRays - 1) / 2f);
            index = 0;

            for (int i = 0; i < _totalRays; i++)
            {
                Vector3 origin = startRayPosition + rightDirection * (_lineDistance * i);
                result[i] = Physics.Raycast(origin, _myTransform.forward, out var hit, _slopeDistanceForward, _slopeForwardLayer);
                _rayPositions[i] = origin;
                _forwardHit[i] = hit;
                index = i;
            }

            return result;
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

            Gizmos.color = Color.green;
            Vector3 rightDirection = tr.right;
            Vector3 startRayPosition = from - rightDirection * (_lineDistance * (_totalRays - 1) / 2f);

            for (int i = 0; i < _totalRays; i++)
            {
                Vector3 from2 = startRayPosition + rightDirection * (_lineDistance * i);
                Gizmos.DrawRay(from2, tr.forward * _slopeDistanceForward);
            }
        }

        #endregion
    }
}