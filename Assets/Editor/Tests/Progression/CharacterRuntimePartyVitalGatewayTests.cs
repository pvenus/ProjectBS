using System;
using System.Collections.Generic;
using System.IO;
using Character;
using Character.ProgressionBridge;
using NUnit.Framework;
using Party;
using Progression;
using Stat;
using UnityEngine;

namespace ProjectBS.EditorTests.Progression
{
    public sealed class CharacterRuntimePartyVitalGatewayTests
    {
        [Test]
        public void CaptureReadsExactSessionAuthority()
        {
            PartyRuntimeData party = Party(Runtime("b", 51, 101), Runtime("a", 100, 100));
            CharacterRuntimePartyVitalGateway gateway = new(party);
            Assert.That(gateway.TryCapture(out IReadOnlyList<PartyVitalSnapshot> roster), Is.True);
            Assert.That(roster[0].MemberId, Is.EqualTo("a"));
            Assert.That(roster[0].CurrentHp, Is.EqualTo(100));
            Assert.That(roster[1].MaxHp, Is.EqualTo(101));
        }

        [Test]
        public void FractionalApplyAndRestorePreserveExactBits()
        {
            PartyRuntimeData party = Party(Runtime("a", 499.75f, 500f));
            CharacterRuntimePartyVitalGateway gateway = new(party);
            Assert.That(gateway.TryCapture(out IReadOnlyList<PartyVitalSnapshot> roster), Is.True);
            PartyVitalCostPlan plan = PartyVitalCostPolicy.Evaluate(roster);
            Assert.That(gateway.TryApply("tx.fractional", plan, out PartyVitalMutationReceipt receipt), Is.True);
            AssertHp(party.Members[0], 449.75f);
            Assert.That(gateway.TryRestore(receipt), Is.True);
            AssertHp(party.Members[0], 499.75f);
        }

        [Test]
        public void OneBitCurrentOrMaxChangeIsStaleAndMutatesNothing()
        {
            PartyRuntimeData party = Party(Runtime("a", 100f, 100f));
            CharacterRuntimePartyVitalGateway gateway = new(party);
            gateway.TryCapture(out IReadOnlyList<PartyVitalSnapshot> roster);
            PartyVitalCostPlan plan = PartyVitalCostPolicy.Evaluate(roster);
            float adjacent = BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(100f) + 1);
            party.Members[0].stats[0].value = adjacent;
            party.Members[0].finalStats[0].value = adjacent;
            Assert.That(gateway.TryApply("tx.bit-current", plan, out _), Is.False);
            AssertHp(party.Members[0], adjacent);

            party.Members[0].stats[0].value = 100f;
            party.Members[0].finalStats[0].value = 100f;
            party.Members[0].finalStats[1].value = adjacent;
            Assert.That(gateway.TryApply("tx.bit-max", plan, out _), Is.False);
            party.Members[0].finalStats[1].value = 100f;
            AssertHp(party.Members[0], 100f);
        }

        [Test]
        public void InvalidFloatStatesFailClosedAndCurrentAboveMaxIsPreserved()
        {
            float negativeZero = BitConverter.Int32BitsToSingle(unchecked((int)0x80000000));
            float subnormal = BitConverter.Int32BitsToSingle(1);
            foreach (float invalid in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity, 0f, negativeZero })
                Assert.That(new CharacterRuntimePartyVitalGateway(Party(Runtime("a", invalid, 100f))).TryCapture(out _), Is.False);
            Assert.That(new CharacterRuntimePartyVitalGateway(Party(Runtime("a", 10f, subnormal))).TryCapture(out _), Is.False);
            CharacterRuntimePartyVitalGateway subnormalCurrent = new(Party(Runtime("a", subnormal, 100f)));
            Assert.That(subnormalCurrent.TryCapture(out IReadOnlyList<PartyVitalSnapshot> subnormalRoster), Is.True);
            Assert.That(PartyVitalCostPolicy.Evaluate(subnormalRoster).IsEligible, Is.False);

