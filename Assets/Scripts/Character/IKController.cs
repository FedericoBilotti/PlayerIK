using UnityEngine;

namespace Character
{
    public class IKController : MonoBehaviour
    {
        [SerializeField] private bool _enabled = true;
        [SerializeField, Range(0f, 2f)] private float _distance = 1.1f;
        [SerializeField] private LayerMask _groundMask;

        private PlayerAnimatorController _playerAnimatorController;

        private void Awake() => _playerAnimatorController = GetComponentInParent<PlayerAnimatorController>();

        private void FixedUpdate()
        {
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!_enabled) return;

            CalculatePositionIK(AvatarIKGoal.RightFoot, "RightFootIK");
            CalculatePositionIK(AvatarIKGoal.LeftFoot, "LeftFootIK");
        }

        private void CalculatePositionIK(AvatarIKGoal ikGoal, string foot)
        {
            _playerAnimatorController.Animator.SetIKPositionWeight(ikGoal, _playerAnimatorController.GetFloat(foot));
            _playerAnimatorController.Animator.SetIKRotationWeight(ikGoal, _playerAnimatorController.GetFloat(foot));

            var ray = new Ray(_playerAnimatorController.Animator.GetIKPosition(ikGoal) + Vector3.up, Vector3.down);
            if (!Physics.Raycast(ray, out RaycastHit hit, _distance + 1f, _groundMask)) return;
            Debug.DrawLine(_playerAnimatorController.Animator.GetIKPosition(ikGoal) + Vector3.up, _playerAnimatorController.Animator.GetIKPosition(ikGoal) + Vector3.down * _distance, Color.red);

            Vector3 footPos = hit.point;
            footPos.y += _distance;

            _playerAnimatorController.Animator.SetIKPosition(ikGoal, footPos);
            _playerAnimatorController.Animator.SetIKRotation(ikGoal, Quaternion.FromToRotation(Vector3.up, hit.normal) * transform.rotation);
        }
    }
}