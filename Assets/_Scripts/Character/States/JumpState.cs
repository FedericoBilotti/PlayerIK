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
            Debug.Log("Enter jump state");
            animatorController.SetBool("InGround", false);
            _groundIKController.Enabled = false;
        }

        public override void OnFixedUpdate()
        {
            Vector3 directionTarget = playerMovement.GetDirectionTarget(); 
            Vector3 rootTarget = playerMovement.VelocityRootMotion() + directionTarget;
            
            playerMovement.Jump();
            playerMovement.Rotation(directionTarget);
            playerMovement.MoveInGround(rootTarget.normalized);
        }

        public override void OnExit()
        {
            Debug.Log("Exit jump state");
            _groundIKController.Enabled = true;
        }
    }
}