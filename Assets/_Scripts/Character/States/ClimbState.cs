namespace Character.States
{
    public class ClimbState : BasePlayerState
    {
        public ClimbState(PlayerMovement playerMovement) : base(playerMovement)
        {
        }

        public override void OnEnter() => playerMovement.StartClimb();

        public override void OnFixedUpdate()
        {
            playerMovement.ClimbWall();
        }

        public override void OnExit() => playerMovement.StopClimb();
    }
}