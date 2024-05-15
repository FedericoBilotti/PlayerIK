using UnityEngine;

namespace Character.States
{
    public class FallingState : BasePlayerState
    {
        private readonly GroundIKController _groundIKController;

        public FallingState(PlayerMovement playerMovement, AnimatorController animatorController, GroundIKController groundIKController) : base(playerMovement, animatorController)
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
            Vector3 directionTarget = playerMovement.GetDirectionTarget(); 
            Vector3 rootTarget = playerMovement.VelocityRootMotion() + directionTarget;
            
            playerMovement.Rotation(directionTarget);
            playerMovement.MoveInGround(rootTarget.normalized);
        }

        public override void OnExit()
        {
            animatorController.SetBool("InGround", true);
            _groundIKController.Enabled = true;
        }
    }
}