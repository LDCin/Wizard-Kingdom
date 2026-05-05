using Managers;
using UnityEngine;

namespace Player
{
    public class PlayerIdleState : IState
    {
        public Player _player;
        public PlayerIdleState(Player player) => this._player = player;

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
        public Player _player;
        public PlayerSpellState(Player player) => this._player = player;

        public void Enter()
        {
            _player.BodyAnimator.SetBool("Spell", true);
        }

        public void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                _player.StateMachine.ChangeState(_player.IdleState);
            }
        }

        public void Exit()
        {
            _player.BodyAnimator.SetBool("Spell", false);
        }
    }
    
    public class PlayerSnapState : IState
    {
        public Player _player;
        public PlayerSnapState(Player player) => this._player = player;

        public void Enter()
        {
            _player.BodyAnimator.SetTrigger("Snap");
        }

        public void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                _player.StateMachine.ChangeState(_player.IdleState);
            }
        }

        public void Exit()
        {
            _player.BodyAnimator.SetBool("Spell", false);
        }
    }
}