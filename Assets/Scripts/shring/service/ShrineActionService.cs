using UnityEngine;
using Bless;

namespace Shrine
{
    /// <summary>
    /// Shrine 행동 실행 전담 서비스.
    /// Pray / Donate / Blessing 선택 / Heal 처리 등
    /// 실제 Shrine 액션 흐름을 담당한다.
    /// </summary>
    public class ShrineActionService
    {
        private readonly ShrineManager shrineManager;

        private readonly ShrineConfigSO config;

        private readonly ShrineFaithService faithService;

        private readonly ShrinePlayerContext playerContext;

        private readonly bool logDebug;

        public ShrineActionService(
            ShrineManager shrineManager,
            ShrineConfigSO config,
            ShrineFaithService faithService,
            ShrinePlayerContext playerContext,
            bool logDebug)
        {
            this.shrineManager = shrineManager;
            this.config = config;
            this.faithService = faithService;
            this.playerContext = playerContext;
            this.logDebug = logDebug;
        }

        public bool ConfirmPray()
        {
            ShrineRuntimeData shrine =
                shrineManager.CurrentShrine;

            if (!ValidateShrineAndGod(shrine))
            {
                return false;
            }

            int currentFaithLevel =
                shrineManager.PlayerRuntimeData != null
                    ? shrineManager.PlayerRuntimeData.GetFaithLevel(shrine.selectedGod)
                    : 0;

            if (currentFaithLevel >= 10)
            {
                Debug.LogWarning(
                    $"[ShrineActionService] Faith already max level. god={shrine.selectedGod}");

                return false;
            }

            int level =
                faithService != null
                    ? faithService.Pray(shrine.selectedGod)
                    : 0;

            if (level <= 0)
            {
                return false;
            }

            shrine.MarkFaithActionApplied();

            shrineManager.NotifyFaithChanged(
                shrine.selectedGod,
                level);

            shrineManager.Refresh();
            shrineManager.CompleteShrine();

            if (logDebug)
            {
                Debug.Log(
                    $"[ShrineActionService] Pray completed. god={shrine.selectedGod}, level={level}");
            }

            return true;
        }

        public bool ConfirmDonate()
        {
            ShrineRuntimeData shrine =
                shrineManager.CurrentShrine;

            if (!ValidateShrineAndGod(shrine))
            {
                return false;
            }

            int currentFaithLevel =
                shrineManager.PlayerRuntimeData != null
                    ? shrineManager.PlayerRuntimeData.GetFaithLevel(shrine.selectedGod)
                    : 0;

            if (currentFaithLevel >= 10)
            {
                Debug.LogWarning(
                    $"[ShrineActionService] Faith already max level. god={shrine.selectedGod}");

                return false;
            }

            int cost =
                config != null
                    ? config.GetDonationCost(currentFaithLevel)
                    : 0;

            if (!playerContext.HasEnoughGold(cost))
            {
                Debug.LogWarning(
                    $"[ShrineActionService] Not enough gold. cost={cost}, currentGold={playerContext.CurrentGold}");

                return false;
            }

            bool spendSuccess =
                playerContext.SpendGold(cost);

            if (!spendSuccess)
            {
                return false;
            }

            int level =
                faithService != null
                    ? faithService.Donate(shrine.selectedGod)
                    : 0;

            if (level <= 0)
            {
                playerContext.AddGold(cost);
                return false;
            }

            shrine.MarkFaithActionApplied();

            shrineManager.NotifyFaithChanged(
                shrine.selectedGod,
                level);

            shrineManager.Refresh();
            shrineManager.CompleteShrine();

            if (logDebug)
            {
                Debug.Log(
                    $"[ShrineActionService] Donate completed. god={shrine.selectedGod}, cost={cost}, level={level}");
            }

            return true;
        }

        public bool SelectBlessing(
            BlessRuntimeData.BlessEntry blessing)
        {
            ShrineRuntimeData shrine =
                shrineManager.CurrentShrine;

            if (shrine == null)
            {
                return false;
            }

            if (blessing == null)
            {
                return false;
            }

            blessing.TrySelect();

            if (BlessManager.Instance != null)
            {
                BlessManager.Instance.AddBless(
                    blessing.source,
                    blessing.generatedFromPoolId,
                    blessing.slotIndex);
            }

            shrineManager.NotifyBlessingSelected(blessing);

            shrineManager.Refresh();
            shrineManager.CompleteShrine();

            if (logDebug)
            {
                Debug.Log(
                    $"[ShrineActionService] Blessing selected. blessing={blessing.DisplayName}");
            }

            return true;
        }

        public void ApplyHeal()
        {
            ShrineRuntimeData shrine =
                shrineManager.CurrentShrine;

            if (shrine == null)
            {
                return;
            }

            if (config == null)
            {
                return;
            }

            int healAmount =
                Mathf.RoundToInt(
                    playerContext.MaxHp * config.partyHealRatio);

            playerContext.HealHp(healAmount);

            shrine.MarkHealApplied();

            shrineManager.Refresh();

            if (logDebug)
            {
                Debug.Log(
                    $"[ShrineActionService] Heal applied. amount={healAmount}, hp={playerContext.CurrentHp}/{playerContext.MaxHp}");
            }
        }

        private bool ValidateShrineAndGod(
            ShrineRuntimeData shrine)
        {
            if (shrine == null)
            {
                Debug.LogWarning(
                    "[ShrineActionService] Shrine is null.");

                return false;
            }

            if (!shrine.isOpened)
            {
                Debug.LogWarning(
                    "[ShrineActionService] Shrine is not opened.");

                return false;
            }

            if (!shrine.HasSelectedGod)
            {
                Debug.LogWarning(
                    "[ShrineActionService] God is not selected.");

                return false;
            }

            return true;
        }
    }
}
