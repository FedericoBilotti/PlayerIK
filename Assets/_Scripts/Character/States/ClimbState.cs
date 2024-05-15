namespace Character.States
{
    public class ClimbState : BasePlayerState
    {
        public ClimbState(PlayerMovement playerMovement, AnimatorController animatorController) : base(playerMovement, animatorController)
        {
        }

        public override void OnEnter()
        {
            playerMovement.EnterClimb();
        }

        public override void OnUpdate() => playerMovement.ApplyClimbValues();

        public override void OnFixedUpdate()
        {
            playerMovement.ClimbWall();
            playerMovement.RestrictVelocityInGround();
        }

        public override void OnExit() => playerMovement.ExitClimb();
    }
}