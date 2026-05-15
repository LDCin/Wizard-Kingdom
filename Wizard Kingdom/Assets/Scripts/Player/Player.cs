using Managers;
using UnityEngine;
using StateMachines;

namespace Players
{
    public class Player : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] private Animator _bodyAnimator;
        [SerializeField] private Animator _headAnimator;
        public Animator BodyAnimator => _bodyAnimator;
        public Animator HeadAnimator => _headAnimator;
        
        [Header("State")]
        private StateMachine _stateMachine;
        public StateMachine StateMachine => _stateMachine;
        private PlayerIdleState _idleState;
        public PlayerIdleState IdleState => _idleState;
        private PlayerSpellState _spellState;
        public PlayerSpellState SpellState => _spellState;
        private PlayerSnapState _snapState;
        public PlayerSnapState SnapState => _snapState;

        private void Start()
        {
            _stateMachine = new StateMachine();
            _idleState = new PlayerIdleState(this);
            _spellState = new PlayerSpellState(this);
            _snapState = new PlayerSnapState(this);
            _stateMachine.ChangeState(_idleState);
        }
        private void Update()
        {
            _stateMachine.Update();
        }

        private void OnDestroy()
        {
            _stateMachine?.CurrentState?.Exit();
        }

        public void OnSnapAnimationFinished()
        {
            if (_stateMachine.CurrentState == _snapState)
            {
                _stateMachine.ChangeState(_idleState);
            }
        }
    }
}