using System;
using Sensor;
using UnityEngine;

namespace Character
{
    [Serializable]
    public class StairsMovement
    {
        [Header("Stairs Values")]
        [SerializeField, Range(0f, 1f)] private float _stairHeight = 0.2f;

        [SerializeField, Range(0.01f, 100f)] private float _moveSmoothness = 2f;
        [SerializeField] private LayerMask _stairLayer;

        [Header("Rays Stairs Check")]
        [SerializeField, Range(0, 10)] private int _totalRays = 3;

        [SerializeField, Range(1f, 360f)] private float _angleRays;
        [SerializeField, Range(0f, 360f)] private float _offset;
        [SerializeField, Range(0f, 1f)] private float _stairRayUpperLength = 0.2f;
        [SerializeField, Range(-5f, 5f)] private float _startRayUpper;
        [SerializeField, Range(0f, 1f)] private float _stairRayLowerLength = 0.2f;
        [SerializeField, Range(-5f, 5f)] private float _startRayLower;

        private float _playerHeight;
        private Transform _myTransform;
        private Rigidbody _rigidbody;

        private ISensor _stairSensor;

        public void Initialize(Rigidbody rigidbody, Transform transform, float playerHeight)
        {
            _rigidbody = rigidbody;
            _myTransform = transform;
            _playerHeight = playerHeight;

            _stairSensor = new StairSensor(_myTransform, _playerHeight, _stairHeight, _stairRayLowerLength, _stairRayUpperLength,
                    _startRayLower, _startRayUpper, _totalRays, _angleRays, _offset, _stairLayer);
        }

        public void UpdateSensor()
        {
            _stairSensor.Execute();
        }

        public void MoveInStairs()
        {
            _rigidbody.position += new Vector3(0, _moveSmoothness, 0) * Time.fixedDeltaTime;
        }

        public ISensor GetStairsSensor() => _stairSensor;

        public void DrawStairs()
        {
            Vector3 pos = _myTransform.position;
            Vector3 forward = _myTransform.forward;
            Vector3 fromLower = pos + Vector3.up * (_playerHeight * _startRayLower);
            Vector3 fromUpper = pos + Vector3.up * (_playerHeight * _startRayUpper * _stairHeight);
            Gizmos.color = Color.blue;

            for (int i = 0; i < _totalRays; i++)
            {
                float angle = i * (_angleRays / _totalRays);
                Vector3 rayDirection = Quaternion.Euler(0, angle + _offset, 0) * forward;

                Gizmos.DrawRay(fromLower, rayDirection * _stairRayLowerLength);
                Gizmos.DrawRay(fromUpper, rayDirection * _stairRayUpperLength);
            }
        }
    }
}