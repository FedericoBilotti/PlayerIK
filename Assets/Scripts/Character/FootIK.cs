using UnityEngine;

namespace Character
{
    public class FootIK : MonoBehaviour
    {
        [SerializeField] private bool _enabled = true;

        [SerializeField, Range(0f, 2f)] private float _heightFromGroundRaycast = 1.14f;
        [SerializeField, Range(0f, 2f)] private float _raycastDownDistance = 1.5f;
        [SerializeField] private LayerMask _environmentLayer;
        [SerializeField] private float _pelvisOffset;
        [SerializeField, Range(0f, 100f)] private float _pelvisUpDownSpeed = 0.28f;
        [SerializeField, Range(0f, 100f)] private float _feetToIkPositionSpeed = 0.5f;

        [Header("Running")]
        [SerializeField, Range(0f, 100f)] private float _speedFuturePosition = 1f;
        [SerializeField, Range(0f, 1f)] private float _maxFuturePosition = 1f;
        [SerializeField] private float _futurePosition = 1f;

        [SerializeField] private string _rightFootName = "RightFootIK";
        [SerializeField] private string _leftFootName = "LeftFootIK";

        private Vector3 _rightFootPosition;
        private Vector3 _leftFootPosition;
        private Vector3 _leftFootIKPosition;
        private Vector3 _rightFootIKPosition;
        private Quaternion _leftFootIKRotation;
        private Quaternion _rightFootIKRotation;
        private float _lastPelvisPositionY;
        private float _lastRightFootPositionY;
        private float _lastLeftFootPositionY;

        private AnimatorController _animatorController;

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

        private void MoveToIkPoint(AvatarIKGoal foot, Vector3 positionIKHolder, Quaternion rotationIKHolder, ref float lastFootPositionY)
        {
            Vector3 targetIKPosition = _animatorController.Animator.GetIKPosition(foot);

            if (positionIKHolder != Vector3.zero)
            {
                targetIKPosition = transform.InverseTransformPoint(targetIKPosition);
                positionIKHolder = transform.InverseTransformPoint(positionIKHolder);

                float yVariable = Mathf.Lerp(lastFootPositionY, positionIKHolder.y, _feetToIkPositionSpeed);
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

        private void FeetPositionSolver(Vector3 footPosition, ref Vector3 feetIKPosition, ref Quaternion feetIKRotation)
        {
            _futurePosition = Mathf.Lerp(_futurePosition, _maxFuturePosition, _speedFuturePosition * Time.deltaTime); // Chequear si estoy quieto o corriendo. Depende qué hago X o Y cosa
            Vector3 origin = footPosition + transform.forward * _futurePosition;
            Vector3 direction = Vector3.down;
            float distance = _raycastDownDistance * _heightFromGroundRaycast;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, _environmentLayer))
            {
                feetIKPosition = footPosition;
                feetIKPosition.y = hit.point.y + _pelvisOffset;
                feetIKRotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * transform.rotation;

                return;
            }

            feetIKPosition = Vector3.zero;
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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;

            Vector3 forward = transform.forward;
            Vector3 originRightFoot = _rightFootPosition + forward * _futurePosition;
            Vector3 originLeftFoot = _leftFootPosition + forward * _futurePosition;
            Gizmos.DrawLine(originRightFoot, originRightFoot + Vector3.down * (_raycastDownDistance * _heightFromGroundRaycast));
            Gizmos.DrawLine(originLeftFoot, originLeftFoot + Vector3.down * (_raycastDownDistance * _heightFromGroundRaycast));
        }
    }
}