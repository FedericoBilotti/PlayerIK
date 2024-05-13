using StateMachine;
using UnityEngine;

namespace Character.States
{
    public class BasePlayerState : IState
    {
        protected readonly PlayerMovement playerMovement;

        public BasePlayerState(PlayerMovement playerMovement)
        {
            this.playerMovement = playerMovement;
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