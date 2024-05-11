using UnityEngine;

namespace Character
{
    public class AnimatorController : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField, Range(0f, 1f)] private float _dampSmoothness = .1f;

        [SerializeField] private Animator _animator;
        public Animator Animator => _animator;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
        }

        public float GetFloat(string name) => _animator.GetFloat(name);
        public void SetFloat(string name, float value) => _animator.SetFloat(name, value, _dampSmoothness, Time.deltaTime);
    }
}
