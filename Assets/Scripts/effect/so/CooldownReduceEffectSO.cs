

using System;
using System.Reflection;
using Character;
using UnityEngine;

namespace Effect
{
    public enum CooldownReduceType
    {
        Percent = 0,
        FlatSeconds = 1,
        PercentAndFlat = 2
    }

    [CreateAssetMenu(
        fileName = "CooldownReduceEffect",
        menuName = "Effect/Cooldown Reduce Effect")]
    public class CooldownReduceEffectSO : EffectSO
    {
        [Header("Cooldown Reduce")]
        public CooldownReduceType reduceType = CooldownReduceType.Percent;

        [Tooltip("남은 쿨타임 감소 비율. 0.2 = 남은 쿨타임 20% 감소")]
        [Range(0f, 1f)]
        public float reducePercent = 0.2f;

        [Tooltip("남은 쿨타임에서 직접 차감할 초 단위 값")]
        [Min(0f)]
        public float reduceSeconds = 0f;


        public EffectRuntimeData CreateRuntimeData(
            CharacterManager targetCharacter)
        {
            return new CooldownReduceEffectRuntime(
                this,
                targetCharacter);
        }
    }
}