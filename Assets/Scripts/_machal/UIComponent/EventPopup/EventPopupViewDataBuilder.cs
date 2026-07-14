using System;
using System.Collections.Generic;
using System.Text;
using Character;
using Stage;
using UIFramework.Data;
using UnityEngine;

public static class EventPopupViewDataBuilder
{
    public static EventPopupViewData Build(PopupEventSO popupEvent, RoundNode node)
    {
        if (popupEvent == null) return default;

        return new EventPopupViewData
        {
            Title = node != null ? node.Title : (popupEvent != null ? popupEvent.Title : string.Empty),
            Description = popupEvent != null ? popupEvent.Body : string.Empty,
            Illustration = popupEvent != null ? popupEvent.mainImage : null,
            Choices = BuildChoices(popupEvent)
        };
    }

    private static IReadOnlyList<EventChoiceViewData> BuildChoices(PopupEventSO popupEvent)
    {
        List<EventChoiceViewData> resultList = new List<EventChoiceViewData>();
        if (popupEvent == null || popupEvent.choices == null) return resultList;

        foreach (var choice in popupEvent.choices)
        {
            if (IsChoiceVisible(choice))
            {
                string result = choice.ResultText;
                bool hasResultText = !string.IsNullOrWhiteSpace(result)
                    && !string.Equals(result, choice.choiceId + ".result");

                string rewardText = string.Empty;
                if (choice.rewards != null && choice.rewards.Count > 0)
                {
                    List<string> rewardStrings = new List<string>();
                    foreach (var reward in choice.rewards)
                    {
                        string formatted = GetRewardText(reward);
                        if (!string.IsNullOrEmpty(formatted))
                        {
                            rewardStrings.Add(formatted);
                        }
                    }
                    if (rewardStrings.Count > 0)
                    {
                        rewardText = "\n\n<b>[획득 보상]</b>\n" + string.Join("\n", rewardStrings);
                    }
                }

                bool hasResultOrRewards = hasResultText || (choice.rewards != null && choice.rewards.Count > 0);

                resultList.Add(new EventChoiceViewData
                {
                    Id = choice.choiceId,
                    Text = choice.Label,
                    ResultText = (hasResultText ? result : "보상을 획득했습니다.") + rewardText,
                    HasResult = hasResultOrRewards
                });
            }
        }
        return resultList;
    }

    private static bool IsChoiceVisible(PopupEventChoice choice)
    {
        if (choice == null) return false;
        if (choice.visibleConditions == null || choice.visibleConditions.Count == 0) return true;

        foreach (PopupEventChoiceConditionData condition in choice.visibleConditions)
        {
            if (!IsChoiceConditionSatisfied(condition))
            {
                return false;
            }
        }
        return true;
    }

    private static bool IsChoiceConditionSatisfied(PopupEventChoiceConditionData condition)
    {
        if (condition == null || condition.conditionType == PopupEventChoiceConditionType.None) return true;

        bool satisfied = condition.conditionType switch
        {
            PopupEventChoiceConditionType.HasCharacter => HasCharacter(condition.targetId),
            PopupEventChoiceConditionType.HasCharacterJob => HasCharacterJob(condition.targetId),
            PopupEventChoiceConditionType.HasCharacterJobFamily => HasCharacterJobFamily(condition.targetId),
            PopupEventChoiceConditionType.HasCharacterJobTier => HasCharacterJobTier(condition.targetId),
            PopupEventChoiceConditionType.HasTag => HasTag(condition.tag),
            PopupEventChoiceConditionType.HasRelic => HasRelic(condition.targetId),
            PopupEventChoiceConditionType.HasBless => HasBless(condition.targetId),
            PopupEventChoiceConditionType.HasItem => HasItem(condition.targetId),
            _ => true
        };

        return condition.invert ? !satisfied : satisfied;
    }

