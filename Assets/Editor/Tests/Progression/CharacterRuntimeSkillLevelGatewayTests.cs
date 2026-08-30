using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Character;
using Character.ProgressionBridge;
using NUnit.Framework;
using Progression;
using Skill;
using UnityEditor;
using UnityEngine;

namespace ProjectBS.EditorTests.Progression
{
    public sealed class CharacterRuntimeSkillLevelGatewayTests
    {
        private const string OwnerId = "character.test.progression";
        private const string EquipmentId = "skill.character.test.progression.active_1";

        [Test]
        public void ContentAuthorityHasUniqueCanonicalTargets()
        {
            string[] characterGuids = AssetDatabase.FindAssets(
                "t:CharacterSO",
                new[] { "Assets/Contents/Character" });
            Assert.That(characterGuids, Has.Length.EqualTo(35));
            HashSet<string> characterIds = new(StringComparer.Ordinal);
            foreach (string guid in characterGuids)
            {
                CharacterSO character = AssetDatabase.LoadAssetAtPath<CharacterSO>(
                    AssetDatabase.GUIDToAssetPath(guid));
                Assert.That(character, Is.Not.Null);
                Assert.That(characterIds.Add(character.CharacterId), Is.True, character.CharacterId);
                HashSet<string> equipmentIds = new(StringComparer.Ordinal);
                foreach (CharacterSkillEntry entry in character.Skills)
                {
                    Assert.That(entry?.skillSo, Is.Not.Null, character.CharacterId);
                    Assert.That(entry.skillSo.EquipmentId, Is.Not.Empty);
                    Assert.That(equipmentIds.Add(entry.skillSo.EquipmentId), Is.True,
                        character.CharacterId + ":" + entry.skillSo.EquipmentId);
                }
            }

            AssertJsonIdsUnique("Assets/Contents/Character/json", "characterId", 32);
            AssertJsonIdsUnique("Assets/Contents/Skill/json", "equipmentId", 95);
        }

        [Test]
        public void ExactOneAppliesAndRollbackIsIdempotent()
        {
            CharacterRuntimeData runtime = Runtime(OwnerId, EquipmentId, 2);
            CharacterRuntimeSkillLevelGateway gateway = Gateway(runtime, 2, 10);
            ProgressionSkillMutationKey key = Key();

            SkillLevelMutationResult result = gateway.TryApplyExactOne(key, 2, out ProgressionSkillLevelMutation mutation);

            Assert.That(result, Is.EqualTo(SkillLevelMutationResult.Applied));
            Assert.That(runtime.GetSkillLevel(EquipmentId), Is.EqualTo(3));
            Assert.That(mutation.PreviousLevel, Is.EqualTo(2));
            Assert.That(mutation.AppliedLevel, Is.EqualTo(3));
            Assert.That(gateway.TryRollback(mutation), Is.True);
            Assert.That(gateway.TryRollback(mutation), Is.True);
            Assert.That(runtime.GetSkillLevel(EquipmentId), Is.EqualTo(2));
        }

        [Test]
        public void DuplicateOwnerFailsClosed()
        {
            CharacterRuntimeData first = Runtime(OwnerId, EquipmentId, 1);
            CharacterRuntimeData second = Runtime(OwnerId, EquipmentId, 1);
            CharacterRuntimeSkillLevelGateway gateway = Gateway(new[] { first, second }, 1, 10);

            Assert.That(gateway.TryApplyExactOne(Key(), 1, out _), Is.EqualTo(SkillLevelMutationResult.RejectedIdentity));
            Assert.That(gateway.LastFailure, Is.EqualTo(CharacterRuntimeProgressionFailure.InvalidRosterDuplicateOwner));
            Assert.That(first.GetSkillLevel(EquipmentId), Is.EqualTo(1));
            Assert.That(second.GetSkillLevel(EquipmentId), Is.EqualTo(1));
        }

