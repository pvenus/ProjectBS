#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Battle;
using Battle.Prop;
using Battle.Prop.SO;
using UnityEngine;

namespace ResourceTools
{
    internal static class BattleJsonValidation
    {
        public static bool ValidateParsed(
            BattleJsonGenerator.BattleJson data,
            string sourceLabel)
        {
            List<string> errors = new();
            ValidateCommon(data, sourceLabel, errors);
            LogErrors(errors);
            return errors.Count == 0;
        }

        public static bool ValidateBuildData(
            BattleJsonGenerator.BattleJson data,
            string outputFolder,
            string sourceLabel,
            out BattleVictoryRule victoryRule,
            out SpawnSequenceSO spawnSequence)
        {
            victoryRule = BattleVictoryRule.ClearAllEnemies;
            spawnSequence = null;

            List<string> errors = new();
            ValidateCommon(data, sourceLabel, errors);

            if (!string.IsNullOrEmpty(outputFolder))
            {
                spawnSequence = BattleAssetBuilderUtility.FindSpawnSequenceSO(
                    data?.spawnSequenceId,
                    data?.spawnSequencePath,
                    outputFolder,
                    false);

                if (data != null && spawnSequence == null)
                {
                    errors.Add(
                        $"{sourceLabel}: SpawnSequenceSO를 찾을 수 없습니다. spawnSequenceId={data.spawnSequenceId}, spawnSequencePath={data.spawnSequencePath}");
                }
            }

            if (data != null &&
                TryParseEnum(data.victoryRule, out BattleVictoryRule parsedVictoryRule))
            {
                victoryRule = parsedVictoryRule;
            }

            LogErrors(errors);
            return errors.Count == 0;
        }

        public static bool TryParsePropRole(
            string value,
            out BattlePropRole role)
        {
            return TryParseEnum(value, out role);
        }

        public static bool TryParsePropState(
            string value,
            out BattlePropState state)
        {
            return TryParseEnum(value, out state);
        }

        private static void ValidateCommon(
            BattleJsonGenerator.BattleJson data,
            string sourceLabel,
            List<string> errors)
        {
            if (data == null)
            {
                errors.Add($"{sourceLabel}: Battle json 데이터가 비어 있습니다.");
                return;
            }

            RequireText(data.battleId, "battleId", sourceLabel, errors);

            if (!TryParseEnum(data.victoryRule, out BattleVictoryRule _))
            {
                errors.Add(
                    $"{sourceLabel}: victoryRule 값이 올바르지 않습니다. value={data.victoryRule}, allowed={FormatEnumNames<BattleVictoryRule>()}");
            }

            if (string.IsNullOrEmpty(data.spawnSequenceId) &&
                string.IsNullOrEmpty(data.spawnSequencePath))
            {
                errors.Add(
                    $"{sourceLabel}: spawnSequenceId 또는 spawnSequencePath 중 하나는 필수입니다.");
            }

            ValidateNonNegative(
                data.survivalTimeSeconds,
                "survivalTimeSeconds",
                sourceLabel,
                errors);
            ValidateNonNegative(
                data.rewardExperience,
                "rewardExperience",
                sourceLabel,
                errors);
            ValidateChance(
                data.normalRelicDropChance,
                "normalRelicDropChance",
                sourceLabel,
                errors);
            ValidateChance(
                data.bossRelicDropChance,
                "bossRelicDropChance",
                sourceLabel,
                errors);

            ValidatePropDefinitions(data.propDefinitions, sourceLabel, errors);
            ValidateTimedPropPlacements(data.timedPropPlacements, sourceLabel, errors);
        }

        private static void ValidatePropDefinitions(
            List<BattleJsonGenerator.PropDefinitionJson> propDefinitions,
            string sourceLabel,
            List<string> errors)
        {
            if (propDefinitions == null)
            {
                return;
            }

            HashSet<string> propIds = new();

            for (int i = 0; i < propDefinitions.Count; i++)
            {
                BattleJsonGenerator.PropDefinitionJson prop = propDefinitions[i];

                if (prop == null)
                {
                    errors.Add($"{sourceLabel}: propDefinitions[{i}] 항목이 비어 있습니다.");
                    continue;
                }

                if (string.IsNullOrEmpty(prop.propId))
                {
                    errors.Add($"{sourceLabel}: propDefinitions[{i}].propId는 필수입니다.");
                }
                else if (!propIds.Add(BattleAssetBuilderUtility.NormalizeKey(prop.propId)))
                {
                    errors.Add($"{sourceLabel}: propDefinitions[{i}].propId가 중복됩니다. propId={prop.propId}");
                }

                if (!string.IsNullOrEmpty(prop.role) &&
                    !TryParseEnum(prop.role, out BattlePropRole _))
                {
                    errors.Add(
                        $"{sourceLabel}: propDefinitions[{i}].role 값이 올바르지 않습니다. value={prop.role}, allowed={FormatEnumNames<BattlePropRole>()}");
                }

                ValidateStateVisuals(prop.stateVisuals, sourceLabel, i, errors);
                ValidateSpawnOnHit(prop.spawnOnHit, sourceLabel, i, errors);
                ValidateSpawnSequenceSpawner(prop.spawnSequenceSpawner, sourceLabel, i, errors);
            }
        }

