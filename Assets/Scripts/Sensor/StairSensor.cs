using UnityEngine;

namespace Sensor
{
    public class StairSensor : ISensor
    {
        private readonly Transform _myTransform;
        private readonly float _playerHeight;
        private readonly float _stairHeight;
        private readonly float _stairRayLowerLength;
        private readonly float _stairRayUpperLength;
        private readonly float _startRayLower;
        private readonly float _startRayUpper;
        private readonly float _angleRays;
        private readonly float _offset;
        private readonly int _totalRays;
        private readonly LayerMask _stairLayer;

        public RaycastHit Hit => _hit;
        private RaycastHit _hit;

        public bool OnCollision { get; private set; }

        public StairSensor(Transform myTransform, float playerHeight, float stairHeight, float stairRayLowerLength, float stairRayUpperLength, float startRayLower,
                float startRayUpper, int totalRays, float angleRays, float offset, LayerMask stairLayer)
        {
            _myTransform = myTransform;
            _playerHeight = playerHeight;
            _stairHeight = stairHeight;
            _stairRayLowerLength = stairRayLowerLength;
            _stairRayUpperLength = stairRayUpperLength;
            _startRayLower = startRayLower;
            _startRayUpper = startRayUpper;
            _stairLayer = stairLayer;
            _angleRays = angleRays;
            _offset = offset;
            _totalRays = totalRays;
        }

        public void Execute()
        {
            OnCollision = StairsDetect();
        }

        private bool StairsDetect()
        {
            Vector3 pos = _myTransform.position;
            Vector3 forward = _myTransform.forward;
            Vector3 fromLower = pos + Vector3.up * (_playerHeight * _startRayLower);
            Vector3 fromUpper = pos + Vector3.up * (_playerHeight * _startRayUpper * _stairHeight);

            for (int i = 0; i < _totalRays; i++)
            {
                float angle = i * (_angleRays / _totalRays);
                Vector3 rayDirection = Quaternion.Euler(0, angle + _offset, 0) * forward;

                if (Physics.Raycast(fromLower, rayDirection, out var hit, _stairRayLowerLength, _stairLayer))
                {
                    return !Physics.Raycast(fromUpper, rayDirection, out _hit, _stairRayUpperLength, _stairLayer);
                }
            }

            return false;
        }
    }
}