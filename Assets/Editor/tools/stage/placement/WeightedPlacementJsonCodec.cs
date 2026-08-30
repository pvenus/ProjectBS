#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Stage;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Stage.Placement
{
    internal static class WeightedPlacementJsonCodec
    {
        internal static WeightedPlacementJsonDocument Parse(string json) =>
            JsonUtility.FromJson<WeightedPlacementJsonDocument>(json);

        internal static string Serialize(StagePlacementRuleSO rule)
        {
            WeightedPlacementJsonDocument value = FromRule(rule);
            value.canonicalContentSha256 = ComputeCanonicalDigest(value);
            return SerializeDocument(value);
        }

        internal static string ComputeCanonicalDigest(WeightedPlacementJsonDocument value)
        {
            WeightedPlacementJsonDocument normalized = Normalize(value);
            normalized.canonicalContentSha256 = string.Empty;
            return Sha256(SerializeDocument(normalized));
        }

        internal static WeightedPlacementJsonDocument FromRule(StagePlacementRuleSO rule)
        {
            WeightedPoolPlacementConfig catalog = rule?.weightedPool
                ?? throw new ArgumentNullException(nameof(rule));
            var value = new WeightedPlacementJsonDocument
            {
                schemaVersion = catalog.schemaVersion, documentType = catalog.documentType,
                contractVersion = catalog.contractVersion, coefficientVersion = catalog.coefficientVersion,
                sourceRewardRiskDefinition = catalog.sourceRewardRiskDefinition,
                rewardRiskContractVersion = catalog.rewardRiskContractVersion,
                rewardRiskDefinitionSha256 = catalog.rewardRiskDefinitionSha256,
                staleRecalcState = catalog.staleRecalcState.ToString(), staleReason = catalog.staleReason,
                catalogId = catalog.catalogId, chapterId = catalog.chapterId,
                sourceRuntimeAssetPath = catalog.sourceRuntimeAssetPath,
                sourceRuntimeAssetGuid = catalog.sourceRuntimeAssetGuid,
                sourceRuntimeAssetSha256 = catalog.sourceRuntimeAssetSha256,
                runEncounterBudget = catalog.runEncounterBudget, earlyBudget = catalog.earlyBudget,
                midBudget = catalog.midBudget, lateBudget = catalog.lateBudget,
                legacyMass = catalog.legacyMass, newMass = catalog.newMass,
                minEligibleCandidates = catalog.minEligibleCandidates,
                minEligiblePurposes = catalog.minEligiblePurposes,
                generatorVersion = catalog.generatorVersion
            };
            value.sectionBands = catalog.sectionBands.Where(x => x != null)
                .OrderBy(x => x.sectionId, StringComparer.Ordinal).Select(x => new SectionBandJson
                { sectionId = x.sectionId, band = x.band.ToString().ToLowerInvariant() }).ToList();
            value.rows = catalog.rows.Where(x => x != null)
                .OrderBy(x => x.order).ThenBy(x => x.eventId, StringComparer.Ordinal).Select(x =>
                    new EventRowJson {
                        eventId=x.eventId, topLevelNodeId=x.topLevelNodeId, popupId=x.popupId,
                        nodeAssetPath=AssetDatabase.GetAssetPath(x.node),
                        nodeGuid=AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(x.node)),
                        generation=x.generation.ToString().ToLowerInvariant(),
                        primaryBand=x.primaryBand.ToString().ToLowerInvariant(), rawWeight=x.rawWeight,
                        topLevelEligible=x.topLevelEligible, capabilityGate=x.capabilityGate,
                        requiredCharacterId=x.requiredCharacterId,
                        primaryPurpose=x.primaryPurpose.ToString(), secondaryPurpose=x.secondaryPurpose.ToString(),
                        oneShot=x.oneShot,
                        exclusionGroupIds=(x.exclusionGroupIds ?? new()).Distinct().OrderBy(v=>v,StringComparer.Ordinal).ToList(),
                        motifTags=(x.motifTags ?? new()).Distinct().OrderBy(v=>v,StringComparer.Ordinal).ToList(),
                        cooldown=x.cooldown, chainChildren=(x.chainChildren ?? new()).Distinct()
                            .OrderBy(v=>v,StringComparer.Ordinal).ToList(), order=x.order,
                        rationale=x.rationale, sourceAuthority=x.sourceAuthority,
                        staleState=x.staleState.ToString() }).ToList();
            value.overrides = (catalog.overrides ?? new()).Where(x => x != null)
                .OrderBy(x => x.overrideId, StringComparer.Ordinal).Select(x => new OverrideJson {
                    overrideId=x.overrideId,rowId=x.rowId,field=x.field,baseContractVersion=x.baseContractVersion,
                    oldValue=x.oldValue,newValue=x.newValue,rationale=x.rationale,evidence=x.evidence,
                    owner=x.owner,approver=x.approver,reviewGate=x.reviewGate,
                    affectedDistributionCells=(x.affectedDistributionCells ?? new()).Distinct()
                        .OrderBy(v=>v,StringComparer.Ordinal).ToList() }).ToList();
            return value;
        }

        private static WeightedPlacementJsonDocument Normalize(WeightedPlacementJsonDocument value)
        {
            WeightedPlacementJsonDocument copy = Parse(JsonUtility.ToJson(value));
            copy.sectionBands = (copy.sectionBands ?? new()).Where(x=>x!=null).OrderBy(x=>x.sectionId,StringComparer.Ordinal).ToList();
            copy.rows = (copy.rows ?? new()).Where(x=>x!=null).OrderBy(x=>x.order).ThenBy(x=>x.eventId,StringComparer.Ordinal).ToList();
            foreach (EventRowJson row in copy.rows)
            {
                row.exclusionGroupIds = (row.exclusionGroupIds ?? new()).Distinct().OrderBy(x=>x,StringComparer.Ordinal).ToList();
                row.motifTags = (row.motifTags ?? new()).Distinct().OrderBy(x=>x,StringComparer.Ordinal).ToList();
                row.chainChildren = (row.chainChildren ?? new()).Distinct().OrderBy(x=>x,StringComparer.Ordinal).ToList();
            }
            copy.overrides = (copy.overrides ?? new()).Where(x=>x!=null).OrderBy(x=>x.overrideId,StringComparer.Ordinal).ToList();
            foreach (OverrideJson item in copy.overrides)
                item.affectedDistributionCells=(item.affectedDistributionCells ?? new()).Distinct().OrderBy(x=>x,StringComparer.Ordinal).ToList();
            return copy;
        }

        private static string SerializeDocument(WeightedPlacementJsonDocument value) => JsonUtility.ToJson(value, true) + "\n";

        internal static string Sha256(string value)
        {
            using SHA256 sha = SHA256.Create();
            return string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(value))
                .Select(x => x.ToString("x2")));
        }
    }
}
#endif
