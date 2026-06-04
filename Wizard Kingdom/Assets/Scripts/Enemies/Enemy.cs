using System;
using System.Collections;
using System.Collections.Generic;
using Balloons;
using Managers;
using ObjectPool;
using Particles;
using SOs;
using StateMachines;
using UnityEngine;
using Utils;

namespace Enemies
{
    // public class EnemyDieInfor
    // {
    //     private int _goldReward;
    //     public int GoldReward => _goldReward;
    //     private int _scoreReward;
    //     public int ScoreReward => _scoreReward;
    //     public EnemyDieInfor()
    //     {
    //         _goldReward = 0;
    //         _scoreReward = 0;
    //     }
    // }
    public class Enemy : MonoBehaviour
    {
        public static event Action<int, int> OnEnemyDie;
        public static event Action OnBalloonPop;
        public static event Action<Enemy> OnReturnEnemyToPool;
        public static event Action OnEnemyReachCastle;

        [Header("Data")]
        [SerializeField] private string _enemyName;
        private EnemyData _data;

        [SerializeField] private Sprite _sprite;
        [SerializeField] private EnemyType _enemyType;

        private Animator _animator;

        [SerializeField] private int _goldReward;
        [SerializeField] private int _scoreReward;
        // private EnemyDieInfor _enemyDieInfor;

        [Header("Movement")]
        [SerializeField] private float _normalSpeed = 1f;
        public float NormalSpeed => _normalSpeed;

        [SerializeField] private float _moveSpeed = 1f;

        [Header("Offscreen")]
        [SerializeField] private float _offscreenDespawnMargin = 1f;
        private bool _isDespawning;

        [Header("Stat")]
        private int _remainingBalloon = 1;
        [SerializeField] private GameObject _fireOnInvadeCastle;

        [Header("Balloon")]
        [SerializeField] private Transform _balloonRoot;
        [SerializeField] private Transform _ropeTargetPoint;
        [SerializeField] private List<Balloon> _balloonList = new List<Balloon>();

        #region Analysis And Design Properties

        public string EnemyName => _enemyName;
        public EnemyData Data => _data;
        public Sprite Sprite => _sprite;
        public EnemyType EnemyType => _enemyType;
        public Animator Animator => _animator;
        public int GoldReward => _goldReward;
        public int ScoreReward => _scoreReward;
        public float MoveSpeed
        {
            get => _moveSpeed;
            set => _moveSpeed = value;
        }
        public int RemainingBalloon => _remainingBalloon;
        public IReadOnlyList<Balloon> BalloonList => _balloonList;

        #endregion

