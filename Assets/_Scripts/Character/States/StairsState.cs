using UnityEngine;

namespace Character.States
{
    public class StairsState : BasePlayerState
    {
        public StairsState(PlayerMovement playerMovement) : base(playerMovement)
        {
        }

        public override void OnUpdate() => playerMovement.ApplyWalkingValues();

        public override void OnFixedUpdate()
        {
            Vector3 directionTarget = playerMovement.GetDirectionTarget(); 

            playerMovement.Rotation(directionTarget);
            
            playerMovement.MoveInStairs();
        }
    }
}