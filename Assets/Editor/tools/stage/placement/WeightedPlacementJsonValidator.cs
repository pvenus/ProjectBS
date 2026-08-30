#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Stage;
using UnityEditor;

namespace ResourceTools.Stage.Placement
{
    internal static class WeightedPlacementJsonValidator
    {
        internal static IReadOnlyList<string> Validate(WeightedPlacementJsonDocument value)
        {
            var errors = new List<string>();
            if (value == null) return new[] { "DOCUMENT_NULL" };
            if (value.schemaVersion != WeightedPoolPlacementConfig.CurrentSchemaVersion
                || value.documentType != WeightedPoolPlacementConfig.DocumentType)
                errors.Add("SCHEMA_OR_DOCUMENT_TYPE_INVALID");
            if (value.runEncounterBudget != 12 || value.earlyBudget != 4
                || value.midBudget != 4 || value.lateBudget != 4) errors.Add("BUDGET_INVALID");
            if (value.legacyMass != 45 || value.newMass != 55) errors.Add("GENERATION_MASS_INVALID");
            if (string.IsNullOrWhiteSpace(value.sourceRewardRiskDefinition)
                || string.IsNullOrWhiteSpace(value.rewardRiskContractVersion)
                || value.rewardRiskDefinitionSha256?.Length != 64) errors.Add("REWARD_RISK_AUTHORITY_MISSING");
            if (!Enum.TryParse(value.staleRecalcState, true, out WeightedPlacementStaleState rootState))
                errors.Add("STALE_RECALC_STATE_INVALID");
            else if (rootState != WeightedPlacementStaleState.Current) errors.Add("STALE_RECALC_REQUIRED");
            if (value.rows == null || value.rows.Count != 46) errors.Add("EXACT46_REQUIRED");
            EventRowJson[] rows = value.rows?.Where(x => x != null).ToArray() ?? Array.Empty<EventRowJson>();
            if (rows.Select(x => x.eventId).Distinct(StringComparer.Ordinal).Count() != rows.Length)
                errors.Add("DUPLICATE_EVENT_ID");
            foreach (EventRowJson row in rows)
            {
                if (!Enum.TryParse(row.primaryBand, true, out WeightedPlacementBand _))
                    errors.Add("BAND_INVALID:" + row.eventId);
                if (row.topLevelEligible && row.rawWeight <= 0) errors.Add("WEIGHT_INVALID:" + row.eventId);
                if (string.IsNullOrWhiteSpace(row.rationale) || string.IsNullOrWhiteSpace(row.sourceAuthority))
                    errors.Add("ROW_AUTHORING_AUTHORITY_MISSING:" + row.eventId);
                if (!Enum.TryParse(row.staleState, true, out WeightedPlacementStaleState rowState)
                    || rowState != WeightedPlacementStaleState.Current) errors.Add("ROW_STALE:" + row.eventId);
                if (!string.IsNullOrWhiteSpace(row.nodeAssetPath))
                {
                    string actual = AssetDatabase.AssetPathToGUID(row.nodeAssetPath);
                    if (string.IsNullOrEmpty(actual) || actual != row.nodeGuid)
                        errors.Add("NODE_GUID_PATH_MISMATCH:" + row.eventId);
                }
            }
            EventRowJson event11 = rows.SingleOrDefault(x => x.eventId?.Contains("random_event.11.") == true);
            if (event11 == null || event11.topLevelEligible || event11.rawWeight != 0)
                errors.Add("EVENT11_CHILD_ONLY_INVALID");
            EventRowJson event16 = rows.SingleOrDefault(x => x.eventId?.Contains("random_event.16.") == true);
            if (event16 == null || event16.capabilityGate != "relic_p1")
                errors.Add("EVENT16_GATE_INVALID");
            if (rows.Any(x => x.eventId?.Contains("half_vein_map.followup") == true))
                errors.Add("EVENT34_CHILD_TOP_LEVEL_INVALID");
            OverrideJson[] overrides = value.overrides?.Where(x=>x!=null).ToArray() ?? Array.Empty<OverrideJson>();
            if (overrides.Select(x=>x.overrideId).Distinct(StringComparer.Ordinal).Count() != overrides.Length)
                errors.Add("DUPLICATE_OVERRIDE_ID");
            foreach (OverrideJson item in overrides)
                if (string.IsNullOrWhiteSpace(item.overrideId) || item.baseContractVersion != value.contractVersion
                    || string.IsNullOrWhiteSpace(item.rationale) || string.IsNullOrWhiteSpace(item.evidence))
                    errors.Add("OVERRIDE_AUTHORITY_INVALID:" + item.overrideId);
            if (!string.IsNullOrWhiteSpace(value.canonicalContentSha256)
                && !string.Equals(value.canonicalContentSha256,
                    WeightedPlacementJsonCodec.ComputeCanonicalDigest(value), StringComparison.Ordinal))
                errors.Add("DIGEST_MISMATCH");
            return errors;
        }
    }
}
#endif