        [Test]
        public void DuplicateTargetFailsClosed()
        {
            CharacterRuntimeData runtime = Runtime(OwnerId, EquipmentId, 1);
            runtime.skillInstances.Add(new EquipmentSkillInstanceData { equipmentId = EquipmentId, currentLevel = 1 });
            CharacterRuntimeSkillLevelGateway gateway = Gateway(runtime, 1, 10);

            Assert.That(gateway.TryApplyExactOne(Key(), 1, out _), Is.EqualTo(SkillLevelMutationResult.RejectedIdentity));
            Assert.That(gateway.LastFailure, Is.EqualTo(CharacterRuntimeProgressionFailure.InvalidRosterDuplicateTarget));
            Assert.That(runtime.skillInstances.Select(value => value.currentLevel), Is.All.EqualTo(1));
        }

        [TestCase(null, CharacterRuntimeProgressionFailure.EmptyEquipmentId)]
        [TestCase("", CharacterRuntimeProgressionFailure.EmptyEquipmentId)]
        public void EmptyEquipmentIdFailsWithoutMutation(string equipmentId, CharacterRuntimeProgressionFailure expected)
        {
            CharacterRuntimeData runtime = Runtime(OwnerId, EquipmentId, 1);
            CharacterRuntimeSkillLevelGateway gateway = Gateway(runtime, 1, 10);
            ProgressionSkillMutationKey key = new(OwnerId, equipmentId, equipmentId);

            Assert.That(gateway.TryApplyExactOne(key, 1, out _), Is.EqualTo(SkillLevelMutationResult.RejectedIdentity));
            Assert.That(gateway.LastFailure, Is.EqualTo(expected));
            Assert.That(runtime.GetSkillLevel(EquipmentId), Is.EqualTo(1));
        }

        [Test]
        public void MissingOwnerAndTargetAreDistinct()
        {
            CharacterRuntimeData runtime = Runtime(OwnerId, EquipmentId, 1);
            CharacterRuntimeSkillLevelGateway gateway = Gateway(runtime, 1, 10);

            Assert.That(gateway.TryApplyExactOne(new ProgressionSkillMutationKey("missing", EquipmentId, EquipmentId), 1, out _),
                Is.EqualTo(SkillLevelMutationResult.RejectedNotFound));
            Assert.That(gateway.LastFailure, Is.EqualTo(CharacterRuntimeProgressionFailure.MissingOwner));
            Assert.That(gateway.TryApplyExactOne(new ProgressionSkillMutationKey(OwnerId, "missing", "missing"), 1, out _),
                Is.EqualTo(SkillLevelMutationResult.RejectedNotFound));
            Assert.That(gateway.LastFailure, Is.EqualTo(CharacterRuntimeProgressionFailure.MissingTarget));
        }

        [Test]
        public void CanonicalMismatchFailsWithoutMutation()
        {
            CharacterRuntimeData runtime = Runtime(OwnerId, EquipmentId, 1);
            CharacterRuntimeSkillLevelGateway gateway = Gateway(runtime, 1, 10);

            Assert.That(gateway.TryApplyExactOne(
                    new ProgressionSkillMutationKey(OwnerId, EquipmentId, "different"), 1, out _),
                Is.EqualTo(SkillLevelMutationResult.RejectedIdentity));
            Assert.That(gateway.LastFailure, Is.EqualTo(CharacterRuntimeProgressionFailure.CanonicalSkillMismatch));
            Assert.That(runtime.GetSkillLevel(EquipmentId), Is.EqualTo(1));
        }

        [TestCase(false, true)]
        [TestCase(true, false)]
        public void InactiveOrInapplicableFailsWithoutMutation(bool active, bool applicable)
        {
            CharacterRuntimeData runtime = Runtime(OwnerId, EquipmentId, 1);
            CharacterRuntimeSkillLevelGateway gateway = Gateway(runtime, 1, 10, active, applicable);

            Assert.That(gateway.TryApplyExactOne(Key(), 1, out _), Is.EqualTo(SkillLevelMutationResult.RejectedIdentity));
            Assert.That(gateway.LastFailure, Is.EqualTo(CharacterRuntimeProgressionFailure.InactiveSkill));
            Assert.That(runtime.GetSkillLevel(EquipmentId), Is.EqualTo(1));
        }

