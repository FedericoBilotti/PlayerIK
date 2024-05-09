using System;
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
        [SerializeField, Range(0f, 20f)] private float _pelvisUpDownSpeed = 0.28f;
        [SerializeField, Range(0f, 20f)] private float _feetToIkPositionSpeed = 0.5f;
        
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
            
        }

        private void MovePelvis()
        {
            
        }

        private void FeetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIKPosition, ref Quaternion feetIKRotation)
        {
            
        }

        private void AdjustFeetTarget(ref Vector3 feetPositions, HumanBodyBones foot)
        {
            
        }

        private void WeightFoots()
        {
            _animatorController.Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            _animatorController.Animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, _animatorController.Animator.GetFloat(_rightFootName));

            _animatorController.Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            _animatorController.Animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, _animatorController.Animator.GetFloat(_leftFootName));
        }
    }
}