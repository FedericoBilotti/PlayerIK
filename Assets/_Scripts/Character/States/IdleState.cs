namespace Character.States
{
    public class IdleState : BasePlayerState
    {
        public IdleState(PlayerMovement playerMovement, AnimatorController animatorController) : base(playerMovement, animatorController)
        {
        }

        public override void OnUpdate() => playerMovement.ApplyWalkingValues();
    }
}