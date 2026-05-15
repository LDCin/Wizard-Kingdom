using System;
using System.Collections;
using System.Collections.Generic;
using Balloons;
using Managers;
using Particles;
using UnityEngine;
using StateMachines;
using ObjectPool;

namespace Enemies
{
    public class Enemy : MonoBehaviour
    {
        public static event Action OnEnemyDie;
        public static event Action OnBalloonPop;
        public static event Action<Enemy> OnReturnEnemyToPool;
        public static event Action<Vector3> OnEnemyExplode;
        public static event Action OnEnemyReachCastle;

        [Header("Data")]
        [SerializeField] private string _enemyName;
        public string EnemyName => _enemyName;

        [SerializeField] private Sprite _sprite;

        private Animator _animator;
        public Animator Animator => _animator;

        [SerializeField] private int _goldReward;
        [SerializeField] private int _scoreReward;

        [Header("Movement")]
        [SerializeField] private float _normalSpeed = 1f;
        public float NormalSpeed => _normalSpeed;

        [SerializeField] private float _moveSpeed = 1f;

        public float MoveSpeed
        {
            get => _moveSpeed;
            set => _moveSpeed = value;
        }

        private int _remainingBalloon = 1;
        public int RemainingBalloon => _remainingBalloon;

        [Header("Balloon")]
        [SerializeField] private Transform _balloonRoot;
        [SerializeField] private Transform _ropeTargetPoint;
        [SerializeField] private List<Balloon> _balloonList = new List<Balloon>();

        [Header("Balloon Layout")]
        [SerializeField] private Vector2 _balloonClusterOffset = Vector2.zero;
        [SerializeField] private float _balloonSpacingX = 0.28f;
        [SerializeField] private float _balloonSpacingY = 0.22f;

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

        public bool IsIdle => _stateMachine != null && _stateMachine.CurrentState == _idleState;
        public bool IsFalling => _stateMachine != null && _stateMachine.CurrentState == _fallState;
        public bool IsDead => _stateMachine != null && _stateMachine.CurrentState == _deadState;
        public bool IsEnemyVictory => _stateMachine != null && _stateMachine.CurrentState == _victoryState;

        private void Awake()
        {
            _animator = GetComponent<Animator>();

            _stateMachine = new StateMachine();

            _idleState = new EnemyIdleState(this);
            _fallState = new EnemyFallState(this);
            _victoryState = new EnemyVictoryState(this);
            _deadState = new EnemyDeadState(this);
        }

        private void Start()
        {
            _stateMachine.ChangeState(_idleState);
        }

        private void OnEnable()
        {
            GestureResultHandler.OnDrawSymbol += TryPopBalloon;
        }

        private void OnDisable()
        {
            GestureResultHandler.OnDrawSymbol -= TryPopBalloon;
        }

        private void Update()
        {
            _stateMachine.Update();
        }

        public void InitEnemyData(
            string enemyName,
            Sprite sprite,
            RuntimeAnimatorController runtimeAnimatorController,
            int goldReward,
            int scoreReward,
            float moveSpeed)
        {
            _enemyName = enemyName;
            _sprite = sprite;

            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }

            _animator.runtimeAnimatorController = runtimeAnimatorController;

            _goldReward = goldReward;
            _scoreReward = scoreReward;

            _normalSpeed = moveSpeed;
            _moveSpeed = _normalSpeed;
        }

        public void ResetEnemyState()
        {
            _moveSpeed = _normalSpeed;
            _remainingBalloon = 0;

            if (_stateMachine != null && _idleState != null)
            {
                _stateMachine.ChangeState(_idleState);
            }
        }

        public void MoveDown()
        {
            transform.Translate(Vector3.down * _moveSpeed * Time.deltaTime);
        }

        public void StartFalling()
        {
            if (_stateMachine.CurrentState == _fallState)
            {
                return;
            }

            if (_stateMachine.CurrentState == _deadState)
            {
                return;
            }

            if (_stateMachine.CurrentState == _victoryState)
            {
                return;
            }

            _stateMachine.ChangeState(_fallState);
        }

        public void Die()
        {
            if (_stateMachine.CurrentState == _deadState)
            {
                return;
            }

            _stateMachine.ChangeState(_deadState);
            OnEnemyDie?.Invoke();
        }

        public void ReachCastle()
        {
            if (_stateMachine.CurrentState == _victoryState)
            {
                return;
            }

            if (_stateMachine.CurrentState == _deadState)
            {
                return;
            }

            _stateMachine.ChangeState(_victoryState);
            OnEnemyReachCastle?.Invoke();

            Debug.Log("Enemy reached castle!");
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.gameObject.CompareTag(GameConfig.GROUND_TAG))
            {
                return;
            }

