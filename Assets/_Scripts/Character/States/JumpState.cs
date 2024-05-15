using UnityEngine;

namespace Character.States
{
    public class JumpState : BasePlayerState
    {
        private readonly GroundIKController _groundIKController;

        public JumpState(PlayerMovement playerMovement, AnimatorController animatorController, GroundIKController groundIKController) : base(playerMovement, animatorController)
        {
            _groundIKController = groundIKController;
        }

        public override void OnEnter()
        {
            animatorController.SetBool("InGround", false);
            _groundIKController.Enabled = false;
        }

        public override void OnFixedUpdate()
        {
            Vector3 directionTarget = playerMovement.GetInputTargetDirection(); 
            Vector3 rootTarget = playerMovement.VelocityRootMotion() + directionTarget;
            
            playerMovement.Jump();
            playerMovement.Rotation(directionTarget);
            playerMovement.MoveInGround(rootTarget.normalized);
            playerMovement.RestrictVelocityInGround();
        }

        public override void OnExit()
        {
            _groundIKController.Enabled = true;
        }
    }
}