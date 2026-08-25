using System.Collections.Generic;
using Presentation;
using UnityEngine;

namespace Shrine.UI
{
    public sealed class FaithPagePresenter : UIComponent
    {
        [Header("View")]
        [SerializeField] private FaithPageView pageView;

        [Header("Configured Preview Sources")]
        [SerializeField] private List<ShrineGodSO> configuredGods = new();
        [SerializeField, Range(1, 10)] private int configuredFaithLevel = 1;

        [Header("Build")]
        [SerializeField] private bool buildOnStart;

        private IReadOnlyList<ShrineGodSO> activeGods;
        private FaithRuntimeData runtimeData;
        private int selectedGodIndex;

        public IReadOnlyList<ShrineGodSO> ConfiguredGods => configuredGods;
        public ShrineGodSO SelectedGod { get; private set; }

        private void Start()
        {
            if (buildOnStart)
            {
                BuildConfiguredFaithPage();
            }
        }

        [ContextMenu("Build Configured Faith Page")]
        public void BuildConfiguredFaithPage()
        {
            ShowFaiths(configuredGods, null);
        }

        public void ShowFaiths(
            IReadOnlyList<ShrineGodSO> gods,
            FaithRuntimeData faithRuntimeData)
        {
            runtimeData = faithRuntimeData;
            activeGods = BuildAvailableGods(gods, faithRuntimeData);
            selectedGodIndex = ResolveInitialGodIndex(
                activeGods,
                faithRuntimeData);
            RebuildPage();
        }

        public void SelectGod(int index)
        {
            if (activeGods == null
                || index < 0
                || index >= activeGods.Count
                || activeGods[index] == null)
            {
                return;
            }

            selectedGodIndex = index;
            RebuildPage();
        }

        public void SetPageView(FaithPageView value)
        {
            pageView = value;
        }

        private void RebuildPage()
        {
            if (pageView == null)
            {
                Debug.LogError(
                    "[FaithPagePresenter] FaithPageView is not assigned.",
                    this);
                return;
            }

            pageView.SetTitle(ResolveLabel("presentation.faith.page_title"));

            List<ShrineGodSO> availableGods = BuildAvailableGods(
                activeGods,
                runtimeData);
            if (availableGods.Count == 0)
            {
                SelectedGod = null;
                pageView.ShowEmpty(
                    ResolveLabel("presentation.faith.empty"));
                return;
            }

            selectedGodIndex = Mathf.Clamp(
                selectedGodIndex,
                0,
                availableGods.Count - 1);
            SelectedGod = availableGods[selectedGodIndex];

            List<FaithGodTabItemData> tabs = new();
            for (int index = 0; index < availableGods.Count; index++)
            {
                ShrineGodSO god = availableGods[index];
                int level = ResolveFaithLevel(god);
                bool locked = runtimeData != null
                    && runtimeData.HasLockedFaith
                    && runtimeData.lockedGod == god.GodType;
                bool active = runtimeData == null
                    || !runtimeData.HasLockedFaith
                    || locked;

                tabs.Add(new FaithGodTabItemData(
                    god.Icon,
                    ResolveRequired(god.LocalizationMainKey, "name"),
                    FormatLevel(level),
                    index == selectedGodIndex,
                    locked,
                    active));
            }

            pageView.BuildGodTabs(tabs, SelectGod);

            int currentLevel = ResolveFaithLevel(SelectedGod);
            pageView.BindGodSummary(
                SelectedGod.Icon,
                ResolveRequired(SelectedGod.LocalizationMainKey, "name"),
                ResolveRequired(SelectedGod.LocalizationMainKey, "desc"),
                FormatLevel(currentLevel),
                null,
                string.Empty,
                ResolveFaithStateLabel(SelectedGod));
            pageView.BindRoadmap(currentLevel, HandleMilestoneSelected);

            // [PLACEHOLDER] The source-backed Faith progression resolver will
            // provide the current and next ContentPresentationData objects.
            pageView.BindLevelComparison(
                currentLevel,
                null,
                null);
            Debug.Log(
                "[PLACEHOLDER] FaithPagePresenter comparison data resolver is pending; "
                + "the prefab, tabs, summary, roadmap, and two effect-card call sites are ready.",
                this);
        }

        private int ResolveFaithLevel(ShrineGodSO god)
        {
            if (god == null)
            {
                return 0;
            }

            return runtimeData != null
                ? runtimeData.GetFaithLevel(god.GodType)
                : configuredFaithLevel;
        }

        private string ResolveFaithStateLabel(ShrineGodSO god)
        {
            if (god == null || runtimeData == null)
            {
                return string.Empty;
            }

            if (runtimeData.HasLockedFaith
                && runtimeData.lockedGod == god.GodType)
            {
                return ResolveLabel("presentation.faith.state.locked");
            }

            if (runtimeData.HasLockedFaith)
            {
                return ResolveLabel("presentation.faith.state.inactive");
            }

            return ResolveLabel("presentation.faith.state.active");
        }

        private void HandleMilestoneSelected(int level)
        {
            Debug.Log(
                $"[PLACEHOLDER] Faith roadmap milestone selected: level={level}. "
                + "The bottom cards remain actual current/next comparison.",
                this);
        }

        private static List<ShrineGodSO> BuildAvailableGods(
            IReadOnlyList<ShrineGodSO> gods,
            FaithRuntimeData runtime)
        {
            List<ShrineGodSO> result = new();
            if (gods == null)
            {
                return result;
            }

            for (int index = 0; index < gods.Count; index++)
            {
                ShrineGodSO god = gods[index];
                if (god == null
                    || result.Contains(god)
                    || runtime != null
                    && runtime.GetFaithLevel(god.GodType) <= 0)
                {
                    continue;
                }

                result.Add(god);
            }

            return result;
        }

        private static int ResolveInitialGodIndex(
            IReadOnlyList<ShrineGodSO> gods,
            FaithRuntimeData runtime)
        {
            if (gods == null || gods.Count == 0)
            {
                return 0;
            }

            int highestIndex = 0;
            int highestLevel = -1;
            for (int index = 0; index < gods.Count; index++)
            {
                ShrineGodSO god = gods[index];
                if (god == null)
                {
                    continue;
                }

                if (runtime != null
                    && runtime.HasLockedFaith
                    && runtime.lockedGod == god.GodType)
                {
                    return index;
                }

                int level = runtime != null
                    ? runtime.GetFaithLevel(god.GodType)
                    : 0;
                if (level > highestLevel)
                {
                    highestLevel = level;
                    highestIndex = index;
                }
            }

            return highestIndex;
        }

        private static string FormatLevel(int level)
        {
            string format = ResolveLabel("presentation.faith.level_format");
            return format.Contains("{0}")
                ? string.Format(format, level)
                : $"{format} {level}";
        }

        private static string ResolveRequired(
            string mainKey,
            string subKey)
        {
            string resolved =
                PresentationLocalizedTextResolver.ResolveRequired(
                    subKey,
                    mainKey);
            return string.IsNullOrWhiteSpace(resolved)
                ? $"{mainKey}.{subKey}"
                : resolved;
        }

        private static string ResolveLabel(string localizationKey)
        {
            string resolved =
                PresentationLocalizedTextResolver.ResolveLabel(localizationKey);
            return string.IsNullOrWhiteSpace(resolved)
                ? localizationKey ?? string.Empty
                : resolved;
        }
    }
}
