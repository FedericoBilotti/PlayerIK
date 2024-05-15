using System;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Character
{
    public class ClimbIKController : MonoBehaviour
    {
        [SerializeField] private bool _enabled;

        [SerializeField] private float _pelvisOffset;
        [SerializeField, Range(0f, 100f)] private float _pelvisUpDownSpeed = 58f;
        [SerializeField, Range(0f, 100f)] private float _feetToIkPositionSpeed = 0.5f;
        [SerializeField] private float _distanceHandsWall;
        [SerializeField] private float _distanceFootsWall = 0.02f;
        [SerializeField] private float _footRotationWeight, _handRotationWeight;
        [SerializeField, Range(-2f, 2f)] private float _distanceFromGroundRaycast = 1.14f;
        [SerializeField, Range(0f, 2f)] private float _raycastForwardDistance = 1.5f;
        [SerializeField] private LayerMask _environmentLayer;

        [Header("References")]
        [SerializeField] private AnimatorController _animatorController;


        [Header("Animator Curves Names")]
        [SerializeField] private string _rightFootName = "RightFootIK";
        [SerializeField] private string _leftFootName = "LeftFootIK";
        [SerializeField] private string _rightHandName = "RightHandIK";
        [SerializeField] private string _leftHandName = "LeftHandIK";

        private readonly AvatarIKGoal[] _footGoal = { AvatarIKGoal.LeftFoot, AvatarIKGoal.RightFoot };
        private readonly AvatarIKGoal[] _handGoal = { AvatarIKGoal.LeftHand, AvatarIKGoal.RightHand };
        private readonly HumanBodyBones[] _allHumanBone = { HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot, HumanBodyBones.LeftHand, HumanBodyBones.RightHand };
        private readonly RaycastHit[] _hits = new RaycastHit[4];

        private void Awake() => _animatorController = GetComponent<AnimatorController>();

        private void FixedUpdate()
        {
            if (!_enabled) return;

            var count = 0;
            foreach (HumanBodyBones humanBone in _allHumanBone)
            {
                ObtainIKPosition(humanBone, count++);
            }
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!_enabled) return;

            Animator animator = _animatorController.Animator;

            SetWeightFoots();
            SetWeightHands();

            var count = 0;
            
            foreach (AvatarIKGoal item in _footGoal)
            {
                MoveIKFoots(item, animator, _hits[count++]);
            }            
            
            foreach (AvatarIKGoal item in _handGoal)
            {
                MoveIKHands(item, animator, _hits[count++]);
            }
        }

        private void ObtainIKPosition(HumanBodyBones humanBone, int count)
        {
            Animator animator = _animatorController.Animator;

            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 origin = animator.GetBoneTransform(humanBone).position + forward * _distanceFromGroundRaycast;
            if (!Physics.Raycast(origin, forward, out RaycastHit hit, _raycastForwardDistance, _environmentLayer)) return;
            
            Debug.DrawRay(origin, forward * _raycastForwardDistance, Color.magenta);
            _hits[count] = hit;
        }

        private void MoveIKFoots(AvatarIKGoal goal, Animator animator, RaycastHit hit)
        {
            var position = hit.point;
            position.z += _distanceFootsWall;
            animator.SetIKPosition(goal, position);
            animator.SetIKRotation(goal, Quaternion.FromToRotation(Vector3.up, hit.normal) * transform.rotation);
        }

        private void MoveIKHands(AvatarIKGoal goal, Animator animator, RaycastHit hit)
        {
            var position = hit.point;
            position.z += _distanceHandsWall;
            animator.SetIKPosition(goal, position);
            animator.SetIKRotation(goal, Quaternion.FromToRotation(Vector3.up, hit.normal) * transform.rotation);
        }

        private void SetWeightHands()
        {
            _animatorController.Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            _animatorController.Animator.SetIKRotationWeight(AvatarIKGoal.RightHand, _handRotationWeight); //_animatorController.Animator.GetFloat(_rightHandName));

            _animatorController.Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            _animatorController.Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, _handRotationWeight); //_animatorController.Animator.GetFloat(_leftHandName));
        }

        private void SetWeightFoots()
        {
            _animatorController.Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            _animatorController.Animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, _footRotationWeight); //_animatorController.Animator.GetFloat(_rightFootName));

            _animatorController.Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            _animatorController.Animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, _footRotationWeight); //_animatorController.Animator.GetFloat(_leftFootName));
        }

        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        #region Gizmos

        
        private void OnDrawGizmos()
        {
            if (!_enabled) return;
            
            Gizmos.color = Color.cyan;
        
            Vector3 rightFoot = _animatorController.Animator.GetBoneTransform(HumanBodyBones.RightFoot).position;
            Vector3 leftFoot = _animatorController.Animator.GetBoneTransform(HumanBodyBones.LeftFoot).position;
            Vector3 leftHand = _animatorController.Animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
            Vector3 rightHand = _animatorController.Animator.GetBoneTransform(HumanBodyBones.RightHand).position;
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Gizmos.DrawRay(rightFoot + forward * (_distanceFromGroundRaycast), forward * (_raycastForwardDistance));
            Gizmos.DrawRay(leftFoot + forward * (_distanceFromGroundRaycast), forward * (_raycastForwardDistance));
            Gizmos.DrawRay(leftHand + forward * (_distanceFromGroundRaycast), forward * (_raycastForwardDistance));
            Gizmos.DrawRay(rightHand + forward * (_distanceFromGroundRaycast), forward * (_raycastForwardDistance));
        }

        #endregion
    }
}