        [Test]
        public void MaxAndStaleLevelsFailWithoutMutation()
        {
            CharacterRuntimeData runtime = Runtime(OwnerId, EquipmentId, 3);
            CharacterRuntimeSkillLevelGateway maxGateway = Gateway(runtime, 3, 3);
            Assert.That(maxGateway.TryApplyExactOne(Key(), 3, out _), Is.EqualTo(SkillLevelMutationResult.RejectedMaxLevel));
            Assert.That(maxGateway.LastFailure, Is.EqualTo(CharacterRuntimeProgressionFailure.MaxLevel));

            CharacterRuntimeSkillLevelGateway staleGateway = Gateway(runtime, 2, 10);
            Assert.That(staleGateway.TryApplyExactOne(Key(), 2, out _), Is.EqualTo(SkillLevelMutationResult.RejectedExpectedLevel));
            Assert.That(staleGateway.LastFailure, Is.EqualTo(CharacterRuntimeProgressionFailure.StaleLevel));
            Assert.That(runtime.GetSkillLevel(EquipmentId), Is.EqualTo(3));
        }

        [Test]
        public void DoubleApplyDoesNotIncrementTwice()
        {
            CharacterRuntimeData runtime = Runtime(OwnerId, EquipmentId, 1);
            CharacterRuntimeSkillLevelGateway gateway = Gateway(runtime, 1, 10);
            Assert.That(gateway.TryApplyExactOne(Key(), 1, out _), Is.EqualTo(SkillLevelMutationResult.Applied));
            Assert.That(gateway.TryApplyExactOne(Key(), 1, out _), Is.EqualTo(SkillLevelMutationResult.RejectedExpectedLevel));
            Assert.That(runtime.GetSkillLevel(EquipmentId), Is.EqualTo(2));
        }

        [Test]
        public void RestoreConflictDoesNotOverwriteInterveningChange()
        {
            CharacterRuntimeData runtime = Runtime(OwnerId, EquipmentId, 1);
            CharacterRuntimeSkillLevelGateway gateway = Gateway(runtime, 1, 10);
            gateway.TryApplyExactOne(Key(), 1, out ProgressionSkillLevelMutation mutation);
            runtime.skillInstances[0].currentLevel = 4;

            Assert.That(gateway.TryRollback(mutation), Is.False);
            Assert.That(gateway.LastFailure, Is.EqualTo(CharacterRuntimeProgressionFailure.RestoreConflict));
            Assert.That(runtime.GetSkillLevel(EquipmentId), Is.EqualTo(4));
        }

        [Test]
        public void C1PostMutationFaultUsesActualAdapterExactRestore()
        {
            CharacterRuntimeData runtime = Runtime(OwnerId, EquipmentId, 1);
            CharacterRuntimeSkillLevelGateway actual = Gateway(runtime, 1, 10);
            ThrowAfterApplyGateway fault = new(actual);
            RunProgressionLedger ledger = new(
                new ProgressionRunId("run.actual-adapter"),
                ProgressionCapPolicy.Chapter1P0,
                new ProgressionSourceRegistry());
            ledger.TryEarn(new ProgressionEarnRequest(
                ProgressionSourceRegistry.FixedRescueSegment,
                ProgressionSourceCategory.Fixed,
                ProgressionSourceType.BattleVictory,
                ProgressionSourceRegistry.RescueBattleSource,
                "result.actual-adapter"), out ProgressionOpportunitySnapshot earned);
            new FixedOfferService().GetOrCreate(
                ledger,
                earned.OpportunityId,
                earned.Revision,
                new[] { new ProgressionSkillCandidateSnapshot(OwnerId, EquipmentId, EquipmentId, 1, 10) },
                out ProgressionOpportunitySnapshot offered);
            ProgressionSkillCandidateSnapshot candidate = offered.Offer.Candidates[0];
            ProgressionConsumeService service = new(ledger, fault);

            ProgressionApplyResult result = service.TryApplyCandidate(new ProgressionApplyCandidateCommand(
                offered.OpportunityId,
                offered.Offer.Fingerprint,
                candidate.OwnerCharacterId,
                candidate.SkillInstanceId,
                candidate.CanonicalSkillId,
                candidate.CurrentLevel,
                offered.Revision));

            Assert.That(result.Code, Is.EqualTo(ProgressionApplyResultCode.GatewayFaulted));
            Assert.That(result.Code, Is.Not.EqualTo(ProgressionApplyResultCode.CompensationFaulted));
            Assert.That(result.Opportunity.State, Is.EqualTo(ProgressionOpportunityState.Pending));
            Assert.That(runtime.GetSkillLevel(EquipmentId), Is.EqualTo(1));
        }

