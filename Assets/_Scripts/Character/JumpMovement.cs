using System;
using Sensor;
using UnityEngine;
using Utilities;

namespace Character
{
    [Serializable]
    public class JumpMovement
    {
        [Header("Jump")]
        [SerializeField] private float _jumpForce = 10f;
        [SerializeField] private float _jumpDelay = .4f;
        
        private Timer _jumpTimer;
        private Rigidbody _rigidbody;
        
        public void Initialize(Rigidbody rigidbody)
        {
            _rigidbody = rigidbody;
            
            CreateTimer();
        }

        public void OnUpdate()
        {
            UpdateTimer();
        }

        public void Jump()
        {
            _rigidbody.velocity = Vector3.up * (_jumpForce * 100f * Time.fixedDeltaTime);
        }
        
        private void CreateTimer() => _jumpTimer = new CountdownTimer(_jumpDelay);
        private void UpdateTimer() => _jumpTimer.Tick(Time.deltaTime);
        public Timer GetJumpTimer() => _jumpTimer;
    }
}