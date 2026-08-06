using Character;
using UnityEngine;

namespace Effect
{
    public class OnHitPoisonDotEffectRuntime : EffectRuntimeData
    {
        private readonly EffectSO effectSO;
        private readonly OnHitPoisonDotEffectConfig config;
        private readonly CharacterManager sourceCharacter;

        public OnHitPoisonDotEffectRuntime(
            EffectSO effectSO,
            OnHitPoisonDotEffectConfig config,
            CharacterManager sourceCharacter)
        {
            this.effectSO = effectSO;
            this.config = config;
            this.sourceCharacter = sourceCharacter;
            RuntimeId = $"OnHitPoisonDot_{effectSO.EffectId}_{GetSourceRuntimeId()}";
        }

        public override void OnApply()
        {
            IsActive = effectSO != null
                && config != null
                && sourceCharacter != null;
        }

        public void OnHit(
            CharacterDamageRequest request,
            CharacterDamageResult result)
        {
            if (!IsActive
                || request == null
                || request.attacker == null
                || request.target == null
                || request.attacker.GetComponent<CharacterManager>() != sourceCharacter)
            {
                return;
            }

            CharacterManager target =
                request.target.GetComponent<CharacterManager>()
                ?? request.target.GetComponentInParent<CharacterManager>();

            if (target == null || target.RuntimeData == null || target.RuntimeData.isDead)
            {
                return;
            }

            float chance = Mathf.Clamp(config.ChancePercent, 0f, 100f);
            if (chance <= 0f || Random.Range(0f, 100f) > chance)
            {
                return;
            }

            float attackSnapshot =
                Mathf.Max(0f, sourceCharacter.GetStatValue(Stat.StatType.Attack));

            float damagePerTick =
                attackSnapshot * (config.AttackRatioPercentPerTick / 100f);

            if (damagePerTick <= 0f)
            {
                return;
            }

            EffectManager targetEffectManager =
                target.GetComponent<EffectManager>()
                ?? target.GetComponentInChildren<EffectManager>()
                ?? target.GetComponentInParent<EffectManager>();

            if (targetEffectManager == null)
            {
                return;
            }

            TargetPoisonDotEffectRuntime targetRuntime =
                new TargetPoisonDotEffectRuntime(
                    effectSO,
                    target,
                    sourceCharacter,
                    damagePerTick,
                    config.TickIntervalSeconds);

            targetEffectManager.AddEffect(
                targetRuntime,
                EffectLifetimeType.Timed,
                config.DurationSeconds,
                EffectCategoryType.Debuff);
        }

        private string GetSourceRuntimeId()
        {
            return sourceCharacter != null
                ? sourceCharacter.GetInstanceID().ToString()
                : "NoSource";
        }
    }

    public class TargetPoisonDotEffectRuntime : EffectRuntimeData
    {
        private readonly EffectSO effectSO;
        private readonly CharacterManager targetCharacter;
        private readonly CharacterManager sourceCharacter;
        private readonly float damagePerTick;
        private readonly float tickIntervalSeconds;

        private float tickTimer;

        public TargetPoisonDotEffectRuntime(
            EffectSO effectSO,
            CharacterManager targetCharacter,
            CharacterManager sourceCharacter,
            float damagePerTick,
            float tickIntervalSeconds)
        {
            this.effectSO = effectSO;
            this.targetCharacter = targetCharacter;
            this.sourceCharacter = sourceCharacter;
            this.damagePerTick = damagePerTick;
            this.tickIntervalSeconds = Mathf.Max(0.05f, tickIntervalSeconds);
            RuntimeId =
                $"TargetPoisonDot_{effectSO.EffectId}_{GetSourceRuntimeId()}_{GetTargetRuntimeId()}";
        }

        public override void OnApply()
        {
            IsActive = effectSO != null
                && targetCharacter != null
                && damagePerTick > 0f;
            tickTimer = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (!IsActive
                || targetCharacter == null
                || targetCharacter.RuntimeData == null
                || targetCharacter.RuntimeData.isDead)
            {
                IsActive = false;
                return;
            }

            tickTimer -= deltaTime;
            if (tickTimer > 0f)
            {
                return;
            }

            tickTimer = tickIntervalSeconds;

            targetCharacter.TakeDamage(
                damagePerTick,
                false);
        }

        private string GetSourceRuntimeId()
        {
            return sourceCharacter != null
                ? sourceCharacter.GetInstanceID().ToString()
                : "NoSource";
        }

        private string GetTargetRuntimeId()
        {
            return targetCharacter != null
                ? targetCharacter.GetInstanceID().ToString()
                : "NoTarget";
        }
    }
}
