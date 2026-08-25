using System;
using System.Collections.Generic;
using Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shrine.UI
{
    [AutoBindPrefix("Faith")]
    public sealed class FaithPageView : UIView
    {
        [Header("Header")]
        [AutoBind] [SerializeField] private TMP_Text titleText;
        [AutoBind] [SerializeField] private Button closeButton;

        [Header("Faith Tabs")]
        [AutoBind] [SerializeField] private ScrollRect godTabScrollRect;
        [AutoBind] [SerializeField] private RectTransform godTabRoot;
        [AutoBind] [SerializeField] private FaithGodTabView godTabTemplate;

        [Header("Selected Faith")]
        [AutoBind] [SerializeField] private RectTransform selectedGodPage;
        [AutoBind] [SerializeField] private Image godIconImage;
        [AutoBind] [SerializeField] private TMP_Text godNameText;
        [AutoBind] [SerializeField] private TMP_Text godDescriptionText;
        [AutoBind] [SerializeField] private TMP_Text levelText;
        [AutoBind] [SerializeField] private Slider levelProgressSlider;
        [AutoBind] [SerializeField] private TMP_Text affinityText;
        [AutoBind] [SerializeField] private TMP_Text stateText;
        [AutoBind] [SerializeField] private TMP_Text emptyStateText;

        [Header("Roadmap")]
        [AutoBind] [SerializeField] private ScrollRect roadmapScrollRect;
        [AutoBind] [SerializeField] private RectTransform levelNodeRoot;
        [SerializeField] private List<FaithLevelNodeView> levelNodes = new();

        [Header("Level Comparison")]
        [AutoBind] [SerializeField] private FaithLevelEffectCardView currentLevelEffectCard;
        [AutoBind] [SerializeField] private FaithLevelEffectCardView nextLevelEffectCard;

        private readonly List<FaithGodTabView> spawnedGodTabs = new();

        public event Action CloseRequested;

        public IReadOnlyList<FaithLevelNodeView> LevelNodes => levelNodes;
        public int SpawnedGodTabCount => spawnedGodTabs.Count;

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(HandleCloseClicked);
            }
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(HandleCloseClicked);
            }
        }

        public override void ClearCallbacks()
        {
            CloseRequested = null;
        }

        public void SetTitle(string localizedTitle)
        {
            if (titleText != null)
            {
                titleText.text = localizedTitle ?? string.Empty;
            }
        }

        public void BuildGodTabs(
            IReadOnlyList<FaithGodTabItemData> items,
            Action<int> selectedCallback)
        {
            ClearGodTabs();

            if (items == null
                || godTabRoot == null
                || godTabTemplate == null)
            {
                return;
            }

            for (int index = 0; index < items.Count; index++)
            {
                FaithGodTabItemData item = items[index];
                if (item == null)
                {
                    continue;
                }

                int selectedIndex = index;
                FaithGodTabView tab = Instantiate(
                    godTabTemplate,
                    godTabRoot);
                tab.gameObject.name = $"Faith_GodTab_{index + 1:00}";
                tab.gameObject.SetActive(true);
                tab.Bind(
                    item.Icon,
                    item.DisplayName,
                    item.LevelLabel,
                    item.Selected,
                    item.Locked,
                    item.Active,
                    () => selectedCallback?.Invoke(selectedIndex));
                spawnedGodTabs.Add(tab);
            }

            RefreshScroll(godTabScrollRect, godTabRoot);
        }

        public void BindGodSummary(
            Sprite icon,
            string displayName,
            string description,
            string levelLabel,
            float? progress,
            string affinityLabel,
            string stateLabel)
        {
            SetSelectedGodVisible(true);

            if (godIconImage != null)
            {
                godIconImage.sprite = icon;
                godIconImage.enabled = icon != null;
            }

            SetText(godNameText, displayName);
            SetText(godDescriptionText, description);
            SetText(levelText, levelLabel);
            SetText(affinityText, affinityLabel);
            SetText(stateText, stateLabel);

            if (levelProgressSlider != null)
            {
                levelProgressSlider.gameObject.SetActive(progress.HasValue);
                if (progress.HasValue)
                {
                    levelProgressSlider.normalizedValue =
                        Mathf.Clamp01(progress.Value);
                }
            }
        }

        public void BindRoadmap(
            int currentLevel,
            Action<int> milestoneSelected = null)
        {
            for (int index = 0; index < levelNodes.Count; index++)
            {
                FaithLevelNodeView node = levelNodes[index];
                if (node == null)
                {
                    continue;
                }

                node.Bind(index + 1, currentLevel, milestoneSelected);
            }

            RefreshScroll(roadmapScrollRect, levelNodeRoot);
        }

        public void BindLevelComparison(
            int currentLevel,
            ContentPresentationData currentContent,
            ContentPresentationData nextContent)
        {
            if (currentLevelEffectCard != null)
            {
                currentLevelEffectCard.Bind(
                    currentLevel,
                    "presentation.faith.current_level_effects",
                    currentContent);
            }

            if (nextLevelEffectCard == null)
            {
                return;
            }

            if (currentLevel >= 10)
            {
                nextLevelEffectCard.BindNoNextLevel(currentLevel);
                return;
            }

            nextLevelEffectCard.Bind(
                currentLevel + 1,
                "presentation.faith.next_level_effects",
                nextContent);
        }

        public void ShowEmpty(string message)
        {
            ClearGodTabs();
            SetSelectedGodVisible(false);

            if (emptyStateText != null)
            {
                emptyStateText.gameObject.SetActive(true);
                emptyStateText.text = message ?? string.Empty;
            }
        }

        public void SetLevelNodes(IEnumerable<FaithLevelNodeView> nodes)
        {
            levelNodes.Clear();
            if (nodes == null)
            {
                return;
            }

            foreach (FaithLevelNodeView node in nodes)
            {
                if (node != null)
                {
                    levelNodes.Add(node);
                }
            }
        }

        private void ClearGodTabs()
        {
            for (int index = spawnedGodTabs.Count - 1; index >= 0; index--)
            {
                FaithGodTabView tab = spawnedGodTabs[index];
                if (tab == null)
                {
                    continue;
                }

                tab.gameObject.SetActive(false);
                if (Application.isPlaying)
                {
                    Destroy(tab.gameObject);
                }
                else
                {
                    DestroyImmediate(tab.gameObject);
                }
            }

            spawnedGodTabs.Clear();
        }

        private void SetSelectedGodVisible(bool visible)
        {
            if (selectedGodPage != null)
            {
                selectedGodPage.gameObject.SetActive(visible);
            }

            if (emptyStateText != null)
            {
                emptyStateText.gameObject.SetActive(!visible);
            }
        }

        private void HandleCloseClicked()
        {
            CloseRequested?.Invoke();
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value ?? string.Empty;
            }
        }

        private static void RefreshScroll(
            ScrollRect scrollRect,
            RectTransform content)
        {
            if (content != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            }

            Canvas.ForceUpdateCanvases();
            if (scrollRect == null)
            {
                return;
            }

            scrollRect.StopMovement();
            scrollRect.horizontalNormalizedPosition = 0f;
        }
    }

    public sealed class FaithGodTabItemData
    {
        public Sprite Icon { get; }
        public string DisplayName { get; }
        public string LevelLabel { get; }
        public bool Selected { get; }
        public bool Locked { get; }
        public bool Active { get; }

        public FaithGodTabItemData(
            Sprite icon,
            string displayName,
            string levelLabel,
            bool selected,
            bool locked,
            bool active)
        {
            Icon = icon;
            DisplayName = displayName ?? string.Empty;
            LevelLabel = levelLabel ?? string.Empty;
            Selected = selected;
            Locked = locked;
            Active = active;
        }
    }
}
