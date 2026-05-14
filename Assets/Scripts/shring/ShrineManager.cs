

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

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
        [SerializeField] private FaithRuntimeData faithData = new();
        [SerializeField] private ShrinePlayerRuntimeData playerRuntimeData = new();

        [Header("Player Debug")]
        [SerializeField] private int currentGold = 500;
        [SerializeField] private int partyCurrentHp = 60;
        [SerializeField] private int partyMaxHp = 100;

        [Header("Debug")]
        [SerializeField] private string shrineId = "test_shrine";
        [SerializeField] private string shrineName = "Shrine";
        [SerializeField] private bool logDebug = true;

        public ShrineConfigSO Config => config;
        public ShrineRuntimeData CurrentShrine => currentShrine;
        public FaithRuntimeData FaithData => faithData;
        public ShrinePlayerRuntimeData PlayerRuntimeData => playerRuntimeData;
        public int CurrentGold => currentGold;
        public int PartyCurrentHp => partyCurrentHp;
        public int PartyMaxHp => partyMaxHp;
        public bool HasShrine => currentShrine != null;
        public bool IsOpened => currentShrine != null && currentShrine.isOpened;

        public event Action<ShrineRuntimeData> OnShrineOpened;
        public event Action<ShrineRuntimeData> OnShrineRefreshed;
        public event Action<ShrineRuntimeData> OnShrineCompleted;
        public event Action<ShrineActionType> OnShrineActionSelected;
        public event Action<ShrineBlessingRuntime> OnBlessingCandidatesGenerated;
        public event Action<ShrineBlessingRuntime> OnBlessingSelected;
        public event Action<ShrineGodType, int> OnFaithChanged;
        public event Action<int, int> OnPartyHpChanged;
        public event Action<int> OnGoldChanged;

        private System.Random fixedRandom;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (faithData == null)
            {
                faithData = new FaithRuntimeData();
            }

            faithData.InitializeDefaults();

            if (playerRuntimeData == null)
            {
                playerRuntimeData = new ShrinePlayerRuntimeData();
            }
        }

        public void OpenShrine()
        {
            if (config == null)
            {
                Debug.LogWarning("[ShrineManager] OpenShrine failed. Config is null.");
                return;
            }

            fixedRandom = config.useFixedSeed ? new System.Random(config.seed) : null;

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
            if (!EnsureShrineOpened())
            {
                return false;
            }

            if (!currentShrine.HasSelectedGod)
            {
                Debug.LogWarning("[ShrineManager] ConfirmPray failed. God is not selected.");
                return false;
            }

            int currentFaithLevel =
                faithData.GetFaithLevel(currentShrine.selectedGod);

            if (currentFaithLevel >= 10)
            {
                Debug.LogWarning(
                    $"[ShrineManager] Faith already max level. god={currentShrine.selectedGod}");

                return false;
            }

            int gain = config != null ? config.prayFaithGain : 1;
            bool success = faithData.TryIncreaseFaith(currentShrine.selectedGod, gain);
            if (!success)
            {
                return false;
            }

            currentShrine.MarkFaithActionApplied();
            int level = faithData.GetFaithLevel(currentShrine.selectedGod);

            playerRuntimeData.AddFaith(currentShrine.selectedGod, gain);

            if (logDebug)
            {
                Debug.Log($"[ShrineManager] Pray completed. god={currentShrine.selectedGod}, level={level}");
            }

            OnFaithChanged?.Invoke(currentShrine.selectedGod, level);
            Refresh();
            CompleteShrine();
            return true;
        }

        public bool ConfirmDonate()
        {
            if (!EnsureShrineOpened())
            {
                return false;
            }

            if (!currentShrine.HasSelectedGod)
            {
                Debug.LogWarning("[ShrineManager] ConfirmDonate failed. God is not selected.");
                return false;
            }

            int currentFaithLevelCheck =
                faithData.GetFaithLevel(currentShrine.selectedGod);

            if (currentFaithLevelCheck >= 10)
            {
                Debug.LogWarning(
                    $"[ShrineManager] Faith already max level. god={currentShrine.selectedGod}");

                return false;
            }

            int currentFaithLevel = faithData.GetFaithLevel(currentShrine.selectedGod);
            int cost = config != null ? config.GetDonationCost(currentFaithLevel) : 0;

            if (currentGold < cost)
            {
                Debug.LogWarning($"[ShrineManager] Not enough gold. cost={cost}, currentGold={currentGold}");
                return false;
            }

            currentGold -= cost;
            OnGoldChanged?.Invoke(currentGold);

            int gain = config != null ? config.donateFaithGain : 2;
            bool success = faithData.TryIncreaseFaith(currentShrine.selectedGod, gain);
            if (!success)
            {
                currentGold += cost;
                OnGoldChanged?.Invoke(currentGold);
                return false;
            }

            currentShrine.MarkFaithActionApplied();
            int level = faithData.GetFaithLevel(currentShrine.selectedGod);

            playerRuntimeData.AddFaith(currentShrine.selectedGod, gain);

            if (logDebug)
            {
                Debug.Log($"[ShrineManager] Donate completed. god={currentShrine.selectedGod}, cost={cost}, level={level}");
            }

            OnFaithChanged?.Invoke(currentShrine.selectedGod, level);
            Refresh();
            CompleteShrine();
            return true;
        }

        public bool SelectBlessingBySlot(int slotIndex)
        {
            if (!EnsureShrineOpened())
            {
                return false;
            }

            bool success = currentShrine.SelectBlessingBySlot(slotIndex);
            if (!success)
            {
                return false;
            }

            ShrineBlessingRuntime selected = currentShrine.selectedBlessing;
            selected?.Select();

            if (selected != null
                && selected.blessing != null
                && selected.blessing.godType == ShrineGodType.None)
            {
                playerRuntimeData.AddBlessing(selected.blessing);
            }

            if (logDebug && selected != null)
            {
                Debug.Log($"[ShrineManager] Blessing selected. blessing={selected.DisplayName}, effect={selected.GetEffectDescription()}");
            }

            OnBlessingSelected?.Invoke(selected);
            Refresh();
            CompleteShrine();
            return true;
        }

        public bool SelectBlessing(string runtimeId)
        {
            if (!EnsureShrineOpened())
            {
                return false;
            }

            bool success = currentShrine.SelectBlessing(runtimeId);
            if (!success)
            {
                return false;
            }

            ShrineBlessingRuntime selected = currentShrine.selectedBlessing;
            selected?.Select();

            if (selected != null
                && selected.blessing != null
                && selected.blessing.godType == ShrineGodType.None)
            {
                playerRuntimeData.AddBlessing(selected.blessing);
            }

            OnBlessingSelected?.Invoke(selected);
            Refresh();
            CompleteShrine();
            return true;
        }

        public int GetDonationCost(ShrineGodType godType)
        {
            if (config == null)
            {
                return 0;
            }

            int currentFaithLevel = faithData.GetFaithLevel(godType);
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
            currentGold = Mathf.Max(0, gold);
            OnGoldChanged?.Invoke(currentGold);
            Refresh();
        }

        public void AddGold(int amount)
        {
            currentGold = Mathf.Max(0, currentGold + amount);
            OnGoldChanged?.Invoke(currentGold);
            Refresh();
        }

        public void SetPartyHp(int currentHp, int maxHp)
        {
            partyMaxHp = Mathf.Max(1, maxHp);
            partyCurrentHp = Mathf.Clamp(currentHp, 0, partyMaxHp);
            OnPartyHpChanged?.Invoke(partyCurrentHp, partyMaxHp);
            Refresh();
        }

        private void ApplyHeal()
        {
            if (config == null)
            {
                return;
            }

            int healAmount = Mathf.RoundToInt(partyMaxHp * config.partyHealRatio);
            partyCurrentHp = Mathf.Clamp(partyCurrentHp + healAmount, 0, partyMaxHp);
            currentShrine.MarkHealApplied();

            if (logDebug)
            {
                Debug.Log($"[ShrineManager] Heal applied. amount={healAmount}, hp={partyCurrentHp}/{partyMaxHp}");
            }

            OnPartyHpChanged?.Invoke(partyCurrentHp, partyMaxHp);
        }

        private void GenerateBlessingCandidates()
        {
            if (config == null)
            {
                return;
            }

            List<ShrineBlessingSO> candidates = GetAvailableBlessingPool();
            if (candidates.Count == 0)
            {
                Debug.LogWarning("[ShrineManager] No available blessings in pool.");
                currentShrine.SetBlessingCandidates(new List<ShrineBlessingRuntime>());
                return;
            }

            List<ShrineBlessingSO> workingCandidates = new List<ShrineBlessingSO>(candidates);
            List<ShrineBlessingRuntime> runtimeCandidates = new();
            int count = Mathf.Max(1, config.blessingCandidateCount);

            for (int i = 0; i < count; i++)
            {
                if (workingCandidates.Count == 0)
                {
                    break;
                }

                ShrineBlessingSO selected = PickWeightedBlessing(workingCandidates);
                if (selected == null)
                {
                    continue;
                }

                runtimeCandidates.Add(new ShrineBlessingRuntime(selected, i, config.configId));

                if (!config.allowDuplicateBlessingCandidates)
                {
                    workingCandidates.Remove(selected);
                }
            }

            currentShrine.SetBlessingCandidates(runtimeCandidates);

            foreach (ShrineBlessingRuntime blessing in runtimeCandidates)
            {
                OnBlessingCandidatesGenerated?.Invoke(blessing);
            }
        }

        private List<ShrineBlessingSO> GetAvailableBlessingPool()
        {
            if (config == null || config.blessingPool == null)
            {
                return new List<ShrineBlessingSO>();
            }

            List<ShrineBlessingSO> result = new();

            foreach (ShrineBlessingSO blessing in config.blessingPool)
            {
                if (blessing == null)
                {
                    continue;
                }

                if (blessing.weight <= 0)
                {
                    continue;
                }

                if (blessing.godType != ShrineGodType.None)
                {
                    continue;
                }

                result.Add(blessing);
            }

            return result;
        }

        private ShrineBlessingSO PickWeightedBlessing(List<ShrineBlessingSO> candidates)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return null;
            }

            int totalWeight = candidates.Sum(x => Mathf.Max(0, x.weight));
            if (totalWeight <= 0)
            {
                return candidates[GetRandomRange(0, candidates.Count)];
            }

            int roll = GetRandomRange(1, totalWeight + 1);
            int accumulated = 0;

            foreach (ShrineBlessingSO candidate in candidates)
            {
                accumulated += Mathf.Max(0, candidate.weight);
                if (roll <= accumulated)
                {
                    return candidate;
                }
            }

            return candidates[^1];
        }

        private List<ShrineGodType> GetAvailableGodTypes()
        {
            if (config == null)
            {
                return new List<ShrineGodType>();
            }

            List<ShrineGodType> defaults = config.GetDefaultAvailableGods();

            if (!playerRuntimeData.HasLockedFaith)
            {
                return defaults;
            }

            return defaults
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

        private void Refresh()
        {
            OnShrineRefreshed?.Invoke(currentShrine);
        }

        private int GetRandomRange(int minInclusive, int maxExclusive)
        {
            if (fixedRandom != null)
            {
                return fixedRandom.Next(minInclusive, maxExclusive);
            }

            return Random.Range(minInclusive, maxExclusive);
        }
    }
}