using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Item;
using Mission;
using Bless;

namespace Shrine
{
    /// <summary>
    /// 신전 시스템 런타임 매니저.
    /// 회복/축복/기도/기부/신앙 증가 흐름을 관리한다.
    /// 현재는 실제 파티 체력/골드/스탯 시스템 연결 전 단계이므로,
    /// 디버그용 파티 체력과 골드를 내부에서 관리한다.
    /// </summary>
    public class ShrineManager : MonoBehaviour
    {
        public static ShrineManager Instance { get; private set; }

        [Header("Config")]
        [SerializeField] private ShrineConfigSO config;


        [Header("Runtime")]
        [SerializeField] private ShrineRuntimeData currentShrine;
        [SerializeField] private MissionManager missionManager;
        [SerializeField] private ShrinePlayerContext playerContext = new();
        [SerializeField] private ShrinePlayerRuntimeData playerRuntimeData = new();

        [Header("Debug")]
        [SerializeField] private string shrineId = "test_shrine";
        [SerializeField] private string shrineName = "Shrine";
        [SerializeField] private bool logDebug = true;

        public ShrineConfigSO Config => config;
        public ShrineRuntimeData CurrentShrine => currentShrine;
        public ShrinePlayerRuntimeData PlayerRuntimeData => playerRuntimeData;
        public int CurrentGold => playerContext.CurrentGold;
        public int PartyCurrentHp => playerContext.CurrentHp;
        public int PartyMaxHp => playerContext.MaxHp;
        public bool HasShrine => currentShrine != null;
        public bool IsOpened => currentShrine != null && currentShrine.isOpened;

        public event Action<ShrineRuntimeData> OnShrineOpened;
        public event Action<ShrineRuntimeData> OnShrineRefreshed;
        public event Action<ShrineRuntimeData> OnShrineCompleted;
        public event Action<ShrineActionType> OnShrineActionSelected;
        public event Action<BlessRuntimeData.BlessEntry> OnBlessingCandidatesGenerated;
        public event Action<BlessRuntimeData.BlessEntry> OnBlessingSelected;
        public event Action<ShrineGodType, int> OnFaithChanged;
        public event Action<int, int> OnPartyHpChanged;
        public event Action<int> OnGoldChanged;

        private ShrineBlessingService blessingService;
        private ShrineMissionService missionService;
        private ShrineRewardService rewardService;
        private ShrineFaithService faithService;
        private ShrineActionService actionService;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;


            if (playerContext == null)
            {
                playerContext = new ShrinePlayerContext();
            }

            playerContext.OnGoldChanged += HandleGoldChanged;
            playerContext.OnHpChanged += HandleHpChanged;

            blessingService = new ShrineBlessingService(
                this,
                config,
                logDebug);

            missionService = new ShrineMissionService(
                config,
                missionManager,
                logDebug);

            rewardService = new ShrineRewardService(
                logDebug);

            faithService = new ShrineFaithService(
                this,
                playerRuntimeData,
                config,
                rewardService,
                missionService,
                logDebug);

            actionService = new ShrineActionService(
                this,
                config,
                faithService,
                playerContext,
                logDebug);

            missionService?.RegisterUnlockMissions();
        }

        public void OpenShrine()
        {
            if (config == null)
            {
                Debug.LogWarning("[ShrineManager] OpenShrine failed. Config is null.");
                return;
            }


            currentShrine = new ShrineRuntimeData(shrineId, shrineName)
            {
                seed = config.seed,
                generatedFromConfigId = config.configId
            };

            currentShrine.SetAvailableGods(GetAvailableGodTypes());
            currentShrine.Open();

            if (logDebug)
            {
                Debug.Log($"[ShrineManager] Shrine opened. shrine={shrineId}");
            }

            OnShrineOpened?.Invoke(currentShrine);
            Refresh();
        }

        public void CompleteShrine()
        {
            if (currentShrine == null)
            {
                return;
            }

            currentShrine.Complete();

            if (logDebug)
            {
                Debug.Log("[ShrineManager] Shrine completed.");
            }

            OnShrineCompleted?.Invoke(currentShrine);
            Refresh();
        }

        public void ClearShrine()
        {
            currentShrine = null;
            Refresh();
        }

        public void SelectHealAndBless()
        {
            if (!EnsureShrineOpened())
            {
                return;
            }

            currentShrine.SetAction(ShrineActionType.HealAndBless);
            ApplyHeal();
            GenerateBlessingCandidates();

            OnShrineActionSelected?.Invoke(ShrineActionType.HealAndBless);
            Refresh();
        }

        public void SelectPray()
        {
            if (!EnsureShrineOpened())
            {
                return;
            }

            currentShrine.SetAction(ShrineActionType.Pray);
            currentShrine.SetAvailableGods(GetAvailableGodTypes());

            OnShrineActionSelected?.Invoke(ShrineActionType.Pray);
            Refresh();
        }

