#if UNITY_EDITOR
using System;
using System.IO;
using Battle.Prop.SO;
using Character;
using UnityEditor;
using UnityEngine;

namespace ResourceTools
{
    internal static class BattleAssetBuilderUtility
    {
        public static void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) ||
                AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string[] parts = folderPath.Split('/');

            if (parts.Length == 0 || parts[0] != "Assets")
            {
                return;
            }

            string current = "Assets";

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

        public static SpawnSequenceSO FindSpawnSequenceSO(
            string spawnSequenceId,
            string spawnSequencePath,
            string outputFolder,
            bool logWarning = true)
        {
            if (!string.IsNullOrEmpty(spawnSequencePath))
            {
                string resolvedPath = NormalizeAssetPath(
                    Path.Combine(outputFolder, spawnSequencePath));

                SpawnSequenceSO sequence =
                    AssetDatabase.LoadAssetAtPath<SpawnSequenceSO>(resolvedPath);

                if (sequence != null)
                {
                    return sequence;
                }

                sequence =
                    AssetDatabase.LoadAssetAtPath<SpawnSequenceSO>(
                        NormalizeAssetPath(spawnSequencePath));

                if (sequence != null)
                {
                    return sequence;
                }
            }

            if (string.IsNullOrEmpty(spawnSequenceId))
            {
                return null;
            }

            string normalizedTarget = NormalizeKey(spawnSequenceId);
            string[] guids = AssetDatabase.FindAssets("t:SpawnSequenceSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SpawnSequenceSO sequence =
                    AssetDatabase.LoadAssetAtPath<SpawnSequenceSO>(path);

                if (sequence == null)
                {
                    continue;
                }

                string assetName = Path.GetFileNameWithoutExtension(path);

                if (NormalizeKey(assetName) == normalizedTarget ||
                    NormalizeKey(sequence.SequenceId) == normalizedTarget)
                {
                    return sequence;
                }
            }

            if (logWarning)
            {
                Debug.LogWarning($"[BattleAssetBuilderUtility] SpawnSequenceSO not found. spawnSequenceId={spawnSequenceId}");
            }

            return null;
        }

        public static BattlePropSO FindBattlePropSO(string propId)
        {
            if (string.IsNullOrEmpty(propId))
            {
                return null;
            }

            string normalizedTarget = NormalizeKey(propId);
            string[] guids = AssetDatabase.FindAssets("t:BattlePropSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                BattlePropSO prop = AssetDatabase.LoadAssetAtPath<BattlePropSO>(path);

                if (prop == null)
                {
                    continue;
                }

                string assetName = Path.GetFileNameWithoutExtension(path);

                if (NormalizeKey(assetName) == normalizedTarget ||
                    NormalizeKey(prop.propId) == normalizedTarget)
                {
                    return prop;
                }
            }

            return null;
        }

        public static AnimationClip FindAnimationClip(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            string normalizedTarget = NormalizeKey(key);
            string[] guids = AssetDatabase.FindAssets("t:AnimationClip");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

                if (clip == null)
                {
                    continue;
                }

                string assetName = Path.GetFileNameWithoutExtension(path);

                if (NormalizeKey(assetName) == normalizedTarget ||
                    NormalizeKey(clip.name) == normalizedTarget)
                {
                    return clip;
                }
            }

            Debug.LogWarning($"[BattleAssetBuilderUtility] AnimationClip not found. key={key}");
            return null;
        }

        public static GameObject FindPrefab(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            string normalizedTarget = NormalizeKey(key);
            string[] guids = AssetDatabase.FindAssets("t:Prefab");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null)
                {
                    continue;
                }

                string assetName = Path.GetFileNameWithoutExtension(path);

                if (NormalizeKey(assetName) == normalizedTarget ||
                    NormalizeKey(prefab.name) == normalizedTarget)
                {
                    return prefab;
                }
            }

            Debug.LogWarning($"[BattleAssetBuilderUtility] Prefab not found. key={key}");
            return null;
        }

        public static CharacterSO FindCharacterSO(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
            {
                return null;
            }

            string normalizedTarget = NormalizeKey(characterId);
            string[] guids = AssetDatabase.FindAssets("t:CharacterSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CharacterSO characterSO = AssetDatabase.LoadAssetAtPath<CharacterSO>(path);

                if (characterSO == null)
                {
                    continue;
                }

                string assetName = Path.GetFileNameWithoutExtension(path);

                if (NormalizeKey(assetName) == normalizedTarget ||
                    NormalizeKey(characterSO.CharacterId) == normalizedTarget)
                {
                    return characterSO;
                }
            }

            Debug.LogWarning($"[BattleAssetBuilderUtility] CharacterSO not found. characterId={characterId}");
            return null;
        }

        public static string ToSafeAssetName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "BattleAsset";
            }

            return value
                .Trim()
                .Replace(" ", "_")
                .Replace("/", "_")
                .Replace("\\", "_")
                .Replace(":", "_");
        }

        public static string NormalizeAssetPath(string path)
        {
            return string.IsNullOrEmpty(path)
                ? string.Empty
                : path.Replace("\\", "/");
        }

        public static string NormalizeKey(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Trim().Replace(" ", "_").Replace("-", "_").ToLowerInvariant();
        }
    }
}
#endif
