using System;
using System.Collections.Generic;
using Presentation;
using Skill;
using Stat;
using String;

namespace Character
{
    public sealed class CharacterPresentationResolver
    {
        public CharacterPresentationData ResolveData(
            CharacterSO character,
            PresentationContext context)
        {
            return ResolveCore(character, null, context ?? PresentationContext.Preview);
        }

        public CharacterPresentationData ResolveData(
            CharacterRuntimeData runtime,
            PresentationContext context)
        {
            return ResolveCore(
                runtime?.characterSO,
                runtime,
                context ?? PresentationContext.Runtime);
        }

        public ContentPresentationData Resolve(
            CharacterSO character,
            PresentationContext context)
        {
            return CreateContent(ResolveData(character, context));
        }

        public ContentPresentationData Resolve(
            CharacterRuntimeData runtime,
            PresentationContext context)
        {
            return CreateContent(ResolveData(runtime, context));
        }

        public ContentPresentationData ResolveForPlayerDisplay(
            CharacterSO character,
            PresentationContext context)
        {
            return CreatePlayerContent(ResolveData(character, context));
        }

        public ContentPresentationData ResolveForPlayerDisplay(
            CharacterRuntimeData runtime,
            PresentationContext context)
        {
            return CreatePlayerContent(ResolveData(runtime, context));
        }

        private CharacterPresentationData ResolveCore(
            CharacterSO character,
            CharacterRuntimeData runtime,
            PresentationContext context)
        {
            if (character == null)
            {
                return new CharacterPresentationData(
                    new PresentationIdentityData(string.Empty, string.Empty),
                    CharacterType.Npc,
                    CharacterJob.None,
                    CharacterJobFamily.None,
                    CharacterJobTier.None,
                    CharacterJobBranch.None,
                    Array.Empty<PresentationEntryData>(),
                    Array.Empty<PresentationEntryData>(),
                    null,
                    new PresentationProvenanceData(PresentationProvenanceKind.Unknown),
                    ContentPresentationStatus.Unsupported);
            }

            bool useRuntime = runtime != null
                && context.Mode == PresentationContextMode.Runtime;
            return new CharacterPresentationData(
                new PresentationIdentityData(
                    character.CharacterId,
                    StringManager.Instance != null ? character.DisplayName : character.name),
                character.CharacterType,
                character.Job,
                character.JobFamily,
                character.JobTier,
                character.JobBranch,
                CreateStats(character, runtime, useRuntime),
                CreateSkills(character, runtime, useRuntime),
                useRuntime ? runtime.isDead : null,
                Provenance(
                    character,
                    useRuntime
                        ? PresentationProvenanceKind.RuntimeResolved
                        : PresentationProvenanceKind.AuthoredAsset),
                ContentPresentationStatus.Supported);
        }

        private static IReadOnlyList<PresentationEntryData> CreateStats(
            CharacterSO character,
            CharacterRuntimeData runtime,
            bool useRuntime)
        {
            IReadOnlyList<StatEntry> source = useRuntime
                ? runtime.finalStats
                : character.BaseStats;
            if (source == null)
            {
                return Array.Empty<PresentationEntryData>();
            }

            List<PresentationEntryData> result = new();
            for (int index = 0; index < source.Count; index++)
            {
                StatEntry stat = source[index];
                if (stat == null)
                {
                    continue;
                }

                result.Add(new PresentationEntryData(
                    $"Character.Stat.{stat.statType}",
                    new[]
                    {
                        PresentationValueData.Number(
                            stat.value,
                            GetStatUnit(stat.statType),
                            Provenance(
                                character,
                                useRuntime
                                    ? PresentationProvenanceKind.RuntimeResolved
                                    : PresentationProvenanceKind.AuthoredAsset,
                                useRuntime
                                    ? $"finalStats[{index}]"
                                    : $"BaseStats[{index}]")),
                    }));
            }

            return result;
        }

        private static IReadOnlyList<PresentationEntryData> CreateSkills(
            CharacterSO character,
            CharacterRuntimeData runtime,
            bool useRuntime)
        {
            List<PresentationEntryData> result = new();
            IReadOnlyList<CharacterSkillEntry> skills = character.Skills;
            if (skills == null)
            {
                return result;
            }

            foreach (CharacterSkillEntry entry in skills)
            {
                EquipmentSkillSO skill = entry?.skillSo;
                if (skill == null)
                {
                    continue;
                }

                List<PresentationValueData> values = new()
                {
                    PresentationValueData.SemanticToken(
                        StringManager.Instance != null ? skill.DisplayName : skill.name),
                };
                if (useRuntime)
                {
                    values.Add(PresentationValueData.Number(
                        runtime.GetSkillLevel(skill.EquipmentId),
                        PresentationValueUnit.Count,
                        Provenance(character, PresentationProvenanceKind.RuntimeResolved, "skillInstances.level")));
                }

                result.Add(new PresentationEntryData(
                    string.IsNullOrWhiteSpace(entry.slotKey)
                        ? "Character.Skill"
                        : $"Character.Skill.{entry.slotKey}",
                    values,
                    skill.EquipmentId));
            }

            return result;
        }

