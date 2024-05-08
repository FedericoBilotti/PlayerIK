using System;
using UnityEngine;

namespace Character
{
    public class IKController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerAnimatorController _playerAnimatorController;

        [Header("Settings")]
        [SerializeField] private bool _enabled = true;

        [SerializeField] private float _offsetDistance = 1f;
        [SerializeField, Range(0f, 2f)] private float _distance = 1.1f;
        [SerializeField] private float _velocityPelvis = .2f;
        [SerializeField] private LayerMask _groundMask;

        [Header("Gizmos")]
        [SerializeField] private bool _drawGizmos = true;

        [SerializeField] private Color _color = Color.blue;

        private float _lastPelvisPosY;
        private float _pelvisOffset;

        private Transform _transform;

        private void Awake() => _playerAnimatorController = GetComponent<PlayerAnimatorController>();
        private void Start() => _transform = transform;

        private void FixedUpdate()
        {
            if (!_enabled) return;
        }

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

            Vector3 ikPosition = _playerAnimatorController.Animator.GetIKPosition(ikGoal) + Vector3.up;
            var ray = new Ray(ikPosition, Vector3.down);

            if (!Physics.Raycast(ray, out RaycastHit hit, _offsetDistance, _groundMask)) return;

            Vector3 footPos = hit.point;
            footPos.y += _distance;

            _playerAnimatorController.Animator.SetIKPosition(ikGoal, footPos);
            _playerAnimatorController.Animator.SetIKRotation(ikGoal, Quaternion.FromToRotation(Vector3.up, hit.normal) * _transform.rotation);
        }

        private void MovePelvis()
        {
            Vector3 rightFoot = _playerAnimatorController.Animator.GetIKPosition(AvatarIKGoal.RightFoot);
            Vector3 leftFoot = _playerAnimatorController.Animator.GetIKPosition(AvatarIKGoal.LeftFoot);

            if (rightFoot == Vector3.zero || leftFoot == Vector3.zero)
            {
                _lastPelvisPosY = _playerAnimatorController.Animator.bodyPosition.y;
                return;
            }

            Vector3 position = _transform.position;
            float rightOffset = rightFoot.y - position.y;
            float leftOffset = leftFoot.y - position.y;

            float totalOffset = leftOffset < rightOffset ? leftOffset : rightOffset;

            Vector3 pelvisPos = _playerAnimatorController.Animator.bodyPosition + Vector3.up * totalOffset;

            pelvisPos.y = Mathf.Lerp(_lastPelvisPosY, pelvisPos.y, _velocityPelvis * Time.deltaTime);

            _playerAnimatorController.Animator.bodyPosition = pelvisPos;

            _lastPelvisPosY = _playerAnimatorController.Animator.bodyPosition.y;
        }

        private void OnDrawGizmos()
        {
            if (!_drawGizmos) return;
            {
                Gizmos.color = _color;  
                
                Vector3 fromRight = _playerAnimatorController.Animator.GetBoneTransform(HumanBodyBones.RightFoot).position + Vector3.up;
                Vector3 fromLeft = _playerAnimatorController.Animator.GetBoneTransform(HumanBodyBones.LeftFoot).position + Vector3.up;
                
                Gizmos.DrawLine(fromRight, fromRight + Vector3.down * _offsetDistance);
                Gizmos.DrawLine(fromLeft , fromLeft + Vector3.down * _offsetDistance);
            }
        }
    }
}