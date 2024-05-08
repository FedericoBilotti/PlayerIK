using UnityEngine;

namespace Character
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _speed = 1f;
        [SerializeField] private float _slerpSmoothness = 10f;
        [SerializeField, Range(0f, 1f)] private float _dampSmoothnessRun = .5f;
        [SerializeField, Range(0f, 1f)] private float _dampSmoothnessIdle = .25f;

        [Header("Ground check")]
        [SerializeField] private float _distance = 2f;
        [SerializeField] private LayerMask _groundLayer;

        private PlayerAnimatorController _playerAnimatorController;
        private PlayerInput _playerInput;
        private Rigidbody _rigidbody;
        private Transform _cameraTransform;

        private Vector3 _velocityRootMotion;

        private void Awake()
        {
            _playerAnimatorController = GetComponent<PlayerAnimatorController>();
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
            _velocityRootMotion += _playerAnimatorController.Animator.deltaPosition;
        }

        private void MoveAndRotateCharacter()
        {
            if (_playerInput.InputMovement == Vector2.zero) return;

            Vector3 positionTarget = _playerInput.InputMovement.x * _cameraTransform.right + _playerInput.InputMovement.y * _cameraTransform.forward;
            Vector3 rootTarget = _velocityRootMotion + positionTarget;
            positionTarget.y = 0;
            rootTarget.y = 0;

            rootTarget = Vector3.ProjectOnPlane(rootTarget, GetGroundNormal());
            
            Quaternion desiredRotation = Quaternion.LookRotation(positionTarget.normalized);
            Quaternion newRotation = Quaternion.Slerp(transform.rotation, desiredRotation, _slerpSmoothness * Time.fixedDeltaTime);
            _rigidbody.MoveRotation(newRotation); 

            _rigidbody.AddForce(rootTarget.normalized * (_speed * 100f * Time.fixedDeltaTime)); 
            
            _velocityRootMotion = Vector3.zero;
        }
        
        private Vector3 GetGroundNormal()
        {
            Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, _distance, _groundLayer);
            return hit.normal;
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

                _playerAnimatorController.SetFloat("Vertical", input.y, _dampSmoothnessRun, Time.deltaTime);
                _playerAnimatorController.SetFloat("Horizontal", input.x, _dampSmoothnessRun, Time.deltaTime);
                _playerAnimatorController.SetFloat("InputMagnitude", input.magnitude, _dampSmoothnessRun, Time.deltaTime);
                return;
            }

            _playerAnimatorController.SetFloat("InputMagnitude", 0, _dampSmoothnessRun, Time.deltaTime);
            _playerAnimatorController.SetFloat("Vertical", 0, _dampSmoothnessIdle, Time.deltaTime);
            _playerAnimatorController.SetFloat("Horizontal", 0, _dampSmoothnessIdle, Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            var from = transform.position + Vector3.up;
            Gizmos.DrawLine(from, from + Vector3.down * _distance);
        }
    }
}