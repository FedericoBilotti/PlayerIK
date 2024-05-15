using System;
using Sensor;
using UnityEngine;

namespace Character
{
    [Serializable]
    public class SlopeMovement
    {
        [Header("Slope Check")]
        [SerializeField] private float _maxSpeed = 5f;
        [SerializeField] private float _slopeRayLength = 1.4f;
        [SerializeField] private float _downSpeedOnSlope = 5f;
        [SerializeField] private float _slopeAngleLimit = 85f;
        [SerializeField] private LayerMask _slopeLayer;

        private GroundMovement _groundMovement;
        private Rigidbody _rigidbody;
        private ISensor _slopeSensor;

        public void Initialize(Rigidbody rigidbody, GroundMovement groundMovement, Transform transform, float playerHeight)
        {
            _groundMovement = groundMovement;
            _rigidbody = rigidbody;
            _slopeSensor = new SlopeSensor(transform, playerHeight, _slopeRayLength, _slopeAngleLimit, _slopeLayer);
        }

        public void UpdateSensor()
        {
            _slopeSensor.Execute();
        }

        public void MoveInSlope(Vector3 rootTarget)
        {
            _groundMovement.MoveInGround(GetSlopeDirection(rootTarget).normalized);

            if (_rigidbody.velocity.y > 0)
            {
                _groundMovement.MoveInGround(Vector3.down * _downSpeedOnSlope);
            }
        }

        private Vector3 GetSlopeDirection(Vector3 direction)
        {
            return Vector3.ProjectOnPlane(direction, _slopeSensor.Hit.normal);
        }

        public void RestrictVelocityOnSlope()
        {
            if (_rigidbody.velocity.magnitude < _maxSpeed) return;

            _rigidbody.velocity = _rigidbody.velocity.normalized * _maxSpeed;
        }

        public ISensor GetSlopeSensor() => _slopeSensor;

        public void DrawSlope(Vector3 bodyPosition)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(bodyPosition, Vector3.down * _slopeRayLength);
        }
    }
}