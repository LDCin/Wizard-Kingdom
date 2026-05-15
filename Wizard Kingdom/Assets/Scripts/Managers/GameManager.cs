using System;
using System.Collections.Generic;
using Enemies;
using GestureRecognizer;
using Particles;
using StateMachines;
using UI;
using UnityEngine;
using Utils;
using ObjectPool;

namespace Managers
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private EnemySpawner _enemySpawnerPrefab;
        private EnemySpawner _currentEnemySpawner;
        [SerializeField] private Recognizer _recognizer;
        [SerializeField] private ParticlePool _particlePool;
        [SerializeField] private List<string> _spawnEnemyNameList;
        [SerializeField] private float _delayTime = 1f;
        private StateMachine _stateMachine;
        private IState _playState;
        private IState _pauseState;
        private IState _menuState;
        private IState _gameOverState;

        private void Awake()
        {
            base.Awake();
            _stateMachine = new StateMachine();
            _playState = new PlayState(this);
            _pauseState = new PauseState(this);
            _menuState = new MenuState(this);
            _gameOverState = new GameOverState(this);
        }

        private void OnEnable()
        {
            Enemy.OnEnemyReachCastle += ChangeToGameOverState;
            GamePanel.OnPauseGame += ChangeToPauseState;
            MenuPanel.OnPlayGame += ChangeToPlayState;
            PausePanel.OnBackToMenu += ChangeToMenuState;
            PausePanel.OnContinueGame += ChangeToPlayState;
            PausePanel.OnRestartGame += ChangeToPlayState;
            PausePanel.OnRestartGame += DestroyEnemySpawner;
            // SceneLoader.OnTransitionComplete += StartGame;
        }
        private void OnDisable()
        {
            Enemy.OnEnemyReachCastle -= ChangeToGameOverState;
            GamePanel.OnPauseGame -= ChangeToPauseState;
            MenuPanel.OnPlayGame -= ChangeToPlayState;
            PausePanel.OnBackToMenu -= ChangeToMenuState;
            PausePanel.OnContinueGame -= ChangeToPlayState;
            PausePanel.OnRestartGame -= ChangeToPlayState;
            PausePanel.OnRestartGame -= DestroyEnemySpawner;
            // SceneLoader.OnTransitionComplete -= StartGame;
        }

        private void Start()
        {
            _stateMachine.ChangeState(_menuState);
        }
        private void ChangeToPlayState()
        {
            _stateMachine.ChangeState(_playState);
        }
        private void ChangeToGameOverState()
        {
            _stateMachine.ChangeState(_gameOverState);
        }
        private void ChangeToMenuState()
        {
            _stateMachine.ChangeState(_menuState);
        }

        private void ChangeToPauseState()
        {
            _stateMachine.ChangeState(_pauseState);
        }

        public void StartGame()
        {
            if (_stateMachine.CurrentState == _playState && _currentEnemySpawner == null)
            {
                _currentEnemySpawner = Instantiate(_enemySpawnerPrefab, transform);
                _currentEnemySpawner.StartSpawn(_spawnEnemyNameList, _delayTime);
            }
        }

        public void StopSpawnEnemy()
        {
            _currentEnemySpawner.StopSpawn();
        }
        
        public void DestroyEnemySpawner()
        {
            if (_currentEnemySpawner != null)
            {
                Destroy(_currentEnemySpawner.gameObject);
                _currentEnemySpawner = null;
            }
        }
    }
}