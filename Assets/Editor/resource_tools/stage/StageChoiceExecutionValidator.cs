using System;
using System.Collections.Generic;
using System.Linq;
using Stage;
using UnityEditor;
using UnityEngine;

namespace StageEditor
{
    /// <summary>
    /// 활성 stage_new PopupEventSO와 Choice 실행 config를 수정 없이 검사한다.
    /// </summary>
    public static class StageChoiceExecutionValidator
    {
        private const int MaxLoggedIssues = 100;
        private const string ActivePopupRoot =
            "Assets/Resources/stage_new/popup_events/";

        public sealed class ValidationSummary
        {
            public int EventCount { get; internal set; }
            public int ChoiceCount { get; internal set; }
            public int ConfiguredChoiceCount { get; internal set; }
            public int NextEventCount { get; internal set; }
            public int BattleCount { get; internal set; }
            public int ShopCount { get; internal set; }
            public int ShrineCount { get; internal set; }
            public int CompleteEventCount { get; internal set; }
            public int ErrorCount { get; internal set; }
            public int WarningCount { get; internal set; }

            internal List<ValidationIssue> Issues { get; } = new();
        }

        internal enum IssueSeverity
        {
            Warning,
            Error
        }

        internal sealed class ValidationIssue
        {
            public IssueSeverity Severity { get; }
            public string Message { get; }

            public ValidationIssue(
                IssueSeverity severity,
                string message)
            {
                Severity = severity;
                Message = message;
            }
        }

        [MenuItem("Tools/Stage/Validate Choice Execution")]
        public static void ValidateFromMenu()
        {
            ValidateProject(true);
        }

        /// <summary>
        /// Unity batchmode -executeMethod 진입점.
        /// 활성 에셋에서 구조 오류가 하나라도 있으면 실패시킨다.
        /// </summary>
        public static void ValidateBatch()
        {
            ValidationSummary summary = ValidateProject(true);

            if (summary.ErrorCount > 0)
            {
                throw new InvalidOperationException(
                    $"Event Choice validation failed: "
                    + $"{summary.ErrorCount} error(s).");
            }
        }

        public static ValidationSummary ValidateProject(
            bool logResult = false)
        {
            ValidationSummary summary = new();
            List<PopupEventSO> events = LoadPopupEvents();
            Dictionary<PopupEventSO, string> paths = events.ToDictionary(
                popupEvent => popupEvent,
                AssetDatabase.GetAssetPath);

            summary.EventCount = events.Count;

            foreach (PopupEventSO popupEvent in events)
            {
                ValidatePopupEvent(
                    popupEvent,
                    paths[popupEvent],
                    summary);
            }

            ValidateCycles(events, paths, summary);

            if (logResult)
            {
                LogSummary(summary);
            }

            return summary;
        }

        private static List<PopupEventSO> LoadPopupEvents()
        {
            string[] guids = AssetDatabase.FindAssets("t:PopupEventSO");
            List<PopupEventSO> events = new();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                PopupEventSO popupEvent =
                    AssetDatabase.LoadAssetAtPath<PopupEventSO>(path);

                if (popupEvent != null
                    && path.StartsWith(
                        ActivePopupRoot,
                        StringComparison.Ordinal))
                {
                    events.Add(popupEvent);
                }
            }

            return events
                .OrderBy(AssetDatabase.GetAssetPath)
                .ToList();
        }

