using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
                titleText.text = popupEvent.title;
            }

            if (bodyText != null)
            {
                bodyText.text = popupEvent.body;
            }

            if (resultText != null)
            {
                resultText.text = string.Empty;
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
                    buttonText.text = string.IsNullOrWhiteSpace(choice.label)
                        ? $"Choice {i + 1}"
                        : choice.label;
                }
            }
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
            if (resultText != null && choice != null)
            {
                resultText.text = choice.resultText;
            }

            if (closeOnChoiceSelected)
            {
                Hide();
            }
        }

        private void HandlePopupEventClosed(PopupEventSO popupEvent, RoundNode node)
        {
            Hide();
        }
    }
}