using System;
using UnityEngine;

namespace Character
{
    public class ClimbIKController : MonoBehaviour
    {
        [SerializeField] private bool _enabled;

        [SerializeField] private float _pelvisOffset;
        [SerializeField, Range(0f, 2f)] private float _heightFromGroundRaycast = 1.14f;
        [SerializeField, Range(0f, 2f)] private float _raycastDownDistance = 1.5f;
        [SerializeField, Range(0f, 100f)] private float _pelvisUpDownSpeed = 0.28f;
        [SerializeField, Range(0f, 100f)] private float _feetToIkPositionSpeed = 0.5f;
        [SerializeField] private LayerMask _environmentLayer;
        
        [Header("References")]
        [SerializeField] private AnimatorController _animatorController;
        
        [Header("Animator Curves Names")]
        [SerializeField] private string _rightFootName = "RightFootIK";
        [SerializeField] private string _leftFootName = "LeftFootIK";
        [SerializeField] private string _rightHandName = "RightHandIK";
        [SerializeField] private string _leftHandName = "LeftHandIK";

        private Vector3 _leftHandPosition;
        private Vector3 _rightHandPosition;
        
        private Vector3 _leftHandIKPosition;
        private Vector3 _rightHandIKPosition;
        
        private Vector3 _rightFootPosition;
        private Vector3 _leftFootPosition;
        
        private Vector3 _leftFootIKPosition;
        private Vector3 _rightFootIKPosition;
        
        private Quaternion _leftFootIKRotation;
        private Quaternion _rightFootIKRotation;        
        
        private Quaternion _leftHandIKRotation;
        private Quaternion _rightHandIKRotation;
        
        private float _lastPelvisPositionY;
        
        private float _lastRightFootPositionZ;
        private float _lastLeftFootPositionZ;
        
        private float _lastRightHandPositionZ;
        private float _lastLeftHandPositionZ;

        private void Awake() => _animatorController = GetComponent<AnimatorController>();


        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }
    }
}
