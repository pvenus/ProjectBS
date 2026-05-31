using Item;
using UnityEngine;

namespace Shrine
{
    /// <summary>
    /// Shrine 보상 처리 전담 서비스.
    /// Faith Reward / Relic 지급 / 제거 등을 담당한다.
    /// </summary>
    public class ShrineRewardService
    {

        private readonly bool logDebug;

        public ShrineRewardService(
            bool logDebug)
        {
            this.logDebug = logDebug;
        }

        /// <summary>
        /// Faith 레벨 도달 보상 유물을 지급한다.
        /// </summary>
        public bool GiveFaithRelicReward(
            ShrineGodSO god,
            int faithLevel)
        {
            if (god == null)
            {
                return false;
            }

            if (ItemManager.Instance == null)
            {
                return false;
            }

            RelicSO rewardRelic =
                god.GetFaithRelicReward(faithLevel);

            if (rewardRelic == null)
            {
                return false;
            }

            bool added =
                ItemManager.Instance.AddRelic(
                    rewardRelic);

            if (logDebug)
            {
                Debug.Log(
                    $"[ShrineRewardService] Faith relic granted. god={god.godType}, level={faithLevel}, relic={rewardRelic.DisplayName}, added={added}");
            }

            return added;
        }

        /// <summary>
        /// 특정 신의 모든 Faith 유물을 제거한다.
        /// </summary>
        public void RemoveFaithRelics(
            ShrineGodSO god)
        {
            if (god == null)
            {
                return;
            }

            if (ItemManager.Instance == null)
            {
                return;
            }

            if (god.faithRelicRewards == null)
            {
                return;
            }

            for (int i = 0; i < god.faithRelicRewards.Count; i++)
            {
                ShrineFaithRelicReward reward =
                    god.faithRelicRewards[i];

                if (reward == null)
                {
                    continue;
                }

                if (reward.relicReward == null)
                {
                    continue;
                }

                bool removed =
                    ItemManager.Instance.RemoveRelic(
                        reward.relicReward);

                if (logDebug && removed)
                {
                    Debug.Log(
                        $"[ShrineRewardService] Faith relic removed. relic={reward.relicReward.DisplayName}");
                }
            }
        }
    }
}