        [Header("Balloon Layout")]
        [SerializeField] private Vector2 _balloonClusterOffset = Vector2.zero;
        [SerializeField] private float _balloonSpacingX = 0.28f;
        [SerializeField] private float _balloonSpacingY = 0.22f;
        [SerializeField] private float _balloonAttractionStrength = 6f;
        [SerializeField] private float _balloonSeparationStrength = 4f;
        [SerializeField] private float _balloonMinSeparation = 0.18f;
        [SerializeField] private float _balloonReflowDuration = 0.25f;

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
            _isDespawning = false;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterEnemy(this);
            }

        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UnregisterEnemy(this);
            }

        }

        private void Update()
        {
            _stateMachine.Update();
            TryDespawnIfOffscreen();
        }

        public void InitEnemyData(
            string enemyName,
            Sprite sprite,
            RuntimeAnimatorController runtimeAnimatorController,
            EnemyType enemyType,
            int goldReward,
            int scoreReward,
            float moveSpeed)
        {
            _enemyName = enemyName;
            _sprite = sprite;
            _enemyType = enemyType;

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
            _isDespawning = false;

            if (_stateMachine != null && _idleState != null)
            {
                _stateMachine.ChangeState(_idleState);
            }
        }

        public void MoveDown()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                return;
            }
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
            OnEnemyDie?.Invoke(_scoreReward, _goldReward);
        }

        public void DestroyEnemy() => Die();

        public void InvadeCastle()
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
            if (!other.gameObject.CompareTag(GameConfig.Tags.Ground))
            {
                return;
            }

            if (_stateMachine.CurrentState == _fallState)
            {
                Die();
            }
            else
            {
                if (_data != null)
                {
                    _data.InvadeCastle(this);
                }
                else
                {
                    InvadeCastle();
                }
            }
        }

        private void TryPopBalloon(string shapeName)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                return;
            }

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

                if (balloon.Data != null)
                {
                    balloon.Data.Destroy(balloon);
                }
                else
                {
                    balloon.Destroy();
                }

                OnBalloonPop?.Invoke();

                Debug.Log("Pop balloon has symbol: " + drawnSymbol);

                _remainingBalloon -= 1;

                StartCoroutine(ReflowBalloonsCoroutine());

                if (_remainingBalloon <= 0)
                {
                    bool timeAttackNoGround = GameManager.Instance != null
                        && GameManager.Instance.CurrentModeData != null
                        && GameManager.Instance.CurrentModeData.modeType == SOs.GameModeType.TimeAttack;

                    if (timeAttackNoGround)
                    {
                        if (_data != null)
                        {
                            _data.Destroy(this);
                        }
                        else
                        {
                            Die();
                        }
                    }
                    else
                    {
                        StartFalling();
                    }
                }

                return;
            }
        }

        #region Analysis And Design Methods

        public void Init(EnemyData data, List<Balloon> balloons)
        {
            if (data == null) return;
            _data = data.Get();

            InitEnemyData(
                data.enemyName,
                data.sprite,
                data.runtimeAnimatorController,
                data.enemyType,
                data.goldReward,
                data.scoreReward,
                data.moveSpeed);

            ResetEnemyState();
            SetupBalloons(balloons ?? new List<Balloon>());
        }

        public void CheckSymbol(string symbol)
        {
            TryPopBalloon(symbol);
        }

        public void Destroy() => DestroyEnemy();

        #endregion

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
                balloon.ResetAnimatorState();
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
            ParticleEvent.RequestParticle(GetDeathParticleType(), transform.position);

            yield return null;

            OnReturnEnemyToPool?.Invoke(this);
        }

        private ParticleType GetDeathParticleType()
        {
            return _enemyType switch
            {
                EnemyType.SmallEnemy => ParticleType.SmallEnemyExplosion,
                EnemyType.BigEnemy => ParticleType.BigEnemyExplosion,
                EnemyType.BossEnemy => ParticleType.BossEnemyExplosion,
                _ => ParticleType.SmallEnemyExplosion
            };
        }

        private IEnumerator ReflowBalloonsCoroutine()
        {
            if (_balloonList.Count <= 1)
            {
                yield break;
            }

            float elapsed = 0f;
            Vector3 center = _balloonClusterOffset;

            while (elapsed < _balloonReflowDuration)
            {
                float deltaTime = Time.deltaTime;

                for (int i = 0; i < _balloonList.Count; i++)
                {
                    Balloon balloon = _balloonList[i];

                    if (balloon == null || !balloon.gameObject.activeSelf)
                    {
                        continue;
                    }

                    Vector3 position = balloon.transform.localPosition;
                    Vector3 attraction = (center - position) * _balloonAttractionStrength;
                    Vector3 separation = Vector3.zero;

                    for (int j = 0; j < _balloonList.Count; j++)
                    {
                        if (i == j)
                        {
                            continue;
                        }

                        Balloon other = _balloonList[j];

                        if (other == null || !other.gameObject.activeSelf)
                        {
                            continue;
                        }

                        Vector3 otherPosition = other.transform.localPosition;
                        Vector3 delta = position - otherPosition;
                        float distance = delta.magnitude;

                        if (distance > 0f && distance < _balloonMinSeparation)
                        {
                            float push = (_balloonMinSeparation - distance) / _balloonMinSeparation;
                            separation += delta.normalized * push * _balloonSeparationStrength;
                        }
                    }

                    Vector3 velocity = (attraction + separation) * deltaTime;
                    balloon.transform.localPosition = position + velocity;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private void TryDespawnIfOffscreen()
        {
            if (_isDespawning) return;
            if (IsDead || IsEnemyVictory) return;
            if (!IsTimeAttackNoGround()) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            float zDistance = Mathf.Abs(transform.position.z - cam.transform.position.z);
            float minY = cam.ViewportToWorldPoint(new Vector3(0f, 0f, zDistance)).y;

            if (transform.position.y < minY - _offscreenDespawnMargin)
            {
                _isDespawning = true;
                OnReturnEnemyToPool?.Invoke(this);
            }
        }

        private bool IsTimeAttackNoGround()
        {
            return GameManager.Instance != null
                && GameManager.Instance.CurrentModeData != null
                && GameManager.Instance.CurrentModeData.modeType == SOs.GameModeType.TimeAttack;
        }
    }
}
