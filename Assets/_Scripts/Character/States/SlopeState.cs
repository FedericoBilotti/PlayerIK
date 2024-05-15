using UnityEngine;

namespace Character.States
{
    public class SlopeState : BasePlayerState
    {
        public SlopeState(PlayerMovement playerMovement, AnimatorController animatorController) : base(playerMovement, animatorController)
        {
        }

        public override void OnUpdate() => playerMovement.ApplyWalkingValues();

        public override void OnFixedUpdate()
        {
            Vector3 directionTarget = playerMovement.GetDirectionTarget(); 

            playerMovement.Rotation(directionTarget);

            Vector3 rootTarget = playerMovement.VelocityRootMotion() + directionTarget;
            
            playerMovement.MoveInSlope(rootTarget);
        }
    }
}