#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Character;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Character
{
    public static class CharacterAnimationProfileAssetBuilder
    {
        public const string GeneratorVersion = "character-animation-profile-builder/2.0.0";
        [Serializable] private sealed class ProfileJson
        {
            public int schemaVersion;
            public string generatorVersion;
            public string characterId;
            public string sourceDigest;
            public string changeLog;
            public string contentContractReceipt;
            public string productionGovernanceReceipt;
            public bool attackDisabledCcReachable;
            public StateJson[] states;
            public SkillJson[] skillActions;
        }

        [Serializable] private sealed class StateJson
        {
            public string slot;
            public string animationId;
            public bool loop;
            public bool mirrorLeftFromRight = true;
            public bool rootMotion;
            public ClipJson[] clips;
        }

        [Serializable] private sealed class ClipJson
        {
            public string direction;
            public string clipPath;
        }

        [Serializable] private sealed class SkillJson
        {
            public string skillId;
            public string animationId;
            public string clipPath;
            public float playbackSpeed = 1f;
            public bool mirrorWithFacing = true;
            public string fallback = "BasicAttack";
            public string fallbackReceipt;
            public string[] markerIntents;
            public string interruptPolicy = "CancelToLocomotion";
            public string restorePolicy = "PreviousLocomotion";
            public string gradeReuseSource;
            public string exceptionReceipt;
        }

        public static CharacterAnimationProfileSO Build(string jsonPath, string outputPath)
        {
            if (string.IsNullOrWhiteSpace(jsonPath) || string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Animation profile json/output paths are required.");
            ProfileJson dto = JsonUtility.FromJson<ProfileJson>(File.ReadAllText(jsonPath));
            Validate(dto, jsonPath);

            EnsureFolder(Path.GetDirectoryName(outputPath)?.Replace('\\', '/'));
            CharacterAnimationProfileSO asset = AssetDatabase.LoadAssetAtPath<CharacterAnimationProfileSO>(outputPath);
            if (asset == null)
            {
                if (File.Exists(outputPath))
                    throw new InvalidOperationException($"Refusing to replace an unreadable profile asset: {outputPath}");
                asset = ScriptableObject.CreateInstance<CharacterAnimationProfileSO>();
                AssetDatabase.CreateAsset(asset, outputPath);
            }

            CharacterAnimationStateEntry[] states = dto.states.Select(BuildState).ToArray();
            CharacterSkillAnimationEntry[] skills = (dto.skillActions ?? Array.Empty<SkillJson>())
                .Select(BuildSkill).ToArray();
            asset.ApplyEditorData(dto.characterId, GeneratorVersion, dto.sourceDigest,
                dto.contentContractReceipt, dto.productionGovernanceReceipt,
                dto.attackDisabledCcReachable, states, skills);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceSynchronousImport);
            CharacterAnimationProfileSO persisted =
                AssetDatabase.LoadAssetAtPath<CharacterAnimationProfileSO>(outputPath);
            if (persisted == null)
                throw new InvalidOperationException($"Profile did not reload after serialization: {outputPath}");
            return persisted;
        }

        private static CharacterAnimationStateEntry BuildState(StateJson dto)
        {
            CharacterAnimationSlot slot = ParseEnum<CharacterAnimationSlot>(dto.slot);
            CharacterAnimationClipEntry[] clips = dto.clips.Select(item => new CharacterAnimationClipEntry
            {
                clipType = ParseEnum<CharacterAnimationClipType>(item.direction),
                clip = LoadClip(item.clipPath)
            }).ToArray();
            CharacterAnimationStateEntry entry = new();
            entry.ApplyEditorData(dto.animationId, slot, clips, dto.loop, dto.mirrorLeftFromRight, false);
            return entry;
        }

        private static CharacterSkillAnimationEntry BuildSkill(SkillJson dto)
        {
            CharacterSkillAnimationEntry entry = new();
            entry.ApplyEditorData(dto.skillId, dto.animationId,
                string.IsNullOrWhiteSpace(dto.clipPath) ? null : LoadClip(dto.clipPath),
                dto.playbackSpeed,
                dto.mirrorWithFacing,
                ParseEnum<CharacterAnimationFallbackPolicy>(dto.fallback), dto.fallbackReceipt,
                dto.markerIntents,
                ParseEnum<CharacterAnimationInterruptPolicy>(dto.interruptPolicy),
                ParseEnum<CharacterAnimationRestorePolicy>(dto.restorePolicy),
                dto.gradeReuseSource, dto.exceptionReceipt);
            return entry;
        }

        private static AnimationClip LoadClip(string path)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null) throw new InvalidOperationException($"Missing AnimationClip: {path}");
            return clip;
        }

        private static void Validate(ProfileJson dto, string source)
        {
            if (dto == null || dto.schemaVersion != CharacterAnimationProfileSO.CurrentSchemaVersion ||
                !string.Equals(dto.generatorVersion, GeneratorVersion, StringComparison.Ordinal) ||
                string.IsNullOrWhiteSpace(dto.characterId) || string.IsNullOrWhiteSpace(dto.sourceDigest) ||
                string.IsNullOrWhiteSpace(dto.changeLog) || string.IsNullOrWhiteSpace(dto.contentContractReceipt) ||
                string.IsNullOrWhiteSpace(dto.productionGovernanceReceipt))
                throw new InvalidOperationException($"Invalid animation profile header: {source}");
            if (dto.states == null) throw new InvalidOperationException("states is required.");
            if (dto.states.Any(x => x == null))
                throw new InvalidOperationException("states cannot contain null entries.");
            if (dto.skillActions != null && dto.skillActions.Any(x => x == null))
                throw new InvalidOperationException("skillActions cannot contain null entries.");
            CharacterAnimationSlot[] required = { CharacterAnimationSlot.Idle, CharacterAnimationSlot.Move,
                CharacterAnimationSlot.BasicAttack, CharacterAnimationSlot.Death };
            foreach (CharacterAnimationSlot slot in required)
                if (dto.states.Count(x => string.Equals(x.slot, slot.ToString(), StringComparison.OrdinalIgnoreCase)) != 1)
                    throw new InvalidOperationException($"Required state must appear exactly once: {slot}");
            if (dto.attackDisabledCcReachable && dto.states.Count(x => string.Equals(
                    x.slot, CharacterAnimationSlot.AttackDisabledCc.ToString(), StringComparison.OrdinalIgnoreCase)) != 1)
                throw new InvalidOperationException("AttackDisabledCc is required when attack-disabled CC is reachable.");
            if (dto.states.Any(x => x == null || x.clips == null || x.clips.Length == 0 || x.rootMotion))
                throw new InvalidOperationException("Every state requires clips and rootMotion must remain false.");
            string[] animationIds = dto.states.Select(x => x.animationId)
                .Concat((dto.skillActions ?? Array.Empty<SkillJson>()).Select(x => x.animationId)).ToArray();
            if (animationIds.Any(string.IsNullOrWhiteSpace) ||
                animationIds.Distinct(StringComparer.Ordinal).Count() != animationIds.Length)
                throw new InvalidOperationException("Animation IDs must be non-empty and unique.");
            string[] ids = (dto.skillActions ?? Array.Empty<SkillJson>()).Select(x => x.skillId).ToArray();
            if (ids.Any(string.IsNullOrWhiteSpace) || ids.Distinct(StringComparer.Ordinal).Count() != ids.Length)
                throw new InvalidOperationException("Skill animation IDs must be non-empty and unique.");
            if (dto.skillActions != null && dto.skillActions.Any(x =>
                    string.IsNullOrWhiteSpace(x.clipPath) && string.IsNullOrWhiteSpace(x.fallbackReceipt)))
                throw new InvalidOperationException("Missing skill clips require an explicit reviewed fallback receipt.");
        }

        private static T ParseEnum<T>(string value) where T : struct
        {
            if (!Enum.TryParse(value, true, out T result))
                throw new InvalidOperationException($"Unsupported {typeof(T).Name}: {value}");
            return result;
        }

        private static void EnsureFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
#endif
