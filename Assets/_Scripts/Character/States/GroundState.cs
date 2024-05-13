using UnityEngine;

namespace Character.States
{
    public class GroundState : BasePlayerState
    {
        public GroundState(PlayerMovement playerMovement) : base(playerMovement)
        {
        }

        public override void OnFixedUpdate()
        {
            Vector3 directionTarget = playerMovement.GetDirectionTarget(); 

            playerMovement.Rotation(directionTarget);

            Vector3 rootTarget = playerMovement.VelocityRootMotion() + directionTarget;
            
            playerMovement.MoveInGround(rootTarget.normalized);
        }
    }
}