            if (_stateMachine.CurrentState == _fallState)
            {
                Die();
            }
            else
            {
                ReachCastle();
            }
        }

        private void TryPopBalloon(string shapeName)
        {
            if (_stateMachine.CurrentState != _idleState)
            {
                return;
            }

            if (!Enum.TryParse(shapeName, out Symbol drawnSymbol))
            {
                return;
            }

            foreach (Balloon balloon in _balloonList)
            {
                if (balloon == null || !balloon.gameObject.activeSelf)
                {
                    continue;
                }

                if (balloon.Symbol != drawnSymbol)
                {
                    continue;
                }

                balloon.Pop();

                OnBalloonPop?.Invoke();

                Debug.Log("Pop balloon has symbol: " + drawnSymbol);

                _remainingBalloon -= 1;

                if (_remainingBalloon <= 0)
                {
                    StartFalling();
                }

                return;
            }
        }

        public void SetupBalloons(List<Balloon> balloons)
        {
            _balloonList.Clear();

            Transform balloonParent = _balloonRoot != null ? _balloonRoot : transform;
            Transform ropeTarget = _ropeTargetPoint != null ? _ropeTargetPoint : transform;

            for (int i = 0; i < balloons.Count; i++)
            {
                Balloon balloon = balloons[i];

                if (balloon == null)
                {
                    continue;
                }

                balloon.transform.SetParent(balloonParent, false);
                balloon.transform.localPosition = GetBalloonLocalPosition(i, balloons.Count);
                balloon.transform.localRotation = Quaternion.identity;
                balloon.transform.localScale = Vector3.one;

                balloon.gameObject.SetActive(true);
                balloon.SetupRope(ropeTarget);

                _balloonList.Add(balloon);
            }

            _remainingBalloon = _balloonList.Count;
        }

        private Vector3 GetBalloonLocalPosition(int index, int totalCount)
        {
            Vector2[] pattern = GetBalloonPattern(totalCount);

            if (index < 0 || index >= pattern.Length)
            {
                return Vector3.zero;
            }

            Vector2 offset = pattern[index];

            float x = _balloonClusterOffset.x + offset.x * _balloonSpacingX;
            float y = _balloonClusterOffset.y + offset.y * _balloonSpacingY;

            return new Vector3(x, y, 0f);
        }

        private Vector2[] GetBalloonPattern(int count)
        {
            switch (count)
            {
                case 1:
                    return new[]
                    {
                        new Vector2(0f, 0f)
                    };

                case 2:
                    return new[]
                    {
                        new Vector2(-0.6f, 0f),
                        new Vector2(0.6f, 0f)
                    };

                case 3:
                    return new[]
                    {
                        new Vector2(-0.8f, -0.2f),
                        new Vector2(0.8f, -0.2f),
                        new Vector2(0f, 0.85f)
                    };

                case 4:
                    return new[]
                    {
                        new Vector2(-0.9f, -0.25f),
                        new Vector2(0.9f, -0.25f),
                        new Vector2(-0.45f, 0.8f),
                        new Vector2(0.45f, 0.8f)
                    };

                case 5:
                    return new[]
                    {
                        new Vector2(-1f, -0.25f),
                        new Vector2(0f, -0.45f),
                        new Vector2(1f, -0.25f),
                        new Vector2(-0.55f, 0.8f),
                        new Vector2(0.55f, 0.8f)
                    };

                case 6:
                    return new[]
                    {
                        new Vector2(-1f, -0.25f),
                        new Vector2(0f, -0.45f),
                        new Vector2(1f, -0.25f),
                        new Vector2(-1f, 0.8f),
                        new Vector2(0f, 1f),
                        new Vector2(1f, 0.8f)
                    };

                default:
                    return GenerateCirclePattern(count);
            }
        }

        private Vector2[] GenerateCirclePattern(int count)
        {
            Vector2[] result = new Vector2[count];

            if (count <= 0)
            {
                return result;
            }

            float radius = 1.15f;

            for (int i = 0; i < count; i++)
            {
                float angle = Mathf.PI * 2f * i / count + Mathf.PI / 2f;

                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;

                result[i] = new Vector2(x, y);
            }

            return result;
        }

        public void ReturnBalloonsToPool(BalloonPool balloonPool)
        {
            foreach (Balloon balloon in _balloonList)
            {
                if (balloon == null)
                {
                    continue;
                }

                balloon.ClearRope();
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