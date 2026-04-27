using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SkillUpgradeMono
///
/// ScriptableObject 자체를 직접 수정하지 않고,
/// 스킬 업그레이드/버프/레벨업에 따른 런타임 보정값을 관리하는 컴포넌트.
/// 각 스킬별로 누적된 추가값/배율값을 보관하고,
/// 이후 SO -> DTO 변환 시 이 값을 함께 전달하는 용도로 사용한다.
/// </summary>
[DisallowMultipleComponent]
public class SkillUpgradeMono : MonoBehaviour
{
    [Serializable]
    public class SkillUpgradeState
    {
        [Tooltip("0 = BasicAttack, 1 = Skill1, 2 = Skill2, 3 = Skill3")]
        public int slotIndex;

        [Header("Damage")]
        public int damageLevel;
        public float damagePerLevel = 1f;

        [Header("Range")]
        public int rangeLevel;
        public float rangePerLevel = 0.5f;

        [Header("Cooldown")]
        public int cooldownLevel;
        public float cooldownPerLevel = -0.1f;

        [Header("Projectile")]
        public int projectileScaleLevel;
        public float projectileScalePerLevel = 0.15f;
        public int projectileSpeedLevel;
        public float projectileSpeedPerLevel = 0.5f;
        public int projectileLifetimeLevel;
        public float projectileLifetimePerLevel = 0.2f;
        public int projectileCountLevel;
        public float projectileCountPerLevel = 1f;

        [Header("Hit")]
        public int knockbackForceLevel;
        public float knockbackForcePerLevel = 0.5f;

        public SkillUpgradeData ToData()
        {
            return new SkillUpgradeData
            {
                damageAdd = damageLevel * damagePerLevel,
                rangeAdd = rangeLevel * rangePerLevel,
                cooldownAdd = cooldownLevel * cooldownPerLevel,
                projectileScaleAdd = projectileScaleLevel * projectileScalePerLevel,
                projectileSpeedAdd = projectileSpeedLevel * projectileSpeedPerLevel,
                projectileLifetimeAdd = projectileLifetimeLevel * projectileLifetimePerLevel,
                projectileCountAdd = projectileCountLevel * projectileCountPerLevel,
                knockbackForceAdd = knockbackForceLevel * knockbackForcePerLevel
            };
        }

        public void ApplyDelta(SkillUpgradeData delta)
        {
            damageAddToLevel(ref damageLevel, delta.damageAdd, damagePerLevel);
            damageAddToLevel(ref rangeLevel, delta.rangeAdd, rangePerLevel);
            damageAddToLevel(ref cooldownLevel, delta.cooldownAdd, cooldownPerLevel);
            damageAddToLevel(ref projectileScaleLevel, delta.projectileScaleAdd, projectileScalePerLevel);
            damageAddToLevel(ref projectileSpeedLevel, delta.projectileSpeedAdd, projectileSpeedPerLevel);
            damageAddToLevel(ref projectileLifetimeLevel, delta.projectileLifetimeAdd, projectileLifetimePerLevel);
            damageAddToLevel(ref projectileCountLevel, delta.projectileCountAdd, projectileCountPerLevel);
            damageAddToLevel(ref knockbackForceLevel, delta.knockbackForceAdd, knockbackForcePerLevel);
        }

        public void AddDamageLevel(int amount = 1) => damageLevel = Mathf.Max(0, damageLevel + amount);
        public void AddRangeLevel(int amount = 1) => rangeLevel = Mathf.Max(0, rangeLevel + amount);
        public void AddCooldownLevel(int amount = 1) => cooldownLevel = Mathf.Max(0, cooldownLevel + amount);
        public void AddProjectileScaleLevel(int amount = 1) => projectileScaleLevel = Mathf.Max(0, projectileScaleLevel + amount);
        public void AddProjectileSpeedLevel(int amount = 1) => projectileSpeedLevel = Mathf.Max(0, projectileSpeedLevel + amount);
        public void AddProjectileLifetimeLevel(int amount = 1) => projectileLifetimeLevel = Mathf.Max(0, projectileLifetimeLevel + amount);
        public void AddProjectileCountLevel(int amount = 1) => projectileCountLevel = Mathf.Max(0, projectileCountLevel + amount);
        public void AddKnockbackForceLevel(int amount = 1) => knockbackForceLevel = Mathf.Max(0, knockbackForceLevel + amount);

