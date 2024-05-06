using UnityEngine;

namespace Character
{
    public class PlayerAnimatorController : MonoBehaviour
    {
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
        }

        public float GetFloat(string name) => _animator.GetFloat(name);
        public void SetFloat(string name, float value, float dampTime = 0f, float deltaTime = 0f) => _animator.SetFloat(name, value, dampTime, deltaTime);
    }
}
