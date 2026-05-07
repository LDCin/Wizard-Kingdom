using Enemies;
using Managers;
using UnityEngine;

namespace Player
{
    public class PlayerIdleState : IState
    {
        public Player _player;
        public PlayerIdleState(Player player) => _player = player;

        public void Enter()
        {
            Debug.Log("Player: Enter Idle State!");
            _player.BodyAnimator.SetBool("Idle", true);
        }

        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _player.StateMachine.ChangeState(_player.SpellState);
            }
        }

        public void Exit()
        {
            _player.BodyAnimator.SetBool("Idle", false);
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

            _player.BodyAnimator.SetBool("Spell", true);

            Enemy.OnBalloonPop += HandleBalloonPop;
            GestureResultHandler.OnRecognitionFinished += HandleRecognitionFinished;
        }

        public void Update()
        {
            
        }

        public void Exit()
        {
            _player.BodyAnimator.SetBool("Spell", false);

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
            _player.BodyAnimator.SetTrigger("Snap");
        }

        public void Update()
        {
            
        }

        public void Exit()
        {
        }
    }
}