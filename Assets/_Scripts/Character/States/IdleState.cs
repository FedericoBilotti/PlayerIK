using UnityEngine;

namespace Character.States
{
    public class IdleState : BasePlayerState
    {
        public IdleState(PlayerMovement playerMovement) : base(playerMovement)
        {
        }

        public override void OnUpdate() => playerMovement.ApplyWalkingValues();
    }
}