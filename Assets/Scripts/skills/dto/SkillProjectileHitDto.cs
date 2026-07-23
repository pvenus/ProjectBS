using Skills.Dto;
using System.Collections.Generic;
using Effect;
using Skill;
using UnityEngine;

[System.Serializable]
public class SkillProjectileHitDto
{
    public int maxHitCount = 1;
    public bool ignoreSameRoot = true;
    public bool useRepeatInterval;
    public float repeatInterval = 0.25f;

    public float hitStartTime;
    public bool deactivateAfterFirstHit;

    public LayerMask targetLayerMask = ~0;

    public float projectileColliderRadius = 0.5f;

    public SkillDamageProfileDto damageProfile;

    public EquipmentSkillSO spawnSkill;

    public EffectEntrySO[] buffEffectEntries;
    public EffectEntrySO[] debuffEffectEntries;
    public List<EffectUpgradeModifierData> effectUpgradeModifiers;

    public int splitHitCount = 1;
    public float splitHitInterval;
}
