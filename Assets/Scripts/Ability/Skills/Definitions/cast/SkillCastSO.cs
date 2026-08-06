using System;
using UnityEngine;

namespace Skill
{
    /// <summary>
    /// 스킬이 언제, 어떤 방식으로 발동되는지를 정의하는 SO.
    /// 데미지나 속성, 업그레이드 결과는 포함하지 않고
    /// 순수하게 발동 구조만 담당한다.
    /// </summary>
    [CreateAssetMenu(fileName = "SkillCastSO", menuName = "Game/Skill/SkillCastSO")]
    public class SkillCastSO : ScriptableObject
    {
        [SerializeField] private string castId;
        [Header("Timing")]
        [SerializeField, Min(0f)] private float cooldown = 1f;
        [SerializeField, Min(0f)] private float castTime = 0f;
        [SerializeField, Min(0f)] private float range = 5f;
        [SerializeField] private TargetingType targetingType = TargetingType.AutoTarget;

        [Header("Burst / Repeat")]
        [SerializeField] private BurstProfile burst = new();

        [Header("Cast Settings")]
        [SerializeField] private CastMoveProfile castMove = new();

        [Header("Self Effects")]
        [SerializeField] private Effect.EffectEntrySO[] selfEffects;

        [Header("Flags")]
        [SerializeField] private bool skipAttackAnimation;
        public string CastId => castId;
        public float Cooldown => cooldown;
        public float CastTime => castTime;
        public float Range => range;
        public int BurstCount => burst.Count;
        public float BurstInterval => burst.Interval;

        public TargetingType TargetingType => targetingType;
        public CastMoveProfile CastMove => castMove;

        public Effect.EffectEntrySO[] SelfEffects => selfEffects;

        public bool SkipAttackAnimation => skipAttackAnimation;

#if UNITY_EDITOR
        public void ApplyEditorData(
            string castId,
            TargetingType targetingType,
            float castTime,
            float cooldown,
            float range,
            bool skipAttackAnimation,
            Effect.EffectEntrySO[] selfEffects)
        {
            this.castId = castId;
            this.targetingType = targetingType;
            this.castTime = castTime;
            this.cooldown = cooldown;
            this.range = range;
            this.skipAttackAnimation = skipAttackAnimation;
            this.selfEffects = selfEffects;
        }

        public void ApplyEditorBurst(
            int count,
            float interval)
        {
            burst.ApplyEditorData(count, interval);
        }

        public void ApplyEditorCastMove(
            CastMoveType moveType,
            float distance,
            float duration)
        {
            castMove.ApplyEditorData(moveType, distance, duration);
        }
#endif
    }

    [Serializable]
    public class CastMoveProfile
    {
        [SerializeField] private CastMoveType moveType = CastMoveType.None;
        [SerializeField, Min(0f)] private float distance = 0f;
        [SerializeField, Min(0f)] private float duration = 0f;

        public CastMoveType MoveType => moveType;
        public float Distance => Mathf.Max(0f, distance);
        public float Duration => Mathf.Max(0f, duration);

#if UNITY_EDITOR
        public void ApplyEditorData(
            CastMoveType moveType,
            float distance,
            float duration)
        {
            this.moveType = moveType;
            this.distance = distance;
            this.duration = duration;
        }
#endif
    }

    [Serializable]
    public class BurstProfile
    {
        [SerializeField, Min(1)] private int count = 1;
        [SerializeField, Min(0f)] private float interval = 0f;

        public int Count => Mathf.Max(1, count);
        public float Interval => Mathf.Max(0f, interval);

#if UNITY_EDITOR
        public void ApplyEditorData(
            int count,
            float interval)
        {
            this.count = count;
            this.interval = interval;
        }
#endif
    }
}