        public void ResetValues()
        {
            damageLevel = 0;
            rangeLevel = 0;
            cooldownLevel = 0;
            projectileScaleLevel = 0;
            projectileSpeedLevel = 0;
            projectileLifetimeLevel = 0;
            projectileCountLevel = 0;
            knockbackForceLevel = 0;
        }

        private static void damageAddToLevel(ref int currentLevel, float addValue, float perLevel)
        {
            if (Mathf.Approximately(addValue, 0f))
                return;

            if (Mathf.Approximately(perLevel, 0f))
                return;

            int deltaLevel = Mathf.RoundToInt(addValue / perLevel);
            currentLevel = Mathf.Max(0, currentLevel + deltaLevel);
        }
    }

    [Serializable]
    public struct SkillUpgradeData
    {
        public float damageAdd;
        public float rangeAdd;
        public float cooldownAdd;
        public float projectileScaleAdd;
        public float projectileSpeedAdd;
        public float projectileLifetimeAdd;
        public float projectileCountAdd;
        public float knockbackForceAdd;

        public static SkillUpgradeData Default => new SkillUpgradeData();

        public static SkillUpgradeData Damage(float addValue)
        {
            return new SkillUpgradeData { damageAdd = addValue };
        }

        public static SkillUpgradeData Range(float addValue)
        {
            return new SkillUpgradeData { rangeAdd = addValue };
        }

        public static SkillUpgradeData Cooldown(float addValue)
        {
            return new SkillUpgradeData { cooldownAdd = addValue };
        }

        public static SkillUpgradeData ProjectileScale(float addValue)
        {
            return new SkillUpgradeData { projectileScaleAdd = addValue };
        }

        public static SkillUpgradeData ProjectileSpeed(float addValue)
        {
            return new SkillUpgradeData { projectileSpeedAdd = addValue };
        }

        public static SkillUpgradeData ProjectileLifetime(float addValue)
        {
            return new SkillUpgradeData { projectileLifetimeAdd = addValue };
        }

        public static SkillUpgradeData ProjectileCount(float addValue)
        {
            return new SkillUpgradeData { projectileCountAdd = addValue };
        }

        public static SkillUpgradeData KnockbackForce(float addValue)
        {
            return new SkillUpgradeData { knockbackForceAdd = addValue };
        }
    }

    [Header("Reference")]
    [SerializeField] private SkillLoadoutMono skillLoadout;

    [Header("Upgrade State")]
    [SerializeField] private List<SkillUpgradeState> skillStates = new List<SkillUpgradeState>();

    [Header("Debug")]
    [SerializeField] private bool debugLog;

    private readonly Dictionary<int, SkillUpgradeState> _stateMap = new Dictionary<int, SkillUpgradeState>();
    private bool _initialized;

    private void Awake()
    {
        RebuildCache();
    }

    private void Reset()
    {
        if (skillLoadout == null)
            skillLoadout = GetComponent<SkillLoadoutMono>();
    }

    /// <summary>
    /// 캐시를 다시 만든다. 인스펙터 수정 후 수동 동기화가 필요할 때 사용.
    /// </summary>
    public void RebuildCache()
    {
        if (skillLoadout == null)
            skillLoadout = GetComponent<SkillLoadoutMono>();

        _stateMap.Clear();

        for (int i = 0; i < skillStates.Count; i++)
        {
            SkillUpgradeState state = skillStates[i];
            if (state == null)
                continue;

            int slotIndex = Mathf.Clamp(state.slotIndex, 0, 3);
            state.slotIndex = slotIndex;

            if (_stateMap.ContainsKey(slotIndex))
                continue;

            _stateMap.Add(slotIndex, state);
        }

        _initialized = true;
    }

    public ScriptableObject GetSkillBySlot(int slotIndex)
    {
        EnsureInitialized();

        if (skillLoadout == null)
            return null;

        EquipmentSkillLoadoutEntry entry = skillLoadout.GetEntryBySlot(slotIndex);
        return entry != null ? entry.SkillSo : null;
    }

    public SkillUpgradeData GetUpgradeDataBySlot(int slotIndex)
    {
        EnsureInitialized();
        int safeSlotIndex = Mathf.Clamp(slotIndex, 0, 3);

        if (_stateMap.TryGetValue(safeSlotIndex, out SkillUpgradeState state) && state != null)
            return state.ToData();

        return SkillUpgradeData.Default;
    }

    /// <summary>
    /// 스킬의 현재 업그레이드 데이터를 반환한다.
    /// 등록된 상태가 없으면 기본값을 반환한다.
    /// </summary>
    public SkillUpgradeData GetUpgradeData(ScriptableObject skill)
    {
        int slotIndex = ResolveSlotIndex(skill);
        if (slotIndex < 0)
            return SkillUpgradeData.Default;

        return GetUpgradeDataBySlot(slotIndex);
    }

