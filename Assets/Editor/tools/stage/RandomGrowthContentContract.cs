#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace ResourceTools.Stage
{
    [Serializable] public sealed class RandomGrowthContentDocument
    {
        public int schemaVersion; public string documentType, contentContractVersion, generatorVersion;
        public string poolId, eventId, stageNodeId, nodeId, segmentId, reservationId, poolMode;
        public List<string> pairedSectionIds = new(); public string startNodeId, mainImagePath, mainImageSha256;
        public string presentationTextDigestKo; public RandomGrowthPresentationTextKo presentationKo;
        public RandomGrowthCapPolicyContent capPolicy; public List<RandomGrowthContentNode> nodes = new();
    }
    [Serializable] public sealed class RandomGrowthPresentationTextKo
    {
        public string title, discoveryBody, riskLabel, riskCostLine, riskEligibilityLine, riskRewardLine;
        public string declineLabel, declineHelper, confirmTitle, confirmBody, confirmCta, confirmCancelCta;
        public string disabledInsufficientParty, disabledInsufficientMemberTemplate, disabledNoCandidate;
        public string disabledCapReached, disabledTechnical, revalidateCta, riskSuccessBody, riskSuccessStatus;
        public string riskSuccessCta, declineResultBody, declineResultStatus, declineResultCta;
        public string transactionFailureBody, transactionFailureHelper, transactionRetryCta, growthAppliedFollowupTemplate;
    }
    [Serializable] public sealed class RandomGrowthContentNode
    { public string nodeId, nodeType, sourcePopupId; public List<RandomGrowthContentChoice> choices = new(); }
    [Serializable] public sealed class RandomGrowthContentChoice
    {
        public string choiceId, sourcePopupId; public bool isTerminal; public int disabledExecutorCallCount;
        public RandomGrowthTypedExecution execution; public List<string> rewards = new();
    }
    [Serializable] public sealed class RandomGrowthTypedExecution
    {
        public int schemaVersion; public string executionType, resultKind, successResultKind, failureState;
        public string contentContractVersion, eventId, stageNodeId, nodeId, choiceId, segmentId, reservationId, poolMode;
        public RandomGrowthInteractionReservationContent interactionReservation;
        public RandomGrowthCostPolicyContent costPolicy; public RandomGrowthCapPolicyContent capPolicy;
        public int cost, growthGrant; public List<string> rewards = new();
    }
    [Serializable] public sealed class RandomGrowthInteractionReservationContent
    {
        public string authority, lifetime; public List<string> stableKeyFields = new();
        public List<string> orderedStates = new(); public bool locksDecline, blocksDuplicateConfirm;
        public int mutationCountBeforeAtomicTransaction;
    }
    [Serializable] public sealed class RandomGrowthCostPolicyContent
    { public string type; public int rateBasisPoints; public string rounding; public float minimumRemainingHp; }
    [Serializable] public sealed class RandomGrowthCapPolicyContent
    { public int fixedApplied; public int randomApplied; public int totalApplied; }
    [Serializable] public sealed class RandomGrowthPoolDocument
    {
        public int schemaVersion; public string documentType, contentContractVersion, poolId, displayName, probabilityOwner;
        public List<RandomGrowthPoolEntryContent> entries = new();
    }
    [Serializable] public sealed class RandomGrowthPoolEntryContent
    {
        public string entryId, nodeJsonPath, nodeId; public int weight; public bool oneShot;
        public int cooldownRounds, minDepth, maxDepth; public List<string> tags = new();
    }
    public sealed class RandomGrowthContentBuildPlan
    {
        public string RoundNodeAssetPath { get; set; }
        public string PopupEventAssetPath { get; set; }
        public string EventPoolAssetPath { get; set; }
        public string StageAssetPath => RoundNodeAssetPath;
        public string PopupAssetPath => PopupEventAssetPath;
        public string PoolAssetPath => EventPoolAssetPath;
        public string DefinitionFingerprint { get; set; } public string PresentationTextDigestKo { get; set; }
    }
}
#endif
