using Enemies;
using GestureRecognizer;
using Managers;
using Wizards;
using UnityEngine;
using Utils;

namespace StateMachines
{
    public class WizardIdleState : IState
    {
        public Wizard _wizard;
        public WizardIdleState(Wizard wizard) => _wizard = wizard;

        public void Enter()
        {
            Debug.Log("Wizard: Enter Idle State!");
            _wizard.BodyAnimator.SetBool(GameConfig.AnimatorParams.Idle, true);
            _wizard.HeadAnimator.SetBool(GameConfig.AnimatorParams.Idle, true);

            Observer.Subscribe(ObserverEvent.DrawStart, HandleDrawStart);
        }

        public void Update()
        {
        }

        public void Exit()
        {
            _wizard.BodyAnimator.SetBool(GameConfig.AnimatorParams.Idle, false);
            _wizard.HeadAnimator.SetBool(GameConfig.AnimatorParams.Idle, false);

            Observer.Unsubscribe(ObserverEvent.DrawStart, HandleDrawStart);
        }

        private void HandleDrawStart()
        {
            if (GameManager.Instance.StateMachine.CurrentStateType == GameStateType.Pause)
                return;

            _wizard.StateMachine.ChangeState(WizardStateType.Spell);
        }
    }
    public class WizardSpellState : IState
    {
        private Wizard _wizard;
        private bool _hasPoppedBalloon;

        public WizardSpellState(Wizard wizard)
        {
            _wizard = wizard;
        }

        public void Enter()
        {
            _hasPoppedBalloon = false;

            _wizard.BodyAnimator.SetBool(GameConfig.AnimatorParams.Spell, true);

            Observer.Subscribe(ObserverEvent.BalloonPopped, HandleBalloonPop);
            Observer.Subscribe(ObserverEvent.RecognitionFinished, HandleRecognitionFinished);
        }

        public void Update()
        {
            
        }

        public void Exit()
        {
            _wizard.BodyAnimator.SetBool(GameConfig.AnimatorParams.Spell, false);

            Observer.Unsubscribe(ObserverEvent.BalloonPopped, HandleBalloonPop);
            Observer.Unsubscribe(ObserverEvent.RecognitionFinished, HandleRecognitionFinished);
        }

        private void HandleBalloonPop()
        {
            _hasPoppedBalloon = true;
            _wizard.StateMachine.ChangeState(WizardStateType.Snap);
        }

        private void HandleRecognitionFinished()
        {
            if (_hasPoppedBalloon)
                return;

            _wizard.StateMachine.ChangeState(WizardStateType.Idle);
        }
    }
    public class WizardSnapState : IState
    {
        public Wizard _wizard;
        public WizardSnapState(Wizard wizard) => _wizard = wizard;

        public void Enter()
        {
            _wizard.BodyAnimator.SetTrigger(GameConfig.AnimatorParams.Snap);
        }

        public void Update()
        {
            
        }

        public void Exit()
        {
        }
    }
    public class WizardDeadState : IState
    {
        public Wizard _wizard;
        public WizardDeadState(Wizard wizard) => _wizard = wizard;

        public void Enter()
        {
            _wizard.SetHeadActive(false);
            _wizard.BodyAnimator.SetBool(GameConfig.AnimatorParams.Idle, false);
            _wizard.HeadAnimator.SetBool(GameConfig.AnimatorParams.Idle, false);
            _wizard.BodyAnimator.SetBool(GameConfig.AnimatorParams.Dead, true);
            _wizard.HeadAnimator.SetBool(GameConfig.AnimatorParams.Dead, true);
        }

        public void Update()
        {
            
        }

        public void Exit()
        {
        }
    }
}



