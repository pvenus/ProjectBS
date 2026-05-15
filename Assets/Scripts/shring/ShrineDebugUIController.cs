using System.Text;
using TMPro;
using UnityEngine;

namespace Shrine
{
    public class ShrineDebugUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ShrineManager shrineManager;

        [Header("Texts")]
        [SerializeField] private TMP_Text flowStateText;
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private TMP_Text selectedGodText;
        [SerializeField] private TMP_Text blessingListText;
        [SerializeField] private TMP_Text faithListText;

        [Header("Options")]
        [SerializeField] private bool autoFindManager = true;
        [SerializeField] private bool refreshEveryFrame;

        private void Awake()
        {
            if (autoFindManager && shrineManager == null)
            {
                shrineManager = FindFirstObjectByType<ShrineManager>();
            }
        }

        private void OnEnable()
        {
            SubscribeEvents();
            RefreshAll();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void Update()
        {
            if (!refreshEveryFrame)
            {
                return;
            }

            RefreshAll();
        }

        private void SubscribeEvents()
        {
            if (shrineManager == null)
            {
                return;
            }

            shrineManager.OnShrineOpened += HandleShrineChanged;
            shrineManager.OnShrineRefreshed += HandleShrineChanged;
            shrineManager.OnGoldChanged += HandleGoldChanged;
            shrineManager.OnPartyHpChanged += HandleHpChanged;
            shrineManager.OnFaithChanged += HandleFaithChanged;
            shrineManager.OnBlessingSelected += HandleBlessingSelected;
        }

        private void UnsubscribeEvents()
        {
            if (shrineManager == null)
            {
                return;
            }

            shrineManager.OnShrineOpened -= HandleShrineChanged;
            shrineManager.OnShrineRefreshed -= HandleShrineChanged;
            shrineManager.OnGoldChanged -= HandleGoldChanged;
            shrineManager.OnPartyHpChanged -= HandleHpChanged;
            shrineManager.OnFaithChanged -= HandleFaithChanged;
            shrineManager.OnBlessingSelected -= HandleBlessingSelected;
        }

        private void HandleShrineChanged(ShrineRuntimeData shrine)
        {
            RefreshAll();
        }

        private void HandleGoldChanged(int value)
        {
            RefreshGold();
        }

        private void HandleHpChanged(int currentHp, int maxHp)
        {
            RefreshHp();
        }

        private void HandleFaithChanged(ShrineGodType godType, int level)
        {
            RefreshFaithList();
        }

        private void HandleBlessingSelected(ShrineBlessingRuntime runtime)
        {
            RefreshBlessingList();
        }

        private void RefreshAll()
        {
            RefreshFlowState();
            RefreshGold();
            RefreshHp();
            RefreshSelectedGod();
            RefreshBlessingList();
            RefreshFaithList();
        }

        private void RefreshFlowState()
        {
            if (flowStateText == null)
            {
                return;
            }

            if (shrineManager == null || shrineManager.CurrentShrine == null)
            {
                flowStateText.text = "Flow : None";
                return;
            }

            flowStateText.text =
                $"Flow : {shrineManager.CurrentShrine.flowState}";
        }

        private void RefreshGold()
        {
            if (goldText == null || shrineManager == null)
            {
                return;
            }

            goldText.text =
                $"Gold : {shrineManager.CurrentGold}";
        }

        private void RefreshHp()
        {
            if (hpText == null || shrineManager == null)
            {
                return;
            }

            hpText.text =
                $"HP : {shrineManager.PartyCurrentHp} / {shrineManager.PartyMaxHp}";
        }

        private void RefreshSelectedGod()
        {
            if (selectedGodText == null)
            {
                return;
            }

            if (shrineManager == null || shrineManager.CurrentShrine == null)
            {
                selectedGodText.text = "Selected God : None";
                return;
            }

            selectedGodText.text =
                $"Selected God : {shrineManager.CurrentShrine.selectedGod}";
        }

        private void RefreshBlessingList()
        {
            if (blessingListText == null)
            {
                return;
            }

            if (shrineManager == null || shrineManager.PlayerRuntimeData == null)
            {
                blessingListText.text = "Blessings : None";
                return;
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Base Blessing");

            ShrineBlessingSO commonBlessing =
                shrineManager.PlayerRuntimeData.BaseBlessing;

            if (commonBlessing == null)
            {
                sb.AppendLine("- None");
            }
            else
            {
                sb.AppendLine($"- {commonBlessing.name}");
            }

            sb.AppendLine();
            sb.AppendLine("Enhanced Blessing");

            ShrineBlessingSO enhancedBlessing =
                shrineManager.PlayerRuntimeData.EnhancedBlessing;

            if (enhancedBlessing == null)
            {
                sb.AppendLine("- None");
            }
            else
            {
                sb.AppendLine($"- {enhancedBlessing.name}");
            }

            sb.AppendLine();
            sb.AppendLine("Active God Blessings");

            bool hasActiveBlessing = false;

            foreach (ShrineFaithEntry entry in shrineManager.PlayerRuntimeData.FaithEntries)
            {
                if (entry == null)
                {
                    continue;
                }

                ShrineGodSO god =
                    shrineManager.GetGodSO(entry.godType);

                if (god == null)
                {
                    continue;
                }

                ShrineBlessingSO activeBlessing =
                    shrineManager.PlayerRuntimeData.GetActiveGodBlessing(
                        entry.godType,
                        god.exclusiveBlessings);

                if (activeBlessing == null)
                {
                    continue;
                }

                hasActiveBlessing = true;

                sb.AppendLine(
                    $"- {entry.godType} : {activeBlessing.name}");
            }

            if (!hasActiveBlessing)
            {
                sb.AppendLine("- None");
            }

            blessingListText.text = sb.ToString();
        }

        private void RefreshFaithList()
        {
            if (faithListText == null)
            {
                return;
            }

            if (shrineManager == null || shrineManager.PlayerRuntimeData == null)
            {
                faithListText.text = "Faith : None";
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Faith Levels");

            var faithEntries = shrineManager.PlayerRuntimeData.FaithEntries;

            if (faithEntries.Count == 0)
            {
                sb.AppendLine("- None");
            }
            else
            {
                foreach (ShrineFaithEntry entry in faithEntries)
                {
                    if (entry == null)
                    {
                        continue;
                    }

                    sb.AppendLine($"- {entry.godType} : {entry.faithLevel}");
                }
            }

            if (shrineManager.PlayerRuntimeData.HasLockedFaith)
            {
                sb.AppendLine();
                sb.AppendLine(
                    $"Locked God : {shrineManager.PlayerRuntimeData.LockedGod}");
            }

            faithListText.text = sb.ToString();
        }
    }
}