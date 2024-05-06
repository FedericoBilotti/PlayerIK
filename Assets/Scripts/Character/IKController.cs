using UnityEngine;

namespace Character
{
    public class IKController : MonoBehaviour
    {
        [SerializeField] private Transform[] _ikTargets;

        private PlayerAnimatorController _playerAnimatorController;

        private void Awake() => _playerAnimatorController = GetComponent<PlayerAnimatorController>();
    }
    
}
