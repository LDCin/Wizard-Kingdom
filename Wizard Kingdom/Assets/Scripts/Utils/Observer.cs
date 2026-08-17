using System;
using System.Collections.Generic;
using GestureRecognizer;
using Particles;
using SOs;
using UnityEngine;

namespace Utils
{
    public readonly struct ScoreAndGoldPayload
    {
        public readonly int Score;
        public readonly int Gold;

        public ScoreAndGoldPayload(int score, int gold)
        {
            Score = score;
            Gold = gold;
        }
    }

    public readonly struct RewardPayload
    {
        public readonly int Score;
        public readonly int Gold;

        public RewardPayload(int score, int gold)
        {
            Score = score;
            Gold = gold;
        }
    }

    public readonly struct TimePayload
    {
        public readonly float RemainingTime;
        public readonly float TotalTime;

        public TimePayload(float remainingTime, float totalTime)
        {
            RemainingTime = remainingTime;
            TotalTime = totalTime;
        }
    }

    public readonly struct SpellCastRequestPayload
    {
        public readonly GesturePattern Pattern;
        public readonly SpellItemData Spell;

        public SpellCastRequestPayload(GesturePattern pattern, SpellItemData spell)
        {
            Pattern = pattern;
            Spell = spell;
        }
    }

    public readonly struct SpellUsagePayload
    {
        public readonly Sprite Icon;
        public readonly int RemainingUses;

        public SpellUsagePayload(Sprite icon, int remainingUses)
        {
            Icon = icon;
            RemainingUses = remainingUses;
        }
    }

    public readonly struct ParticleRequestPayload
    {
        public readonly ParticleType ParticleType;
        public readonly Vector3 Position;

        public ParticleRequestPayload(ParticleType particleType, Vector3 position)
        {
            ParticleType = particleType;
            Position = position;
        }
    }

    public enum ObserverEvent
    {
        PlayGame,
        PauseGame,
        ContinueGame,
        RestartGame,
        BackToMenu,
        GameOver,
        ScoreAndGoldChanged,
        ModeLoaded,
        TimeChanged,
        TimeExpired,
        WizardDead,
        EnemyDied,
        EnemyReachedCastle,
        EnemyReturnedToPool,
        BalloonPopped,
        DrawStart,
        DrawSymbol,
        RecognitionFinished,
        SpellCastRequested,
        SpellUsageUpdated,
        ParticleRequested,
        CoinChanged,
        SettingsChanged,
        InventoryChanged,
        EquippedBackgroundChanged,
        EquippedWizardChanged,
        TransitionComplete
    }

    public static class Observer
    {
        private static readonly Dictionary<ObserverEvent, Delegate> eventTable = new();

        public static void Subscribe(ObserverEvent eventType, Action listener)
        {
            AddListener(eventType, listener);
        }

        public static void Subscribe<T>(ObserverEvent eventType, Action<T> listener)
        {
            AddListener(eventType, listener);
        }

        public static void Unsubscribe(ObserverEvent eventType, Action listener)
        {
            RemoveListener(eventType, listener);
        }

        public static void Unsubscribe<T>(ObserverEvent eventType, Action<T> listener)
        {
            RemoveListener(eventType, listener);
        }

        public static void Publish(ObserverEvent eventType)
        {
            if (!eventTable.TryGetValue(eventType, out Delegate callback))
            {
                return;
            }

            if (callback is Action action)
            {
                action.Invoke();
                return;
            }

            Debug.LogWarning($"Observer event {eventType} was published without payload, but listeners expect payload.");
        }

        public static void Publish<T>(ObserverEvent eventType, T payload)
        {
            if (!eventTable.TryGetValue(eventType, out Delegate callback))
            {
                return;
            }

            if (callback is Action<T> action)
            {
                action.Invoke(payload);
                return;
            }

            Debug.LogWarning($"Observer event {eventType} was published with payload {typeof(T).Name}, but listener signature does not match.");
        }

        private static void AddListener(ObserverEvent eventType, Delegate listener)
        {
            if (listener == null)
            {
                return;
            }

            eventTable.TryGetValue(eventType, out Delegate currentDelegate);
            eventTable[eventType] = Delegate.Combine(currentDelegate, listener);
        }

        private static void RemoveListener(ObserverEvent eventType, Delegate listener)
        {
            if (listener == null || !eventTable.TryGetValue(eventType, out Delegate currentDelegate))
            {
                return;
            }

            Delegate newDelegate = Delegate.Remove(currentDelegate, listener);
            if (newDelegate == null)
            {
                eventTable.Remove(eventType);
                return;
            }

            eventTable[eventType] = newDelegate;
        }
    }
}
