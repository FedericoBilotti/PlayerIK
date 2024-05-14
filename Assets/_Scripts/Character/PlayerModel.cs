using Character.States;
using StateMachine;
using UnityEngine;

namespace Character
{
    public class PlayerModel : MonoBehaviour
    {
        private PlayerMovement _playerMovement;
                
        private FiniteStateMachine _fsm;

        private void Awake()
        {
            _playerMovement = GetComponent<PlayerMovement>();
            
            InitializeFSM();
        }

        private void Update() => _fsm.Update();
        private void FixedUpdate() => _fsm.FixedUpdate();

        private void InitializeFSM()
        {
            _fsm = new FiniteStateMachine();

            var idle = new IdleState(_playerMovement);
            var movement = new GroundState(_playerMovement);
            var slope = new SlopeState(_playerMovement);
            var stairs = new StairsState(_playerMovement);
            var climb = new ClimbState(_playerMovement);

            // From Idle
            AddTransition(idle, movement, new FuncPredicate(() => _playerMovement.CanMove()));
            
            // From Movement
            AddTransition(movement, idle, new FuncPredicate(() => !_playerMovement.CanMove()));
            AddTransition(movement, slope, new FuncPredicate(() => _playerMovement.GetSlopeSensor().OnCollision));
            AddTransition(movement, stairs, new FuncPredicate(() => _playerMovement.GetStairSensor().OnCollision));
            AddTransition(movement, climb, new FuncPredicate(() => _playerMovement.GetClimbSensor().OnCollision));
            
            // From Slope
            AddTransition(slope, idle, new FuncPredicate(() => !_playerMovement.CanMove()));
            AddTransition(slope, movement, new FuncPredicate(() => !_playerMovement.GetSlopeSensor().OnCollision));
            
            // From stairs
            AddTransition(stairs, idle, new FuncPredicate(() => !_playerMovement.CanMove()));
            AddTransition(stairs, movement, new FuncPredicate(() => !_playerMovement.GetStairSensor().OnCollision));
            
            // From Climb
            AddTransition(climb, movement, new FuncPredicate(() => !_playerMovement.GetClimbSensor().OnCollision));
            
            _fsm.SetState(idle);            
        }

        public void AddTransition(IState from, IState to, IPredicate condition) => _fsm.AddTransition(from, to, condition);
        public void AddAnyTransition(IState to, IPredicate condition) => _fsm.AddAnyTransition(to, condition);
    }
}