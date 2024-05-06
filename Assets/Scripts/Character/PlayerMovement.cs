using UnityEngine;

namespace Character
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        private Transform _cameraTransform;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _cameraTransform = Camera.main.transform;
        }

        public void FixedUpdate()
        {
            MoveCharacter();
        }

        private void MoveCharacter()
        {
            
        }
    }
}