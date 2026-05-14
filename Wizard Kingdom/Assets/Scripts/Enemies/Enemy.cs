using System;
using System.Collections;
using System.Collections.Generic;
using Balloons;
using Managers;
using Particles;
using UnityEngine;

namespace Enemies
{
    public class Enemy : MonoBehaviour
    {
        public static event Action OnEnemyDie;
        public static event Action OnBalloonPop;
        public static event Action<Enemy> OnReturnEnemyToPool;
        public static event Action<Vector3> OnEnemyExplode;
        
        [SerializeField] private string _enemyName;
        public string EnemyName => _enemyName;
        [SerializeField] private Sprite _sprite;
        private Animator _animator;
        public Animator Animator => _animator;
        [SerializeField] private List<Balloon> _balloonList = new List<Balloon>();
        [SerializeField] private int _goldReward;
        [SerializeField] private int _scoreReward;
        [SerializeField] private float _normalSpeed = 1f;
        public float NormalSpeed => _normalSpeed;
        [SerializeField] private float _moveSpeed = 1f;
        public float MoveSpeed
        {
            get => _moveSpeed;
            set => _moveSpeed = value;
        }
        [SerializeField] private bool _isGameOver = false;
        public bool IsGameOver => _isGameOver;
        
        [SerializeField] private bool _isFalling = false;
        public bool IsFalling => _isFalling;
        [SerializeField] private bool _isDead = false;
        public bool IsDead => _isDead;

        private int _remainingBalloon = 1;
        public int RemainingBalloon => _remainingBalloon;
        [Header("Balloon Layout")]
        [SerializeField] private Vector2 _balloonCenterOffset = new Vector2(0f, 1.2f);
        [SerializeField] private float _balloonHorizontalSpacing = 0.35f;
        [SerializeField] private float _balloonVerticalSpacing = 0.35f;
        [SerializeField] private int _maxBalloonPerRow = 3;
        
        
        [Header("State")]
        private StateMachine _stateMachine;
        public StateMachine StateMachine => _stateMachine;
        private EnemyIdleState _idleState;
        public EnemyIdleState IdleState => _idleState;
        private EnemyFallState _fallState;
        public EnemyFallState FallState => _fallState;
        private EnemyVictoryState _victoryState;
        public EnemyVictoryState VictoryState => _victoryState;
        private EnemyDeadState _deadState;
        public EnemyDeadState DeadState => _deadState;
        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            GestureResultHandler.OnDrawSymbol += TryPopBalloon;
            if (_stateMachine != null && _idleState != null)
            {
                _stateMachine.ChangeState(_idleState);
            }
        }

        private void OnDisable()
        {
            GestureResultHandler.OnDrawSymbol -= TryPopBalloon;
        }

        private void Start()
        {
            _stateMachine = new StateMachine();
            _idleState = new EnemyIdleState(this);
            _fallState = new EnemyFallState(this);
            _victoryState = new EnemyVictoryState(this);
            _deadState = new EnemyDeadState(this);
            _stateMachine.ChangeState(_idleState);
        }
        public void InitEnemyData(string enemyName, Sprite sprite, RuntimeAnimatorController runtimeAnimatorController, int goldReward, int scoreReward, float moveSpeed)
        {
            _enemyName = enemyName;
            _sprite = sprite;
            _animator.runtimeAnimatorController = runtimeAnimatorController;
            _goldReward = goldReward;
            _scoreReward = scoreReward;
            _normalSpeed = moveSpeed;
            _moveSpeed = _normalSpeed;
        }

        public void MoveDown()
        {
            transform.Translate(Vector3.down * _moveSpeed * Time.deltaTime * (_isGameOver ? 0 : 1));
        }

        public void Update()
        {
            _stateMachine.Update();
        }

        public void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag(GameConfig.GROUND_TAG))
            {
                if (StateMachine.CurrentState != FallState)
                {
                    _isGameOver = true;
                    Debug.Log("Game Over!");
                }
                else
                {
                    _isDead = true;
                    OnEnemyDie?.Invoke();
                }
            }
        }
        public void ResetEnemyState()
        {
            _isGameOver = false;
            _isFalling = false;
            _isDead = false;
            _moveSpeed = _normalSpeed;

            if (_stateMachine != null && _idleState != null)
            {
                _stateMachine.ChangeState(_idleState);
            }
        }

        private void TryPopBalloon(string shapeName)
        {
            if (!Enum.TryParse(shapeName, out Symbol drawnSymbol))
            {
                return;
            }
            foreach (var balloon in _balloonList)
            {
                if (!balloon.gameObject.activeSelf)
                {
                    continue;
                }
                if (balloon.Symbol == drawnSymbol)
                {
                    balloon.Pop();
                    OnBalloonPop?.Invoke();
                    Debug.Log("Pop balloon has symbol: " + drawnSymbol);
                    _remainingBalloon -= 1;
                    if (_remainingBalloon <= 0)
                    {
                        _isFalling = true;
                    }
                }
                
            }
        }
        private Vector3 GetBalloonLocalPosition(int index, int totalCount)
        {
            int maxPerRow = Mathf.Max(1, _maxBalloonPerRow);

            int currentIndex = 0;
            int row = 0;

            while (currentIndex < totalCount)
            {
                int remaining = totalCount - currentIndex;
                int rowCount = Mathf.Min(maxPerRow, remaining);

                if (index < currentIndex + rowCount)
                {
                    int column = index - currentIndex;

                    float rowWidth = (rowCount - 1) * _balloonHorizontalSpacing;
                    float x = column * _balloonHorizontalSpacing - rowWidth / 2f;
                    float y = row * _balloonVerticalSpacing;

                    if (row % 2 == 1)
                    {
                        x += _balloonHorizontalSpacing * 0.25f;
                    }

                    return new Vector3(
                        _balloonCenterOffset.x + x,
                        _balloonCenterOffset.y + y,
                        0f
                    );
                }

                currentIndex += rowCount;
                row++;
            }

            return _balloonCenterOffset;
        }
        public void SetupBalloons(List<Balloon> balloons)
        {
            _balloonList.Clear();
            
            for (int i = 0; i < balloons.Count; i++)
            {
                Balloon balloon = balloons[i];

                balloon.transform.SetParent(transform, false);
                balloon.transform.localPosition = GetBalloonLocalPosition(i, balloons.Count);
                balloon.transform.localRotation = Quaternion.identity;

                balloon.gameObject.SetActive(true);

                _balloonList.Add(balloon);
            }

            _remainingBalloon = _balloonList.Count;
        }
        public void ReturnBalloonsToPool(BalloonPool balloonPool)
        {
            foreach (Balloon balloon in _balloonList)
            {
                balloonPool.ReturnBalloon(balloon);
            }

            _balloonList.Clear();
            _remainingBalloon = 0;
        }
        public void ReturnToPoolAfterDeath()
        {
            StartCoroutine(ReturnToPoolAfterDeathCoroutine());
        }

        private IEnumerator ReturnToPoolAfterDeathCoroutine()
        {
            ParticleEvent.RequestParticle(ParticleType.Explosion, transform.position);

            yield return null;

            OnReturnEnemyToPool?.Invoke(this);
        }
    }
}