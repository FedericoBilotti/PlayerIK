using UnityEngine;

namespace Sensor
{
    public class GroundSensor : ISensor
    {
        private readonly Transform _myTransform;
        private readonly float _playerHeight;
        private readonly float _radius;
        private readonly float _maxDistance;
        private readonly LayerMask _groundLayer;

        public bool OnCollision { get; private set; }
        public RaycastHit Hit => _hit;
        private RaycastHit _hit;

        public GroundSensor(Transform myTransform, float playerHeight, float radius, float maxDistance, LayerMask groundLayer)
        {
            _myTransform = myTransform;
            _playerHeight = playerHeight;
            _radius = radius;
            _maxDistance = maxDistance;
            _groundLayer = groundLayer;
        }

        public void Execute()
        {
            OnCollision = OnGround();
        }

        private bool OnGround()
        {
            Vector3 from = _myTransform.position + Vector3.up * _playerHeight;
            return Physics.SphereCast(from, _radius, Vector3.down, out _hit, _maxDistance, _groundLayer);
            //return Physics.CheckSphere(from, _radius, _groundLayer);
        }
    }
}