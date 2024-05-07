using UnityEngine;

namespace Character
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float _speed;
        [SerializeField] private float _slerpSmoothness = 10f;
        [SerializeField, Range(0f, 1f)] private float _dampSmoothnessRun = .5f;
        [SerializeField, Range(0f, 1f)] private float _dampSmoothnessIdle = .25f;

        private PlayerAnimatorController _playerAnimatorController;
        private PlayerInput _playerInput;
        private Rigidbody _rigidbody;
        private Transform _cameraTransform;
        private Transform _myTransform;

        private void Awake()
        {
            _playerAnimatorController = GetComponent<PlayerAnimatorController>();
            _playerInput = GetComponent<PlayerInput>();
            _rigidbody = GetComponent<Rigidbody>();

            _cameraTransform = Camera.main.transform;
            _myTransform = transform;
        }

        private void Update()
        {
            AnimationMovement();
        }

        public void FixedUpdate()
        {
            MoveAndRotateCharacter();
        }

        private void MoveAndRotateCharacter()
        {
            if (_playerInput.InputMovement == Vector2.zero) return;

            Vector3 positionTarget = _playerInput.InputMovement.x * _cameraTransform.right;
            positionTarget += _playerInput.InputMovement.y * _cameraTransform.forward;
            positionTarget.y = 0;
            
            // Vector3 p = Vector3.zero; 
            // if (Physics.Raycast(_myTransform.position, -_myTransform.up * 2f, out RaycastHit hit, 2f, 6))
            // {
            //     p = Vector3.ProjectOnPlane(hit.point, hit.normal);
            // }
            
            _rigidbody.AddForce(positionTarget.normalized * (_speed * 100f * Time.fixedDeltaTime));
            _rigidbody.MoveRotation(Quaternion.Slerp(_myTransform.rotation, Quaternion.LookRotation(positionTarget), _slerpSmoothness * Time.fixedDeltaTime));
        }

        private void AnimationMovement()
        {
            SmoothVelocity(_playerInput.InputMovement != Vector2.zero);
        }

        private void SmoothVelocity(bool isMoving)
        {
            if (isMoving)
            {
                Vector3 input = _playerInput.InputMovement;
                input.Normalize();

                _playerAnimatorController.SetFloat("Vertical", input.y, _dampSmoothnessRun, Time.deltaTime);
                _playerAnimatorController.SetFloat("Horizontal", input.x, _dampSmoothnessRun, Time.deltaTime);
                return;
            }

            _playerAnimatorController.SetFloat("Vertical", 0, _dampSmoothnessIdle, Time.deltaTime);
            _playerAnimatorController.SetFloat("Horizontal", 0, _dampSmoothnessIdle, Time.deltaTime);
        }
    }
}