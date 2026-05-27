using System;
using System.Collections;
using Managers;
using StateMachines;
using UnityEngine;
using Utils;

namespace Players
{
    public class Player : MonoBehaviour
    {
        public static event Action OnDead;
        [Header("Animator")]
        [SerializeField] private Animator _bodyAnimator;
        [SerializeField] private Animator _headAnimator;
        [SerializeField] private GameObject _headRoot;
        private bool _deadAnimationFinished;

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
            if (_headRoot == null && _headAnimator != null)
            {
                _headRoot = _headAnimator.gameObject;
            }

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
            if (other.gameObject.CompareTag(GameConfig.Tags.Fire))
            {
                StartCoroutine(DeadRoutine());
            }
        }
        private IEnumerator DeadRoutine()
        {
            Debug.Log("DeadRoutine called");
            _deadAnimationFinished = false;
            ChangeToDeadState();

            yield return new WaitUntil(() => _deadAnimationFinished);
            OnDead?.Invoke();
        }

        public void OnDeadAnimationFinished()
        {
            _deadAnimationFinished = true;
        }

        public void SetHeadActive(bool isActive)
        {
            if (_headRoot != null)
            {
                _headRoot.SetActive(isActive);
            }
        }

        public void ResetToIdleForNewGame()
        {
            _deadAnimationFinished = false;
            SetHeadActive(true);

            if (_bodyAnimator != null)
            {
                _bodyAnimator.SetBool(GameConfig.AnimatorParams.Dead, false);
                _bodyAnimator.SetBool(GameConfig.AnimatorParams.Idle, true);
            }

            if (_headAnimator != null)
            {
                _headAnimator.SetBool(GameConfig.AnimatorParams.Dead, false);
                _headAnimator.SetBool(GameConfig.AnimatorParams.Idle, true);
            }

            if (_stateMachine != null)
            {
                _stateMachine.ChangeState(_idleState);
            }
        }

        //TEST
        private void OnEnable()
        {
            GameManager.OnGameOver += HandleGameOver;
        }
        private void OnDisable()
        {
            GameManager.OnGameOver -= HandleGameOver;
        }
        private void HandleGameOver()
        {
            StartCoroutine(DeadRoutine());
        }
    }
}