    /// <summary>
    /// 스킬 상태가 없으면 생성해서 반환한다.
    /// </summary>
    public SkillUpgradeState GetOrCreateState(ScriptableObject skill)
    {
        int slotIndex = ResolveSlotIndex(skill);
        if (slotIndex < 0)
            return null;

        return GetOrCreateStateBySlot(slotIndex);
    }

    public SkillUpgradeState GetOrCreateStateBySlot(int slotIndex)
    {
        EnsureInitialized();

        int safeSlotIndex = Mathf.Clamp(slotIndex, 0, 3);
        if (_stateMap.TryGetValue(safeSlotIndex, out SkillUpgradeState state) && state != null)
            return state;

        state = new SkillUpgradeState
        {
            slotIndex = safeSlotIndex
        };
        state.ResetValues();
        skillStates.Add(state);
        _stateMap[safeSlotIndex] = state;

        if (debugLog)
        {
            ScriptableObject skill = GetSkillBySlot(safeSlotIndex);
            string label = skill != null ? skill.name : $"slot={safeSlotIndex}";
            Debug.Log($"[SkillUpgradeMono] Created upgrade state for {label}", this);
        }

        return state;
    }

    /// <summary>
    /// 특정 스킬에 업그레이드 델타를 누적 적용한다.
    /// </summary>
    public void ApplyUpgrade(ScriptableObject skill, SkillUpgradeData delta)
    {
        int slotIndex = ResolveSlotIndex(skill);
        if (slotIndex < 0)
            return;

        ApplyUpgradeBySlot(slotIndex, delta);
    }

    public void ApplyUpgradeBySlot(int slotIndex, SkillUpgradeData delta)
    {
        SkillUpgradeState state = GetOrCreateStateBySlot(slotIndex);
        if (state == null)
            return;

        state.ApplyDelta(delta);

        if (debugLog)
        {
            ScriptableObject skill = GetSkillBySlot(slotIndex);
            string label = skill != null ? skill.name : $"slot={slotIndex}";
            Debug.Log($"[SkillUpgradeMono] ApplyUpgrade {label}", this);
        }
    }

    public void AddDamageUpgrade(ScriptableObject skill, int amount = 1)
    {
        int slotIndex = ResolveSlotIndex(skill);
        if (slotIndex < 0)
            return;

        AddDamageUpgradeBySlot(slotIndex, amount);
    }

    public void AddDamageUpgradeBySlot(int slotIndex, int amount = 1)
    {
        SkillUpgradeState state = GetOrCreateStateBySlot(slotIndex);
        if (state == null)
            return;

        state.AddDamageLevel(amount);
    }

    public void AddRangeUpgrade(ScriptableObject skill, int amount = 1)
    {
        int slotIndex = ResolveSlotIndex(skill);
        if (slotIndex < 0)
            return;

        AddRangeUpgradeBySlot(slotIndex, amount);
    }

    public void AddRangeUpgradeBySlot(int slotIndex, int amount = 1)
    {
        SkillUpgradeState state = GetOrCreateStateBySlot(slotIndex);
        if (state == null)
            return;

        state.AddRangeLevel(amount);
    }

    public void AddCooldownUpgrade(ScriptableObject skill, int amount = 1)
    {
        int slotIndex = ResolveSlotIndex(skill);
        if (slotIndex < 0)
            return;

        AddCooldownUpgradeBySlot(slotIndex, amount);
    }

    public void AddCooldownUpgradeBySlot(int slotIndex, int amount = 1)
    {
        SkillUpgradeState state = GetOrCreateStateBySlot(slotIndex);
        if (state == null)
            return;

        state.AddCooldownLevel(amount);
    }

    public void AddProjectileScaleUpgrade(ScriptableObject skill, int amount = 1)
    {
        int slotIndex = ResolveSlotIndex(skill);
        if (slotIndex < 0)
            return;

        AddProjectileScaleUpgradeBySlot(slotIndex, amount);
    }

    public void AddProjectileScaleUpgradeBySlot(int slotIndex, int amount = 1)
    {
        SkillUpgradeState state = GetOrCreateStateBySlot(slotIndex);
        if (state == null)
            return;

        state.AddProjectileScaleLevel(amount);
    }