        private static ContentPresentationData CreateContent(
            CharacterPresentationData data)
        {
            if (data == null)
            {
                return null;
            }

            List<string> classifications = new()
            {
                $"Character.Type.{data.CharacterType}",
                $"Character.Job.{data.Job}",
                $"Character.JobFamily.{data.JobFamily}",
                $"Character.JobTier.{data.JobTier}",
            };
            if (data.JobBranch != CharacterJobBranch.None)
            {
                classifications.Add($"Character.JobBranch.{data.JobBranch}");
            }

            List<PresentationGroupData> groups = new();
            if (data.Stats.Count > 0)
            {
                groups.Add(new PresentationGroupData("Character.Stats", data.Stats));
            }
            if (data.Skills.Count > 0)
            {
                groups.Add(new PresentationGroupData("Character.Skills", data.Skills));
            }
            if (data.IsDead.HasValue)
            {
                groups.Add(new PresentationGroupData(
                    "Character.Runtime",
                    new[]
                    {
                        new PresentationEntryData(
                            "Character.Runtime.State",
                            new[]
                            {
                                PresentationValueData.SemanticToken(
                                    data.IsDead.Value ? "Dead" : "Alive"),
                            }),
                    }));
            }

            return new ContentPresentationData(
                data.Identity,
                string.Empty,
                classifications,
                groups,
                data.Provenance,
                data.Status);
        }

        private static ContentPresentationData CreatePlayerContent(
            CharacterPresentationData data)
        {
            if (data == null)
            {
                return null;
            }

            List<string> classifications = new();
            AddPlayerTag(classifications, $"Character.Type.{data.CharacterType}");
            AddPlayerTag(classifications, $"Character.Job.{data.Job}");

            List<PresentationEntryData> visibleStats = new();
            for (int index = 0; index < data.Stats.Count; index++)
            {
                PresentationEntryData entry = data.Stats[index];
                if (entry != null
                    && PresentationDisplayCatalog.IsPlayerVisibleEntry(entry.Key))
                {
                    visibleStats.Add(entry);
                }
            }

            List<PresentationGroupData> groups = new();
            if (visibleStats.Count > 0
                && !string.IsNullOrWhiteSpace(
                    PresentationDisplayCatalog.GetGroupLabelKey("Character.Stats")))
            {
                groups.Add(new PresentationGroupData("Character.Stats", visibleStats));
            }

            return new ContentPresentationData(
                data.Identity,
                string.Empty,
                classifications,
                groups,
                data.Provenance,
                data.Status);
        }

        private static void AddPlayerTag(
            ICollection<string> result,
            string tag)
        {
            if (PresentationDisplayCatalog.IsPlayerVisibleTag(tag))
            {
                result.Add(tag);
            }
        }

        private static PresentationValueUnit GetStatUnit(StatType stat)
        {
            switch (stat)
            {
                case StatType.MaxHpPercent:
                case StatType.HpRegenMaxHpPercent:
                case StatType.AttackPercent:
                case StatType.AttackSpeedPercent:
                case StatType.MoveSpeedPercent:
                case StatType.LifeStealPercent:
                case StatType.BossDamagePercent:
                case StatType.EliteDamagePercent:
                case StatType.EliteApproachMoveSpeedPercent:
                case StatType.MissingHpAttackPercent:
                case StatType.MissingHpFinalDamageAmplify:
                case StatType.FinalDamageAmplify:
                case StatType.SurroundedAttackPercent:
                case StatType.SurroundedDamageReductionPercent:
                case StatType.ReflectDamagePercent:
                case StatType.StatusResistancePercent:
                case StatType.ShieldPercent:
                case StatType.BonusGoldDropPercent:
                case StatType.RelicDropRatePercent:
                case StatType.GoldInterestPercent:
                case StatType.BattleEndGoldInterestPercent:
                case StatType.KillStackAttackPercent:
                case StatType.KillStackAttackPercentAmplify:
                case StatType.AiReactionSpeedPercent:
                case StatType.ConsumableEffectivenessPercent:
                case StatType.SkillRangePercent:
                    return PresentationValueUnit.Percent;

                case StatType.StunDuration:
                case StatType.RootDuration:
                    return PresentationValueUnit.Seconds;

                case StatType.MoveSpeed:
                    return PresentationValueUnit.MetersPerSecond;

                case StatType.CritChance:
                case StatType.CritDamage:
                    return PresentationValueUnit.Percent;

                case StatType.EliteApproachRadius:
                case StatType.SkillRange:
                    return PresentationValueUnit.Meters;

                case StatType.Level:
                case StatType.KillStack:
                case StatType.KillCount:
                case StatType.EliteKillCount:
                case StatType.BossKillCount:
                case StatType.ResurrectionToken:
                    return PresentationValueUnit.Count;

                default:
                    return PresentationValueUnit.Flat;
            }
        }

        private static PresentationProvenanceData Provenance(
            CharacterSO character,
            PresentationProvenanceKind kind,
            string field = null)
        {
            return new PresentationProvenanceData(
                kind,
                character != null ? character.CharacterId : string.Empty,
                sourceField: field);
        }
    }
}
