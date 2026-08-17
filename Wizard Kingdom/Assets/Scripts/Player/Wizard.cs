using System.Collections;
using Managers;
using StateMachines;
using UnityEngine;
using Utils;

namespace Wizards
{
    public class Wizard : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] private Animator _bodyAnimator;
        [SerializeField] private Animator _headAnimator;
        [SerializeField] private GameObject _headRoot;
        private bool _deadAnimationFinished;

        public Animator BodyAnimator => _bodyAnimator;
        public Animator HeadAnimator => _headAnimator;

        [Header("State")]
        private StateMachine<WizardStateType> _stateMachine;
        public StateMachine<WizardStateType> StateMachine => _stateMachine;

        private void Start()
        {
            if (_headRoot == null && _headAnimator != null)
            {
                _headRoot = _headAnimator.gameObject;
            }

            _stateMachine = new StateMachine<WizardStateType>();
            _stateMachine.RegisterState(WizardStateType.Idle, new WizardIdleState(this));
            _stateMachine.RegisterState(WizardStateType.Spell, new WizardSpellState(this));
            _stateMachine.RegisterState(WizardStateType.Snap, new WizardSnapState(this));
            _stateMachine.RegisterState(WizardStateType.Dead, new WizardDeadState(this));
            _stateMachine.ChangeState(WizardStateType.Idle);
        }

        private void Update()
        {
            _stateMachine.Update();
        }

        private void OnEnable()
        {
            Observer.Subscribe(ObserverEvent.GameOver, HandleGameOver);
        }

        private void OnDisable()
        {
            Observer.Unsubscribe(ObserverEvent.GameOver, HandleGameOver);
        }

        private void OnDestroy()
        {
            _stateMachine?.CurrentState?.Exit();
        }

        public void OnSnapAnimationFinished()
        {
            if (_stateMachine.CurrentStateType == WizardStateType.Snap)
            {
                _stateMachine.ChangeState(WizardStateType.Idle);
            }
        }

        private void ChangeToDeadState()
        {
            _stateMachine.ChangeState(WizardStateType.Dead);
        }

        private IEnumerator DeadRoutine()
        {
            Debug.Log("DeadRoutine called");
            _deadAnimationFinished = false;
            ChangeToDeadState();

            yield return new WaitUntil(() => _deadAnimationFinished);
            Observer.Publish(ObserverEvent.WizardDead);
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

            _stateMachine?.ChangeState(WizardStateType.Idle);
        }

        private void HandleGameOver()
        {
            StartCoroutine(DeadRoutine());
        }
    }
}
