using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Character
{
    public class IKController : MonoBehaviour
    {
        [SerializeField] private bool _enabled = true;
        [SerializeField, Range(0f, 2f)] private float _distance = 1.1f;
        [SerializeField, Range(0f, 1f)] private float _velocityPelvis = .2f;
        [SerializeField] private LayerMask _groundMask;

        private float _lastPelvisPosY;
        private float _pelvisOffset;

        private Transform _transform;
        private PlayerAnimatorController _playerAnimatorController;

        private void Awake() => _playerAnimatorController = GetComponentInParent<PlayerAnimatorController>();
        private void Start() => _transform = transform;

        private void OnAnimatorIK(int layerIndex)
        {
            if (!_enabled) return;

            MovePelvis();

            CalculatePositionIK(AvatarIKGoal.RightFoot, "RightFootIK");
            CalculatePositionIK(AvatarIKGoal.LeftFoot, "LeftFootIK");
        }

        private void CalculatePositionIK(AvatarIKGoal ikGoal, string foot)
        {
            _playerAnimatorController.Animator.SetIKPositionWeight(ikGoal, _playerAnimatorController.GetFloat(foot));
            _playerAnimatorController.Animator.SetIKRotationWeight(ikGoal, _playerAnimatorController.GetFloat(foot));

            var ray = new Ray(_playerAnimatorController.Animator.GetIKPosition(ikGoal) + Vector3.up, Vector3.down);
            if (!Physics.Raycast(ray, out RaycastHit hit, _distance + 1f, _groundMask)) return;

            Vector3 footPos = hit.point;
            footPos.y += _distance;

            _playerAnimatorController.Animator.SetIKPosition(ikGoal, footPos);
            _playerAnimatorController.Animator.SetIKRotation(ikGoal, Quaternion.FromToRotation(Vector3.up, hit.normal) * _transform.rotation);
        }

        private void MovePelvis()
        {
            Vector3 rightFoot = _playerAnimatorController.Animator.GetIKPosition(AvatarIKGoal.RightFoot);
            Vector3 leftFoot = _playerAnimatorController.Animator.GetIKPosition(AvatarIKGoal.LeftFoot);

            Vector3 bodyPosition = _playerAnimatorController.Animator.bodyPosition;

            if (rightFoot == Vector3.zero || leftFoot == Vector3.zero)
            {
                _lastPelvisPosY = bodyPosition.y;
                return;
            }

            float rightOffset = rightFoot.y - _transform.position.y;
            float leftOffset = leftFoot.y - _transform.position.y;

            float totalOffset = rightOffset < leftOffset ? leftOffset : rightOffset;

            Vector3 pelvisPos = bodyPosition + Vector3.up * totalOffset;

            pelvisPos.y = Mathf.Lerp(_lastPelvisPosY, pelvisPos.y, _velocityPelvis);

            _playerAnimatorController.Animator.bodyPosition = pelvisPos;

            _lastPelvisPosY = bodyPosition.y;
        }
    }
}