        [Test]
        public void BattleReconstructionAuthorityUsesSameRuntimeObject()
        {
            CharacterRuntimeData runtime = Runtime(OwnerId, EquipmentId, 1);
            List<CharacterRuntimeData> sessionMembers = new() { runtime };
            CharacterRuntimeSkillLevelGateway gateway = Gateway(sessionMembers, 1, 10);

            gateway.TryApplyExactOne(Key(), 1, out _);

            CharacterRuntimeData nextBattleMember = sessionMembers[0];
            Assert.That(nextBattleMember, Is.SameAs(runtime));
            Assert.That(nextBattleMember.GetSkillLevel(EquipmentId), Is.EqualTo(2));
        }

        private static CharacterRuntimeSkillLevelGateway Gateway(
            CharacterRuntimeData runtime,
            int currentLevel,
            int maxLevel,
            bool active = true,
            bool applicable = true) =>
            Gateway(new[] { runtime }, currentLevel, maxLevel, active, applicable);

        private static CharacterRuntimeSkillLevelGateway Gateway(
            IReadOnlyList<CharacterRuntimeData> roster,
            int currentLevel,
            int maxLevel,
            bool active = true,
            bool applicable = true) =>
            new(
                roster,
                new[]
                {
                    new CharacterSkillEligibilityDescriptor(
                        OwnerId, EquipmentId, EquipmentId, currentLevel, maxLevel, active, applicable)
                });

        private static CharacterRuntimeData Runtime(string ownerId, string equipmentId, int level)
        {
            CharacterSO definition = ScriptableObject.CreateInstance<CharacterSO>();
            definition.ApplyEditorData(ownerId, default, default, null, null, null);
            return new CharacterRuntimeData
            {
                characterSO = definition,
                skillInstances = new List<EquipmentSkillInstanceData>
                {
                    new() { equipmentId = equipmentId, currentLevel = level, upgradeLevel = level - 1 }
                }
            };
        }

        private static ProgressionSkillMutationKey Key() => new(OwnerId, EquipmentId, EquipmentId);

        private static void AssertJsonIdsUnique(string directory, string property, int expectedCount)
        {
            string[] paths = Directory.GetFiles(directory, "*.json", SearchOption.TopDirectoryOnly);
            Assert.That(paths, Has.Length.EqualTo(expectedCount));
            Regex idPattern = new($"\\\"{Regex.Escape(property)}\\\"\\s*:\\s*\\\"([^\\\"]+)\\\"");
            HashSet<string> ids = new(StringComparer.Ordinal);
            foreach (string path in paths)
            {
                Match match = idPattern.Match(File.ReadAllText(path));
                Assert.That(match.Success, Is.True, path);
                Assert.That(ids.Add(match.Groups[1].Value), Is.True, path);
            }
        }

        private sealed class ThrowAfterApplyGateway : IProgressionSkillLevelGateway
        {
            private readonly CharacterRuntimeSkillLevelGateway inner;

            public ThrowAfterApplyGateway(CharacterRuntimeSkillLevelGateway inner) => this.inner = inner;

            public bool TryGetCurrentLevel(ProgressionSkillMutationKey key, out int currentLevel) =>
                inner.TryGetCurrentLevel(key, out currentLevel);

            public SkillLevelMutationResult TryApplyExactOne(
                ProgressionSkillMutationKey key,
                int expectedLevel,
                out ProgressionSkillLevelMutation mutation)
            {
                SkillLevelMutationResult result = inner.TryApplyExactOne(key, expectedLevel, out mutation);
                if (result == SkillLevelMutationResult.Applied)
                {
                    mutation = null;
                    throw new InvalidOperationException("Injected after actual mutation.");
                }

                return result;
            }

            public bool TryRollback(ProgressionSkillLevelMutation mutation) => inner.TryRollback(mutation);

            public bool TryRestoreExactLevel(
                ProgressionSkillMutationKey key,
                int expectedAppliedLevel,
                int restoreLevel) =>
                inner.TryRestoreExactLevel(key, expectedAppliedLevel, restoreLevel);
        }
    }
}
