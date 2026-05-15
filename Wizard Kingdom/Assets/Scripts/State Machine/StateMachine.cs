namespace StateMachines
{
    public class StateMachine
    {
        private IState _currentState;

        public IState CurrentState => _currentState;

        public void ChangeState(IState newState)
        {
            if (newState == null)
                return;

            if (_currentState == newState)
                return;
            
            _currentState?.Exit();
            _currentState = newState;
            _currentState.Enter();
        }

        public void Update()
        {
            _currentState?.Update();
        }
    }
}