using UnityEngine;

namespace Character
{
    public class GroundIKController : MonoBehaviour
    {
        [SerializeField] private bool _enabled = true;

        [SerializeField] private float _pelvisOffset;
        [SerializeField, Range(0f, 2f)] private float _heightFromGroundRaycast = 1.14f;
        [SerializeField, Range(0f, 2f)] private float _raycastDownDistance = 1.5f;
        [SerializeField, Range(0f, 100f)] private float _pelvisUpDownSpeed = 0.28f;
        [SerializeField, Range(0f, 100f)] private float _feetToIkPositionSpeed = 0.5f;
        [SerializeField] private LayerMask _environmentLayer;

        [Header("Animator Curves Names")]
        [SerializeField] private string _rightFootName = "RightFootIK";
        [SerializeField] private string _leftFootName = "LeftFootIK";

        [Header("References")]
        [SerializeField] private AnimatorController _animatorController;

        private Vector3 _rightFootPosition;
        private Vector3 _leftFootPosition;
        private Vector3 _leftFootIKPosition;
        private Vector3 _rightFootIKPosition;
        private Quaternion _leftFootIKRotation;
        private Quaternion _rightFootIKRotation;
        private float _lastPelvisPositionY;
        private float _lastRightFootPositionY;
        private float _lastLeftFootPositionY;

        private void Awake()
        {
            _animatorController = GetComponent<AnimatorController>();
        }

        private void FixedUpdate()
        {
            if (!_enabled) return;

            AdjustFeetTarget(ref _rightFootPosition, HumanBodyBones.RightFoot);
            AdjustFeetTarget(ref _leftFootPosition, HumanBodyBones.LeftFoot);

            FeetPositionSolver(_rightFootPosition, ref _rightFootIKPosition, ref _rightFootIKRotation);
            FeetPositionSolver(_leftFootPosition, ref _leftFootIKPosition, ref _leftFootIKRotation);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!_enabled) return;

            MovePelvis();

            WeightFoots();
            MoveToIkPoint(AvatarIKGoal.RightFoot, _rightFootIKPosition, _rightFootIKRotation, ref _lastRightFootPositionY);
            MoveToIkPoint(AvatarIKGoal.LeftFoot, _leftFootIKPosition, _leftFootIKRotation, ref _lastLeftFootPositionY);
        }

        private void MoveToIkPoint(AvatarIKGoal foot, Vector3 footPosition, Quaternion rotationIKHolder, ref float lastFootPositionY)
        {
            Vector3 targetIKPosition = _animatorController.Animator.GetIKPosition(foot);

            if (footPosition != Vector3.zero)
            {
                targetIKPosition = transform.InverseTransformPoint(targetIKPosition);
                footPosition = transform.InverseTransformPoint(footPosition);

                float yVariable = Mathf.Lerp(lastFootPositionY, footPosition.y, _feetToIkPositionSpeed);
                targetIKPosition.y += yVariable;

                lastFootPositionY = yVariable;

                targetIKPosition = transform.TransformPoint(targetIKPosition);

                _animatorController.Animator.SetIKRotation(foot, rotationIKHolder);
            }

            _animatorController.Animator.SetIKPosition(foot, targetIKPosition);
        }

        private void MovePelvis()
        {
            if (_rightFootIKPosition == Vector3.zero || _leftFootIKPosition == Vector3.zero || _lastPelvisPositionY == 0f)
            {
                _lastPelvisPositionY = _animatorController.Animator.bodyPosition.y;
                return;
            }

            Vector3 position = transform.position;
            float rightOffset = _rightFootIKPosition.y - position.y;
            float leftOffset = _leftFootIKPosition.y - position.y;

            float totalOffset = leftOffset < rightOffset ? leftOffset : rightOffset;

            Vector3 pelvisPos = _animatorController.Animator.bodyPosition + Vector3.up * totalOffset;

            pelvisPos.y = Mathf.Lerp(_lastPelvisPositionY, pelvisPos.y, _pelvisUpDownSpeed * Time.deltaTime);

            _animatorController.Animator.bodyPosition = pelvisPos;

            _lastPelvisPositionY = _animatorController.Animator.bodyPosition.y;
        }

        private void FeetPositionSolver(Vector3 footPosition, ref Vector3 footIKPosition, ref Quaternion feetIKRotation)
        {
            Vector3 direction = Vector3.down;
            float distance = _raycastDownDistance + _heightFromGroundRaycast;

            if (Physics.Raycast(footPosition, direction, out RaycastHit hit, distance, _environmentLayer))
            {
                footIKPosition = footPosition;
                footIKPosition.y = hit.point.y + _pelvisOffset;
                feetIKRotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * transform.rotation;

                return;
            }

            footIKPosition = Vector3.zero;
        }

        private void AdjustFeetTarget(ref Vector3 feetPositions, HumanBodyBones foot)
        {
            feetPositions = _animatorController.Animator.GetBoneTransform(foot).position;
            feetPositions.y = transform.position.y + _heightFromGroundRaycast;
        }

        private void WeightFoots()
        {
            _animatorController.Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            _animatorController.Animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, _animatorController.Animator.GetFloat(_rightFootName));

            _animatorController.Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            _animatorController.Animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, _animatorController.Animator.GetFloat(_leftFootName));
        }

        #region Gizmos
        
        private void OnDrawGizmos()
        {
            if (!_enabled) return;
            
            Gizmos.color = Color.yellow;

            Vector3 rightFoot = _animatorController.Animator.GetBoneTransform(HumanBodyBones.RightFoot).position;
            Vector3 leftFoot = _animatorController.Animator.GetBoneTransform(HumanBodyBones.LeftFoot).position;
            Gizmos.DrawRay(rightFoot, Vector3.down * (_raycastDownDistance * _heightFromGroundRaycast));
            Gizmos.DrawRay(leftFoot, Vector3.down * (_raycastDownDistance * _heightFromGroundRaycast));
        }

        #endregion
    }
}