#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Progression.Portfolio;
using Stage;
using UnityEditor;

namespace ResourceTools.Stage.Placement
{
    internal static class WeightedPlacementCompiler
    {
        internal static void ApplyDocument(WeightedPlacementJsonDocument source, StagePlacementRuleSO target)
        {
            IReadOnlyList<string> errors = WeightedPlacementJsonValidator.Validate(source);
            if (errors.Count != 0) throw new InvalidOperationException(string.Join("\n", errors));
            Undo.RecordObject(target, "Apply weighted placement JSON");
            target.weightedPool ??= new WeightedPoolPlacementConfig();
            WeightedPoolPlacementConfig config = target.weightedPool;
            config.schemaVersion = source.schemaVersion;
            config.documentType = source.documentType;
            config.contractVersion = source.contractVersion;
            config.coefficientVersion = source.coefficientVersion;
            config.sourceRewardRiskDefinition = source.sourceRewardRiskDefinition;
            config.rewardRiskContractVersion = source.rewardRiskContractVersion;
            config.rewardRiskDefinitionSha256 = source.rewardRiskDefinitionSha256;
            config.staleRecalcState = Parse<WeightedPlacementStaleState>(source.staleRecalcState);
            config.staleReason = source.staleReason;
            config.catalogId = source.catalogId;
            config.chapterId = source.chapterId;
            config.sourceRuntimeAssetPath = source.sourceRuntimeAssetPath;
            config.sourceRuntimeAssetGuid = source.sourceRuntimeAssetGuid;
            config.sourceRuntimeAssetSha256 = source.sourceRuntimeAssetSha256;
            config.runEncounterBudget = source.runEncounterBudget;
            config.earlyBudget = source.earlyBudget;
            config.midBudget = source.midBudget;
            config.lateBudget = source.lateBudget;
            config.legacyMass = source.legacyMass;
            config.newMass = source.newMass;
            config.minEligibleCandidates = source.minEligibleCandidates;
            config.minEligiblePurposes = source.minEligiblePurposes;
            config.generatorVersion = source.generatorVersion;
            config.canonicalContentSha256 = source.canonicalContentSha256;
            config.sectionBands = (source.sectionBands ?? new List<SectionBandJson>())
                .OrderBy(x => x.sectionId, StringComparer.Ordinal)
                .Select(x => new WeightedPlacementSectionBand {
                    sectionId = x.sectionId,
                    band = Parse<WeightedPlacementBand>(x.band)
                }).ToList();
            config.rows = (source.rows ?? new List<EventRowJson>())
                .OrderBy(x => x.order).ThenBy(x => x.eventId, StringComparer.Ordinal)
                .Select(x => new WeightedPlacementEventRow {
                    eventId=x.eventId, topLevelNodeId=x.topLevelNodeId, popupId=x.popupId,
                    node=AssetDatabase.LoadAssetAtPath<RoundNodeSO>(x.nodeAssetPath),
                    generation=Parse<WeightedPlacementGeneration>(x.generation),
                    primaryBand=Parse<WeightedPlacementBand>(x.primaryBand), rawWeight=x.rawWeight,
                    topLevelEligible=x.topLevelEligible, capabilityGate=x.capabilityGate,
                    requiredCharacterId=x.requiredCharacterId,
                    primaryPurpose=Parse<PortfolioPurpose>(x.primaryPurpose),
                    secondaryPurpose=Parse<PortfolioPurpose>(x.secondaryPurpose), oneShot=x.oneShot,
                    exclusionGroupIds=(x.exclusionGroupIds ?? new List<string>()).Distinct().OrderBy(v=>v,StringComparer.Ordinal).ToList(),
                    motifTags=(x.motifTags ?? new List<string>()).Distinct().OrderBy(v=>v,StringComparer.Ordinal).ToList(),
                    cooldown=x.cooldown,
                    chainChildren=(x.chainChildren ?? new List<string>()).Distinct().OrderBy(v=>v,StringComparer.Ordinal).ToList(),
                    order=x.order, rationale=x.rationale, sourceAuthority=x.sourceAuthority,
                    staleState=Parse<WeightedPlacementStaleState>(x.staleState)
                }).ToList();
            config.overrides = (source.overrides ?? new List<OverrideJson>()).OrderBy(x=>x.overrideId,StringComparer.Ordinal)
                .Select(x=>new WeightedPlacementOverride { overrideId=x.overrideId,rowId=x.rowId,field=x.field,
                    baseContractVersion=x.baseContractVersion,oldValue=x.oldValue,newValue=x.newValue,
                    rationale=x.rationale,evidence=x.evidence,owner=x.owner,approver=x.approver,reviewGate=x.reviewGate,
                    affectedDistributionCells=(x.affectedDistributionCells ?? new List<string>()).Distinct()
                        .OrderBy(v=>v,StringComparer.Ordinal).ToList() }).ToList();
            EditorUtility.SetDirty(target);
        }

        private static T Parse<T>(string value) where T : struct =>
            Enum.TryParse(value, true, out T parsed) ? parsed : throw new InvalidOperationException("INVALID_ENUM:" + value);
    }
}
#endif
