#if UNITY_EDITOR
using System.Collections.Generic;
using Battle;
using Battle.Prop.SO;
using Character;
using UnityEditor;
using UnityEngine;

namespace ResourceTools
{
    public static class BattleSOAssetBuilder
    {
        public static BattleSO CreateOrUpdate(
            BattleJsonGenerator.BattleJson data,
            string outputFolder,
            bool importAssetAfterSave = true)
        {
            if (data == null || string.IsNullOrEmpty(data.battleId))
            {
                Debug.LogError("[BattleSOAssetBuilder] Battle data is invalid.");
                return null;
            }

            if (string.IsNullOrEmpty(outputFolder))
            {
                Debug.LogError("[BattleSOAssetBuilder] Output folder is empty.");
                return null;
            }

            BattleAssetBuilderUtility.EnsureFolder(outputFolder);

            if (!BattleJsonValidation.ValidateBuildData(
                data,
                outputFolder,
                $"battleId={data.battleId}",
                out BattleVictoryRule parsedVictoryRule,
                out SpawnSequenceSO spawnSequence))
            {
                return null;
            }

            BattlePropSOAssetBuilder.CreateOrUpdateDefinitions(
                data.propDefinitions,
                outputFolder,
                outputFolder);

            string safeBattleAssetName =
                BattleAssetBuilderUtility.ToSafeAssetName(data.battleId);
            string assetPath = $"{outputFolder}/{safeBattleAssetName}.asset";
            BattleSO battleSO = AssetDatabase.LoadAssetAtPath<BattleSO>(assetPath);
            bool isNewAsset = false;

            if (battleSO == null)
            {
                battleSO = ScriptableObject.CreateInstance<BattleSO>();
                AssetDatabase.CreateAsset(battleSO, assetPath);
                isNewAsset = true;
            }

            ApplyData(battleSO, data, parsedVictoryRule, spawnSequence);

            EditorUtility.SetDirty(battleSO);
            AssetDatabase.SaveAssetIfDirty(battleSO);

            if (importAssetAfterSave)
            {
                AssetDatabase.ImportAsset(
                    assetPath,
                    ImportAssetOptions.ForceUpdate);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            if (isNewAsset)
            {
                Debug.Log($"[BattleSOAssetBuilder] Created BattleSO: {assetPath}");
            }
            else
            {
                Debug.Log($"[BattleSOAssetBuilder] Updated BattleSO: {assetPath}");
            }

            return battleSO;
        }

        private static void ApplyData(
            BattleSO battleSO,
            BattleJsonGenerator.BattleJson data,
            BattleVictoryRule victoryRule,
            SpawnSequenceSO spawnSequence)
        {
            battleSO.battleId = data.battleId;
            battleSO.battleName = data.battleName;
            battleSO.victoryRule = victoryRule;
            battleSO.survivalTimeSeconds = data.survivalTimeSeconds;
            battleSO.rewardExperience = data.rewardExperience;
            battleSO.normalRelicDropChance = data.normalRelicDropChance;
            battleSO.bossRelicDropChance = data.bossRelicDropChance;
            battleSO.backgroundSprite =
                BattleAssetBuilderUtility.FindSprite(
                    ResolveBackgroundSpriteKey(data));
            battleSO.spawnSequence = spawnSequence;
            battleSO.spawnUnitBindings =
                ConvertSpawnUnitBindings(data.spawnUnitBindings);
            battleSO.timedPropPlacements =
                ConvertTimedPropPlacements(data.timedPropPlacements);
        }

        private static string ResolveBackgroundSpriteKey(
            BattleJsonGenerator.BattleJson data)
        {
            if (!string.IsNullOrEmpty(data.backgroundSprite))
            {
                return data.backgroundSprite;
            }

            if (!string.IsNullOrEmpty(data.battleId))
            {
                return $"{data.battleId}.background";
            }

            return string.Empty;
        }

        private static SpawnUnitBinding[] ConvertSpawnUnitBindings(
            List<BattleJsonGenerator.SpawnUnitBindingJson> jsonBindings)
        {
            if (jsonBindings == null || jsonBindings.Count == 0)
            {
                return new SpawnUnitBinding[0];
            }

            List<SpawnUnitBinding> result = new();

            foreach (BattleJsonGenerator.SpawnUnitBindingJson jsonBinding in jsonBindings)
            {
                if (jsonBinding == null ||
                    string.IsNullOrEmpty(jsonBinding.characterId))
                {
                    continue;
                }

                CharacterSO characterSO =
                    BattleAssetBuilderUtility.FindCharacterSO(jsonBinding.characterId);

                if (characterSO == null)
                {
                    Debug.LogWarning($"[BattleSOAssetBuilder] CharacterSO not found. characterId={jsonBinding.characterId}");
                    continue;
                }

                SpawnUnitBinding binding = new();

                EditorFieldSetter.SetFirstExistingField(
                    binding,
                    jsonBinding.unitKey,
                    "unitKey");

                if (BattleJsonValidation.TryParseSpawnUnitRole(
                        jsonBinding.role,
                        out SpawnUnitRole role))
                {
                    EditorFieldSetter.SetFirstExistingField(
                        binding,
                        role,
                        "role");
                }

                EditorFieldSetter.SetFirstExistingField(
                    binding,
                    characterSO,
                    "character");

                result.Add(binding);
            }

            return result.ToArray();
        }

        private static List<BattleSO.TimedPropPlacement> ConvertTimedPropPlacements(
            List<BattleJsonGenerator.TimedPropPlacementJson> jsonPlacements)
        {
            List<BattleSO.TimedPropPlacement> result = new();

            if (jsonPlacements == null)
            {
                return result;
            }

            foreach (BattleJsonGenerator.TimedPropPlacementJson jsonPlacement in jsonPlacements)
            {
                if (jsonPlacement == null || string.IsNullOrEmpty(jsonPlacement.propId))
                {
                    continue;
                }

                BattlePropSO prop =
                    BattleAssetBuilderUtility.FindBattlePropSO(jsonPlacement.propId);

                if (prop == null)
                {
                    Debug.LogWarning($"[BattleSOAssetBuilder] BattlePropSO not found. propId={jsonPlacement.propId}");
                    continue;
                }

                result.Add(new BattleSO.TimedPropPlacement
                {
                    spawnTimeSeconds = jsonPlacement.spawnTimeSeconds,
                    prop = prop,
                    position = jsonPlacement.position != null
                        ? jsonPlacement.position.ToVector3()
                        : Vector3.zero,
                    rotation = Quaternion.Euler(
                        0f,
                        0f,
                        jsonPlacement.rotationZ),
                    runtimeId = jsonPlacement.runtimeId
                });
            }

            return result;
        }

    }
}
#endif
