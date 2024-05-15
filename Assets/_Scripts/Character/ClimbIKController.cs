using UnityEngine;

namespace Character
{
    public class ClimbIKController : MonoBehaviour
    {
        [SerializeField] private bool _enabled;
        [SerializeField] private bool _enabledGizmos = true;

        [SerializeField] private float _pelvisOffset;
        [SerializeField, Range(0f, 100f)] private float _pelvisUpDownSpeed = 58f;
        [SerializeField, Range(0f, 100f)] private float _feetToIkPositionSpeed = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _footRotationWeight, _handRotationWeight;
        [SerializeField] private LayerMask _environmentLayer;
        
        [Header("Raycast")]
        [SerializeField] private float _startPositionRaycast;
        [SerializeField, Range(-2f, 2f)] private float _distanceFromWallRaycast = 1.14f;
        [SerializeField, Range(0f, 2f)] private float _raycastForwardDistance = 1.5f;

        [Header("References")]
        [SerializeField] private AnimatorController _animatorController;

        private Vector3 _rightFootPosition;
        private Vector3 _rightFootIKPosition;
        private Quaternion _rightFootIKRotation;
        private float _lastRightFootPositionZ;

        private Vector3 _leftFootPosition;
        private Vector3 _leftFootIKPosition;
        private Quaternion _leftFootIKRotation;
        private float _lastLeftFootPositionZ;
        
        private Vector3 _rightHandPosition;
        private Vector3 _rightHandIKPosition;
        private Quaternion _rightHandIKRotation;
        private float _lastRightHandPositionZ;
        
        private Vector3 _leftHandPosition;
        private Vector3 _leftHandIKPosition;
        private Quaternion _leftHandIKRotation;
        private float _lastLeftHandPositionZ;
        
        private float _lastPelvisPositionZ;

        private void Awake() => _animatorController = GetComponent<AnimatorController>();

        private void FixedUpdate()
        {
            if (!_enabled) return;

            AdjustTarget(ref _rightFootPosition, HumanBodyBones.RightFoot);
            AdjustTarget(ref _leftFootPosition, HumanBodyBones.LeftFoot);
            
            AdjustTarget(ref _rightHandPosition, HumanBodyBones.RightHand);
            AdjustTarget(ref _leftHandPosition, HumanBodyBones.LeftHand);

            PositionSolver(_rightFootPosition, ref _rightFootIKPosition, ref _rightFootIKRotation);
            PositionSolver(_leftFootPosition, ref _leftFootIKPosition, ref _leftFootIKRotation);
            
            PositionSolver(_rightHandPosition, ref _rightHandIKPosition, ref _rightHandIKRotation);
            PositionSolver(_leftHandPosition, ref _leftHandIKPosition, ref _leftHandIKRotation);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!_enabled) return;

            MovePelvis();

            SetWeightFoots();
            SetWeightHands();
            
            MoveToIkPoint(AvatarIKGoal.RightFoot, _rightFootIKPosition, _rightFootIKRotation, ref _lastRightFootPositionZ);
            MoveToIkPoint(AvatarIKGoal.LeftFoot, _leftFootIKPosition, _leftFootIKRotation, ref _lastLeftFootPositionZ);
            
            MoveToIkPoint(AvatarIKGoal.RightHand, _rightHandIKPosition, _rightHandIKRotation, ref _lastRightHandPositionZ);
            MoveToIkPoint(AvatarIKGoal.LeftHand, _leftHandIKPosition, _leftHandIKRotation, ref _lastLeftHandPositionZ);
        }

        private void MoveToIkPoint(AvatarIKGoal goal, Vector3 ikPosition, Quaternion rotationIKHolder, ref float lastFootPositionZ)
        {
            Vector3 targetIKPosition = _animatorController.Animator.GetIKPosition(goal);

            if (ikPosition != Vector3.zero)
            {
                targetIKPosition = transform.InverseTransformPoint(targetIKPosition);
                ikPosition = transform.InverseTransformPoint(ikPosition);

                float zVariable = Mathf.Lerp(lastFootPositionZ, ikPosition.z, _feetToIkPositionSpeed);
                targetIKPosition.z += zVariable;

                lastFootPositionZ = zVariable;

                targetIKPosition = transform.TransformPoint(targetIKPosition);

                _animatorController.Animator.SetIKRotation(goal, rotationIKHolder);
            }

            _animatorController.Animator.SetIKPosition(goal, targetIKPosition);
        }        
        
