using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Item;
using Common;
using Common.SO;
using Character;

namespace Stage.UI
{
    /// <summary>
    /// StagePopupEventManager 이벤트를 받아 팝업 이벤트 내용을 화면에 표시한다.
    /// 선택지 버튼은 최대 choiceButtons 개수만큼 사용한다.
    /// </summary>
    public class PopupEventPanelUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StagePopupEventManager popupEventManager;

        [Header("Root")]
        [SerializeField] private GameObject panelRoot;

        [Header("Content")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Image mainImage;
        [SerializeField] private Image iconImage;

        [Header("Choices")]
        [SerializeField] private List<Button> choiceButtons = new();
        [SerializeField] private List<TMP_Text> choiceButtonTexts = new();

        [Header("Result")]
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private TMP_Text confirmButtonText;

        [Header("Rewards")]
        [SerializeField] private List<Image> rewardIcons = new();
        [SerializeField] private List<TMP_Text> rewardTexts = new();

        [Header("Options")]
        [SerializeField] private bool hideOnAwake = true;
        [SerializeField] private bool closeOnChoiceSelected = false;

        private PopupEventSO currentEvent;
        private RoundNode currentNode;

        private void Awake()
        {
            if (popupEventManager == null)
            {
                popupEventManager = StagePopupEventManager.Instance;
            }

            if (hideOnAwake)
            {
                Hide();
            }
        }

        private void OnEnable()
        {
            if (popupEventManager == null)
            {
                popupEventManager = StagePopupEventManager.Instance;
            }

            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Show(PopupEventSO popupEvent, RoundNode node)
        {
            currentEvent = popupEvent;
            currentNode = node;

            if (popupEvent == null)
            {
                Hide();
                return;
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[PopupEventPanelUI] panelRoot is not assigned. Assign a child panel root instead of enabling/disabling this component object.");
            }

            if (titleText != null)
            {
                string title = node != null
                    ? node.Title
                    : string.Empty;

                titleText.text = title;
                titleText.gameObject.SetActive(!string.IsNullOrWhiteSpace(title));
            }

            if (bodyText != null)
            {
                bodyText.text = popupEvent.Body;
            }

            if (resultText != null)
            {
                resultText.text = string.Empty;
                resultText.gameObject.SetActive(false);
            }

            ClearRewardViews();

            if (confirmButton != null)
            {
                confirmButton.gameObject.SetActive(false);
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(HandleConfirmButtonClicked);
            }

            if (confirmButtonText != null)
            {
                confirmButtonText.text = "확인";
            }

            if (mainImage != null)
            {
                mainImage.sprite = popupEvent.mainImage;
                mainImage.enabled = popupEvent.mainImage != null;
            }

            if (iconImage != null)
            {
                iconImage.sprite = popupEvent.icon;
                iconImage.enabled = popupEvent.icon != null;
            }

            RefreshChoiceButtons(popupEvent);
        }

        public void Hide()
        {
            currentEvent = null;
            currentNode = null;

            ClearChoiceButtons();
            ClearRewardViews();

            if (resultText != null)
            {
                resultText.text = string.Empty;
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.gameObject.SetActive(false);
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
                return;
            }

            Debug.LogWarning("[PopupEventPanelUI] panelRoot is not assigned. PopupEventPanelUI object will stay active so event subscription can work.");
        }

        public void CloseWithoutComplete()
        {
            if (popupEventManager != null)
            {
                popupEventManager.CloseWithoutComplete();
            }
            else
            {
                Hide();
            }
        }

        private void RefreshChoiceButtons(PopupEventSO popupEvent)
        {
            ClearChoiceButtons();

            if (popupEvent.choices == null || popupEvent.choices.Count == 0)
            {
                return;
            }

            List<int> visibleChoiceIndexes = GetVisibleChoiceIndexes(popupEvent);
            int count = Mathf.Min(choiceButtons.Count, visibleChoiceIndexes.Count);
            for (int i = 0; i < count; i++)
            {
                int choiceIndex = visibleChoiceIndexes[i];
                PopupEventChoice choice = popupEvent.choices[choiceIndex];
                Button button = choiceButtons[i];

                if (button == null)
                {
                    continue;
                }

                button.gameObject.SetActive(true);
                button.interactable = true;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SelectChoice(choiceIndex));

                TMP_Text buttonText = GetChoiceText(i, button);
                if (buttonText != null)
                {
                    string label = choice.Label;

                    buttonText.text = ShouldUseNextLabelFallback(choice, label)
                        ? "다음"
                        : label;
                }
            }
        }

        private List<int> GetVisibleChoiceIndexes(PopupEventSO popupEvent)
        {
            List<int> result = new();
            if (popupEvent == null || popupEvent.choices == null)
            {
                return result;
            }

            for (int i = 0; i < popupEvent.choices.Count; i++)
            {
                PopupEventChoice choice = popupEvent.choices[i];
                if (IsChoiceVisible(choice))
                {
                    result.Add(i);
                }
            }

            return result;
        }

        private bool IsChoiceVisible(PopupEventChoice choice)
        {
            if (choice == null)
            {
                return false;
            }

            if (choice.visibleConditions == null || choice.visibleConditions.Count == 0)
            {
                return true;
            }

            foreach (PopupEventChoiceConditionData condition in choice.visibleConditions)
            {
                if (!IsChoiceConditionSatisfied(condition))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsChoiceConditionSatisfied(PopupEventChoiceConditionData condition)
        {
            if (condition == null || condition.conditionType == PopupEventChoiceConditionType.None)
            {
                return true;
            }

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

        private bool HasCharacter(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return false;
            }

            Debug.LogWarning($"[PopupEventPanelUI] HasCharacter condition is not connected yet. characterId={characterId}");
            return false;
        }

        private bool HasCharacterJob(string jobText)
        {
            if (string.IsNullOrWhiteSpace(jobText))
            {
                return false;
            }

            if (!System.Enum.TryParse(jobText, out CharacterJob targetJob))
            {
                Debug.LogWarning($"[PopupEventPanelUI] Invalid CharacterJob condition. job={jobText}");
                return false;
            }

            return HasCharacterJob(targetJob);
        }

        private bool HasCharacterJob(CharacterJob targetJob)
        {
            CharacterManager[] managers = FindObjectsByType<CharacterManager>(FindObjectsSortMode.None);

            foreach (CharacterManager manager in managers)
            {
                var runtimeData = manager?.RuntimeData;
                if (runtimeData?.characterSO == null)
                {
                    continue;
                }

                if (runtimeData.characterSO.CharacterType != CharacterType.Player)
                {
                    continue;
                }

                if (runtimeData.characterSO.Job == targetJob)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasCharacterJobFamily(string familyText)
        {
            if (string.IsNullOrWhiteSpace(familyText))
            {
                return false;
            }

            if (!System.Enum.TryParse(familyText, out CharacterJobFamily targetFamily))
            {
                Debug.LogWarning($"[PopupEventPanelUI] Invalid CharacterJobFamily condition. family={familyText}");
                return false;
            }

            CharacterManager[] managers = FindObjectsByType<CharacterManager>(FindObjectsSortMode.None);

            foreach (CharacterManager manager in managers)
            {
                var runtimeData = manager?.RuntimeData;
                if (runtimeData?.characterSO == null)
                {
                    continue;
                }

                if (runtimeData.characterSO.CharacterType != CharacterType.Player)
                {
                    continue;
                }

                if (runtimeData.characterSO.JobFamily == targetFamily)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasCharacterJobTier(string tierText)
        {
            if (string.IsNullOrWhiteSpace(tierText))
            {
                return false;
            }

            if (!System.Enum.TryParse(tierText, out CharacterJobTier targetTier))
            {
                Debug.LogWarning($"[PopupEventPanelUI] Invalid CharacterJobTier condition. tier={tierText}");
                return false;
            }

            CharacterManager[] managers = FindObjectsByType<CharacterManager>(FindObjectsSortMode.None);

            foreach (CharacterManager manager in managers)
            {
                var runtimeData = manager?.RuntimeData;
                if (runtimeData?.characterSO == null)
                {
                    continue;
                }

                if (runtimeData.characterSO.CharacterType != CharacterType.Player)
                {
                    continue;
                }

                if (runtimeData.characterSO.JobTier == targetTier)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return false;
            }

            Debug.LogWarning($"[PopupEventPanelUI] HasTag condition is not connected yet. tag={tag}");
            return false;
        }

        private bool HasRelic(string targetId)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                return false;
            }

            Debug.LogWarning($"[PopupEventPanelUI] HasRelic condition is not connected yet. targetId={targetId}");
            return false;
        }

        private bool HasBless(string targetId)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                return false;
            }

            Debug.LogWarning($"[PopupEventPanelUI] HasBless condition is not connected yet. targetId={targetId}");
            return false;
        }

        private bool HasItem(string targetId)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                return false;
            }

            Debug.LogWarning($"[PopupEventPanelUI] HasItem condition is not connected yet. targetId={targetId}");
            return false;
        }

        private static bool ShouldUseNextLabelFallback(PopupEventChoice choice, string label)
        {
            if (choice == null || string.IsNullOrWhiteSpace(label))
            {
                return true;
            }

            return string.Equals(label, choice.choiceId + ".label");
        }

        private void ClearChoiceButtons()
        {
            for (int i = 0; i < choiceButtons.Count; i++)
            {
                Button button = choiceButtons[i];
                if (button == null)
                {
                    continue;
                }

                button.onClick.RemoveAllListeners();
                button.gameObject.SetActive(false);
            }
        }

        private TMP_Text GetChoiceText(int index, Button button)
        {
            if (index >= 0 && index < choiceButtonTexts.Count && choiceButtonTexts[index] != null)
            {
                return choiceButtonTexts[index];
            }

            return button.GetComponentInChildren<TMP_Text>();
        }

        private void SelectChoice(int choiceIndex)
        {
            if (popupEventManager == null)
            {
                Debug.LogWarning("[PopupEventPanelUI] SelectChoice failed. PopupEventManager is null.");
                return;
            }

            popupEventManager.SelectChoiceByIndex(choiceIndex);
        }

        private void Subscribe()
        {
            if (popupEventManager == null)
            {
                return;
            }

            popupEventManager.OnPopupEventOpened -= HandlePopupEventOpened;
            popupEventManager.OnPopupEventChoiceSelected -= HandlePopupEventChoiceSelected;
            popupEventManager.OnPopupEventClosed -= HandlePopupEventClosed;

            popupEventManager.OnPopupEventOpened += HandlePopupEventOpened;
            popupEventManager.OnPopupEventChoiceSelected += HandlePopupEventChoiceSelected;
            popupEventManager.OnPopupEventClosed += HandlePopupEventClosed;
        }

        private void Unsubscribe()
        {
            if (popupEventManager == null)
            {
                return;
            }

            popupEventManager.OnPopupEventOpened -= HandlePopupEventOpened;
            popupEventManager.OnPopupEventChoiceSelected -= HandlePopupEventChoiceSelected;
            popupEventManager.OnPopupEventClosed -= HandlePopupEventClosed;
        }

        private void HandlePopupEventOpened(PopupEventSO popupEvent, RoundNode node)
        {
            Show(popupEvent, node);
        }

        private void HandlePopupEventChoiceSelected(PopupEventSO popupEvent, PopupEventChoice choice, RoundNode node)
        {
            string result = choice != null
                ? choice.ResultText
                : null;

            bool hasResult = !string.IsNullOrWhiteSpace(result)
                && !string.Equals(result, (choice != null ? choice.choiceId : string.Empty) + ".result");

            bool hasRewards = choice != null
                && choice.rewards != null
                && choice.rewards.Count > 0;

            if (!hasResult && !hasRewards)
            {
                HandleConfirmButtonClicked();
                return;
            }

            if (hasResult)
            {
                if (bodyText != null)
                {
                    bodyText.text = result;
                }

                if (resultText != null)
                {
                    resultText.gameObject.SetActive(false);
                    resultText.text = string.Empty;
                }
            }

            RefreshRewardViews(choice);
            ClearChoiceButtons();

            if (confirmButton != null)
            {
                confirmButton.gameObject.SetActive(true);
                confirmButton.interactable = true;
            }

            if (closeOnChoiceSelected)
            {
                HandleConfirmButtonClicked();
            }
        }

        private void HandlePopupEventClosed(PopupEventSO popupEvent, RoundNode node)
        {
            Hide();
        }

        private void RefreshRewardViews(PopupEventChoice choice)
        {
            ClearRewardViews();

            if (choice == null
                || choice.rewards == null
                || choice.rewards.Count == 0)
            {
                return;
            }

            int count = Mathf.Min(rewardIcons.Count, choice.rewards.Count);

            for (int i = 0; i < count; i++)
            {
                PopupEventRewardData reward = choice.rewards[i];
                Image icon = rewardIcons[i];
                TMP_Text text = i < rewardTexts.Count
                    ? rewardTexts[i]
                    : null;

                if (icon != null)
                {
                    icon.gameObject.SetActive(true);
                    icon.sprite = GetRewardIcon(reward);
                    icon.enabled = icon.sprite != null;
                }

                if (text != null)
                {
                    text.gameObject.SetActive(true);
                    text.text = GetRewardText(reward);
                }
            }
        }

        private void ClearRewardViews()
        {
            foreach (Image icon in rewardIcons)
            {
                if (icon == null)
                {
                    continue;
                }

                icon.sprite = null;
                icon.enabled = false;
                icon.gameObject.SetActive(false);
            }

            foreach (TMP_Text text in rewardTexts)
            {
                if (text == null)
                {
                    continue;
                }

                text.text = string.Empty;
                text.gameObject.SetActive(false);
            }
        }

        private Sprite GetRewardIcon(PopupEventRewardData reward)
        {
            if (reward == null)
            {
                return null;
            }

            Sprite runtimeIcon = GetRuntimeRewardIcon(reward);
            if (runtimeIcon != null)
            {
                return runtimeIcon;
            }

            if (LibraryManager.Instance == null)
            {
                return null;
            }

            RewardVisualLibrarySO.RewardVisualEntry visual =
                LibraryManager.Instance.GetRewardVisual(reward.rewardType);

            return visual != null
                ? visual.icon
                : null;
        }

        private Sprite GetRuntimeRewardIcon(PopupEventRewardData reward)
        {
            switch (reward.targetData)
            {
                case RelicSO relic:
                    return relic.icon;

                case StrategicSkillItemSO strategicSkillItem:
                    return strategicSkillItem.icon;

                case AIFunctionSO function:
                    return function.icon;
            }

            return null;
        }

        private string GetRewardText(PopupEventRewardData reward)
        {
            if (reward == null)
            {
                return string.Empty;
            }

            switch (reward.rewardType)
            {
                case PopupEventRewardType.Gold:
                    return "+" + reward.value;

                case PopupEventRewardType.Hp:
                    return "+" + reward.value;

                case PopupEventRewardType.HpPercent:
                    return "+" + reward.value + "%";

                case PopupEventRewardType.Reputation:
                case PopupEventRewardType.Faith:
                    return reward.value >= 0
                        ? "+" + reward.value
                        : reward.value.ToString();
            }

            return string.Empty;
        }

        private void HandleConfirmButtonClicked()
        {
            if (popupEventManager != null)
            {
                popupEventManager.ConfirmChoiceResult();
            }
            else
            {
                Hide();
            }
        }
    }
}
