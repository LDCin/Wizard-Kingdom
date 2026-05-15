namespace StateMachines
{
    public interface IState
    {
        void Enter();
        void Update();
        void Exit();
    }
}