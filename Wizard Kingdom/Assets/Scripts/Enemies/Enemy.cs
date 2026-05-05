using System;
using Managers;
using UnityEngine;

namespace Enemies
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 1f;
        [SerializeField] private float _isGameOver = 1f;

        public float IsGameOver
        {
            get => _isGameOver;
            set => _isGameOver = value;
        }
        
        [Header("State")]
        private StateMachine _stateMachine;
        private EnemyIdleState _idleState;
        private EnemyVictoryState _victoryState;

        private Animator _animator;
        public Animator Animator => _animator;
        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
        private void Start()
        {
            _stateMachine = new StateMachine();
            _idleState = new EnemyIdleState(this);
            _victoryState = new EnemyVictoryState(this);
            _stateMachine.ChangeState(_idleState);
        }

        public void MoveDown()
        {
            transform.Translate(Vector3.down * _moveSpeed * Time.deltaTime * _isGameOver);
        }

        public void Update()
        {
            _stateMachine.Update();
        }

        public void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag(GameConfig.GROUND_TAG))
            {
                _stateMachine.ChangeState(_victoryState);
                Debug.Log("Game Over!");
            }
        }
    }
}