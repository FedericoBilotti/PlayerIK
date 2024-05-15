using Character.States;
using Sensor;
using StateMachine;
using UnityEngine;
using Utilities;

namespace Character
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerModel : MonoBehaviour
    {
        private AnimatorController _animatorController;
        private PlayerMovement _playerMovement;
        private PlayerInput _playerInput;
        private Rigidbody _rigidbody;
        private FiniteStateMachine _fsm;

        private Timer _jumpTimer;
        private ISensor _groundSensor;
        private ISensor _slopeSensor;
        private ISensor _stairSensor;
        private ISensor _climbSensor;
        private ISensor[] _sensors;

        private void Awake()
        {
            _animatorController = GetComponent<AnimatorController>();
            _playerMovement = GetComponent<PlayerMovement>();
            _playerInput = GetComponent<PlayerInput>();
            _rigidbody = GetComponent<Rigidbody>();

            _playerMovement.Initialize(_animatorController, _playerInput, _rigidbody);

            InitializeFSM();
        }

        private void Update() => _fsm.Update();
        private void FixedUpdate() => _fsm.FixedUpdate();

        private void InitializeFSM()
        {
            _fsm = new FiniteStateMachine();

            var groundIK = GetComponent<GroundIKController>();

            var idle = new IdleState(_playerMovement, _animatorController);
            var movement = new GroundState(_playerMovement, _animatorController);
            var slope = new SlopeState(_playerMovement, _animatorController);
            var stairs = new StairsState(_playerMovement, _animatorController);
            var jump = new JumpState(_playerMovement, _animatorController, groundIK);
            var falling = new FallingState(_playerMovement, _animatorController, groundIK);
            var climb = new ClimbState(_playerMovement, _animatorController);

            // From Idle
            AddTransition(idle, movement, new FuncPredicate(() => _playerMovement.CanMove()));
            AddTransition(idle, jump, new FuncPredicate(() => _playerInput.InputJump && _playerMovement.GetGroundSensor().OnCollision));
            AddTransition(idle, falling, new FuncPredicate(() => !_playerMovement.GetGroundSensor().OnCollision));

            // From Movement
            AddTransition(movement, idle, new FuncPredicate(() => !_playerMovement.CanMove()));
            AddTransition(movement, slope, new FuncPredicate(() => _playerMovement.GetSlopeSensor().OnCollision));
            AddTransition(movement, stairs, new FuncPredicate(() => _playerMovement.GetStairSensor().OnCollision));
            AddTransition(movement, climb, new FuncPredicate(() => _playerMovement.GetClimbSensor().OnCollision && !_playerMovement.GetGroundSensor().OnCollision));
            AddTransition(movement, jump, new FuncPredicate(() => _playerInput.InputJump && _playerMovement.GetGroundSensor().OnCollision));
            AddTransition(movement, falling, new FuncPredicate(() => !_playerMovement.GetGroundSensor().OnCollision));

            // From Slope
            AddTransition(slope, idle, new FuncPredicate(() => !_playerMovement.CanMove()));
            AddTransition(slope, movement, new FuncPredicate(() => _playerInput.InputJump && _playerMovement.GetGroundSensor().OnCollision));
            AddTransition(slope, jump, new FuncPredicate(() => !_playerMovement.GetSlopeSensor().OnCollision));
            AddTransition(slope, falling, new FuncPredicate(() => !_playerMovement.GetGroundSensor().OnCollision));

            // From stairs
            AddTransition(stairs, idle, new FuncPredicate(() => !_playerMovement.CanMove()));
            AddTransition(stairs, movement, new FuncPredicate(() => !_playerMovement.GetStairSensor().OnCollision));
            AddTransition(stairs, jump, new FuncPredicate(() => _playerInput.InputJump && _playerMovement.GetGroundSensor().OnCollision));
            AddTransition(slope, falling, new FuncPredicate(() => !_playerMovement.GetGroundSensor().OnCollision));

            // From Climb
            AddTransition(climb, movement, new FuncPredicate(() => !_playerMovement.GetClimbSensor().OnCollision && _playerMovement.GetGroundSensor().OnCollision));

            // From Jump
            AddTransition(jump, falling, new FuncPredicate(() => !_playerMovement.GetJumpTimer().IsRunning && !_playerMovement.GetGroundSensor().OnCollision));

            // From Falling
            AddTransition(falling, idle, new FuncPredicate(() => _playerMovement.GetGroundSensor().OnCollision));
            AddTransition(falling, climb, new FuncPredicate(() => !_playerMovement.GetJumpTimer().IsRunning && _playerMovement.GetClimbSensor().OnCollision));

            _fsm.SetState(idle);
        }

        private void AddTransition(IState from, IState to, IPredicate condition) => _fsm.AddTransition(from, to, condition);
        private void AddAnyTransition(IState to, IPredicate condition) => _fsm.AddAnyTransition(to, condition);
    }
}