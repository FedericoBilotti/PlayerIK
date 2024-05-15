using Character;
using UnityEngine;

namespace Sensor
{
    public class SlopeSensor : ISensor
    {
        private readonly Transform _myTransform;
        private readonly float _playerHeight;
        private readonly float _rayLength;
        private readonly float _slopeAngleLimit;
        private readonly LayerMask _slopeLayer;

        public bool OnCollision { get; private set; }

        public RaycastHit Hit => _hit;
        private RaycastHit _hit;

        public SlopeSensor(Transform myTransform, float playerHeight, float rayLength, float slopeAngleLimit, LayerMask slopeLayer)
        {
            _myTransform = myTransform;
            _playerHeight = playerHeight;
            _rayLength = rayLength;
            _slopeLayer = slopeLayer;
            _slopeAngleLimit = slopeAngleLimit;
        }

        public void Execute()
        {
            OnCollision = OnSlope();
        }

        private bool OnSlope()
        {
            Vector3 from = _myTransform.position + Vector3.up * _playerHeight;

            if (Physics.Raycast(from, Vector3.down, out _hit, _rayLength, _slopeLayer))
            {
                float angle = Vector3.Angle(Vector3.up, _hit.normal);

                if (angle < _slopeAngleLimit && angle != 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}