        public void SelectDonate()
        {
            if (!EnsureShrineOpened())
            {
                return;
            }

            currentShrine.SetAction(ShrineActionType.Donate);
            currentShrine.SetAvailableGods(GetAvailableGodTypes());

            OnShrineActionSelected?.Invoke(ShrineActionType.Donate);
            Refresh();
        }

        public void SelectGod(ShrineGodType godType)
        {
            if (!EnsureShrineOpened())
            {
                return;
            }

            currentShrine.SelectGod(godType);
            Refresh();
        }

        public bool ConfirmPray()
        {
            return actionService != null
                && actionService.ConfirmPray();
        }

        public bool ConfirmDonate()
        {
            return actionService != null
                && actionService.ConfirmDonate();
        }

        public bool SelectBlessingBySlot(int slotIndex)
        {
            if (!EnsureShrineOpened())
            {
                return false;
            }

            bool success =
                currentShrine.SelectBlessingBySlot(slotIndex);

            if (!success)
            {
                return false;
            }

            return actionService != null
                && actionService.SelectBlessing(
                    currentShrine.selectedBlessing);
        }

        public bool SelectBlessing(string runtimeId)
        {
            if (!EnsureShrineOpened())
            {
                return false;
            }

            bool success =
                currentShrine.SelectBlessing(runtimeId);

            if (!success)
            {
                return false;
            }

            return actionService != null
                && actionService.SelectBlessing(
                    currentShrine.selectedBlessing);
        }

        public int GetDonationCost(ShrineGodType godType)
        {
            if (config == null)
            {
                return 0;
            }

            int currentFaithLevel =
                playerRuntimeData != null
                    ? playerRuntimeData.GetFaithLevel(godType)
                    : 0;
            return config.GetDonationCost(currentFaithLevel);
        }

        public ShrineGodSO GetGodSO(ShrineGodType godType)
        {
            if (config == null)
            {
                return null;
            }

            return config.GetGod(godType);
        }

        public void SetGold(int gold)
        {
            playerContext.SetGold(gold);
            Refresh();
        }

        public void AddGold(int amount)
        {
            playerContext.AddGold(amount);
            Refresh();
        }

        public void SetPartyHp(int currentHp, int maxHp)
        {
            playerContext.SetHp(currentHp, maxHp);
            Refresh();
        }

        private void ApplyHeal()
        {
            actionService?.ApplyHeal();
        }

        public void NotifyFaithChanged(
            ShrineGodType godType,
            int level)
        {
            OnFaithChanged?.Invoke(godType, level);
        }

        public void NotifyBlessingSelected(
            BlessRuntimeData.BlessEntry blessing)
        {
            OnBlessingSelected?.Invoke(blessing);
        }

        private void HandleGoldChanged(int currentGold)
        {
            OnGoldChanged?.Invoke(currentGold);
        }

        private void HandleHpChanged(
            int currentHp,
            int maxHp)
        {
            OnPartyHpChanged?.Invoke(
                currentHp,
                maxHp);
        }

        private void GenerateBlessingCandidates()
        {
            if (config == null)
            {
                return;
            }

            if (blessingService == null)
            {
                Debug.LogWarning(
                    "[ShrineManager] BlessingService is null.");

                return;
            }

            ShrineGodType godType =
                currentShrine != null
                    ? currentShrine.selectedGod
                    : ShrineGodType.None;

            List<BlessSO> selectedBlessings =
                blessingService.GenerateBlessingCandidates(
                    godType,
                    Mathf.Max(1, config.blessingCandidateCount));

            List<BlessRuntimeData.BlessEntry> runtimeCandidates = new();

            for (int i = 0; i < selectedBlessings.Count; i++)
            {
                BlessSO blessing =
                    selectedBlessings[i];

                if (blessing == null)
                {
                    continue;
                }

                runtimeCandidates.Add(
                    new BlessRuntimeData.BlessEntry(
                        blessing,
                        config.configId,
                        i));
            }
            if (currentShrine == null)
            {
                return;
            }
            currentShrine.SetBlessingCandidates(runtimeCandidates);

            foreach (BlessRuntimeData.BlessEntry blessing in runtimeCandidates)
            {
                OnBlessingCandidatesGenerated?.Invoke(blessing);
            }
        }

        private List<ShrineGodType> GetAvailableGodTypes()
        {
            if (config == null)
            {
                return new List<ShrineGodType>();
            }

            List<ShrineGodType> result = new();

            List<ShrineGodType> defaults =
                config.GetDefaultAvailableGods();

            if (defaults != null)
            {
                result.AddRange(defaults);
            }


            if (playerRuntimeData == null
                || !playerRuntimeData.HasLockedFaith)
            {
                return result;
            }

            return result
                .Where(x => x == playerRuntimeData.LockedGod)
                .ToList();
        }


        private bool EnsureShrineOpened()
        {
            if (currentShrine == null || !currentShrine.isOpened)
            {
                Debug.LogWarning("[ShrineManager] Shrine is not opened.");
                return false;
            }

            return true;
        }

        public void Refresh()
        {
            OnShrineRefreshed?.Invoke(currentShrine);
        }

    }
}