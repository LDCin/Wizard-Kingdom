using System.Collections.Generic;

namespace StateMachines
{
    public class StateMachine<TStateType>
    {
        private readonly Dictionary<TStateType, IState> _states = new();
        private IState _currentState;

        public IState CurrentState => _currentState;
        public TStateType CurrentStateType { get; private set; }

        public void RegisterState(TStateType stateType, IState state)
        {
            if (state == null)
            {
                return;
            }

            _states[stateType] = state;
        }

        public void ChangeState(TStateType newStateType)
        {
            if (!_states.TryGetValue(newStateType, out IState newState))
            {
                return;
            }

            if (_currentState == newState)
            {
                return;
            }

            _currentState?.Exit();
            _currentState = newState;
            CurrentStateType = newStateType;
            _currentState.Enter();
        }

        public void Update()
        {
            _currentState?.Update();
        }
    }
}
