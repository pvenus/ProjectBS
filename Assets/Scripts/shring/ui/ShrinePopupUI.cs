using System.Collections.Generic;
using TMPro;
using Bless;
using UnityEngine;
using UnityEngine.UI;

namespace Shrine.UI
{
    /// <summary>
    /// 신전 팝업 메인 UI.
    /// ShrineManager 이벤트를 구독하고 현재 신전 상태에 따라 화면을 갱신한다.
    /// </summary>
    public class ShrinePopupUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ShrineManager shrineManager;

        [Header("Root")]
        [SerializeField] private GameObject panelRoot;

        [Header("Header")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private TMP_Text flowStateText;

        [Header("Main Buttons")]
        [SerializeField] private Button healAndBlessButton;
        [SerializeField] private Button enterFaithButton;
        [SerializeField] private Button leaveButton;

        [Header("God Selection")]
        [SerializeField] private GameObject godSelectionRoot;
        [SerializeField] private RectTransform godContentRoot;
        [SerializeField] private ShrineGodButtonUI godButtonPrefab;

        [Header("Faith Action")]
        [SerializeField] private GameObject faithActionRoot;
        [SerializeField] private Button prayActionButton;
        [SerializeField] private Button donateActionButton;

        [Header("Blessing Selection")]
        [SerializeField] private GameObject blessingSelectionRoot;
        [SerializeField] private RectTransform blessingContentRoot;
        [SerializeField] private ShrineBlessingButtonUI blessingButtonPrefab;

        [Header("Options")]
        [SerializeField] private bool hideOnAwake = true;

        private readonly List<ShrineGodButtonUI> spawnedGodButtons = new();
        private readonly List<ShrineBlessingButtonUI> spawnedBlessingButtons = new();

        private void Awake()
        {
            if (shrineManager == null)
            {
                shrineManager = ShrineManager.Instance;
            }

            BindButtons();

            if (hideOnAwake)
            {
                Hide();
            }
        }

        private void OnEnable()
        {
            if (shrineManager == null)
            {
                shrineManager = ShrineManager.Instance;
            }

            Subscribe();

            if (shrineManager != null && shrineManager.IsOpened)
            {
                Show();
            }
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            UnbindButtons();
        }

        public void Show()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            Refresh();
        }

        public void Hide()
        {
            ClearGodButtons();
            ClearBlessingButtons();

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        public void Refresh()
        {
            if (shrineManager == null || !shrineManager.IsOpened)
            {
                Hide();
                return;
            }

            ShrineRuntimeData shrine = shrineManager.CurrentShrine;
            if (shrine == null)
            {
                Hide();
                return;
            }

            if (panelRoot != null && !panelRoot.activeSelf)
            {
                panelRoot.SetActive(true);
            }

            RefreshHeader(shrine);
            RefreshMainButtons(shrine);
            RefreshGodSelection(shrine);
            RefreshFaithActionSelection(shrine);
            RefreshBlessingSelection(shrine);
        }

        private void RefreshHeader(ShrineRuntimeData shrine)
        {
            if (titleText != null)
            {
                titleText.text = string.IsNullOrWhiteSpace(shrine.shrineName)
                    ? "Shrine"
                    : shrine.shrineName;
            }

            if (descriptionText != null)
            {
                descriptionText.text = shrine.selectedAction == ShrineActionType.None
                    ? "Choose your action."
                    : shrine.selectedAction.ToString();
            }

            if (goldText != null)
            {
                goldText.text = $"Gold : {shrineManager.CurrentGold}";
            }

            if (hpText != null)
            {
                hpText.text = $"HP : {shrineManager.PartyCurrentHp} / {shrineManager.PartyMaxHp}";
            }

            if (flowStateText != null)
            {
                flowStateText.text = shrine.flowState.ToString();
            }
        }

        private void RefreshMainButtons(ShrineRuntimeData shrine)
        {
            bool isMainSelection = shrine.flowState == ShrineFlowState.MainSelection;

            if (healAndBlessButton != null)
            {
                healAndBlessButton.gameObject.SetActive(isMainSelection);
            }

            if (enterFaithButton != null)
            {
                enterFaithButton.gameObject.SetActive(isMainSelection);
            }

            if (leaveButton != null)
            {
                leaveButton.gameObject.SetActive(false);
            }
        }

        private void RefreshGodSelection(ShrineRuntimeData shrine)
        {
            bool visible = shrine.flowState == ShrineFlowState.GodSelection;

            if (godSelectionRoot != null)
            {
                godSelectionRoot.SetActive(visible);
            }

            if (!visible)
            {
                return;
            }

            BuildGodButtons(shrine);
        }

        private void RefreshFaithActionSelection(ShrineRuntimeData shrine)
        {
            bool visible = shrine.flowState == ShrineFlowState.FaithActionSelection;

            if (faithActionRoot != null)
            {
                faithActionRoot.SetActive(visible);
            }

            if (!visible)
            {
                return;
            }

            bool hasSelectedGod = shrine.HasSelectedGod;

            if (prayActionButton != null)
            {
                prayActionButton.interactable = hasSelectedGod;
            }

            if (donateActionButton != null)
            {
                donateActionButton.interactable = hasSelectedGod;
            }
        }

        private void RefreshBlessingSelection(ShrineRuntimeData shrine)
        {
            bool visible = shrine.flowState == ShrineFlowState.BlessingSelection;

            if (blessingSelectionRoot != null)
            {
                blessingSelectionRoot.SetActive(visible);
            }

            if (!visible)
            {
                return;
            }

            BuildBlessingButtons(shrine);
        }

        private void BuildGodButtons(ShrineRuntimeData shrine)
        {
            ClearGodButtons();

            if (godContentRoot == null || godButtonPrefab == null)
            {
                return;
            }

            const float spacing = 260f;
            const float startY = -40f;

            for (int i = 0; i < shrine.availableGods.Count; i++)
            {
                ShrineGodType godType = shrine.availableGods[i];

                ShrineGodSO god = shrineManager.GetGodSO(godType);
                if (god == null)
                {
                    continue;
                }

                ShrineGodButtonUI button = Instantiate(godButtonPrefab, godContentRoot);
                button.Bind(god, shrineManager);

                RectTransform rectTransform = button.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchorMin = new Vector2(0.5f, 1f);
                    rectTransform.anchorMax = new Vector2(0.5f, 1f);
                    rectTransform.pivot = new Vector2(0.5f, 1f);
                    rectTransform.anchoredPosition = new Vector2(0f, startY - (spacing * i));
                }

                spawnedGodButtons.Add(button);
            }
        }

