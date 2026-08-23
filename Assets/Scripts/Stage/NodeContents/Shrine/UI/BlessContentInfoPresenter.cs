using System.Collections.Generic;
using Bless;
using Presentation;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Shrine.UI
{
    public sealed class BlessContentInfoPresenter : UIComponent
    {
        [Header("View")]
        [AutoBind("BlessContentInfoView")]
        [SerializeField] private UIContentInfoView contentView;

        [Header("Bless List")]
        [SerializeField] private List<BlessSO> blessings = new();
        [SerializeField, Min(0)] private int initialSelectedIndex;

        [Header("Tabs")]
        [AutoBind("BlessContentInfoTabRoot")]
        [SerializeField] private RectTransform tabRoot;
        [SerializeField] private UISelectableIconButton tabPrefab;

        [Header("Build")]
        [SerializeField] private bool buildOnStart = true;

        private readonly List<UISelectableIconButton> spawnedTabs = new();
        private readonly List<BlessSO> availableBlesses = new();

        public IReadOnlyList<BlessSO> Blessings => blessings;
        public BlessSO Bless => SelectedBless;
        public BlessSO SelectedBless { get; private set; }
        public int SpawnedTabCount => spawnedTabs.Count;

        private void Start()
        {
            if (buildOnStart)
            {
                BuildBlessTabs();
            }
        }

        [ContextMenu("Build Configured Blesses")]
        public void BuildBlessTabs()
        {
            if (!CanBuildPresentation())
            {
                return;
            }

            ClearBlessTabs();

            if (tabRoot == null || tabPrefab == null)
            {
                Debug.LogError(
                    "[BlessContentInfoPresenter] BlessContentInfoTabRoot or tab prefab is not assigned.",
                    this);
                ClearPresentation();
                return;
            }

            for (int index = 0; index < blessings.Count; index++)
            {
                BlessSO candidate = blessings[index];
                if (candidate == null)
                {
                    Debug.LogWarning(
                        $"[BlessContentInfoPresenter] Ignored null BlessSO at index {index}.",
                        this);
                    continue;
                }

                UISelectableIconButton tab = Instantiate(tabPrefab, tabRoot);
                tab.name = $"BlessContentInfoTab_{candidate.name}";
                tab.SetIcon(candidate.Icon);
                tab.SetLocked(false);
                tab.Bind(() => SelectBless(tab, candidate));
                spawnedTabs.Add(tab);
                availableBlesses.Add(candidate);
            }

            if (spawnedTabs.Count == 0)
            {
                ClearPresentation();
                return;
            }

            SelectBless(Mathf.Clamp(initialSelectedIndex, 0, spawnedTabs.Count - 1));
        }

        [ContextMenu("Build Selected Bless Presentation")]
        public void BuildPresentation()
        {
            if (!CanBuildPresentation())
            {
                return;
            }

            if (SelectedBless == null)
            {
                Debug.LogError(
                    "[BlessContentInfoPresenter] No BlessSO is selected.",
                    this);
                ClearPresentation();
                return;
            }

            BindResolvedBless(SelectedBless);
        }

        public void SetBless(BlessSO value, bool rebuild = true)
        {
            SelectedBless = value;

            if (rebuild && Application.isPlaying)
            {
                BuildPresentation();
            }
        }

        public void ShowBless(BlessSO value)
        {
            SetBless(value);
        }

        public void ShowBless(BlessRuntimeData.BlessEntry runtime)
        {
            SelectedBless = runtime?.source;
            if (runtime == null)
            {
                ClearPresentation();
                return;
            }

            if (!CanBuildPresentation())
            {
                return;
            }

            ContentPresentationData content =
                new BlessPresentationResolver().ResolveForPlayerDisplay(
                    runtime,
                    PresentationContext.Runtime);
            Bind(content);
        }

        public void SetBlesses(
            IEnumerable<BlessSO> values,
            int selectedIndex = 0,
            bool rebuild = true)
        {
            blessings.Clear();
            if (values != null)
            {
                blessings.AddRange(values);
            }

            initialSelectedIndex = Mathf.Max(0, selectedIndex);
            SelectedBless = null;

            if (rebuild && Application.isPlaying)
            {
                BuildBlessTabs();
            }
        }

        public void SelectBless(int index)
        {
            if (index < 0 || index >= spawnedTabs.Count)
            {
                Debug.LogWarning(
                    $"[BlessContentInfoPresenter] Bless tab index is out of range: {index}.",
                    this);
                return;
            }

            SelectBless(spawnedTabs[index], availableBlesses[index]);
        }

        public void ClearBlessTabs()
        {
            SelectedBless = null;

            for (int index = spawnedTabs.Count - 1; index >= 0; index--)
            {
                UISelectableIconButton tab = spawnedTabs[index];
                if (tab == null)
                {
                    continue;
                }

                tab.gameObject.SetActive(false);
                Destroy(tab.gameObject);
            }

            spawnedTabs.Clear();
            availableBlesses.Clear();
        }

        public void ClearPresentation()
        {
            if (contentView != null)
            {
                contentView.Bind(null);
            }
        }

        private bool CanBuildPresentation()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning(
                    "[BlessContentInfoPresenter] Enter Play Mode before building presentation data.",
                    this);
                return false;
            }

            if (contentView == null)
            {
                Debug.LogError(
                    "[BlessContentInfoPresenter] BlessContentInfoView is not assigned.",
                    this);
                return false;
            }

            if (EventSystem.current == null)
            {
                Debug.LogWarning(
                    "[BlessContentInfoPresenter] No active EventSystem was found. " +
                    "The content can be displayed, but ScrollRect input will not work.",
                    this);
            }

            return true;
        }

        private void Bind(ContentPresentationData content)
        {
            contentView.SetFormatter(
                PresentationTextFormatter.CreatePlayerFormatter(
                    PresentationLocalizedTextResolver.ResolveLabel));
            contentView.Bind(content);
        }

        private void SelectBless(
            UISelectableIconButton selectedTab,
            BlessSO selectedBless)
        {
            if (selectedTab == null || selectedBless == null)
            {
                return;
            }

            for (int index = 0; index < spawnedTabs.Count; index++)
            {
                UISelectableIconButton tab = spawnedTabs[index];
                if (tab == null)
                {
                    continue;
                }

                bool selected = tab == selectedTab;
                tab.SetSelected(selected);
                tab.SetInteractable(!selected);
            }

            SelectedBless = selectedBless;
            BindResolvedBless(SelectedBless);
        }

        private void BindResolvedBless(BlessSO value)
        {
            ContentPresentationData content =
                new BlessPresentationResolver().ResolveForPlayerDisplay(
                    value,
                    PresentationContext.Preview);
            Bind(content);
        }
    }
}
