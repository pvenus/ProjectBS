using System;
using System.Collections.Generic;

namespace Stage
{
    [Serializable]
    public sealed class RandomGrowthInteractionReservationData
    {
        public string authority;
        public string lifetime;
        public List<string> stableKeyFields = new();
        public List<string> orderedStates = new();
        public bool locksDecline;
        public bool blocksDuplicateConfirm;
        public int mutationCountBeforeAtomicTransaction;
    }

    [Serializable]
    public sealed class RandomGrowthCostProjectionData
    {
        public string type;
        public int rateBasisPoints;
        public string rounding;
        public float minimumRemainingHp;
    }

    [Serializable]
    public sealed class RandomGrowthCapProjectionData
    {
        public int fixedApplied;
        public int randomApplied;
        public int optionalGranted;
        public int optionalApplied;
        public int totalApplied;
    }

    [Serializable]
    public abstract class RandomGrowthChoiceExecutionData : ChoiceExecutionData
    {
        public int schemaVersion;
        public string contentContractVersion;
        public string definitionFingerprint;
        public string presentationTextDigestKo;
        public string presentationCatalogId;
        public string presentationProjectionKind;
        public string presentationLocale;
        public string eventId;
        public string stageNodeId;
        public string sourcePopupId;
        public string choiceId;
        public string segmentId;
        public string reservationId;
        public string poolMode;
        public int targetCount;
    }

    [Serializable]
    public sealed class RandomGrowthRiskExecutionData : RandomGrowthChoiceExecutionData
    {
        public string resultKind;
        public string successResultKind;
        public string failureState;
        public RandomGrowthInteractionReservationData interactionReservation;
        public RandomGrowthCostProjectionData costPolicy;
        public RandomGrowthCapProjectionData capPolicy;
        public int growthGrant;
    }

    [Serializable]
    public sealed class RandomGrowthDeclineExecutionData : RandomGrowthChoiceExecutionData
    {
        public string resultKind;
        public int cost;
        public int growthGrant;
    }

    [Serializable]
    public sealed class RandomGrowthSafeExecutionData : RandomGrowthChoiceExecutionData
    {
        public string resultKind;
        public string successResultKind;
        public string failureState;
        public string candidateUnavailableState;
        public RandomGrowthCapProjectionData capPolicy;
        public int cost;
        public int growthGrant;
        public bool candidateZeroBlocksExecutor;
        public bool candidateZeroAllowsFallback;
        public int mutationCountBeforeConfirm;
    }
}