    public void AddProjectileSpeedUpgrade(ScriptableObject skill, int amount = 1)
    {
        int slotIndex = ResolveSlotIndex(skill);
        if (slotIndex < 0)
            return;

        AddProjectileSpeedUpgradeBySlot(slotIndex, amount);
    }

    public void AddProjectileSpeedUpgradeBySlot(int slotIndex, int amount = 1)
    {
        SkillUpgradeState state = GetOrCreateStateBySlot(slotIndex);
        if (state == null)
            return;

        state.AddProjectileSpeedLevel(amount);
    }

    public void AddProjectileLifetimeUpgrade(ScriptableObject skill, int amount = 1)
    {
        int slotIndex = ResolveSlotIndex(skill);
        if (slotIndex < 0)
            return;

        AddProjectileLifetimeUpgradeBySlot(slotIndex, amount);
    }

    public void AddProjectileLifetimeUpgradeBySlot(int slotIndex, int amount = 1)
    {
        SkillUpgradeState state = GetOrCreateStateBySlot(slotIndex);
        if (state == null)
            return;

        state.AddProjectileLifetimeLevel(amount);
    }

    public void AddProjectileCountUpgrade(ScriptableObject skill, int amount = 1)
    {
        int slotIndex = ResolveSlotIndex(skill);
        if (slotIndex < 0)
            return;

        AddProjectileCountUpgradeBySlot(slotIndex, amount);
    }

    public void AddProjectileCountUpgradeBySlot(int slotIndex, int amount = 1)
    {
        SkillUpgradeState state = GetOrCreateStateBySlot(slotIndex);
        if (state == null)
            return;

        state.AddProjectileCountLevel(amount);
    }

    public void AddKnockbackForceUpgrade(ScriptableObject skill, int amount = 1)
    {
        int slotIndex = ResolveSlotIndex(skill);
        if (slotIndex < 0)
            return;

        AddKnockbackForceUpgradeBySlot(slotIndex, amount);
    }

    public void AddKnockbackForceUpgradeBySlot(int slotIndex, int amount = 1)
    {
        SkillUpgradeState state = GetOrCreateStateBySlot(slotIndex);
        if (state == null)
            return;

        state.AddKnockbackForceLevel(amount);
    }

    /// <summary>
    /// 특정 스킬의 누적 업그레이드를 초기화한다.
    /// </summary>
    public void ResetUpgrade(ScriptableObject skill)
    {
        int slotIndex = ResolveSlotIndex(skill);
        if (slotIndex < 0)
            return;

        ResetUpgradeBySlot(slotIndex);
    }

    public void ResetUpgradeBySlot(int slotIndex)
    {
        EnsureInitialized();

        int safeSlotIndex = Mathf.Clamp(slotIndex, 0, 3);
        if (!_stateMap.TryGetValue(safeSlotIndex, out SkillUpgradeState state) || state == null)
            return;

        state.ResetValues();

        if (debugLog)
        {
            ScriptableObject skill = GetSkillBySlot(safeSlotIndex);
            string label = skill != null ? skill.name : $"slot={safeSlotIndex}";
            Debug.Log($"[SkillUpgradeMono] ResetUpgrade {label}", this);
        }
    }

    /// <summary>
    /// 모든 스킬 업그레이드를 초기화한다.
    /// </summary>
    public void ResetAllUpgrades()
    {
        EnsureInitialized();

        for (int i = 0; i < skillStates.Count; i++)
        {
            SkillUpgradeState state = skillStates[i];
            if (state == null)
                continue;

            state.ResetValues();
        }

        if (debugLog)
            Debug.Log("[SkillUpgradeMono] ResetAllUpgrades", this);
    }

    public bool HasState(ScriptableObject skill)
    {
        int slotIndex = ResolveSlotIndex(skill);
        if (slotIndex < 0)
            return false;

        return HasStateBySlot(slotIndex);
    }

    public bool HasStateBySlot(int slotIndex)
    {
        EnsureInitialized();
        return _stateMap.ContainsKey(Mathf.Clamp(slotIndex, 0, 3));
    }

    private int ResolveSlotIndex(ScriptableObject skill)
    {
        if (skill == null)
            return -1;

        EnsureInitialized();

        if (skillLoadout == null)
            return -1;

        for (int slotIndex = 0; slotIndex <= 3; slotIndex++)
        {
            EquipmentSkillLoadoutEntry entry = skillLoadout.GetEntryBySlot(slotIndex);
            if (entry != null && entry.SkillSo == skill)
                return slotIndex;
        }

        return -1;
    }

    private void EnsureInitialized()
    {
        if (_initialized)
            return;

        RebuildCache();
    }
}