        private static void ValidatePopupEvent(
            PopupEventSO popupEvent,
            string assetPath,
            ValidationSummary summary)
        {
            string eventLabel = GetEventLabel(popupEvent, assetPath);

            if (string.IsNullOrWhiteSpace(popupEvent.eventId))
            {
                AddIssue(
                    summary,
                    IssueSeverity.Error,
                    $"{eventLabel}: eventId is empty.");
            }

            if (popupEvent.choices == null)
            {
                AddIssue(
                    summary,
                    IssueSeverity.Error,
                    $"{eventLabel}: choices is null.");
                return;
            }

            HashSet<string> choiceIds = new();

            for (int i = 0; i < popupEvent.choices.Count; i++)
            {
                PopupEventChoice choice = popupEvent.choices[i];
                summary.ChoiceCount++;

                if (choice == null)
                {
                    AddIssue(
                        summary,
                        IssueSeverity.Error,
                        $"{eventLabel}: choices[{i}] is null.");
                    continue;
                }

                string choiceLabel =
                    $"{eventLabel} / choice[{i}] '{choice.choiceId}'";

                if (string.IsNullOrWhiteSpace(choice.choiceId))
                {
                    AddIssue(
                        summary,
                        IssueSeverity.Error,
                        $"{choiceLabel}: choiceId is empty.");
                }
                else if (!choiceIds.Add(choice.choiceId))
                {
                    AddIssue(
                        summary,
                        IssueSeverity.Error,
                        $"{choiceLabel}: duplicate choiceId.");
                }

                ValidateChoice(choice, choiceLabel, summary);
            }
        }

        private static void ValidateChoice(
            PopupEventChoice choice,
            string choiceLabel,
            ValidationSummary summary)
        {
            ChoiceExecutionConfig config = choice.executionConfig;

            if (config == null)
            {
                AddIssue(
                    summary,
                    IssueSeverity.Error,
                    $"{choiceLabel}: executionConfig is missing.");
                return;
            }

            summary.ConfiguredChoiceCount++;

            CountExecutionType(config.executionType, summary);

            List<string> errors =
                ChoiceExecutionConfigValidator.Validate(config);

            foreach (string error in errors)
            {
                AddIssue(
                    summary,
                    IssueSeverity.Error,
                    $"{choiceLabel}: {error}");
            }
        }

        private static void CountExecutionType(
            ChoiceExecutionType executionType,
            ValidationSummary summary)
        {
            switch (executionType)
            {
                case ChoiceExecutionType.NextEvent:
                    summary.NextEventCount++;
                    break;
                case ChoiceExecutionType.Battle:
                    summary.BattleCount++;
                    break;
                case ChoiceExecutionType.Shop:
                    summary.ShopCount++;
                    break;
                case ChoiceExecutionType.Shrine:
                    summary.ShrineCount++;
                    break;
                case ChoiceExecutionType.CompleteEvent:
                    summary.CompleteEventCount++;
                    break;
            }
        }

        private static void ValidateCycles(
            IReadOnlyList<PopupEventSO> events,
            IReadOnlyDictionary<PopupEventSO, string> paths,
            ValidationSummary summary)
        {
            Dictionary<PopupEventSO, VisitState> states = new();
            List<PopupEventSO> stack = new();
            HashSet<string> reportedCycles = new();

            foreach (PopupEventSO popupEvent in events)
            {
                Visit(
                    popupEvent,
                    paths,
                    states,
                    stack,
                    reportedCycles,
                    summary);
            }
        }

        private static void Visit(
            PopupEventSO popupEvent,
            IReadOnlyDictionary<PopupEventSO, string> paths,
            Dictionary<PopupEventSO, VisitState> states,
            List<PopupEventSO> stack,
            HashSet<string> reportedCycles,
            ValidationSummary summary)
        {
            if (popupEvent == null)
            {
                return;
            }

            if (states.TryGetValue(
                    popupEvent,
                    out VisitState state))
            {
                if (state == VisitState.Visited)
                {
                    return;
                }

                if (state == VisitState.Visiting)
                {
                    ReportCycle(
                        popupEvent,
                        paths,
                        stack,
                        reportedCycles,
                        summary);
                    return;
                }
            }

            states[popupEvent] = VisitState.Visiting;
            stack.Add(popupEvent);

            foreach (PopupEventSO nextEvent in GetNextEvents(popupEvent))
            {
                Visit(
                    nextEvent,
                    paths,
                    states,
                    stack,
                    reportedCycles,
                    summary);
            }

            stack.RemoveAt(stack.Count - 1);
            states[popupEvent] = VisitState.Visited;
        }

