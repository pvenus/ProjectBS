using System;
using System.Collections.Generic;
using System.Linq;

namespace Stage
{
    public sealed class Chapter1BattlePressureManifestBuilder
    {
        private static readonly Chapter1EncounterKind[] Skeleton =
        {
            Chapter1EncounterKind.Event, Chapter1EncounterKind.DirectBattle,
            Chapter1EncounterKind.Rest, Chapter1EncounterKind.Event,
            Chapter1EncounterKind.DirectBattle, Chapter1EncounterKind.Shop,
            Chapter1EncounterKind.Event, Chapter1EncounterKind.DirectBattle,
            Chapter1EncounterKind.Rest, Chapter1EncounterKind.Event,
            Chapter1EncounterKind.DirectBattle, Chapter1EncounterKind.Shop
        };

        public Chapter1BattlePressureManifest Build(WeightedPoolPlacementConfig catalog, int seed,
            IReadOnlyCollection<string> roster = null, Func<string, bool> capabilityGate = null)
        {
            BattlePressureCompositionConfig config = catalog?.composition;
            string error = Validate(config);
            if (!string.IsNullOrEmpty(error))
                return new Chapter1BattlePressureManifest(false, error, null);

            List<RoundNodeSO> battles = Eligible(config.directBattlePool, RoundNodeType.Battle,
                RoundNodeType.EliteBattle).Distinct().ToList();
            List<RoundNodeSO> shops = Eligible(config.shopPool, RoundNodeType.Shop).Distinct().ToList();
            List<RoundNodeSO> rests = Eligible(config.restPool, RoundNodeType.Rest).Distinct().ToList();
            if (battles.Count < 4) return new(false, "BATTLE_PRESSURE_DIRECT_POOL_INSUFFICIENT", null, true);
            if (shops.Count < 1) return new(false, "BATTLE_PRESSURE_SHOP_POOL_EMPTY", null, true);
            if (rests.Count < 1) return new(false, "BATTLE_PRESSURE_REST_POOL_EMPTY", null, true);

            Shuffle(battles, new Random(seed ^ 0x4b17));
            WeightedPlacementBand[] eventBands =
            {
                WeightedPlacementBand.Early, WeightedPlacementBand.All,
                WeightedPlacementBand.Mid, WeightedPlacementBand.Late
            };
            Chapter1WeightedEventManifest events = new Chapter1WeightedEventManifestBuilder()
                .BuildForBands(catalog, eventBands, seed ^ 0x7e31, roster, capabilityGate);
            if (!events.Success) return new(false, events.Error, null);

            int battleIndex = 0, eventIndex = 0;
            var assignments = new List<Chapter1CompositionAssignment>(12);
            for (int index = 0; index < Skeleton.Length; index++)
            {
                WeightedPlacementBand phase = index < 4 ? WeightedPlacementBand.Early
                    : index < 8 ? WeightedPlacementBand.Mid : WeightedPlacementBand.Late;
                Chapter1EncounterKind kind = Skeleton[index];
                RoundNodeSO node;
                WeightedPlacementEventRow row = null;
                switch (kind)
                {
                    case Chapter1EncounterKind.DirectBattle: node = battles[battleIndex++]; break;
                    case Chapter1EncounterKind.Shop: node = shops[(index / 4) % shops.Count]; break;
                    case Chapter1EncounterKind.Rest: node = rests[(index / 4) % rests.Count]; break;
                    default:
                        Chapter1WeightedEventAssignment selected = events.Assignments[eventIndex++];
                        node = selected.Row.node; row = selected.Row; break;
                }
                assignments.Add(new Chapter1CompositionAssignment(index + 1, phase, kind, node, row));
            }
            return new Chapter1BattlePressureManifest(true, string.Empty, assignments);
        }

        private static IEnumerable<RoundNodeSO> Eligible(EventPoolSO pool,
            params RoundNodeType[] allowed) => pool?.entries?
            .Where(entry => entry?.node != null && entry.weight > 0
                && allowed.Contains(entry.node.nodeType)).Select(entry => entry.node)
            ?? Enumerable.Empty<RoundNodeSO>();

        private static string Validate(BattlePressureCompositionConfig value)
        {
            if (value == null || !value.enabled) return "BATTLE_PRESSURE_DISABLED";
            if (value.schemaVersion != 1 || value.staleState != WeightedPlacementStaleState.Current)
                return "BATTLE_PRESSURE_SCHEMA_OR_STALE_INVALID";
            if (value.directBattleCount != 4 || value.shopCount != 2
                || value.restCount != 2 || value.eventCount != 4
                || value.earlyDirect != 1 || value.midDirect != 2 || value.lateDirect != 1
                || value.maxDirectBattleFreeGap != 3 || value.allowAdjacentDirectBattle)
                return "BATTLE_PRESSURE_QUOTA_INVALID";
            return string.Empty;
        }

        private static void Shuffle<T>(IList<T> values, Random random)
        {
            for (int i = values.Count - 1; i > 0; i--)
            { int j = random.Next(i + 1); (values[i], values[j]) = (values[j], values[i]); }
        }
    }
}
