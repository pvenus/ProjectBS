#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Character;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Character
{
    public static class CharacterAnimationProfileValidator
    {
        public enum Severity { Warn, Error, Blocker }

        [Serializable] public sealed class Issue
        {
            public string severity;
            public string code;
            public string subjectId;
            public string message;
        }

        public static IReadOnlyList<Issue> Validate(CharacterSO character)
        {
            return Validate(character, character != null ? character.AnimationProfile : null);
        }

        public static IReadOnlyList<Issue> Validate(CharacterSO character, CharacterAnimationProfileSO profile)
        {
            List<Issue> issues = new();
            if (character == null)
            {
                Add(issues, Severity.Blocker, "CHARACTER_NULL", "", "CharacterSO is null.");
                return issues;
            }

            if (profile == null)
            {
                Add(issues, Severity.Warn, "LEGACY_ADAPTER", character.CharacterId,
                    "No v2 profile; legacy directional clips remain active for this character.");
                ValidateLegacy(character, issues);
                ValidateLegacyActiveSkills(character, issues);
                if (character.CharacterType == CharacterType.Player)
                    Add(issues, Severity.Blocker, "LEGACY_CC_REQUIRED_MISSING", character.CharacterId,
                        "Player runtime can enter attack-disabled Stun but has no reviewed CC loop.");
                return issues;
            }

            if (!string.Equals(profile.CharacterId, character.CharacterId, StringComparison.Ordinal))
                Add(issues, Severity.Blocker, "IDENTITY_MISMATCH", character.CharacterId,
                    "Profile characterId does not match CharacterSO.");
            if (profile.SchemaVersion != CharacterAnimationProfileSO.CurrentSchemaVersion)
                Add(issues, Severity.Blocker, "SCHEMA_VERSION", character.CharacterId, "Unsupported schema version.");

            RequireState(profile, CharacterAnimationSlot.Idle, true, issues);
            RequireState(profile, CharacterAnimationSlot.Move, true, issues);
            RequireState(profile, CharacterAnimationSlot.BasicAttack, false, issues);
            RequireState(profile, CharacterAnimationSlot.Death, false, issues);
            if (profile.AttackDisabledCcReachable)
                RequireState(profile, CharacterAnimationSlot.AttackDisabledCc, true, issues);

            HashSet<string> expectedSkills = new(StringComparer.Ordinal);
            if (character.Skills != null)
            {
                foreach (CharacterSkillEntry slot in character.Skills)
                {
                    if (slot == null || slot.skillSo == null ||
                        string.IsNullOrWhiteSpace(slot.slotKey) || !slot.slotKey.StartsWith("active_", StringComparison.Ordinal))
                        continue;
                    expectedSkills.Add(slot.skillSo.EquipmentId);
                    bool castOwnsClip = slot.skillSo.CastSo != null && slot.skillSo.CastSo.BodyActionClip != null;
                    if (!profile.TryGetSkill(slot.skillSo.EquipmentId, out CharacterSkillAnimationEntry action))
                    {
                        if (!castOwnsClip) Add(issues, Severity.Blocker, "ACTIVE_SKILL_UNDECLARED",
                            slot.skillSo.EquipmentId, "Equipped active has neither a dedicated body clip nor approved fallback.");
                        continue;
                    }
                    if (action.Clip == null && string.IsNullOrWhiteSpace(action.FallbackReceipt))
                        Add(issues, Severity.Blocker, "FALLBACK_RECEIPT_MISSING", action.SkillId,
                            "Missing body clip has no approved fallback receipt.");
                }
            }

            foreach (CharacterSkillAnimationEntry action in profile.SkillActions ?? Array.Empty<CharacterSkillAnimationEntry>())
                if (action != null && !expectedSkills.Contains(action.SkillId))
                    Add(issues, Severity.Blocker, "NONEXISTENT_SKILL_BINDING", action.SkillId,
                        "Profile binds a skill that is not equipped by this exact character grade.");
            return issues;
        }

        private static void ValidateLegacy(CharacterSO character, List<Issue> issues)
        {
            CharacterAnimationClipType[][] groups =
            {
                new[] { CharacterAnimationClipType.IdleUpRight, CharacterAnimationClipType.IdleUpLeft,
                    CharacterAnimationClipType.IdleDownRight, CharacterAnimationClipType.IdleDownLeft },
                new[] { CharacterAnimationClipType.MoveUpRight, CharacterAnimationClipType.MoveUpLeft,
                    CharacterAnimationClipType.MoveDownRight, CharacterAnimationClipType.MoveDownLeft },
                new[] { CharacterAnimationClipType.AttackUpRight, CharacterAnimationClipType.AttackUpLeft,
                    CharacterAnimationClipType.AttackDownRight, CharacterAnimationClipType.AttackDownLeft },
                new[] { CharacterAnimationClipType.DeathUpRight, CharacterAnimationClipType.DeathUpLeft,
                    CharacterAnimationClipType.DeathDownRight, CharacterAnimationClipType.DeathDownLeft }
            };
            string[] names = { "Idle", "Move", "BasicAttack", "Death" };
            for (int i = 0; i < groups.Length; i++)
            {
                bool found = character.AnimationClips != null && character.AnimationClips.Any(entry =>
                    entry != null && entry.clip != null && groups[i].Contains(entry.clipType));
                if (!found) Add(issues, Severity.Blocker, "LEGACY_REQUIRED_MISSING", names[i],
                    $"Legacy character has no {names[i]} clip.");
            }
        }

        private static void ValidateLegacyActiveSkills(CharacterSO character, List<Issue> issues)
        {
            if (character.Skills == null) return;
            foreach (CharacterSkillEntry slot in character.Skills)
            {
                if (slot == null || slot.skillSo == null ||
                    string.IsNullOrWhiteSpace(slot.slotKey) ||
                    !slot.slotKey.StartsWith("active_", StringComparison.Ordinal)) continue;
                if (slot.skillSo.CastSo != null && slot.skillSo.CastSo.BodyActionClip != null) continue;
                Add(issues, Severity.Blocker, "LEGACY_ACTIVE_FALLBACK_UNREVIEWED",
                    slot.skillSo.EquipmentId,
                    "Active skill uses legacy generic attack behavior without a v2 fallback receipt.");
            }
        }

        private static void RequireState(CharacterAnimationProfileSO profile, CharacterAnimationSlot slot,
            bool mustLoop, List<Issue> issues)
        {
            if (!profile.TryGetState(slot, out CharacterAnimationStateEntry state) || state == null ||
                state.DirectionalClips == null || state.DirectionalClips.Count == 0)
            {
                Add(issues, Severity.Blocker, "REQUIRED_STATE_MISSING", slot.ToString(), "Required state is missing.");
                return;
            }
            if (state.Loop != mustLoop)
                Add(issues, Severity.Error, "LOOP_POLICY", state.AnimationId, "Loop policy does not match the required slot policy.");
            if (state.RootMotion)
                Add(issues, Severity.Blocker, "ROOT_MOTION_UNSUPPORTED", state.AnimationId, "Root motion is not supported.");
        }

        private static void Add(List<Issue> issues, Severity severity, string code, string subject, string message)
        {
            issues.Add(new Issue { severity = severity.ToString().ToUpperInvariant(), code = code,
                subjectId = subject, message = message });
        }
    }
}
#endif
