using System;
using System.Collections;
using Managers;
using StateMachines;
using UnityEngine;

namespace Players
{
    public class Player : MonoBehaviour
    {
        public static event Action OnDead;
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
        private PlayerDeadState _deadState;
        public PlayerDeadState DeadState => _deadState;

        private void Start()
        {
            _stateMachine = new StateMachine();
            _idleState = new PlayerIdleState(this);
            _spellState = new PlayerSpellState(this);
            _snapState = new PlayerSnapState(this);
            _deadState = new PlayerDeadState(this);
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
        private void ChangeToDeadState()
        {
            _stateMachine.ChangeState(_deadState);
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Fire"))
            {
                StartCoroutine(DeadRoutine());
            }
        }
        private IEnumerator DeadRoutine()
        {
            // ChangeToDeadState();
            // yield return new WaitUntil(() =>{
            //     AnimatorStateInfo state = _bodyAnimator.GetCurrentAnimatorStateInfo(0);

            //     return state.IsName("Dead") && state.normalizedTime >= 1f &&  !_bodyAnimator.IsInTransition(0);
            // });
            yield return new WaitForSeconds(3);
            OnDead?.Invoke();
        }

        //TEST
        private void OnEnable()
        {
            GameManager.OnGameOver += TestPlayerDead;
        }
        private void OnDisable()
        {
            GameManager.OnGameOver -= TestPlayerDead;
        }
        private void TestPlayerDead()
        {
            StartCoroutine(DeadRoutine());
        }
    }
}