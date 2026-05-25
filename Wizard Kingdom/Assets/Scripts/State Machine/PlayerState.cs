using Enemies;
using GestureRecognizer;
using Managers;
using Players;
using UnityEngine;
using Utils;

namespace StateMachines
{
    public class PlayerIdleState : IState
    {
        public Player _player;
        public PlayerIdleState(Player player) => _player = player;

        public void Enter()
        {
            Debug.Log("Player: Enter Idle State!");
            _player.BodyAnimator.SetBool(GameConfig.AnimatorParams.Idle, true);

            DrawDetector.OnDrawStart += HandleDrawStart;
        }

        public void Update()
        {
        }

        public void Exit()
        {
            _player.BodyAnimator.SetBool(GameConfig.AnimatorParams.Idle, false);

            DrawDetector.OnDrawStart -= HandleDrawStart;
        }

        private void HandleDrawStart()
        {
            if (GameManager.Instance.StateMachine.CurrentState == GameManager.Instance.PauseState)
                return;

            _player.StateMachine.ChangeState(_player.SpellState);
        }
    }
    public class PlayerSpellState : IState
    {
        private Player _player;
        private bool _hasPoppedBalloon;

        public PlayerSpellState(Player player)
        {
            _player = player;
        }

        public void Enter()
        {
            _hasPoppedBalloon = false;

            _player.BodyAnimator.SetBool(GameConfig.AnimatorParams.Spell, true);

            Enemy.OnBalloonPop += HandleBalloonPop;
            GestureResultHandler.OnRecognitionFinished += HandleRecognitionFinished;
        }

        public void Update()
        {
            
        }

        public void Exit()
        {
            _player.BodyAnimator.SetBool(GameConfig.AnimatorParams.Spell, false);

            Enemy.OnBalloonPop -= HandleBalloonPop;
            GestureResultHandler.OnRecognitionFinished -= HandleRecognitionFinished;
        }

        private void HandleBalloonPop()
        {
            _hasPoppedBalloon = true;
            _player.StateMachine.ChangeState(_player.SnapState);
        }

        private void HandleRecognitionFinished()
        {
            if (_hasPoppedBalloon)
                return;

            _player.StateMachine.ChangeState(_player.IdleState);
        }
    }
    public class PlayerSnapState : IState
    {
        public Player _player;
        public PlayerSnapState(Player player) => _player = player;

        public void Enter()
        {
            _player.BodyAnimator.SetTrigger(GameConfig.AnimatorParams.Snap);
        }

        public void Update()
        {
            
        }

        public void Exit()
        {
        }
    }
    public class PlayerDeadState : IState
    {
        public Player _player;
        public PlayerDeadState(Player player) => _player = player;

        public void Enter()
        {
            _player.BodyAnimator.SetBool(GameConfig.AnimatorParams.Dead, true);
        }

        public void Update()
        {
            
        }

        public void Exit()
        {
        }
    }
}