            CharacterRuntimeData aboveMaxRuntime = Runtime("b", 120.5f, 100f);
            CharacterRuntimePartyVitalGateway aboveMax = new(Party(aboveMaxRuntime));
            Assert.That(aboveMax.TryCapture(out IReadOnlyList<PartyVitalSnapshot> aboveRoster), Is.True);
            Assert.That(aboveMax.TryApply("tx.above", PartyVitalCostPolicy.Evaluate(aboveRoster), out _), Is.True);
            AssertHp(aboveMaxRuntime, 110.5f);
        }

        [Test]
        public void EmptyNullAndDuplicateOwnerFailClosed()
        {
            Assert.That(new CharacterRuntimePartyVitalGateway(Party()).TryCapture(out _), Is.False);
            PartyRuntimeData withNull = Party(Runtime("a", 100, 100)); withNull.Members.Add(null);
            Assert.That(new CharacterRuntimePartyVitalGateway(withNull).TryCapture(out _), Is.False);
            PartyRuntimeData duplicate = Party(Runtime("a", 100, 100), Runtime("a", 100, 100));
            CharacterRuntimePartyVitalGateway gateway = new(duplicate);
            Assert.That(gateway.TryCapture(out _), Is.False);
            Assert.That(gateway.LastFailure, Is.EqualTo(CharacterRuntimeVitalFailure.DuplicateOwner));
        }

        [Test]
        public void MissingDuplicateAndInvalidVitalsFailClosed()
        {
            CharacterRuntimeData missing = Runtime("a", 100, 100); missing.finalStats.Clear();
            Assert.That(new CharacterRuntimePartyVitalGateway(Party(missing)).TryCapture(out _), Is.False);
            CharacterRuntimeData duplicate = Runtime("a", 100, 100);
            duplicate.stats.Add(new StatEntry { statType = StatType.Hp, value = 100 });
            Assert.That(new CharacterRuntimePartyVitalGateway(Party(duplicate)).TryCapture(out _), Is.False);
            CharacterRuntimeData invalid = Runtime("a", float.NaN, 100);
            Assert.That(new CharacterRuntimePartyVitalGateway(Party(invalid)).TryCapture(out _), Is.False);
        }

        [Test]
        public void ExactFullPartyApplyAndRestoreAreCasAndIdempotent()
        {
            PartyRuntimeData party = Party(Runtime("a", 100, 100), Runtime("b", 51, 101));
            CharacterRuntimePartyVitalGateway gateway = new(party);
            gateway.TryCapture(out IReadOnlyList<PartyVitalSnapshot> roster);
            PartyVitalCostPlan plan = PartyVitalCostPolicy.Evaluate(roster);
            Assert.That(gateway.TryApply("tx.1", plan, out PartyVitalMutationReceipt receipt), Is.True);
            AssertHp(party.Members[0], 90); AssertHp(party.Members[1], 40);
            Assert.That(gateway.TryApply("tx.1", plan, out PartyVitalMutationReceipt duplicate), Is.True);
            Assert.That(duplicate, Is.SameAs(receipt));
            Assert.That(gateway.TryRestore(receipt), Is.True);
            Assert.That(gateway.TryRestore(receipt), Is.True);
            AssertHp(party.Members[0], 100); AssertHp(party.Members[1], 51);
            Assert.That(gateway.TryApply("tx.1", plan, out PartyVitalMutationReceipt retried), Is.True);
            Assert.That(retried, Is.Not.SameAs(receipt));
            AssertHp(party.Members[0], 90); AssertHp(party.Members[1], 40);
        }

        [Test]
        public void StalePlanMutatesNoMember()
        {
            PartyRuntimeData party = Party(Runtime("a", 100, 100), Runtime("b", 100, 100));
            CharacterRuntimePartyVitalGateway gateway = new(party);
            gateway.TryCapture(out IReadOnlyList<PartyVitalSnapshot> roster);
            PartyVitalCostPlan plan = PartyVitalCostPolicy.Evaluate(roster);
            Assert.That(party.Members[1].TryCompareExchangeCurrentHp(100, 99), Is.EqualTo(CharacterVitalCasResult.Applied));
            Assert.That(gateway.TryApply("tx.stale", plan, out _), Is.False);
            AssertHp(party.Members[0], 100); AssertHp(party.Members[1], 99);
        }

        [Test]
        public void RestoreConflictDoesNotOverwriteInterveningChange()
        {
            PartyRuntimeData party = Party(Runtime("a", 100, 100));
            CharacterRuntimePartyVitalGateway gateway = new(party);
            gateway.TryCapture(out IReadOnlyList<PartyVitalSnapshot> roster);
            Assert.That(gateway.TryApply("tx.conflict", PartyVitalCostPolicy.Evaluate(roster), out PartyVitalMutationReceipt receipt), Is.True);
            Assert.That(party.Members[0].TryCompareExchangeCurrentHp(90, 89), Is.EqualTo(CharacterVitalCasResult.Applied));
            Assert.That(gateway.TryRestore(receipt), Is.False);
            Assert.That(gateway.LastFailure, Is.EqualTo(CharacterRuntimeVitalFailure.RestoreConflict));
            AssertHp(party.Members[0], 89);
        }

        [Test]
        public void OneBitNearEqualRestoreConflictDoesNotOverwrite()
        {
            PartyRuntimeData party = Party(Runtime("a", 100f, 100f));
            CharacterRuntimePartyVitalGateway gateway = new(party);
            gateway.TryCapture(out IReadOnlyList<PartyVitalSnapshot> roster);
            Assert.That(gateway.TryApply("tx.near", PartyVitalCostPolicy.Evaluate(roster), out PartyVitalMutationReceipt receipt), Is.True);
            float adjacentAfter = BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(90f) + 1);
            party.Members[0].stats[0].value = adjacentAfter;
            party.Members[0].finalStats[0].value = adjacentAfter;
            Assert.That(gateway.TryRestore(receipt), Is.False);
            AssertHp(party.Members[0], adjacentAfter);
        }

        [Test]
        public void F1ServiceCommitsActualRuntimeHpAndPendingOnce()
        {
            PartyRuntimeData party = Party(Runtime("a", 100, 100), Runtime("b", 51, 101));
            CharacterRuntimePartyVitalGateway gateway = new(party);
            gateway.TryCapture(out IReadOnlyList<PartyVitalSnapshot> roster);
            RunProgressionLedger ledger = Ledger();
            RandomGrowthEventTransactionService service = new(ledger, new StageEventResultLedger(), gateway);
            RandomGrowthEventCommand command = Risk(roster);
            Assert.That(service.Execute(command).Result, Is.EqualTo(RandomGrowthEventTransactionResult.Succeeded));
            Assert.That(service.Execute(command).Result, Is.EqualTo(RandomGrowthEventTransactionResult.AlreadyResolved));
            AssertHp(party.Members[0], 90); AssertHp(party.Members[1], 40);
            Assert.That(ledger.Count, Is.EqualTo(1));
        }

        [Test]
        public void F1FailureAfterActualApplyRestoresEveryRuntimeMember()
        {
            PartyRuntimeData party = Party(Runtime("a", 100, 100), Runtime("b", 100, 100));
            CharacterRuntimePartyVitalGateway actual = new(party);
            actual.TryCapture(out IReadOnlyList<PartyVitalSnapshot> roster);
            IPartyVitalMutationGateway failAfterApply = new FailAfterApplyGateway(actual);
            RunProgressionLedger ledger = Ledger();
            RandomGrowthEventTransactionReceipt result = new RandomGrowthEventTransactionService(
                ledger, new StageEventResultLedger(), failAfterApply).Execute(Risk(roster));
            Assert.That(result.Result, Is.EqualTo(RandomGrowthEventTransactionResult.VitalMutationFailed));
            AssertHp(party.Members[0], 100); AssertHp(party.Members[1], 100);
            Assert.That(ledger.Count, Is.Zero);
        }

        [Test]
        public void MemberKFailureReturnsAppliedActualTokensForReverseRestore()
        {
            CharacterRuntimeData first = Runtime("a", 100, 100);
            CharacterRuntimeData second = Runtime("b", 100, 100);
            PartialActualGateway partial = new(first, second) { FailBeforeIndex = 1 };
            partial.TryCapture(out IReadOnlyList<PartyVitalSnapshot> roster);
            RandomGrowthEventTransactionReceipt result = new RandomGrowthEventTransactionService(
                Ledger(), new StageEventResultLedger(), partial).Execute(Risk(roster));
            Assert.That(result.Result, Is.EqualTo(RandomGrowthEventTransactionResult.VitalMutationFailed));
            AssertHp(first, 100); AssertHp(second, 100);
            Assert.That(partial.RestoreOrder, Is.EqualTo(new[] { "a" }));
        }

        [Test]
        public void OneUnableMemberPreventsActualGatewayCallAndAllMutation()
        {
            PartyRuntimeData party = Party(Runtime("a", 100, 100), Runtime("b", 10, 100));
            CharacterRuntimePartyVitalGateway actual = new(party);
            actual.TryCapture(out IReadOnlyList<PartyVitalSnapshot> roster);
            CountingGateway counting = new(actual);
            RandomGrowthEventTransactionReceipt result = new RandomGrowthEventTransactionService(
                Ledger(), new StageEventResultLedger(), counting).Execute(Risk(roster));
            Assert.That(result.Result, Is.EqualTo(RandomGrowthEventTransactionResult.Ineligible));
            Assert.That(counting.ApplyCalls, Is.Zero);
            AssertHp(party.Members[0], 100); AssertHp(party.Members[1], 10);
        }

        [Test]
        public void SameCharacterRuntimeReferenceCarriesHpIntoNextBattleReconstruction()
        {
            CharacterRuntimeData runtime = Runtime("a", 100, 100);
            PartyRuntimeData party = Party(runtime);
            CharacterRuntimePartyVitalGateway gateway = new(party);
            gateway.TryCapture(out IReadOnlyList<PartyVitalSnapshot> roster);
            Assert.That(gateway.TryApply("tx.next", PartyVitalCostPolicy.Evaluate(roster), out _), Is.True);
            CharacterRuntimeData nextBattleInput = party.Members[0];
            Assert.That(nextBattleInput, Is.SameAs(runtime));
            Assert.That(nextBattleInput.TryReadExactVitalStats(out float current, out float max), Is.EqualTo(CharacterVitalCasResult.Applied));
            Assert.That(current, Is.EqualTo(90)); Assert.That(max, Is.EqualTo(100));
        }

        [Test]
        public void BattleReconstructionConsumesSameRuntimeAndDoesNotRefillPositiveHp()
        {
            string partyManager = File.ReadAllText("Assets/Scripts/Actor/Party/PartyManager.cs");
            string characterManager = File.ReadAllText("Assets/Scripts/Actor/Character/CharacterManager.cs");
            StringAssert.Contains("characterManager.Initialize(characterRuntime);", partyManager);
            StringAssert.Contains("if (GetStatValue(StatType.Hp) <= 0f", characterManager);
            StringAssert.Contains("&& !runtimeData.isDead", characterManager);
        }

        [Test]
        public void ReceiptIsDetachedFromPartyMutationLists()
        {
            PartyRuntimeData party = Party(Runtime("a", 100, 100));
            CharacterRuntimePartyVitalGateway gateway = new(party);
            gateway.TryCapture(out IReadOnlyList<PartyVitalSnapshot> roster);
            gateway.TryApply("tx.immutable", PartyVitalCostPolicy.Evaluate(roster), out PartyVitalMutationReceipt receipt);
            Assert.That(receipt.Applied, Is.Not.InstanceOf<List<PartyVitalMutation>>());
            Assert.That(receipt.Applied[0].Before, Is.EqualTo(100));
        }

        private static PartyRuntimeData Party(params CharacterRuntimeData[] members)
        {
            PartyRuntimeData party = new(); foreach (CharacterRuntimeData member in members) party.Members.Add(member); return party;
        }

        private static CharacterRuntimeData Runtime(string id, float current, float max)
        {
            CharacterSO definition = ScriptableObject.CreateInstance<CharacterSO>();
            definition.ApplyEditorData(id, default, default, null, null, null);
            return new CharacterRuntimeData
            {
                characterSO = definition,
                stats = new List<StatEntry>
                {
                    new() { statType = StatType.Hp, value = current },
                    new() { statType = StatType.MaxHp, value = max }
                },
                finalStats = new List<StatEntry>
                {
                    new() { statType = StatType.Hp, value = current },
                    new() { statType = StatType.MaxHp, value = max }
                }
            };
        }

        private static void AssertHp(CharacterRuntimeData runtime, float expected)
        {
            Assert.That(runtime.TryReadExactVitalStats(out float current, out _), Is.EqualTo(CharacterVitalCasResult.Applied));
            Assert.That(CanonicalFloatBits.AreEqual(current, expected), Is.True);
        }

        private static RunProgressionLedger Ledger() => new(
            new ProgressionRunId("run.f2"), ProgressionCapPolicy.Chapter1P0, new ProgressionSourceRegistry());

        private static RandomGrowthEventCommand Risk(IReadOnlyList<PartyVitalSnapshot> roster) => new(
            new StageEventCause("run.f2", "stage.f2", "slot.f2", RandomGrowthEventIds.Event,
                RandomGrowthEventIds.RiskChoice, "result.f2"),
            StageEventChoiceKind.Risk,
            roster,
            new ProgressionEarnRequest(ProgressionSourceRegistry.RandomGrowthSegment,
                ProgressionSourceCategory.Random, ProgressionSourceType.RandomEventRisk,
                ProgressionSourceRegistry.RandomGrowthRiskSource, "result.f2"));

        private sealed class FailAfterApplyGateway : IPartyVitalMutationGateway
        {
            private readonly CharacterRuntimePartyVitalGateway actual;
            public FailAfterApplyGateway(CharacterRuntimePartyVitalGateway actual) => this.actual = actual;
            public bool TryCapture(out IReadOnlyList<PartyVitalSnapshot> roster) => actual.TryCapture(out roster);
            public bool TryApply(string transactionId, PartyVitalCostPlan plan, out PartyVitalMutationReceipt receipt)
            { actual.TryApply(transactionId, plan, out receipt); return false; }
            public bool TryRestore(PartyVitalMutationReceipt receipt) => actual.TryRestore(receipt);
        }

        private sealed class CountingGateway : IPartyVitalMutationGateway
        {
            private readonly CharacterRuntimePartyVitalGateway actual;
            public CountingGateway(CharacterRuntimePartyVitalGateway actual) => this.actual = actual;
            public int ApplyCalls { get; private set; }
            public bool TryCapture(out IReadOnlyList<PartyVitalSnapshot> roster) => actual.TryCapture(out roster);
            public bool TryApply(string transactionId, PartyVitalCostPlan plan, out PartyVitalMutationReceipt receipt)
            { ApplyCalls++; return actual.TryApply(transactionId, plan, out receipt); }
            public bool TryRestore(PartyVitalMutationReceipt receipt) => actual.TryRestore(receipt);
        }

        private sealed class PartialActualGateway : IPartyVitalMutationGateway
        {
            private readonly CharacterRuntimePartyVitalGateway[] gateways;
            public PartialActualGateway(params CharacterRuntimeData[] members)
            {
                gateways = new CharacterRuntimePartyVitalGateway[members.Length];
                for (int i = 0; i < members.Length; i++) gateways[i] = new CharacterRuntimePartyVitalGateway(Party(members[i]));
            }
            public int FailBeforeIndex { get; set; } = -1;
            public List<string> RestoreOrder { get; } = new();
            public bool TryCapture(out IReadOnlyList<PartyVitalSnapshot> roster)
            {
                List<PartyVitalSnapshot> all = new();
                foreach (CharacterRuntimePartyVitalGateway gateway in gateways)
                { if (!gateway.TryCapture(out IReadOnlyList<PartyVitalSnapshot> one)) { roster = Array.Empty<PartyVitalSnapshot>(); return false; } all.Add(one[0]); }
                roster = all.AsReadOnly(); return true;
            }
            public bool TryApply(string transactionId, PartyVitalCostPlan plan, out PartyVitalMutationReceipt receipt)
            {
                List<PartyVitalMutation> applied = new();
                for (int i = 0; i < plan.Mutations.Count; i++)
                {
                    if (i == FailBeforeIndex) { receipt = new PartyVitalMutationReceipt(transactionId, applied); return false; }
                    PartyVitalMutation mutation = plan.Mutations[i];
                    PartyVitalCostPlan one = PartyVitalCostPolicy.Evaluate(new[] { new PartyVitalSnapshot(mutation.MemberId, mutation.Before, mutation.Cost * 10) });
                    if (!gateways[i].TryApply(transactionId + "." + i, one, out _)) { receipt = new PartyVitalMutationReceipt(transactionId, applied); return false; }
                    applied.Add(mutation);
                }
                receipt = new PartyVitalMutationReceipt(transactionId, applied); return true;
            }
            public bool TryRestore(PartyVitalMutationReceipt receipt)
            {
                for (int i = receipt.Applied.Count - 1; i >= 0; i--)
                {
                    PartyVitalMutation mutation = receipt.Applied[i];
                    RestoreOrder.Add(mutation.MemberId);
                    PartyVitalMutationReceipt one = new(receipt.TransactionId + "." + i, new[] { mutation });
                    if (!gateways[i].TryRestore(one)) return false;
                }
                return true;
            }
        }
    }
}
