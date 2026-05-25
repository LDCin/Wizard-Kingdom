using Enemies;
using Utils;

namespace StateMachines
{
    public class EnemyIdleState : IState
    {
        private readonly Enemy _enemy;

        public EnemyIdleState(Enemy enemy)
        {
            _enemy = enemy;
        }

        public void Enter()
        {
            _enemy.Animator.SetBool(GameConfig.AnimatorParams.Idle, true);
            _enemy.MoveSpeed = _enemy.NormalSpeed;
        }

        public void Update()
        {
            _enemy.MoveDown();
        }

        public void Exit()
        {
            _enemy.Animator.SetBool(GameConfig.AnimatorParams.Idle, false);
        }
    }

    public class EnemyFallState : IState
    {
        private readonly Enemy _enemy;

        public EnemyFallState(Enemy enemy)
        {
            _enemy = enemy;
        }

        public void Enter()
        {
            _enemy.Animator.SetBool(GameConfig.AnimatorParams.Fall, true);
            _enemy.MoveSpeed = _enemy.NormalSpeed * 2f;
        }

        public void Update()
        {
            _enemy.MoveDown();
        }

        public void Exit()
        {
            _enemy.Animator.SetBool(GameConfig.AnimatorParams.Fall, false);
        }
    }

    public class EnemyVictoryState : IState
    {
        private readonly Enemy _enemy;

        public EnemyVictoryState(Enemy enemy)
        {
            _enemy = enemy;
        }

        public void Enter()
        {
            _enemy.Animator.SetBool(GameConfig.AnimatorParams.Victory, true);
            _enemy.MoveSpeed = 0f;
        }

        public void Update()
        {
        }

        public void Exit()
        {
            _enemy.Animator.SetBool(GameConfig.AnimatorParams.Victory, false);
        }
    }

    public class EnemyDeadState : IState
    {
        private readonly Enemy _enemy;

        public EnemyDeadState(Enemy enemy)
        {
            _enemy = enemy;
        }

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