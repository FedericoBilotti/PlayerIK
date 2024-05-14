using UnityEngine;

namespace Sensor
{
    public class ClimbSensor : ISensor
    {
        private readonly float _rayLength;
        private readonly float _offset;
        private readonly float _angle;
        private readonly int _totalRays;
        private readonly Transform _myTransform;
        private readonly float _playerHeight;
        private readonly float _startRay;
        private readonly LayerMask _climbLayer;
        
        public RaycastHit Hit => _hit;
        private RaycastHit _hit;
        
        public bool OnCollision { get; private set; }

        public ClimbSensor(Transform myTransform, float playerHeight, float startRay, float rayLength, float offset, float angle, int totalRays, LayerMask climbLayer)
        {
            _rayLength = rayLength;
            _offset = offset;
            _angle = angle;
            _totalRays = totalRays;
            _myTransform = myTransform;
            _playerHeight = playerHeight;
            _startRay = startRay;
            _climbLayer = climbLayer;
        }
        
        public void Execute()
        {
            OnCollision = IsClimbable();
        }

        private bool IsClimbable()
        {
            Vector3 from = _myTransform.position + Vector3.up * _playerHeight;
            Vector3 forward = _myTransform.forward;
            Vector3 startPos = from + Vector3.up * _startRay;

            for (int i = 0; i < _totalRays; i++)
            {
                float angle = i * (_angle / 3);
                Vector3 rayDirection = Quaternion.Euler(0, angle + _offset, 0) * forward;
                if (Physics.Raycast(startPos, rayDirection, out _hit, _rayLength, _climbLayer))
                {
                    return true;
                }
            }

            return false;
        }
    }
}