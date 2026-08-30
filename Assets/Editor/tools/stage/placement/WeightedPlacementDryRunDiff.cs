#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Stage;

namespace ResourceTools.Stage.Placement
{
    internal static class WeightedPlacementDryRunDiff
    {
        internal static IReadOnlyList<string> Compare(StagePlacementRuleSO rule, WeightedPlacementJsonDocument source)
        {
            var changes = new List<string>();
            if (rule == null) return new[] { "RULE_MISSING" };
            WeightedPoolPlacementConfig target = rule.weightedPool;
            if (target == null) return new[] { "CREATE_COMPILED_CONFIG" };
            if (target.schemaVersion != source.schemaVersion) changes.Add("schemaVersion");
            if (target.catalogId != source.catalogId) changes.Add("catalogId");
            if (target.runEncounterBudget != source.runEncounterBudget) changes.Add("runEncounterBudget");
            if (target.legacyMass != source.legacyMass || target.newMass != source.newMass) changes.Add("generationMass");
            string[] current = target.rows.Where(x=>x!=null).OrderBy(x=>x.order).ThenBy(x=>x.eventId,StringComparer.Ordinal)
                .Select(x=>$"{x.eventId}|{x.primaryBand}|{x.rawWeight}|{x.topLevelEligible}").ToArray();
            string[] proposed = source.rows.Where(x=>x!=null).OrderBy(x=>x.order).ThenBy(x=>x.eventId,StringComparer.Ordinal)
                .Select(x=>$"{x.eventId}|{x.primaryBand}|{x.rawWeight}|{x.topLevelEligible}").ToArray();
            if (!current.SequenceEqual(proposed)) changes.Add("rows");
            return changes;
        }
    }
}
#endif
