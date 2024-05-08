using System;
using UnityEngine;
using UnityEditor;

namespace Character
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float _speed = 1f;
        [SerializeField] private float _slerpSmoothness = 10f;
        [SerializeField, Range(0f, 1f)] private float _dampSmoothnessRun = .5f;
        [SerializeField, Range(0f, 1f)] private float _dampSmoothnessIdle = .25f;

        private PlayerAnimatorController _playerAnimatorController;
        private PlayerInput _playerInput;
        private Rigidbody _rigidbody;
        private Transform _cameraTransform;
        private Transform _myTransform;

        private Vector3 _rootMotion;

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

        private void OnAnimatorMove()
        {
            _rootMotion += _playerAnimatorController.Animator.deltaPosition;
        }

        private void MoveAndRotateCharacter()
        {
            if (_playerInput.InputMovement == Vector2.zero) return;

            Vector3 positionTarget = _playerInput.InputMovement.x * _cameraTransform.right;
            positionTarget += _playerInput.InputMovement.y * _cameraTransform.forward;
            positionTarget += _rootMotion;
            positionTarget.y = 0;

            //positionTarget = Vector3.ProjectOnPlane(positionTarget, GetGroundNormal());
            
            _rigidbody.AddForce(positionTarget * (_speed * Time.fixedDeltaTime));
            //_rigidbody.MoveRotation(Quaternion.Slerp(_myTransform.rotation, Quaternion.LookRotation(positionTarget), _slerpSmoothness * Time.fixedDeltaTime));

            _rootMotion = Vector3.zero; // Reiniciar la velocidad de la animacion
        }
        
        private Vector3 GetGroundNormal()
        {
            Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f, 6);
            return hit.normal;
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