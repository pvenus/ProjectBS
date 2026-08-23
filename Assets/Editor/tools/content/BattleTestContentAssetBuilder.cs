#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Character;
using Effect;
using Item;
using Skill;
using Skills.Move.Config;
using Stat;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Content
{
    public static class BattleTestContentAssetBuilder
    {
        private const string CharacterRoot = "Assets/Contents/Character";
        private const string SkillRoot = "Assets/Contents/Skill";
        private const string SkillProfileRoot = "Assets/Contents/Skill/Profiles";
        private const string SkillEffectRoot = "Assets/Contents/Skill/Effects";
        private const string StrategicRoot = "Assets/Contents/StrategicSkill";
        private const string StrategicSkillRoot = "Assets/Contents/StrategicSkill/Skills";
        private const string StrategicProfileRoot = "Assets/Contents/StrategicSkill/Profiles";
        private const string StrategicEffectRoot = "Assets/Contents/StrategicSkill/Effects";

        private const string FirstCharacterId = "battle_test.character.vanguard";
        private const string FirstSkillId = "battle_test.skill.basic_attack";
        private const string ReferenceCharacterAnimationSourcePath =
            "Assets/Resources/character/Player/main/character_military_officer_1.asset";

        private static readonly string[] RequiredSlotKeys =
        {
            "basic_attack",
            "active_1",
            "active_2",
            "active_3",
            "passive_1"
        };

        private static readonly CharacterAnimationClipType[] RequiredAnimationClipTypes =
        {
            CharacterAnimationClipType.IdleUpRight,
            CharacterAnimationClipType.IdleUpLeft,
            CharacterAnimationClipType.IdleDownRight,
            CharacterAnimationClipType.IdleDownLeft,
            CharacterAnimationClipType.MoveUpRight,
            CharacterAnimationClipType.MoveUpLeft,
            CharacterAnimationClipType.MoveDownRight,
            CharacterAnimationClipType.MoveDownLeft,
            CharacterAnimationClipType.AttackUpRight,
            CharacterAnimationClipType.AttackUpLeft,
            CharacterAnimationClipType.AttackDownRight,
            CharacterAnimationClipType.AttackDownLeft
        };

        [Serializable]
        private sealed class CharacterDefinitionJson
        {
            public string characterId;
            public string characterType;
            public string job;
            public CharacterAnimationClipDefinitionJson[] animationClips;
            public StatDefinitionJson[] baseStats;
            public CharacterSkillSlotJson[] skills;
        }

        [Serializable]
        private sealed class CharacterAnimationClipDefinitionJson
        {
            public string clipType;
            public string assetPath;
        }

        [Serializable]
        private sealed class StatDefinitionJson
        {
            public string statType;
            public float value;
        }

        [Serializable]
        private sealed class CharacterSkillSlotJson
        {
            public string slotKey;
            public string equipmentId;
        }

        [Serializable]
        private sealed class SkillDefinitionJson
        {
            public string equipmentId;
            public string skillType;
            public string skillComponentType;
            public string category;
            public string targetType;
            public string tacticalNeed;
            public float basePriority;
            public string targetingType;
            public float cooldown;
            public float castTime;
            public float range;
            public bool skipAttackAnimation;
            public int projectileCount = 1;
            public float projectileScale = 1f;
            public float projectileColliderRadius = 0.35f;
            public float projectileLifetime = 1f;
            public float projectileSpawnOffset;
            public float projectileSpreadAngle;
            public string moveType;
            public float moveSpeed;
            public EffectDefinitionJson[] selfEffects;
            public SkillHitDefinitionJson hit;
        }

        [Serializable]
        private sealed class SkillHitDefinitionJson
        {
            public string hitId;
            public int maxHitCount = 1;
            public bool ignoreSameRoot = true;
            public bool useRepeatInterval;
            public float repeatInterval = 0.25f;
            public float hitStartTime;
            public bool deactivateAfterFirstHit = true;
            public string targetLayer;
            public string damageType;
            public float baseDamage;
            public float attackPercentDamage;
            public bool canCritical;
            public bool ignoreDefense;
            public EffectDefinitionJson[] buffEffects;
            public EffectDefinitionJson[] debuffEffects;
        }

        [Serializable]
        private sealed class EffectDefinitionJson
        {
            public string effectId;
            public string effectType;
            public string targetStat;
            public string modifierType;
            public float value;
            public bool useMaxHpPercent;
            public float maxHpPercent;
            public float flatHealAmount;
            public bool useAttackScaling;
            public float attackPercentHeal;
            public bool clampToMaxHp = true;
            public string lifetimeType;
            public string categoryType;
            public float duration;
            public int maxApplyCount = 1;
        }

        [Serializable]
        private sealed class StrategicDefinitionJson
        {
            public string strategicSkillItemId;
            public int gaugeCost;
            public bool reusable = true;
            public int defaultPrice;
            public string[] tags;
            public SkillDefinitionJson skill;
        }

        [MenuItem(
            "Tools/ProjectBS/Contents/Battle Test/Build First Character + Basic Attack",
            false,
            2220)]
        public static void BuildFirstUnit()
        {
            EnsureOutputFolders();

            SkillDefinitionJson skill = ReadJson<SkillDefinitionJson>(
                $"{SkillRoot}/{FirstSkillId}.json");
            EquipmentSkillSO skillSo = BuildSkill(
                skill,
                SkillRoot,
                SkillProfileRoot,
                SkillEffectRoot);

            CharacterDefinitionJson character = ReadJson<CharacterDefinitionJson>(
                $"{CharacterRoot}/{FirstCharacterId}.json");
            BuildCharacter(character, new Dictionary<string, EquipmentSkillSO>
            {
                [skillSo.EquipmentId] = skillSo
            });

            SaveAndRefresh();
            ValidateFirstUnit();

            Debug.Log(
                "[BattleTestContentAssetBuilder] First unit build passed. " +
                "Character=1, Skill=1, EffectSO=1, EffectEntrySO=1.");
        }

        [MenuItem(
            "Tools/ProjectBS/Contents/Battle Test/Build Full Party + Strategic Skills",
            false,
            2221)]
        public static void BuildFullContent()
        {
            EnsureOutputFolders();

            Dictionary<string, EquipmentSkillSO> characterSkills =
                BuildCharacterSkills();
            BuildCharacters(characterSkills);
            BuildStrategicSkills();

            SaveAndRefresh();
            ValidateFullContent();

            Debug.Log(
                "[BattleTestContentAssetBuilder] Full build passed. " +
                "Characters=3, CharacterSkills=5, StrategicItems=4, StrategicSkills=4.");
        }

        [MenuItem(
            "Tools/ProjectBS/Contents/Battle Test/Validate Battle Test Content",
            false,
            2222)]
        public static void ValidateFullContent()
        {
            string[] characterJsonPaths = GetJsonPaths(
                CharacterRoot,
                "battle_test.character.*.json");
            string[] skillJsonPaths = GetJsonPaths(
                SkillRoot,
                "battle_test.skill.*.json");
            string[] strategicJsonPaths = GetJsonPaths(
                StrategicRoot,
                "battle_test.strategic.*.json");

            RequireCount("Character JSON", characterJsonPaths, 3);
            RequireCount("Character Skill JSON", skillJsonPaths, 5);
            RequireCount("Strategic Skill JSON", strategicJsonPaths, 4);

            foreach (string jsonPath in skillJsonPaths)
            {
                SkillDefinitionJson definition = ReadJson<SkillDefinitionJson>(jsonPath);
                ValidateSkill(
                    definition,
                    SkillRoot,
                    SkillProfileRoot,
                    SkillEffectRoot);
            }

            foreach (string jsonPath in characterJsonPaths)
            {
                ValidateCharacter(ReadJson<CharacterDefinitionJson>(jsonPath));
            }

            HashSet<int> costs = new();
            foreach (string jsonPath in strategicJsonPaths)
            {
                StrategicDefinitionJson definition =
                    ReadJson<StrategicDefinitionJson>(jsonPath);
                ValidateStrategic(definition);

                if (!costs.Add(definition.gaugeCost))
                {
                    throw new InvalidOperationException(
                        $"[BattleTestContentAssetBuilder] Duplicate gaugeCost={definition.gaugeCost}.");
                }
            }

            ValidateGuidUniqueness();

            Debug.Log(
                "[BattleTestContentAssetBuilder] Validation passed. " +
                "Owned content references are valid and CharacterAnimationClip paths match the reference CharacterSO.");
        }

        private static void ValidateFirstUnit()
        {
            CharacterDefinitionJson character = ReadJson<CharacterDefinitionJson>(
                $"{CharacterRoot}/{FirstCharacterId}.json");
            SkillDefinitionJson skill = ReadJson<SkillDefinitionJson>(
                $"{SkillRoot}/{FirstSkillId}.json");
            CharacterSO characterSo = AssetDatabase.LoadAssetAtPath<CharacterSO>(
                $"{CharacterRoot}/{FirstCharacterId}.asset");

            ValidateSkill(skill, SkillRoot, SkillProfileRoot, SkillEffectRoot);

            if (characterSo == null
                || characterSo.CharacterType != CharacterType.Player
                || characterSo.CharacterId != character.characterId
                || characterSo.AnimationClips.Count != RequiredAnimationClipTypes.Length
                || characterSo.Skills.Count != 1
                || characterSo.Skills[0].slotKey != "basic_attack"
                || characterSo.Skills[0].skillSo == null)
            {
                throw new InvalidOperationException(
                    "[BattleTestContentAssetBuilder] First CharacterSO unit is invalid.");
            }

            ValidateAnimationClips(characterSo, character);
        }

        private static Dictionary<string, EquipmentSkillSO> BuildCharacterSkills()
        {
            Dictionary<string, EquipmentSkillSO> result = new(StringComparer.Ordinal);
            string[] jsonPaths = GetJsonPaths(
                SkillRoot,
                "battle_test.skill.*.json");

            RequireCount("Character Skill JSON", jsonPaths, 5);

            foreach (string jsonPath in jsonPaths)
            {
                SkillDefinitionJson definition = ReadJson<SkillDefinitionJson>(jsonPath);
                EquipmentSkillSO skill = BuildSkill(
                    definition,
                    SkillRoot,
                    SkillProfileRoot,
                    SkillEffectRoot);
                result.Add(skill.EquipmentId, skill);
            }

            return result;
        }

        private static void BuildCharacters(
            IReadOnlyDictionary<string, EquipmentSkillSO> skills)
        {
            string[] jsonPaths = GetJsonPaths(
                CharacterRoot,
                "battle_test.character.*.json");

            RequireCount("Character JSON", jsonPaths, 3);

            foreach (string jsonPath in jsonPaths)
            {
                BuildCharacter(
                    ReadJson<CharacterDefinitionJson>(jsonPath),
                    skills);
            }
        }

        private static void BuildStrategicSkills()
        {
            string[] jsonPaths = GetJsonPaths(
                StrategicRoot,
                "battle_test.strategic.*.json");

            RequireCount("Strategic Skill JSON", jsonPaths, 4);

            foreach (string jsonPath in jsonPaths)
            {
                StrategicDefinitionJson definition =
                    ReadJson<StrategicDefinitionJson>(jsonPath);
                ValidateStrategicDefinition(definition, jsonPath);

                EquipmentSkillSO skill = BuildSkill(
                    definition.skill,
                    StrategicSkillRoot,
                    StrategicProfileRoot,
                    StrategicEffectRoot);
                StrategicSkillItemSO item = CreateOrLoad<StrategicSkillItemSO>(
                    $"{StrategicRoot}/{definition.strategicSkillItemId}.asset");

                item.strategicSkillItemId = definition.strategicSkillItemId;
                item.icon = null;
                item.gaugeCost = definition.gaugeCost;
                item.reusable = definition.reusable;
                item.skillSo = skill;
                item.defaultPrice = definition.defaultPrice;
                item.tags = definition.tags ?? Array.Empty<string>();

                EditorUtility.SetDirty(item);
                AssetDatabase.SaveAssetIfDirty(item);
            }
        }

        private static CharacterSO BuildCharacter(
            CharacterDefinitionJson definition,
            IReadOnlyDictionary<string, EquipmentSkillSO> skills)
        {
            ValidateCharacterDefinition(definition);

            List<CharacterSkillEntry> skillEntries = new();
            foreach (CharacterSkillSlotJson slot in definition.skills)
            {
                if (!skills.TryGetValue(slot.equipmentId, out EquipmentSkillSO skill))
                {
                    continue;
                }

                skillEntries.Add(new CharacterSkillEntry
                {
                    slotKey = slot.slotKey,
                    skillSo = skill
                });
            }

            List<StatEntry> stats = definition.baseStats
                .Select(stat => new StatEntry
                {
                    statType = ParseEnum<StatType>(stat.statType),
                    value = stat.value
                })
                .ToList();

            List<CharacterAnimationClipEntry> animationClips =
                ResolveAnimationClips(definition);

            CharacterSO character = CreateOrLoad<CharacterSO>(
                $"{CharacterRoot}/{definition.characterId}.asset");
            character.ApplyEditorData(
                definition.characterId,
                ParseEnum<CharacterType>(definition.characterType),
                ParseEnum<CharacterJob>(definition.job),
                animationClips,
                skillEntries,
                stats);

            EditorUtility.SetDirty(character);
            AssetDatabase.SaveAssetIfDirty(character);
            return character;
        }

        private static EquipmentSkillSO BuildSkill(
            SkillDefinitionJson definition,
            string mainRoot,
            string profileRoot,
            string effectRoot)
        {
            ValidateSkillDefinition(definition);

            EffectEntrySO[] selfEffects = BuildEffectEntries(
                definition.selfEffects,
                effectRoot);
            EffectEntrySO[] buffEffects = BuildEffectEntries(
                definition.hit.buffEffects,
                effectRoot);
            EffectEntrySO[] debuffEffects = BuildEffectEntries(
                definition.hit.debuffEffects,
                effectRoot);

            EquipmentBaseProfileSO baseProfile =
                CreateOrLoad<EquipmentBaseProfileSO>(
                    $"{profileRoot}/{definition.equipmentId}.profile.asset");
            baseProfile.ApplyEditorData(
                $"{definition.equipmentId}.profile",
                ParseEnum<SkillType>(definition.skillType),
                ParseEnum<SkillComponentType>(definition.skillComponentType),
                definition.projectileCount,
                definition.projectileScale,
                definition.projectileColliderRadius,
                definition.projectileLifetime);
            baseProfile.ApplyEditorProjectileArrangement(
                ProjectileArrangementType.Spread,
                0f,
                definition.projectileSpreadAngle,
                0f);
            baseProfile.ApplyEditorProjectileSpawn(
                definition.projectileSpawnOffset,
                0f);
            baseProfile.ApplyEditorBrainMeta(
                ParseEnum<BattleSkillCategory>(definition.category),
                ParseEnum<BattleSkillTargetType>(definition.targetType),
                ParseEnum<BattleSkillTacticalNeed>(definition.tacticalNeed),
                definition.basePriority);
            EditorUtility.SetDirty(baseProfile);

            SkillCastSO cast = CreateOrLoad<SkillCastSO>(
                $"{profileRoot}/{definition.equipmentId}.cast.asset");
            cast.ApplyEditorData(
                $"{definition.equipmentId}.cast",
                ParseEnum<TargetingType>(definition.targetingType),
                definition.castTime,
                definition.cooldown,
                definition.range,
                definition.skipAttackAnimation,
                selfEffects);
            cast.ApplyEditorBurst(1, 0f);
            cast.ApplyEditorCastMove(CastMoveType.None, 0f, 0f);
            EditorUtility.SetDirty(cast);

            SkillMoveSO move = CreateOrLoad<SkillMoveSO>(
                $"{profileRoot}/{definition.equipmentId}.move.asset");
            ProjectileMoveType moveType =
                ParseEnum<ProjectileMoveType>(definition.moveType);
            move.ApplyEditorData(
                $"{definition.equipmentId}.move",
                moveType,
                true,
                0f);
            move.ApplyEditorConfig(CreateMoveConfig(
                moveType,
                definition.moveSpeed));
            EditorUtility.SetDirty(move);

            SkillHitSO hit = BuildHit(
                definition.hit,
                profileRoot,
                buffEffects,
                debuffEffects);

            EquipmentSkillSO skill = CreateOrLoad<EquipmentSkillSO>(
                $"{mainRoot}/{definition.equipmentId}.asset");
            SerializedObject serializedSkill = new(skill);
            SetString(serializedSkill, "equipmentId", definition.equipmentId);
            SetObject(serializedSkill, "icon", null);
            SetObject(serializedSkill, "baseProfileSo", baseProfile);
            SetObject(serializedSkill, "castSo", cast);
            SetObjectArray(serializedSkill, "hitSos", new UnityEngine.Object[] { hit });
            SetObject(serializedSkill, "moveSo", move);
            SetObject(serializedSkill, "spawnSkillSo", null);
            SetObject(serializedSkill, "upgradeTableSo", null);
            SetObject(serializedSkill, "baseVisualSo", null);
            serializedSkill.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(skill);
            AssetDatabase.SaveAssetIfDirty(baseProfile);
            AssetDatabase.SaveAssetIfDirty(cast);
            AssetDatabase.SaveAssetIfDirty(move);
            AssetDatabase.SaveAssetIfDirty(hit);
            AssetDatabase.SaveAssetIfDirty(skill);
            return skill;
        }

        private static SkillHitSO BuildHit(
            SkillHitDefinitionJson definition,
            string profileRoot,
            EffectEntrySO[] buffEffects,
            EffectEntrySO[] debuffEffects)
        {
            SkillHitSO hit = CreateOrLoad<SkillHitSO>(
                $"{profileRoot}/{definition.hitId}.asset");
            SerializedObject serializedHit = new(hit);

            SetString(serializedHit, "hitId", definition.hitId);
            SetInt(serializedHit, "maxHitCount", definition.maxHitCount);
            SetBool(serializedHit, "ignoreSameRoot", definition.ignoreSameRoot);
            SetBool(serializedHit, "useRepeatInterval", definition.useRepeatInterval);
            SetFloat(serializedHit, "repeatInterval", definition.repeatInterval);
            SetFloat(serializedHit, "hitStartTime", definition.hitStartTime);
            SetBool(
                serializedHit,
                "deactivateAfterFirstHit",
                definition.deactivateAfterFirstHit);
            SetInt(
                serializedHit,
                "targetLayerMask",
                LayerMask.GetMask(definition.targetLayer));

            SerializedProperty damage = serializedHit.FindProperty("damage");
            SetRelativeInt(
                damage,
                "damageType",
                (int)ParseEnum<DamageType>(definition.damageType));
            SetRelativeFloat(damage, "baseDamage", definition.baseDamage);
            SetRelativeFloat(
                damage,
                "firstHitBaseDamage",
                definition.baseDamage);
            SetRelativeFloat(
                damage,
                "attackPercentDamage",
                definition.attackPercentDamage);
            SetRelativeBool(damage, "canCritical", definition.canCritical);
            SetRelativeBool(damage, "ignoreDefense", definition.ignoreDefense);

            SerializedProperty effects = serializedHit.FindProperty("effects");
            SetRelativeObjectArray(effects, "buffEffects", buffEffects);
            SetRelativeObjectArray(effects, "debuffEffects", debuffEffects);

            SerializedProperty split = serializedHit.FindProperty("split");
            SetRelativeBool(split, "useSplitMultiHitDamage", false);
            SetRelativeInt(split, "splitHitCount", 1);
            SetRelativeFloat(split, "splitHitInterval", 0f);

            serializedHit.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(hit);
            return hit;
        }

        private static EffectEntrySO[] BuildEffectEntries(
            IReadOnlyList<EffectDefinitionJson> definitions,
            string effectRoot)
        {
            if (definitions == null || definitions.Count == 0)
            {
                return Array.Empty<EffectEntrySO>();
            }

            EffectEntrySO[] result = new EffectEntrySO[definitions.Count];

            for (int i = 0; i < definitions.Count; i++)
            {
                EffectDefinitionJson definition = definitions[i];
                ValidateEffectDefinition(definition);

                EffectConfig config = CreateEffectConfig(definition);
                EffectSO effect = CreateOrLoad<EffectSO>(
                    $"{effectRoot}/{definition.effectId}.asset");
                effect.ApplyEditorData(definition.effectId, null, config);
                EditorUtility.SetDirty(effect);

                EffectEntrySO entry = CreateOrLoad<EffectEntrySO>(
                    $"{effectRoot}/{definition.effectId}.entry.asset");
                entry.ApplyEditorData(
                    effect,
                    ParseEnum<EffectLifetimeType>(definition.lifetimeType),
                    ParseEnum<EffectCategoryType>(definition.categoryType),
                    definition.duration,
                    definition.maxApplyCount,
                    false,
                    0f);
                EditorUtility.SetDirty(entry);

                AssetDatabase.SaveAssetIfDirty(effect);
                AssetDatabase.SaveAssetIfDirty(entry);
                result[i] = entry;
            }

            return result;
        }

        private static EffectConfig CreateEffectConfig(
            EffectDefinitionJson definition)
        {
            if (string.Equals(
                    definition.effectType,
                    "StatModifier",
                    StringComparison.OrdinalIgnoreCase))
            {
                StatModifierEffectConfig config = new();
                config.ApplyEditorData(
                    ParseEnum<StatType>(definition.targetStat),
                    ParseEnum<StatModifierType>(definition.modifierType),
                    definition.value);
                return config;
            }

            if (string.Equals(
                    definition.effectType,
                    "Heal",
                    StringComparison.OrdinalIgnoreCase))
            {
                HealEffectConfig config = new();
                config.ApplyEditorData(
                    definition.useMaxHpPercent,
                    definition.maxHpPercent,
                    definition.flatHealAmount,
                    definition.useAttackScaling,
                    definition.attackPercentHeal,
                    definition.clampToMaxHp);
                return config;
            }

            throw new InvalidOperationException(
                $"[BattleTestContentAssetBuilder] Unsupported effectType={definition.effectType}.");
        }

        private static SkillMoveConfig CreateMoveConfig(
            ProjectileMoveType moveType,
            float speed)
        {
            return moveType switch
            {
                ProjectileMoveType.Linear => new LinearMoveConfig { speed = speed },
                ProjectileMoveType.Homing => new HomingMoveConfig
                {
                    speed = speed,
                    turnSpeed = 360f
                },
                _ => null
            };
        }

        private static void ValidateCharacter(
            CharacterDefinitionJson definition)
        {
            CharacterSO character = AssetDatabase.LoadAssetAtPath<CharacterSO>(
                $"{CharacterRoot}/{definition.characterId}.asset");

            if (character == null
                || character.CharacterId != definition.characterId
                || character.CharacterType != CharacterType.Player
                || character.Job != ParseEnum<CharacterJob>(definition.job)
                || character.AnimationClips.Count != RequiredAnimationClipTypes.Length
                || character.Skills.Count != RequiredSlotKeys.Length
                || character.BaseStats.Count != definition.baseStats.Length)
            {
                throw new InvalidOperationException(
                    $"[BattleTestContentAssetBuilder] CharacterSO mismatch. id={definition.characterId}.");
            }

            ValidateAnimationClips(character, definition);

            for (int i = 0; i < RequiredSlotKeys.Length; i++)
            {
                CharacterSkillEntry entry = character.Skills[i];
                if (entry == null
                    || entry.slotKey != RequiredSlotKeys[i]
                    || entry.skillSo == null
                    || !AssetDatabase.GetAssetPath(entry.skillSo)
                        .StartsWith(SkillRoot + "/", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"[BattleTestContentAssetBuilder] Character skill slot mismatch. " +
                        $"character={definition.characterId} slot={RequiredSlotKeys[i]}.");
                }
            }
        }

        private static void ValidateSkill(
            SkillDefinitionJson definition,
            string mainRoot,
            string profileRoot,
            string effectRoot)
        {
            EquipmentSkillSO skill = AssetDatabase.LoadAssetAtPath<EquipmentSkillSO>(
                $"{mainRoot}/{definition.equipmentId}.asset");

            if (skill == null
                || skill.EquipmentId != definition.equipmentId
                || skill.Icon != null
                || skill.BaseProfileSo == null
                || skill.CastSo == null
                || skill.MoveSo == null
                || skill.HitSos == null
                || skill.HitSos.Length != 1
                || skill.HitSos[0] == null
                || skill.BaseProfileSo.SkillType != ParseEnum<SkillType>(definition.skillType)
                || !Approximately(skill.CastSo.Cooldown, definition.cooldown)
                || skill.CastSo.TargetingType != ParseEnum<TargetingType>(definition.targetingType))
            {
                throw new InvalidOperationException(
                    $"[BattleTestContentAssetBuilder] EquipmentSkillSO mismatch. id={definition.equipmentId}.");
            }

            ValidateOwnedPath(skill, mainRoot, definition.equipmentId);
            ValidateOwnedPath(skill.BaseProfileSo, profileRoot, definition.equipmentId);
            ValidateOwnedPath(skill.CastSo, profileRoot, definition.equipmentId);
            ValidateOwnedPath(skill.MoveSo, profileRoot, definition.equipmentId);
            ValidateOwnedPath(skill.HitSos[0], profileRoot, definition.equipmentId);

            ValidateEffectReferences(definition.selfEffects, skill.CastSo.SelfEffects, effectRoot);
            ValidateEffectReferences(
                definition.hit.buffEffects,
                skill.HitSos[0].BuffEffects,
                effectRoot);
            ValidateEffectReferences(
                definition.hit.debuffEffects,
                skill.HitSos[0].DebuffEffects,
                effectRoot);
        }

        private static void ValidateStrategic(
            StrategicDefinitionJson definition)
        {
            ValidateStrategicDefinition(definition, definition.strategicSkillItemId);

            StrategicSkillItemSO item =
                AssetDatabase.LoadAssetAtPath<StrategicSkillItemSO>(
                    $"{StrategicRoot}/{definition.strategicSkillItemId}.asset");

            if (item == null
                || item.strategicSkillItemId != definition.strategicSkillItemId
                || item.icon != null
                || item.gaugeCost != definition.gaugeCost
                || !item.reusable
                || item.skillSo == null
                || item.skillSo.EquipmentId != definition.skill.equipmentId)
            {
                throw new InvalidOperationException(
                    $"[BattleTestContentAssetBuilder] StrategicSkillItemSO mismatch. " +
                    $"id={definition.strategicSkillItemId}.");
            }

            ValidateOwnedPath(item, StrategicRoot, definition.strategicSkillItemId);
            ValidateSkill(
                definition.skill,
                StrategicSkillRoot,
                StrategicProfileRoot,
                StrategicEffectRoot);
        }

        private static void ValidateEffectReferences(
            IReadOnlyList<EffectDefinitionJson> definitions,
            IReadOnlyList<EffectEntrySO> entries,
            string effectRoot)
        {
            int expectedCount = definitions != null ? definitions.Count : 0;
            int actualCount = entries != null ? entries.Count : 0;

            if (expectedCount != actualCount)
            {
                throw new InvalidOperationException(
                    "[BattleTestContentAssetBuilder] EffectEntry count mismatch.");
            }

            for (int i = 0; i < expectedCount; i++)
            {
                EffectDefinitionJson definition = definitions[i];
                EffectEntrySO entry = entries[i];

                if (entry == null
                    || entry.EffectSO == null
                    || entry.EffectSO.EffectId != definition.effectId
                    || entry.EffectSO.Icon != null)
                {
                    throw new InvalidOperationException(
                        $"[BattleTestContentAssetBuilder] Effect reference mismatch. " +
                        $"effect={definition.effectId}.");
                }

                ValidateOwnedPath(entry, effectRoot, definition.effectId);
                ValidateOwnedPath(entry.EffectSO, effectRoot, definition.effectId);
            }
        }

        private static void ValidateGuidUniqueness()
        {
            string[] roots = { CharacterRoot, SkillRoot, StrategicRoot };
            string[] guids = AssetDatabase.FindAssets(string.Empty, roots);
            Dictionary<string, string> guidToPath = new(StringComparer.Ordinal);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!guidToPath.TryAdd(guid, path)
                    && guidToPath[guid] != path)
                {
                    throw new InvalidOperationException(
                        $"[BattleTestContentAssetBuilder] Duplicate GUID. guid={guid}.");
                }
            }
        }

        private static void ValidateCharacterDefinition(
            CharacterDefinitionJson definition)
        {
            if (definition == null
                || string.IsNullOrWhiteSpace(definition.characterId)
                || ParseEnum<CharacterType>(definition.characterType) != CharacterType.Player
                || definition.baseStats == null
                || definition.baseStats.Length == 0
                || definition.animationClips == null
                || definition.animationClips.Length != RequiredAnimationClipTypes.Length
                || definition.skills == null
                || definition.skills.Length != RequiredSlotKeys.Length)
            {
                throw new InvalidOperationException(
                    "[BattleTestContentAssetBuilder] Invalid Character JSON.");
            }

            ValidateAnimationDefinition(definition);

            for (int i = 0; i < RequiredSlotKeys.Length; i++)
            {
                if (definition.skills[i] == null
                    || definition.skills[i].slotKey != RequiredSlotKeys[i]
                    || string.IsNullOrWhiteSpace(definition.skills[i].equipmentId))
                {
                    throw new InvalidOperationException(
                        $"[BattleTestContentAssetBuilder] Invalid Character slot. " +
                        $"character={definition.characterId} index={i}.");
                }
            }

            foreach (StatDefinitionJson stat in definition.baseStats)
            {
                ParseEnum<StatType>(stat.statType);
            }

            ParseEnum<CharacterJob>(definition.job);
        }

        private static List<CharacterAnimationClipEntry> ResolveAnimationClips(
            CharacterDefinitionJson definition)
        {
            CharacterSO reference = AssetDatabase.LoadAssetAtPath<CharacterSO>(
                ReferenceCharacterAnimationSourcePath);
            if (reference == null
                || reference.AnimationClips.Count != RequiredAnimationClipTypes.Length)
            {
                throw new InvalidOperationException(
                    $"[BattleTestContentAssetBuilder] Reference CharacterSO animation data is invalid. " +
                    $"path={ReferenceCharacterAnimationSourcePath}.");
            }

            List<CharacterAnimationClipEntry> result = new(RequiredAnimationClipTypes.Length);
            for (int i = 0; i < RequiredAnimationClipTypes.Length; i++)
            {
                CharacterAnimationClipDefinitionJson source = definition.animationClips[i];
                CharacterAnimationClipType clipType =
                    ParseEnum<CharacterAnimationClipType>(source.clipType);
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                    source.assetPath);
                CharacterAnimationClipEntry referenceEntry = reference.AnimationClips[i];

                if (clipType != RequiredAnimationClipTypes[i]
                    || clip == null
                    || referenceEntry == null
                    || referenceEntry.clipType != clipType
                    || referenceEntry.clip != clip)
                {
                    throw new InvalidOperationException(
                        $"[BattleTestContentAssetBuilder] AnimationClip reference mismatch. " +
                        $"character={definition.characterId} index={i} " +
                        $"clipType={source.clipType} path={source.assetPath}.");
                }

                result.Add(new CharacterAnimationClipEntry
                {
                    clipType = clipType,
                    clip = clip
                });
            }

            return result;
        }

        private static void ValidateAnimationDefinition(
            CharacterDefinitionJson definition)
        {
            HashSet<CharacterAnimationClipType> clipTypes = new();
            for (int i = 0; i < RequiredAnimationClipTypes.Length; i++)
            {
                CharacterAnimationClipDefinitionJson entry = definition.animationClips[i];
                if (entry == null
                    || string.IsNullOrWhiteSpace(entry.clipType)
                    || string.IsNullOrWhiteSpace(entry.assetPath))
                {
                    throw new InvalidOperationException(
                        $"[BattleTestContentAssetBuilder] Missing AnimationClip JSON data. " +
                        $"character={definition.characterId} index={i}.");
                }

                CharacterAnimationClipType clipType =
                    ParseEnum<CharacterAnimationClipType>(entry.clipType);
                if (clipType != RequiredAnimationClipTypes[i]
                    || !clipTypes.Add(clipType))
                {
                    throw new InvalidOperationException(
                        $"[BattleTestContentAssetBuilder] Invalid AnimationClip slot order. " +
                        $"character={definition.characterId} index={i} clipType={entry.clipType}.");
                }
            }

            ResolveAnimationClips(definition);
        }

        private static void ValidateAnimationClips(
            CharacterSO character,
            CharacterDefinitionJson definition)
        {
            List<CharacterAnimationClipEntry> expected = ResolveAnimationClips(definition);
            if (character.AnimationClips.Count != expected.Count)
            {
                throw new InvalidOperationException(
                    $"[BattleTestContentAssetBuilder] AnimationClip count mismatch. " +
                    $"character={definition.characterId}.");
            }

            for (int i = 0; i < expected.Count; i++)
            {
                CharacterAnimationClipEntry actual = character.AnimationClips[i];
                if (actual == null
                    || actual.clipType != expected[i].clipType
                    || actual.clip == null
                    || actual.clip != expected[i].clip
                    || AssetDatabase.GetAssetPath(actual.clip)
                        != definition.animationClips[i].assetPath)
                {
                    throw new InvalidOperationException(
                        $"[BattleTestContentAssetBuilder] CharacterSO AnimationClip mismatch. " +
                        $"character={definition.characterId} index={i}.");
                }
            }
        }

        private static void ValidateSkillDefinition(
            SkillDefinitionJson definition)
        {
            if (definition == null
                || string.IsNullOrWhiteSpace(definition.equipmentId)
                || definition.projectileCount < 1
                || definition.projectileScale <= 0f
                || definition.projectileColliderRadius <= 0f
                || definition.projectileLifetime <= 0f
                || definition.cooldown < 0f
                || definition.range < 0f
                || definition.hit == null
                || string.IsNullOrWhiteSpace(definition.hit.hitId)
                || definition.hit.maxHitCount < 1
                || string.IsNullOrWhiteSpace(definition.hit.targetLayer)
                || LayerMask.NameToLayer(definition.hit.targetLayer) < 0)
            {
                throw new InvalidOperationException(
                    $"[BattleTestContentAssetBuilder] Invalid Skill JSON. " +
                    $"id={definition?.equipmentId ?? "<null>"}.");
            }

            ParseEnum<SkillType>(definition.skillType);
            ParseEnum<SkillComponentType>(definition.skillComponentType);
            ParseEnum<BattleSkillCategory>(definition.category);
            ParseEnum<BattleSkillTargetType>(definition.targetType);
            ParseEnum<BattleSkillTacticalNeed>(definition.tacticalNeed);
            ParseEnum<TargetingType>(definition.targetingType);
            ParseEnum<ProjectileMoveType>(definition.moveType);
            ParseEnum<DamageType>(definition.hit.damageType);
        }

        private static void ValidateEffectDefinition(
            EffectDefinitionJson definition)
        {
            if (definition == null
                || string.IsNullOrWhiteSpace(definition.effectId)
                || (definition.maxApplyCount < 1))
            {
                throw new InvalidOperationException(
                    "[BattleTestContentAssetBuilder] Invalid Effect JSON.");
            }

            if (string.Equals(
                    definition.effectType,
                    "StatModifier",
                    StringComparison.OrdinalIgnoreCase))
            {
                ParseEnum<StatType>(definition.targetStat);
                ParseEnum<StatModifierType>(definition.modifierType);
            }
            else if (!string.Equals(
                         definition.effectType,
                         "Heal",
                         StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"[BattleTestContentAssetBuilder] Unsupported effectType={definition.effectType}.");
            }

            ParseEnum<EffectLifetimeType>(definition.lifetimeType);
            ParseEnum<EffectCategoryType>(definition.categoryType);
        }

        private static void ValidateStrategicDefinition(
            StrategicDefinitionJson definition,
            string source)
        {
            if (definition == null
                || string.IsNullOrWhiteSpace(definition.strategicSkillItemId)
                || definition.gaugeCost <= 0
                || !definition.reusable
                || definition.skill == null
                || ParseEnum<TargetingType>(definition.skill.targetingType)
                    != TargetingType.Position)
            {
                throw new InvalidOperationException(
                    $"[BattleTestContentAssetBuilder] Invalid Strategic JSON. source={source}.");
            }

            ValidateSkillDefinition(definition.skill);
        }

        private static void ValidateOwnedPath(
            UnityEngine.Object asset,
            string root,
            string ownerId)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            if (asset == null
                || string.IsNullOrWhiteSpace(path)
                || !path.StartsWith(root + "/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"[BattleTestContentAssetBuilder] Asset escaped owned path. " +
                    $"owner={ownerId} expectedRoot={root} actual={path}.");
            }
        }

        private static T ReadJson<T>(string path)
            where T : class
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    "[BattleTestContentAssetBuilder] Required JSON is missing.",
                    path);
            }

            T result = JsonUtility.FromJson<T>(File.ReadAllText(path));
            if (result == null)
            {
                throw new InvalidOperationException(
                    $"[BattleTestContentAssetBuilder] JSON parse failed. path={path}.");
            }

            return result;
        }

        private static string[] GetJsonPaths(
            string root,
            string pattern)
        {
            if (!Directory.Exists(root))
            {
                return Array.Empty<string>();
            }

            string[] paths = Directory.GetFiles(
                root,
                pattern,
                SearchOption.TopDirectoryOnly);
            Array.Sort(paths, StringComparer.Ordinal);
            return paths;
        }

        private static void RequireCount(
            string label,
            IReadOnlyCollection<string> paths,
            int expected)
        {
            if (paths.Count != expected)
            {
                throw new InvalidOperationException(
                    $"[BattleTestContentAssetBuilder] Expected {expected} {label} files, " +
                    $"found {paths.Count}.");
            }
        }

        private static T CreateOrLoad<T>(string path)
            where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureOutputFolders()
        {
            foreach (string folder in new[]
                     {
                         CharacterRoot,
                         SkillRoot,
                         SkillProfileRoot,
                         SkillEffectRoot,
                         StrategicRoot,
                         StrategicSkillRoot,
                         StrategicProfileRoot,
                         StrategicEffectRoot
                     })
            {
                EnsureFolder(folder);
            }
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string[] parts = folder.Split('/');
            string current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static void SaveAndRefresh()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static TEnum ParseEnum<TEnum>(string value)
            where TEnum : struct, Enum
        {
            if (!string.IsNullOrWhiteSpace(value)
                && Enum.TryParse(value, true, out TEnum result)
                && Enum.IsDefined(typeof(TEnum), result))
            {
                return result;
            }

            throw new InvalidOperationException(
                $"[BattleTestContentAssetBuilder] Enum parse failed. " +
                $"enum={typeof(TEnum).Name} value={value ?? "<null>"}.");
        }

        private static bool Approximately(float left, float right)
        {
            return Mathf.Abs(left - right) <= 0.0001f;
        }

        private static void SetString(
            SerializedObject serializedObject,
            string propertyName,
            string value)
        {
            RequireProperty(serializedObject, propertyName).stringValue = value;
        }

        private static void SetObject(
            SerializedObject serializedObject,
            string propertyName,
            UnityEngine.Object value)
        {
            RequireProperty(serializedObject, propertyName).objectReferenceValue = value;
        }

        private static void SetInt(
            SerializedObject serializedObject,
            string propertyName,
            int value)
        {
            RequireProperty(serializedObject, propertyName).intValue = value;
        }

        private static void SetFloat(
            SerializedObject serializedObject,
            string propertyName,
            float value)
        {
            RequireProperty(serializedObject, propertyName).floatValue = value;
        }

        private static void SetBool(
            SerializedObject serializedObject,
            string propertyName,
            bool value)
        {
            RequireProperty(serializedObject, propertyName).boolValue = value;
        }

        private static void SetObjectArray(
            SerializedObject serializedObject,
            string propertyName,
            IReadOnlyList<UnityEngine.Object> values)
        {
            SerializedProperty property = RequireProperty(
                serializedObject,
                propertyName);
            property.arraySize = values.Count;

            for (int i = 0; i < values.Count; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static void SetRelativeObjectArray(
            SerializedProperty parent,
            string propertyName,
            IReadOnlyList<EffectEntrySO> values)
        {
            SerializedProperty property = RequireRelativeProperty(parent, propertyName);
            property.arraySize = values.Count;

            for (int i = 0; i < values.Count; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static void SetRelativeInt(
            SerializedProperty parent,
            string propertyName,
            int value)
        {
            RequireRelativeProperty(parent, propertyName).intValue = value;
        }

        private static void SetRelativeFloat(
            SerializedProperty parent,
            string propertyName,
            float value)
        {
            RequireRelativeProperty(parent, propertyName).floatValue = value;
        }

        private static void SetRelativeBool(
            SerializedProperty parent,
            string propertyName,
            bool value)
        {
            RequireRelativeProperty(parent, propertyName).boolValue = value;
        }

        private static SerializedProperty RequireProperty(
            SerializedObject serializedObject,
            string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"[BattleTestContentAssetBuilder] Serialized property missing. " +
                    $"type={serializedObject.targetObject.GetType().Name} property={propertyName}.");
            }

            return property;
        }

        private static SerializedProperty RequireRelativeProperty(
            SerializedProperty parent,
            string propertyName)
        {
            if (parent == null)
            {
                throw new InvalidOperationException(
                    $"[BattleTestContentAssetBuilder] Serialized parent is missing. property={propertyName}.");
            }

            SerializedProperty property = parent.FindPropertyRelative(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"[BattleTestContentAssetBuilder] Relative serialized property missing. " +
                    $"parent={parent.propertyPath} property={propertyName}.");
            }

            return property;
        }
    }
}
#endif