        private static IEnumerable<PopupEventSO> GetNextEvents(
            PopupEventSO popupEvent)
        {
            if (popupEvent?.choices == null)
            {
                yield break;
            }

            foreach (PopupEventChoice choice in popupEvent.choices)
            {
                if (choice == null)
                {
                    continue;
                }

                if (choice.executionConfig?.executionType
                    == ChoiceExecutionType.NextEvent
                    && choice.executionConfig.data
                    is NextEventExecutionData nextEventData)
                {
                    if (nextEventData.nextEvent != null)
                    {
                        yield return nextEventData.nextEvent;
                    }

                    continue;
                }

            }
        }

        private static void ReportCycle(
            PopupEventSO repeatedEvent,
            IReadOnlyDictionary<PopupEventSO, string> paths,
            IReadOnlyList<PopupEventSO> stack,
            HashSet<string> reportedCycles,
            ValidationSummary summary)
        {
            int startIndex = -1;

            for (int i = 0; i < stack.Count; i++)
            {
                if (stack[i] == repeatedEvent)
                {
                    startIndex = i;
                    break;
                }
            }

            IEnumerable<PopupEventSO> cycleEvents =
                startIndex >= 0
                    ? stack.Skip(startIndex).Concat(new[] { repeatedEvent })
                    : new[] { repeatedEvent };

            string cycle = string.Join(
                " -> ",
                cycleEvents.Select(
                    item => GetEventLabel(
                        item,
                        paths.TryGetValue(item, out string path)
                            ? path
                            : string.Empty)));

            if (!reportedCycles.Add(cycle))
            {
                return;
            }

            AddIssue(
                summary,
                IssueSeverity.Error,
                $"NEXT_EVENT_CYCLE: {cycle}");
        }

        private static string GetEventLabel(
            PopupEventSO popupEvent,
            string assetPath)
        {
            string eventId =
                popupEvent != null
                && !string.IsNullOrWhiteSpace(popupEvent.eventId)
                    ? popupEvent.eventId
                    : "<empty-event-id>";

            return $"{assetPath} [{eventId}]";
        }

        private static void AddIssue(
            ValidationSummary summary,
            IssueSeverity severity,
            string message)
        {
            if (severity == IssueSeverity.Error)
            {
                summary.ErrorCount++;
            }
            else
            {
                summary.WarningCount++;
            }

            if (summary.Issues.Count < MaxLoggedIssues)
            {
                summary.Issues.Add(
                    new ValidationIssue(severity, message));
            }
        }

        private static void LogSummary(ValidationSummary summary)
        {
            foreach (ValidationIssue issue in summary.Issues)
            {
                if (issue.Severity == IssueSeverity.Error)
                {
                    Debug.LogError(issue.Message);
                }
                else
                {
                    Debug.LogWarning(issue.Message);
                }
            }

            int omittedCount =
                summary.ErrorCount
                + summary.WarningCount
                - summary.Issues.Count;

            if (omittedCount > 0)
            {
                Debug.LogWarning(
                    $"{omittedCount} additional validation issue(s) omitted.");
            }

            Debug.Log(
                "Event Choice execution validation complete.\n"
                + $"Events: {summary.EventCount}\n"
                + $"Choices: {summary.ChoiceCount}\n"
                + $"Configured: {summary.ConfiguredChoiceCount}\n"
                + $"Types: NextEvent={summary.NextEventCount}, "
                + $"Battle={summary.BattleCount}, "
                + $"Shop={summary.ShopCount}, "
                + $"Shrine={summary.ShrineCount}, "
                + $"CompleteEvent={summary.CompleteEventCount}\n"
                + $"Errors: {summary.ErrorCount}, "
                + $"Warnings: {summary.WarningCount}");
        }

        private enum VisitState
        {
            Visiting,
            Visited
        }
    }
}