        private void BuildBlessingButtons(ShrineRuntimeData shrine)
        {
            ClearBlessingButtons();

            if (blessingContentRoot == null || blessingButtonPrefab == null)
            {
                return;
            }

            const float spacing = 260f;
            const float startY = -40f;

            for (int i = 0; i < shrine.blessingCandidates.Count; i++)
            {
                BlessRuntimeData.BlessEntry blessing = shrine.blessingCandidates[i];
                if (blessing == null)
                {
                    continue;
                }

                ShrineBlessingButtonUI button = Instantiate(blessingButtonPrefab, blessingContentRoot);
                button.Bind(blessing, shrineManager);

                RectTransform rectTransform = button.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchorMin = new Vector2(0.5f, 1f);
                    rectTransform.anchorMax = new Vector2(0.5f, 1f);
                    rectTransform.pivot = new Vector2(0.5f, 1f);
                    rectTransform.anchoredPosition = new Vector2(0f, startY - (spacing * i));
                }

                spawnedBlessingButtons.Add(button);
            }
        }

        private void ClearGodButtons()
        {
            foreach (ShrineGodButtonUI button in spawnedGodButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }

            spawnedGodButtons.Clear();
        }

        private void ClearBlessingButtons()
        {
            foreach (ShrineBlessingButtonUI button in spawnedBlessingButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }

            spawnedBlessingButtons.Clear();
        }

        private void Subscribe()
        {
            if (shrineManager == null)
            {
                return;
            }

            shrineManager.OnShrineOpened -= HandleShrineOpened;
            shrineManager.OnShrineRefreshed -= HandleShrineRefreshed;
            shrineManager.OnShrineCompleted -= HandleShrineCompleted;

            shrineManager.OnShrineOpened += HandleShrineOpened;
            shrineManager.OnShrineRefreshed += HandleShrineRefreshed;
            shrineManager.OnShrineCompleted += HandleShrineCompleted;
        }

        private void Unsubscribe()
        {
            if (shrineManager == null)
            {
                return;
            }

            shrineManager.OnShrineOpened -= HandleShrineOpened;
            shrineManager.OnShrineRefreshed -= HandleShrineRefreshed;
            shrineManager.OnShrineCompleted -= HandleShrineCompleted;
        }

        private void BindButtons()
        {
            BindButton(healAndBlessButton, HandleHealAndBless);
            BindButton(enterFaithButton, HandlePray);
            BindButton(prayActionButton, HandleConfirmPray);
            BindButton(donateActionButton, HandleConfirmDonate);
            BindButton(leaveButton, HandleLeave);
        }

        private void UnbindButtons()
        {
            UnbindButton(healAndBlessButton, HandleHealAndBless);
            UnbindButton(enterFaithButton, HandlePray);
            UnbindButton(prayActionButton, HandleConfirmPray);
            UnbindButton(donateActionButton, HandleConfirmDonate);
            UnbindButton(leaveButton, HandleLeave);
        }

        private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }

        private static void UnbindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(action);
        }

        private void HandleShrineOpened(ShrineRuntimeData shrine)
        {
            Show();
        }

        private void HandleShrineRefreshed(ShrineRuntimeData shrine)
        {
            Refresh();
        }

        private void HandleShrineCompleted(ShrineRuntimeData shrine)
        {
            Hide();
        }

        private void HandleHealAndBless()
        {
            shrineManager?.SelectHealAndBless();
        }

        private void HandlePray()
        {
            shrineManager?.SelectPray();
        }

        private void HandleDonate()
        {
            shrineManager?.SelectDonate();
        }

        private void HandleLeave()
        {
            shrineManager?.CompleteShrine();
        }

        private void HandleConfirmPray()
        {
            shrineManager?.ConfirmPray();
        }

        private void HandleConfirmDonate()
        {
            shrineManager?.ConfirmDonate();
        }
    }
}