        private static void ValidateStateVisuals(
            List<BattleJsonGenerator.PropStateVisualJson> stateVisuals,
            string sourceLabel,
            int propIndex,
            List<string> errors)
        {
            if (stateVisuals == null)
            {
                return;
            }

            for (int i = 0; i < stateVisuals.Count; i++)
            {
                BattleJsonGenerator.PropStateVisualJson visual = stateVisuals[i];

                if (visual == null)
                {
                    errors.Add($"{sourceLabel}: propDefinitions[{propIndex}].stateVisuals[{i}] 항목이 비어 있습니다.");
                    continue;
                }

                if (!string.IsNullOrEmpty(visual.state) &&
                    !TryParseEnum(visual.state, out BattlePropState _))
                {
                    errors.Add(
                        $"{sourceLabel}: propDefinitions[{propIndex}].stateVisuals[{i}].state 값이 올바르지 않습니다. value={visual.state}, allowed={FormatEnumNames<BattlePropState>()}");
                }
            }
        }

        private static void ValidateSpawnOnHit(
            BattleJsonGenerator.SpawnOnHitJson spawnOnHit,
            string sourceLabel,
            int propIndex,
            List<string> errors)
        {
            if (spawnOnHit == null)
            {
                return;
            }

            if (spawnOnHit.spawnHitThreshold <= 0)
            {
                errors.Add(
                    $"{sourceLabel}: propDefinitions[{propIndex}].spawnOnHit.spawnHitThreshold는 1 이상이어야 합니다.");
            }
        }

        private static void ValidateSpawnSequenceSpawner(
            BattleJsonGenerator.SpawnSequenceSpawnerJson spawner,
            string sourceLabel,
            int propIndex,
            List<string> errors)
        {
            if (spawner == null || !spawner.playOnInitialize)
            {
                return;
            }

            if (string.IsNullOrEmpty(spawner.spawnSequenceId) &&
                string.IsNullOrEmpty(spawner.spawnSequencePath))
            {
                errors.Add(
                    $"{sourceLabel}: propDefinitions[{propIndex}].spawnSequenceSpawner는 playOnInitialize=true일 때 spawnSequenceId 또는 spawnSequencePath가 필요합니다.");
            }
        }

        private static void ValidateTimedPropPlacements(
            List<BattleJsonGenerator.TimedPropPlacementJson> placements,
            string sourceLabel,
            List<string> errors)
        {
            if (placements == null)
            {
                return;
            }

            for (int i = 0; i < placements.Count; i++)
            {
                BattleJsonGenerator.TimedPropPlacementJson placement = placements[i];

                if (placement == null)
                {
                    errors.Add($"{sourceLabel}: timedPropPlacements[{i}] 항목이 비어 있습니다.");
                    continue;
                }

                RequireText(
                    placement.propId,
                    $"timedPropPlacements[{i}].propId",
                    sourceLabel,
                    errors);
                ValidateNonNegative(
                    placement.spawnTimeSeconds,
                    $"timedPropPlacements[{i}].spawnTimeSeconds",
                    sourceLabel,
                    errors);
            }
        }

        private static void RequireText(
            string value,
            string fieldName,
            string sourceLabel,
            List<string> errors)
        {
            if (string.IsNullOrEmpty(value))
            {
                errors.Add($"{sourceLabel}: {fieldName}는 필수입니다.");
            }
        }

        private static void ValidateNonNegative(
            float value,
            string fieldName,
            string sourceLabel,
            List<string> errors)
        {
            if (value < 0f)
            {
                errors.Add($"{sourceLabel}: {fieldName}는 0 이상이어야 합니다. value={value}");
            }
        }

        private static void ValidateChance(
            float value,
            string fieldName,
            string sourceLabel,
            List<string> errors)
        {
            if (value < 0f || value > 100f)
            {
                errors.Add($"{sourceLabel}: {fieldName}는 0~100 범위여야 합니다. value={value}");
            }
        }

        private static bool TryParseEnum<TEnum>(
            string value,
            out TEnum parsed)
            where TEnum : struct, Enum
        {
            if (string.IsNullOrEmpty(value) ||
                !Enum.TryParse(value, true, out parsed))
            {
                parsed = default;
                return false;
            }

            return Enum.IsDefined(typeof(TEnum), parsed);
        }

        private static string FormatEnumNames<TEnum>()
            where TEnum : struct, Enum
        {
            return string.Join(", ", Enum.GetNames(typeof(TEnum)));
        }

        private static void LogErrors(List<string> errors)
        {
            for (int i = 0; i < errors.Count; i++)
            {
                Debug.LogError($"[BattleJsonValidation] {errors[i]}");
            }
        }
    }
}
#endif