    private static bool HasCharacter(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId)) return false;
        return false;
    }

    private static bool HasCharacterJob(string jobText)
    {
        if (string.IsNullOrWhiteSpace(jobText)) return false;

        if (!System.Enum.TryParse(jobText, out CharacterJob targetJob))
        {
            return false;
        }

        return HasCharacterJob(targetJob);
    }

    private static bool HasCharacterJob(CharacterJob targetJob)
    {
        CharacterManager[] managers = UnityEngine.Object.FindObjectsByType<CharacterManager>(FindObjectsSortMode.None);

        foreach (CharacterManager manager in managers)
        {
            var runtimeData = manager?.RuntimeData;
            if (runtimeData?.characterSO == null) continue;
            if (runtimeData.characterSO.CharacterType != CharacterType.Player) continue;

            if (runtimeData.characterSO.Job == targetJob)
            {
                return true;
            }
        }
        return false;
    }

    private static bool HasCharacterJobFamily(string familyText)
    {
        if (string.IsNullOrWhiteSpace(familyText)) return false;

        if (!System.Enum.TryParse(familyText, out CharacterJobFamily targetFamily))
        {
            return false;
        }

        CharacterManager[] managers = UnityEngine.Object.FindObjectsByType<CharacterManager>(FindObjectsSortMode.None);

        foreach (CharacterManager manager in managers)
        {
            var runtimeData = manager?.RuntimeData;
            if (runtimeData?.characterSO == null) continue;
            if (runtimeData.characterSO.CharacterType != CharacterType.Player) continue;

            if (runtimeData.characterSO.JobFamily == targetFamily)
            {
                return true;
            }
        }
        return false;
    }

    private static bool HasCharacterJobTier(string tierText)
    {
        if (string.IsNullOrWhiteSpace(tierText)) return false;

        if (!System.Enum.TryParse(tierText, out CharacterJobTier targetTier))
        {
            return false;
        }

        CharacterManager[] managers = UnityEngine.Object.FindObjectsByType<CharacterManager>(FindObjectsSortMode.None);

        foreach (CharacterManager manager in managers)
        {
            var runtimeData = manager?.RuntimeData;
            if (runtimeData?.characterSO == null) continue;
            if (runtimeData.characterSO.CharacterType != CharacterType.Player) continue;

            if (runtimeData.characterSO.JobTier == targetTier)
            {
                return true;
            }
        }
        return false;
    }

    private static bool HasTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) return false;
        return false;
    }

    private static bool HasRelic(string targetId)
    {
        if (string.IsNullOrWhiteSpace(targetId)) return false;
        return false;
    }

    private static bool HasBless(string targetId)
    {
        if (string.IsNullOrWhiteSpace(targetId)) return false;
        return false;
    }

    private static bool HasItem(string targetId)
    {
        if (string.IsNullOrWhiteSpace(targetId)) return false;
        return false;
    }

    private static string GetRewardText(PopupEventRewardData reward)
    {
        if (reward == null) return string.Empty;

        switch (reward.rewardType)
        {
            case PopupEventRewardType.Gold:
                return $"골드 +{reward.value}";
            case PopupEventRewardType.Hp:
                return $"체력 +{reward.value}";
            case PopupEventRewardType.HpPercent:
                return $"체력 +{reward.value}%";
            case PopupEventRewardType.Reputation:
                return $"명성 {(reward.value >= 0 ? "+" + reward.value : reward.value.ToString())}";
            case PopupEventRewardType.Faith:
                return $"신앙 {(reward.value >= 0 ? "+" + reward.value : reward.value.ToString())}";
            case PopupEventRewardType.Relic:
            case PopupEventRewardType.RelicPool:
                return $"유물 획득: {(reward.targetData != null ? reward.targetData.name : reward.rewardId)}";
            case PopupEventRewardType.StrategicSkillItem:
            case PopupEventRewardType.StrategicSkillItemPool:
                return $"아이템 획득: {(reward.targetData != null ? reward.targetData.name : reward.rewardId)}";
            case PopupEventRewardType.Blessing:
            case PopupEventRewardType.BlessingPool:
                return $"축복 획득: {(reward.targetData != null ? reward.targetData.name : reward.rewardId)}";
            default:
                return !string.IsNullOrEmpty(reward.rewardId) ? $"보상 획득: {reward.rewardId}" : string.Empty;
        }
    }
}
