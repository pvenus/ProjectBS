#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace ResourceTools.Stage.Placement
{
    [Serializable]
    internal sealed class WeightedPlacementJsonDocument
    {
        public int schemaVersion = 1;
        public string documentType = "weightedPoolPlacementRule";
        public string contractVersion;
        public string coefficientVersion;
        public string sourceRewardRiskDefinition;
        public string rewardRiskContractVersion;
        public string rewardRiskDefinitionSha256;
        public string staleRecalcState;
        public string staleReason;
        public List<OverrideJson> overrides = new();
        public string catalogId;
        public string chapterId;
        public string sourceRuntimeAssetPath;
        public string sourceRuntimeAssetGuid;
        public string sourceRuntimeAssetSha256;
        public int runEncounterBudget;
        public int earlyBudget;
        public int midBudget;
        public int lateBudget;
        public int legacyMass;
        public int newMass;
        public int minEligibleCandidates;
        public int minEligiblePurposes;
        public CompositionJson composition = new();
        public List<SectionBandJson> sectionBands = new();
        public List<EventRowJson> rows = new();
        public string canonicalContentSha256;
        public string generatorVersion;
    }

    [Serializable]
    internal sealed class SectionBandJson { public string sectionId; public string band; }

    [Serializable]
    internal sealed class CompositionJson
    {
        public bool enabled;
        public int schemaVersion = 1;
        public string coefficientVersion;
        public string directBattlePoolAssetPath, directBattlePoolGuid;
        public string shopPoolAssetPath, shopPoolGuid;
        public string restPoolAssetPath, restPoolGuid;
        public int directBattleCount, shopCount, restCount, eventCount;
        public int earlyDirect, midDirect, lateDirect, maxDirectBattleFreeGap;
        public bool allowAdjacentDirectBattle;
        public float optionalBattleCredit;
        public string classificationAuthorityVersion, staleState, staleReason;
    }

    [Serializable]
    internal sealed class EventRowJson
    {
        public string eventId;
        public string topLevelNodeId;
        public string popupId;
        public string nodeAssetPath;
        public string nodeGuid;
        public string generation;
        public string primaryBand;
        public int rawWeight;
        public bool topLevelEligible;
        public string capabilityGate;
        public string requiredCharacterId;
        public string primaryPurpose;
        public string secondaryPurpose;
        public bool oneShot;
        public List<string> exclusionGroupIds = new();
        public List<string> motifTags = new();
        public int cooldown;
        public List<string> chainChildren = new();
        public int order;
        public string rationale;
        public string sourceAuthority;
        public string staleState;
        public string combatClass;
        public float expectedBattleCredit;
        public string combatAuthorityVersion;
        public string combatStaleState;
    }

    [Serializable]
    internal sealed class OverrideJson
    {
        public string overrideId;
        public string rowId;
        public string field;
        public string baseContractVersion;
        public string oldValue;
        public string newValue;
        public string rationale;
        public string evidence;
        public string owner;
        public string approver;
        public string reviewGate;
        public List<string> affectedDistributionCells = new();
    }
}
#endif
