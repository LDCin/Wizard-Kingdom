using Enemies;
using Managers;
using Players;
using UnityEngine;

namespace StateMachines
{
    public class PlayState : IState
    {
        public GameManager _gameManager;
        public PlayState(GameManager gameManager) => _gameManager = gameManager;

        public void Enter()
        {
            _gameManager.StartGame();
            Time.timeScale = 1;
            Debug.Log("Is Playing");
        }

        public void Update()
        {
            
        }

        public void Exit()
        {
            
        }
    }
    public class PauseState : IState
    {
        public GameManager _gameManager;
        public PauseState(GameManager gameManager) => _gameManager = gameManager;

        public void Enter()
        {
            Time.timeScale = 0;
            Debug.Log("Paused Game!");
        }

        public void Update()
        {
            
        }

        public void Exit()
        {
            
        }
    }
    public class GameOverState : IState
    {
        public GameManager _gameManager;
        public GameOverState(GameManager gameManager) => _gameManager = gameManager;

        public void Enter()
        {
            // Time.timeScale = 0;
            _gameManager.StopSpawnEnemy();
            _gameManager.DestroyEnemySpawner();
            Debug.Log("Game Over!");
        }

        public void Update()
        {
            
        }

        public void Exit()
        {
            
        }
    }
    public class MenuState : IState
    {
        public GameManager _gameManager;
        public MenuState(GameManager gameManager) => _gameManager = gameManager;

        public void Enter()
        {
            Time.timeScale = 1;
            Debug.Log("In Main Menu!");
        }

        public void Update()
        {
            
        }

        public void Exit()
        {
            
        }
    }
}