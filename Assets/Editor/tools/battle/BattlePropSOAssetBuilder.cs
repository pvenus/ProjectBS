#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Battle.Prop;
using Battle.Prop.SO;
using UnityEditor;
using UnityEngine;

namespace ResourceTools
{
    public static class BattlePropSOAssetBuilder
    {
        public static void CreateOrUpdateDefinitions(
            List<BattleJsonGenerator.PropDefinitionJson> propDefinitions,
            string outputFolder,
            string spawnSequenceBaseFolder)
        {
            if (propDefinitions == null || propDefinitions.Count == 0)
            {
                return;
            }

            if (string.IsNullOrEmpty(outputFolder))
            {
                Debug.LogError("[BattlePropSOAssetBuilder] Cannot resolve prop output folder.");
                return;
            }

            Dictionary<string, BattlePropSO> createdProps = new();
            Dictionary<string, string> assetPaths = new();
            HashSet<string> newAssetKeys = new();

            for (int i = 0; i < propDefinitions.Count; i++)
            {
                BattleJsonGenerator.PropDefinitionJson propData = propDefinitions[i];

                if (propData == null || string.IsNullOrEmpty(propData.propId))
                {
                    continue;
                }

                string normalizedKey = BattleAssetBuilderUtility.NormalizeKey(propData.propId);
                string safeAssetName = BattleAssetBuilderUtility.ToSafeAssetName(propData.propId);
                string assetPath = $"{outputFolder}/{safeAssetName}.asset";
                BattlePropSO propSO = AssetDatabase.LoadAssetAtPath<BattlePropSO>(assetPath);

                if (propSO == null)
                {
                    propSO = ScriptableObject.CreateInstance<BattlePropSO>();
                    AssetDatabase.CreateAsset(propSO, assetPath);
                    newAssetKeys.Add(normalizedKey);
                }

                createdProps[normalizedKey] = propSO;
                assetPaths[normalizedKey] = assetPath;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            for (int i = 0; i < propDefinitions.Count; i++)
            {
                BattleJsonGenerator.PropDefinitionJson propData = propDefinitions[i];

                if (propData == null || string.IsNullOrEmpty(propData.propId))
                {
                    continue;
                }

                string normalizedKey = BattleAssetBuilderUtility.NormalizeKey(propData.propId);

                if (!createdProps.TryGetValue(normalizedKey, out BattlePropSO propSO))
                {
                    continue;
                }

                Apply(propSO, propData, spawnSequenceBaseFolder, createdProps);

                EditorUtility.SetDirty(propSO);
                AssetDatabase.SaveAssetIfDirty(propSO);

                if (assetPaths.TryGetValue(normalizedKey, out string assetPath))
                {
                    AssetDatabase.ImportAsset(
                        assetPath,
                        ImportAssetOptions.ForceUpdate);
                }

                if (newAssetKeys.Contains(normalizedKey))
                {
                    Debug.Log($"[BattlePropSOAssetBuilder] Created BattlePropSO: {assetPaths[normalizedKey]}");
                }
                else
                {
                    Debug.Log($"[BattlePropSOAssetBuilder] Updated BattlePropSO: {assetPaths[normalizedKey]}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private static void Apply(
            BattlePropSO propSO,
            BattleJsonGenerator.PropDefinitionJson propData,
            string spawnSequenceBaseFolder,
            Dictionary<string, BattlePropSO> propLookup)
        {
            if (propSO == null || propData == null)
            {
                return;
            }

            propSO.propId = propData.propId;

            propSO.role = BattlePropRole.None;

            if (!string.IsNullOrEmpty(propData.role))
            {
                if (!BattleJsonValidation.TryParsePropRole(propData.role, out BattlePropRole parsedRole))
                {
                    Debug.LogError(
                        $"[BattlePropSOAssetBuilder] Invalid BattlePropRole. propId={propData.propId}, role={propData.role}");
                    return;
                }

                propSO.role = parsedRole;
            }

            propSO.prefab = BattleAssetBuilderUtility.FindPrefab(propData.prefab);
            propSO.skills = new List<ScriptableObject>();
            propSO.stateVisuals = ConvertPropStateVisuals(propData.stateVisuals);

            if (propData.spawnOnHit != null)
            {
                propSO.spawnHitThreshold = propData.spawnOnHit.spawnHitThreshold;
                propSO.spawnPropOnHit = ResolveBattlePropSO(
                    propData.spawnOnHit.spawnPropOnHit,
                    propLookup);
                propSO.destroyAfterSpawnOnHit = propData.spawnOnHit.destroyAfterSpawnOnHit;
            }

            if (propData.spawnSequenceSpawner != null)
            {
                propSO.spawnSequence =
                    BattleAssetBuilderUtility.FindSpawnSequenceSO(
                        propData.spawnSequenceSpawner.spawnSequenceId,
                        propData.spawnSequenceSpawner.spawnSequencePath,
                        spawnSequenceBaseFolder);

                if (propData.spawnSequenceSpawner.playOnInitialize &&
                    propSO.spawnSequence == null)
                {
                    Debug.LogError(
                        $"[BattlePropSOAssetBuilder] SpawnSequenceSO not found for prop spawner. propId={propData.propId}, spawnSequenceId={propData.spawnSequenceSpawner.spawnSequenceId}, spawnSequencePath={propData.spawnSequenceSpawner.spawnSequencePath}");
                    return;
                }

                propSO.playSpawnSequenceOnInitialize =
                    propData.spawnSequenceSpawner.playOnInitialize;
            }
            else
            {
                propSO.spawnSequence = null;
                propSO.playSpawnSequenceOnInitialize = false;
            }
        }

        private static List<BattlePropSO.PropStateVisualEntry> ConvertPropStateVisuals(
            List<BattleJsonGenerator.PropStateVisualJson> visuals)
        {
            List<BattlePropSO.PropStateVisualEntry> result = new();

            if (visuals == null)
            {
                return result;
            }

            for (int i = 0; i < visuals.Count; i++)
            {
                BattleJsonGenerator.PropStateVisualJson visual = visuals[i];

                if (visual == null)
                {
                    continue;
                }

                BattlePropState state = BattlePropState.Normal;

                if (!string.IsNullOrEmpty(visual.state))
                {
                    if (!BattleJsonValidation.TryParsePropState(visual.state, out state))
                    {
                        Debug.LogError(
                            $"[BattlePropSOAssetBuilder] Invalid BattlePropState. state={visual.state}");
                        continue;
                    }
                }

                result.Add(new BattlePropSO.PropStateVisualEntry
                {
                    state = state,
                    animationClip = BattleAssetBuilderUtility.FindAnimationClip(visual.animationClip),
                    effectPrefab = BattleAssetBuilderUtility.FindPrefab(visual.effectPrefab)
                });
            }

            return result;
        }

        private static BattlePropSO ResolveBattlePropSO(
            string propId,
            Dictionary<string, BattlePropSO> propLookup)
        {
            if (string.IsNullOrEmpty(propId))
            {
                return null;
            }

            string normalizedKey = BattleAssetBuilderUtility.NormalizeKey(propId);

            if (propLookup != null &&
                propLookup.TryGetValue(normalizedKey, out BattlePropSO localProp))
            {
                return localProp;
            }

            return BattleAssetBuilderUtility.FindBattlePropSO(propId);
        }
    }
}
#endif
