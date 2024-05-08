using UnityEngine;

namespace Character
{
    public class PlayerAnimatorController : MonoBehaviour
    {
        [field: SerializeField] public Animator Animator { get; private set; }

        private void Awake()
        {
            Animator = GetComponentInChildren<Animator>();
        }

        public float GetFloat(string name) => Animator.GetFloat(name);
        public void SetFloat(string name, float value, float dampTime = 0f, float deltaTime = 0f) => Animator.SetFloat(name, value, dampTime, deltaTime);
    }
}
