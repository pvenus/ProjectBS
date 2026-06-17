using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Item;
using Common;
using Common.SO;

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

            int count = Mathf.Min(choiceButtons.Count, popupEvent.choices.Count);
            for (int i = 0; i < count; i++)
            {
                int choiceIndex = i;
                PopupEventChoice choice = popupEvent.choices[i];
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

                    buttonText.text = ShouldUseNextLabel(choice, label)
                        ? "다음"
                        : label;
                }
            }
        }

        private static bool ShouldUseNextLabel(PopupEventChoice choice, string label)
        {
            if (choice == null || string.IsNullOrWhiteSpace(label))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(choice.choiceId)
                && choice.choiceId.EndsWith(".next"))
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