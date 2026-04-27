

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Effect가 언제 발동되는지 구분하는 트리거 타입.
/// </summary>
public enum SkillEffectTriggerType
{
    None = 0,
    OnHit = 1,
    OnKill = 2,
    OnCast = 3,
    OnProjectileSpawn = 4,
    OnOverheat = 5,
    Passive = 10
}

/// <summary>
/// Effect가 누구에게 적용되는지 구분하는 대상 타입.
/// </summary>
public enum SkillEffectTargetType
{
    None = 0,
    Target = 1,
    Caster = 2,
    Area = 3,
    Projectile = 4
}

/// <summary>
/// Effect SO 하나를 런타임에서 해석한 결과 데이터.
/// Resolver가 스킬/장비/강화/룬에서 수집한 Effect를 실행 단계에서 쓰기 좋은 형태로 보관한다.
/// </summary>
[Serializable]
public class EffectRuntimeData
{
    [Header("Source")]
    public SkillEffectSO sourceEffect;
    public string effectId;
    public string displayName;

    [Header("Trigger")]
    public SkillEffectTriggerType triggerType = SkillEffectTriggerType.None;
    public SkillEffectTargetType targetType = SkillEffectTargetType.Target;

    [Header("Value")]
    public float value;
    public float duration;
    public float chance = 1f;
    public int maxStack = 1;

    [Header("Tags")]
    public ElementType elementType = ElementType.None;
    public List<string> tags = new();

    public bool HasEffect => sourceEffect != null;
    public bool CanTrigger => chance >= 1f || UnityEngine.Random.value <= chance;

    public static EffectRuntimeData Empty()
    {
        return new EffectRuntimeData();
    }

    public static EffectRuntimeData FromEffect(SkillEffectSO effect)
    {
        var runtime = new EffectRuntimeData();
        runtime.ApplyEffect(effect);
        return runtime;
    }

    public void ApplyEffect(SkillEffectSO effect)
    {
        sourceEffect = effect;

        if (effect == null)
        {
            effectId = string.Empty;
            displayName = string.Empty;
            triggerType = SkillEffectTriggerType.None;
            targetType = SkillEffectTargetType.Target;
            value = 0f;
            duration = 0f;
            chance = 1f;
            maxStack = 1;
            elementType = ElementType.None;
            tags.Clear();
            return;
        }

        effectId = effect.EffectId;
        displayName = effect.DisplayName;
        triggerType = effect.TriggerType;
        targetType = effect.TargetType;
        value = effect.Value;
        duration = effect.Duration;
        chance = Mathf.Clamp01(effect.Chance);
        maxStack = Mathf.Max(1, effect.MaxStack);
        elementType = effect.ElementType;
        tags = effect.Tags != null ? new List<string>(effect.Tags) : new List<string>();
    }
}

/// <summary>
/// 여러 Effect를 런타임에서 해석한 결과 묶음.
/// 스킬, 룬 여러 개, 장비 고유 효과, 강화 효과에서 수집된 Effect를 하나로 모아 보관한다.
/// </summary>
[Serializable]
public class EffectRuntimeSetData
{
    [Header("Effects")]
    public List<EffectRuntimeData> effects = new();

    public bool HasAnyEffect => effects != null && effects.Count > 0;

    public static EffectRuntimeSetData Empty()
    {
        return new EffectRuntimeSetData();
    }

    public static EffectRuntimeSetData FromEffects(IEnumerable<SkillEffectSO> sourceEffects)
    {
        var set = new EffectRuntimeSetData();
        set.ApplyEffects(sourceEffects);
        return set;
    }

    public void ApplyEffects(IEnumerable<SkillEffectSO> sourceEffects)
    {
        effects.Clear();

        if (sourceEffects == null)
        {
            return;
        }

        foreach (SkillEffectSO effect in sourceEffects)
        {
            if (effect == null)
            {
                continue;
            }

            effects.Add(EffectRuntimeData.FromEffect(effect));
        }
    }

    public List<EffectRuntimeData> GetByTrigger(SkillEffectTriggerType triggerType)
    {
        var result = new List<EffectRuntimeData>();

        if (effects == null || effects.Count == 0)
        {
            return result;
        }

        for (int i = 0; i < effects.Count; i++)
        {
            EffectRuntimeData effect = effects[i];
            if (effect != null && effect.triggerType == triggerType)
            {
                result.Add(effect);
            }
        }

        return result;
    }
}