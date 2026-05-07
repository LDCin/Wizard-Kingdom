using System;

namespace Enemies
{
    public class EnemyIdleState : IState
    {
        public Enemy _enemy;
        public EnemyIdleState(Enemy enemy) => _enemy = enemy;

        public void Enter()
        {
            _enemy.Animator.SetBool("Idle", true);
            _enemy.MoveSpeed = _enemy.NormalSpeed;
        }

        public void Update()
        {
            _enemy.MoveDown();
            if (_enemy.IsFalling)
            {
                _enemy.StateMachine.ChangeState(_enemy.FallState);
            }
            else if (_enemy.IsGameOver)
            {
                _enemy.StateMachine.ChangeState(_enemy.VictoryState);
            }
        }

        public void Exit()
        {
            _enemy.Animator.SetBool("Idle", false);
        }
    }
    
    public class EnemyFallState : IState
    {
        public Enemy _enemy;
        public EnemyFallState(Enemy enemy) => _enemy = enemy;

        public void Enter()
        {
            _enemy.Animator.SetBool("Fall", true);
            _enemy.MoveSpeed *= 2;
        }

        public void Update()
        {
            if (_enemy.IsDead)
            {
                _enemy.StateMachine.ChangeState(_enemy.DeadState);
            }
            _enemy.MoveDown();
        }

        public void Exit()
        {
            _enemy.Animator.SetBool("Fall", false);
        }
    }
    public class EnemyVictoryState : IState
    {
        public Enemy _enemy;
        public EnemyVictoryState(Enemy enemy) => this._enemy = enemy;

        public void Enter()
        {
            _enemy.Animator.SetBool("Victory", true);
        }

        public void Update()
        {
        
        }

        public void Exit()
        {
        
        }
    }
    public class EnemyDeadState : IState
    {
        public Enemy _enemy;
        public EnemyDeadState(Enemy enemy) => _enemy = enemy;

        public void Enter()
        {
            _enemy.ReturnToPoolAfterDeath();
        }

        public void Update()
        {
        
        }

        public void Exit()
        {
        
        }
    }
}