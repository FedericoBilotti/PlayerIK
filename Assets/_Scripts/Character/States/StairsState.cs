using UnityEngine;

namespace Character.States
{
    public class StairsState : BasePlayerState
    {
        public StairsState(PlayerMovement playerMovement, AnimatorController animatorController) : base(playerMovement, animatorController)
        {
        }

        public override void OnUpdate() => playerMovement.ApplyWalkingValues();

        public override void OnFixedUpdate()
        {
            Vector3 directionTarget = playerMovement.GetInputTargetDirection(); 

            playerMovement.Rotation(directionTarget);
            playerMovement.MoveInStairs();
            playerMovement.RestrictVelocityInGround();
        }
    }
}