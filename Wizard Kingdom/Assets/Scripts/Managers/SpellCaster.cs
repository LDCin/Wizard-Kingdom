using System;
using System.Collections;
using System.Collections.Generic;
using Enemies;
using GestureRecognizer;
using SOs;
using UnityEngine;

namespace Managers
{
    public class SpellCaster : MonoBehaviour
    {
        public static event Action<Sprite, int> OnSpellUsageUpdated;

        [Serializable]
        private enum SpellActionType
        {
            SlowAll,
            ElectrocuteAll
        }

        [Serializable]
        private class SpellActionBinding
        {
            public GesturePattern pattern;
            public SpellActionType action;
            public Sprite icon;
            [Min(0)] public int maxUsesPerMatch = 1;
            [NonSerialized] public int remainingUses;
        }

        [Header("References")]
        [SerializeField] private GameManager _gameManager;

        [Header("Bindings")]
        [SerializeField] private List<SpellActionBinding> _bindings = new();

        [Header("Slow Settings")]
        [Range(0.1f, 1f)]
        [SerializeField] private float _slowMultiplier = 0.5f;
        [SerializeField] private float _slowDuration = 3f;

        private readonly Dictionary<SpellActionType, Action> _actionByType = new();
        private readonly Dictionary<Enemy, float> _originalSpeeds = new();
        private Coroutine _slowCoroutine;

        private void Awake()
        {
            if (_gameManager == null)
            {
                _gameManager = GetComponentInParent<GameManager>();
            }

            _actionByType.Clear();
            _actionByType[SpellActionType.SlowAll] = SlowAllEnemies;
            _actionByType[SpellActionType.ElectrocuteAll] = ElectrocuteAllEnemies;
        }

        private void OnEnable()
        {
            ResetSpellUses();
            GameManager.OnSpellCastRequested += Cast;
        }

        private void OnDisable()
        {
            GameManager.OnSpellCastRequested -= Cast;
        }

        private void Cast(GesturePattern pattern, SpellItemData spell)
        {
            if (pattern == null) return;

            for (int i = 0; i < _bindings.Count; i++)
            {
                SpellActionBinding binding = _bindings[i];
                if (binding == null || binding.pattern == null) continue;

                if (binding.pattern != pattern) continue;

                if (binding.maxUsesPerMatch > 0 && binding.remainingUses <= 0)
                {
                    OnSpellUsageUpdated?.Invoke(binding.icon, binding.remainingUses);
                    Debug.LogWarning($"SpellCaster: no uses left for pattern '{pattern.id}'.");
                    return;
                }

                if (_actionByType.TryGetValue(binding.action, out Action action))
                {
                    if (binding.maxUsesPerMatch > 0)
                    {
                        binding.remainingUses--;
                    }

                    OnSpellUsageUpdated?.Invoke(binding.icon, binding.remainingUses);
                    action.Invoke();
                }
                else
                {
                    Debug.LogWarning($"SpellCaster: no action for pattern '{pattern.id}'.");
                }

                return;
            }

            Debug.LogWarning($"SpellCaster: pattern '{pattern.id}' not bound to any action.");
        }

        public void ResetSpellUses()
        {
            for (int i = 0; i < _bindings.Count; i++)
            {
                SpellActionBinding binding = _bindings[i];
                if (binding == null) continue;
                binding.remainingUses = binding.maxUsesPerMatch;
            }
        }

        private void ElectrocuteAllEnemies()
        {
            if (_gameManager == null) return;

            foreach (Enemy enemy in _gameManager.ActiveEnemies)
            {
                if (enemy == null || enemy.IsDead) continue;
                enemy.Die();
            }
        }

        private void SlowAllEnemies()
        {
            if (_gameManager == null) return;

            if (_slowCoroutine != null)
            {
                StopCoroutine(_slowCoroutine);
                _slowCoroutine = null;
            }

            _originalSpeeds.Clear();

            foreach (Enemy enemy in _gameManager.ActiveEnemies)
            {
                if (enemy == null) continue;

                _originalSpeeds[enemy] = enemy.MoveSpeed;
                enemy.MoveSpeed = enemy.MoveSpeed * _slowMultiplier;
            }

            _slowCoroutine = StartCoroutine(RestoreSpeedAfterDelay());
        }

        private IEnumerator RestoreSpeedAfterDelay()
        {
            if (_slowDuration > 0f)
            {
                yield return new WaitForSeconds(_slowDuration);
            }

            foreach (var pair in _originalSpeeds)
            {
                if (pair.Key != null)
                {
                    pair.Key.MoveSpeed = pair.Value;
                }
            }

            _originalSpeeds.Clear();
            _slowCoroutine = null;
        }
    }
}
