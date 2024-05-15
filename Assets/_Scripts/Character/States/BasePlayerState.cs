using StateMachine;
using UnityEngine;

namespace Character.States
{
    public class BasePlayerState : IState
    {
        protected readonly PlayerMovement playerMovement;
        protected readonly AnimatorController animatorController;

        public BasePlayerState(PlayerMovement playerMovement, AnimatorController animatorController)
        {
            this.playerMovement = playerMovement;
            this.animatorController = animatorController;
        }

        public virtual void OnEnter()
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnFixedUpdate()
        {
        }

        public virtual void OnExit()
        {
        }

        public virtual void OnTriggerEnter(Collider other)
        {
        }

        public virtual void OnTriggerExit(Collider other)
        {
        }
    }
}