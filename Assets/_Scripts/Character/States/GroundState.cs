using UnityEngine;

namespace Character.States
{
    public class GroundState : BasePlayerState
    {
        public GroundState(PlayerMovement playerMovement, AnimatorController animatorController) : base(playerMovement, animatorController)
        {
        }

        public override void OnUpdate() => playerMovement.ApplyWalkingValues();

        public override void OnFixedUpdate()
        {
            Vector3 directionTarget = playerMovement.GetDirectionTarget();
            Vector3 rootTarget = playerMovement.VelocityRootMotion() + directionTarget;
            
            playerMovement.Rotation(directionTarget);
            playerMovement.MoveInGround(rootTarget.normalized);
        }
    }
}