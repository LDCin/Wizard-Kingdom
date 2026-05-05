namespace Enemies
{
    public class EnemyIdleState : IState
    {
        public Enemy _enemy;
        public EnemyIdleState(Enemy enemy) => this._enemy = enemy;

        public void Enter()
        {
            _enemy.Animator.SetBool("Idle", true);
        }

        public void Update()
        {
            _enemy.MoveDown();
        }

        public void Exit()
        {
            _enemy.Animator.SetBool("Idle", false);

        }
    }
    
    public class EnemyVictoryState : IState
    {
        public Enemy _enemy;
        public EnemyVictoryState(Enemy enemy) => this._enemy = enemy;

        public void Enter()
        {
            _enemy.Animator.SetBool("Victory", true);
            _enemy.IsGameOver = 0;
        }

        public void Update()
        {
        
        }

        public void Exit()
        {
        
        }
    }
}