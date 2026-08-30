using System;
using System.Collections.Generic;

namespace Stage
{
    public enum ConfirmableChoiceDispatchKind
    {
        Unsupported = 0,
        RequiresConfirmation = 10,
        Disabled = 20,
        PendingRetry = 30,
        TerminalReplay = 40
    }

    public enum ConfirmableChoiceDisabledReason
    {
        None = 0,
        InvalidConfig = 10,
        IdentityMismatch = 20
    }

    public enum ConfirmableChoiceRuntimeState
    {
        Offerable = 0,
        PendingRetry = 10,
        Terminal = 20
    }

    /// <summary>Mutable config/data references를 노출하지 않는 immutable query 결과.</summary>
    public sealed class ConfirmableChoiceDispatchResult
    {
        internal ConfirmableChoiceDispatchResult(
            ConfirmableChoiceDispatchKind kind,
            ConfirmableChoiceDisabledReason disabledReason,
            string choiceId)
        {
            Kind = kind;
            DisabledReason = disabledReason;
            ChoiceId = choiceId ?? string.Empty;
        }

        public ConfirmableChoiceDispatchKind Kind { get; }
        public ConfirmableChoiceDisabledReason DisabledReason { get; }
        public string ChoiceId { get; }
    }

    public static class ConfirmableChoiceContract
    {
        public const string ContentContractVersion = "chapter1-random-growth-safe-content.v1";
        public const string EventId = "event.act1.random_growth.02.windworn_sword_marks";
        public const string StageNodeId = "stage.act1.random_growth.02.windworn_sword_marks";
        public const string SourcePopupId = "node.act1.random_growth.02.windworn_sword_marks.intro";
        public const string ObserveChoiceId = "choice.act1.random_growth.02.windworn_sword_marks.observe_sword_path";
        public const string DeclineChoiceId = "choice.act1.random_growth.02.windworn_sword_marks.leave_training_ground";
        public const string DefinitionFingerprint = "0de9e9ac1418ccdce75d0fc2826c919d26790eebbc5b841d69bc2e35814252bb";
        public const string V2ContentContractVersion = "chapter1-random-growth-safe-content.v2";
        public const string V2DefinitionFingerprint = "72acd7c52fdc3aebe4e9c5cedbdb01377c80e1f65512478d25657973b65da6bd";
        public const string V2CatalogId = "presentation.catalog.act1.random_growth.02.windworn_sword_marks.ko-KR";
        public const string V2ProjectionKind = "safe-growth-presentation-copy.v2";
        public const string V2Locale = "ko-KR";

        public static ConfirmableChoiceDispatchResult Query(
            ChoiceExecutionConfig config,
            ConfirmableChoiceRuntimeState state = ConfirmableChoiceRuntimeState.Offerable)
        {
            if (config == null)
            {
                return Disabled(ConfirmableChoiceDisabledReason.InvalidConfig, string.Empty);
            }

            bool safeType = config.executionType == ChoiceExecutionType.RandomGrowthSafe;
            bool declineType = config.executionType == ChoiceExecutionType.RandomGrowthDecline;
            if (!safeType && !declineType)
            {
                return Unsupported();
            }

            RandomGrowthChoiceExecutionData data = config.data as RandomGrowthChoiceExecutionData;
            if (data == null)
            {
                return safeType
                    ? Disabled(ConfirmableChoiceDisabledReason.InvalidConfig, string.Empty)
                    : Unsupported();
            }

            bool claimsSafeIdentity = string.Equals(data.eventId, EventId, StringComparison.Ordinal)
                || string.Equals(data.sourcePopupId, SourcePopupId, StringComparison.Ordinal)
                || string.Equals(data.contentContractVersion, ContentContractVersion, StringComparison.Ordinal)
                || string.Equals(data.contentContractVersion, V2ContentContractVersion, StringComparison.Ordinal);

            if (declineType && !claimsSafeIdentity)
            {
                return Unsupported();
            }

            List<string> validationErrors = ChoiceExecutionConfigValidator.Validate(config);
            if (validationErrors.Count != 0)
            {
                return Disabled(ConfirmableChoiceDisabledReason.InvalidConfig, data.choiceId);
            }

            string expectedChoice = safeType ? ObserveChoiceId : DeclineChoiceId;
            bool exactLegacy = data.schemaVersion == 1
                && string.Equals(data.contentContractVersion, ContentContractVersion, StringComparison.Ordinal)
                && string.Equals(data.definitionFingerprint, DefinitionFingerprint, StringComparison.Ordinal)
                && string.IsNullOrEmpty(data.presentationCatalogId)
                && string.IsNullOrEmpty(data.presentationProjectionKind)
                && string.IsNullOrEmpty(data.presentationLocale);
            bool exactV2 = data.schemaVersion == 2
                && string.Equals(data.contentContractVersion, V2ContentContractVersion, StringComparison.Ordinal)
                && string.Equals(data.definitionFingerprint, V2DefinitionFingerprint, StringComparison.Ordinal)
                && string.Equals(data.presentationCatalogId, V2CatalogId, StringComparison.Ordinal)
                && string.Equals(data.presentationProjectionKind, V2ProjectionKind, StringComparison.Ordinal)
                && string.Equals(data.presentationLocale, V2Locale, StringComparison.Ordinal);
            bool exact = (exactLegacy || exactV2)
                && string.Equals(data.eventId, EventId, StringComparison.Ordinal)
                && string.Equals(data.stageNodeId, StageNodeId, StringComparison.Ordinal)
                && string.Equals(data.sourcePopupId, SourcePopupId, StringComparison.Ordinal)
                && string.Equals(data.choiceId, expectedChoice, StringComparison.Ordinal)
                && data.targetCount == 2
                && ((safeType && data is RandomGrowthSafeExecutionData safe
                    && safe.resultKind == "ObserveSelected"
                    && safe.successResultKind == "SafeGrowthGranted")
                    || (declineType && data is RandomGrowthDeclineExecutionData decline
                    && decline.resultKind == "Declined"));

            if (!exact)
            {
                return Disabled(ConfirmableChoiceDisabledReason.IdentityMismatch, data.choiceId);
            }

            return state switch
            {
                ConfirmableChoiceRuntimeState.PendingRetry =>
                    Result(ConfirmableChoiceDispatchKind.PendingRetry, data.choiceId),
                ConfirmableChoiceRuntimeState.Terminal =>
                    Result(ConfirmableChoiceDispatchKind.TerminalReplay, data.choiceId),
                _ => Result(ConfirmableChoiceDispatchKind.RequiresConfirmation, data.choiceId)
            };
        }

        private static ConfirmableChoiceDispatchResult Unsupported() =>
            Result(ConfirmableChoiceDispatchKind.Unsupported, string.Empty);

        private static ConfirmableChoiceDispatchResult Disabled(
            ConfirmableChoiceDisabledReason reason,
            string choiceId) =>
            new(ConfirmableChoiceDispatchKind.Disabled, reason, choiceId);

        private static ConfirmableChoiceDispatchResult Result(
            ConfirmableChoiceDispatchKind kind,
            string choiceId) =>
            new(kind, ConfirmableChoiceDisabledReason.None, choiceId);
    }
}
