namespace StateMachine
{
    public interface IState
    {
        void OnEnter();
        void OnUpdate();
        void OnFixedUpdate();
        void OnExit();
        void OnTriggerEnter(UnityEngine.Collider other);
        void OnTriggerExit(UnityEngine.Collider other);
    }
}
