using UnityEngine;

namespace Sensor
{
    public class GroundSensor : ISensor
    {
        private readonly Transform _myTransform;
        private readonly float _playerHeight;
        private readonly float _rayLength;
        private readonly LayerMask _groundLayer;

        public bool OnCollision { get; private set; }
        public RaycastHit Hit => _hit;
        private RaycastHit _hit;

        public GroundSensor(Transform myTransform, float playerHeight, float rayLength, LayerMask groundLayer)
        {
            _myTransform = myTransform;
            _playerHeight = playerHeight;
            _rayLength = rayLength;
            _groundLayer = groundLayer;
        }

        public void Execute()
        {
            OnCollision = OnGround();
        }

        private bool OnGround()
        {
            Vector3 from = _myTransform.position + Vector3.up * _playerHeight;
            return Physics.Raycast(from, Vector3.down, out _hit, _rayLength, _groundLayer);
        }
    }
}