        private void MovePelvis()
        {
            if (_rightFootIKPosition == Vector3.zero || _leftFootIKPosition == Vector3.zero || _leftHandIKPosition == Vector3.zero || _rightHandIKPosition == Vector3.zero || _lastPelvisPositionZ == 0f)
            {
                _lastPelvisPositionZ = _animatorController.Animator.bodyPosition.z;
                return;
            }

            Vector3 position = transform.position;
            float rightOffset = _rightFootIKPosition.z - position.z;
            float leftOffset = _leftFootIKPosition.z - position.z;

            float totalOffset = leftOffset < rightOffset ? leftOffset : rightOffset;

            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 pelvisPos = _animatorController.Animator.bodyPosition + forward * totalOffset;

            pelvisPos.z = Mathf.Lerp(_lastPelvisPositionZ, pelvisPos.z, _pelvisUpDownSpeed * Time.deltaTime);

            _animatorController.Animator.bodyPosition = pelvisPos;

            _lastPelvisPositionZ = _animatorController.Animator.bodyPosition.z;
        }

        
        private void PositionSolver(Vector3 position, ref Vector3 goalIKPosition, ref Quaternion goalIKRotation)
        {
            Vector3 direction = transform.TransformDirection(Vector3.forward);
            Vector3 origin = position + direction * _startPositionRaycast;
            float distance = _raycastForwardDistance + _distanceFromWallRaycast;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, _environmentLayer))
            {
                Debug.DrawRay(origin, direction * distance, Color.black);
                goalIKPosition = position;
                goalIKPosition.z = hit.point.z + _pelvisOffset;
                goalIKRotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * transform.rotation;

                return;
            }

            goalIKPosition = Vector3.zero;
        }

        private void AdjustTarget(ref Vector3 positions, HumanBodyBones body)
        {
            positions = _animatorController.Animator.GetBoneTransform(body).position;
            positions.z = transform.position.z + _distanceFromWallRaycast; // Revisar
        }

        private void SetWeightHands()
        {
            _animatorController.Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            _animatorController.Animator.SetIKRotationWeight(AvatarIKGoal.RightHand, _handRotationWeight);

            _animatorController.Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            _animatorController.Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, _handRotationWeight);
        }

        private void SetWeightFoots()
        {
            _animatorController.Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            _animatorController.Animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, _footRotationWeight);

            _animatorController.Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            _animatorController.Animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, _footRotationWeight);
        }

        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        #region Gizmos

        
        private void OnDrawGizmos()
        {
            if (!_enabledGizmos) return;
            
            Gizmos.color = Color.cyan;
        
            Vector3 rightFoot = _animatorController.Animator.GetBoneTransform(HumanBodyBones.RightFoot).position;
            Vector3 leftFoot = _animatorController.Animator.GetBoneTransform(HumanBodyBones.LeftFoot).position;
            Vector3 leftHand = _animatorController.Animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
            Vector3 rightHand = _animatorController.Animator.GetBoneTransform(HumanBodyBones.RightHand).position;
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Gizmos.DrawRay(rightFoot + forward * _startPositionRaycast, forward * (_raycastForwardDistance + _distanceFromWallRaycast));
            Gizmos.DrawRay(leftFoot + forward * _startPositionRaycast, forward * (_raycastForwardDistance + _distanceFromWallRaycast));
            Gizmos.DrawRay(leftHand + forward * _startPositionRaycast, forward * (_raycastForwardDistance + _distanceFromWallRaycast));
            Gizmos.DrawRay(rightHand + forward * _startPositionRaycast, forward * (_raycastForwardDistance + _distanceFromWallRaycast));
        }

        #endregion
    }
}