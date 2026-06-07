using System.Collections.Generic;
using Item;
using Bless;
using Stat;
using Session;
using Battle;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Stage
{
    public class EventRewardExecutor
    {
        private readonly ItemManager itemManager;
        private readonly StatManager statManager;

        public EventRewardExecutor(
            ItemManager itemManager,
            StatManager statManager)
        {
            this.itemManager = itemManager;
            this.statManager = statManager;
        }

        public void Execute(
            List<PopupEventRewardData> rewards)
        {
            if (rewards == null
                || rewards.Count == 0)
            {
                return;
            }

            foreach (PopupEventRewardData reward in rewards)
            {
                ExecuteReward(reward);
            }
        }

        public void ExecuteReward(
            PopupEventRewardData reward)
        {
            if (reward == null)
            {
                return;
            }

            switch (reward.rewardType)
            {
                case PopupEventRewardType.None:
                    break;

                case PopupEventRewardType.Gold:
                    ApplyGold(reward.value);
                    break;

                case PopupEventRewardType.Reputation:
                    ApplyReputation(reward.value);
                    break;

                case PopupEventRewardType.Relic:
                    GiveRelic(reward.targetData);
                    break;

                case PopupEventRewardType.RelicPool:
                    GiveRelicFromPool(reward.targetData);
                    break;

                case PopupEventRewardType.StrategicSkillItem:
                    GiveStrategicSkillItem(reward.targetData);
                    break;

                case PopupEventRewardType.StrategicSkillItemPool:
                    GiveStrategicSkillItemFromPool(reward.targetData);
                    break;

                case PopupEventRewardType.AIFunction:
                    GiveAIFunction(reward.targetData);
                    break;

                case PopupEventRewardType.Blessing:
                    GiveBlessing(reward.targetData);
                    break;

                case PopupEventRewardType.BlessingPool:
                    GiveBlessingFromPool(reward.targetData);
                    break;

                case PopupEventRewardType.RevealHiddenNode:
                    RevealHiddenNode(reward.targetData);
                    break;

                case PopupEventRewardType.UnlockRoute:
                    UnlockRoute(reward.tag, reward.value);
                    break;

                case PopupEventRewardType.SpecialBattle:
                    StartBattle(reward.targetData, reward.rewardType);
                    break;

                case PopupEventRewardType.BossBattle:
                    StartBattle(reward.targetData, reward.rewardType);
                    break;

                default:
                    Debug.LogWarning(
                        $"[EventRewardExecutor] Unsupported reward type. type={reward.rewardType}");
                    break;
            }
        }

        private void StartBattle(
            ScriptableObject targetData,
            PopupEventRewardType rewardType)
        {
            BattleSO battleSO =
                targetData as BattleSO;

            if (battleSO == null)
            {
                Debug.LogWarning(
                    $"[EventRewardExecutor] Invalid battle reward. type={rewardType}");

                return;
            }

            GameSession gameSession =
                GameSession.Instance;

            if (gameSession == null)
            {
                Debug.LogWarning(
                    "[EventRewardExecutor] GameSession is null.");

                return;
            }

            if (gameSession.BattleSession == null)
            {
                Debug.LogWarning(
                    "[EventRewardExecutor] BattleSession is null.");

                return;
            }

            string currentSceneName =
                SceneManager.GetActiveScene().name;

            gameSession.BattleSession.BeginBattle(
                battleSO,
                "BattleScene",
                currentSceneName);

            Debug.Log(
                $"[EventRewardExecutor] Battle reward started. type={rewardType} battle={battleSO.name}");
        }

        private void ApplyGold(int value)
        {
            Debug.Log(
                $"[EventRewardExecutor] Gold changed. value={value}");
        }

        private void ApplyReputation(int value)
        {
            if (statManager == null)
            {
                Debug.LogWarning(
                    "[EventRewardExecutor] StatManager is null.");

                return;
            }

            statManager.AddStat(
                StatType.Reputation,
                value);
        }

        private void GiveRelic(
            ScriptableObject targetData)
        {
            if (itemManager == null)
            {
                Debug.LogWarning(
                    "[EventRewardExecutor] ItemManager is null.");

                return;
            }

            RelicSO relic = targetData as RelicSO;

            if (relic == null)
            {
                Debug.LogWarning(
                    "[EventRewardExecutor] Invalid relic reward.");

                return;
            }

            itemManager.AddRelic(relic);
        }

        private void GiveRelicFromPool(
            ScriptableObject targetData)
        {
            if (itemManager == null)
            {
                Debug.LogWarning(
                    "[EventRewardExecutor] ItemManager is null.");

                return;
            }

            RelicPoolSO pool =
                targetData as RelicPoolSO;

            if (pool == null)
            {
                Debug.LogWarning(
                    "[EventRewardExecutor] Invalid relic pool reward.");

                return;
            }

            RelicSO relic = pool.GetRandomRelic();

            if (relic == null)
            {
                Debug.LogWarning(
                    $"[EventRewardExecutor] Relic pool is empty. pool={pool.name}");

                return;
            }

            itemManager.AddRelic(relic);

            Debug.Log(
                $"[EventRewardExecutor] Relic granted from pool. pool={pool.name} relic={relic.name}");
        }

        private void GiveStrategicSkillItem(
            ScriptableObject targetData)
        {
            if (itemManager == null)
            {
                Debug.LogWarning(
                    "[EventRewardExecutor] ItemManager is null.");

                return;
            }

            StrategicSkillItemSO strategicSkillItem =
                targetData as StrategicSkillItemSO;

            if (strategicSkillItem == null)
            {
                Debug.LogWarning(
                    "[EventRewardExecutor] Invalid strategic skill item reward.");

                return;
            }

            itemManager.AddStrategicSkillItem(strategicSkillItem);
        }

        private void GiveStrategicSkillItemFromPool(
            ScriptableObject targetData)
        {
            if (itemManager == null)
            {
                Debug.LogWarning(
                    "[EventRewardExecutor] ItemManager is null.");

                return;
            }

            StrategicSkillItemPoolSO pool =
                targetData as StrategicSkillItemPoolSO;

            if (pool == null)
            {
                Debug.LogWarning(
                    "[EventRewardExecutor] Invalid strategic skill item pool reward.");

                return;
            }

            StrategicSkillItemSO strategicSkillItem =
                pool.GetRandomStrategicSkillItem();

            if (strategicSkillItem == null)
            {
                Debug.LogWarning(
                    $"[EventRewardExecutor] Strategic skill item pool is empty. pool={pool.name}");

                return;
            }

            itemManager.AddStrategicSkillItem(strategicSkillItem);

            Debug.Log(
                $"[EventRewardExecutor] Strategic skill item granted from pool. pool={pool.name} item={strategicSkillItem.name}");
        }

        private void GiveBlessingFromPool(
            ScriptableObject targetData)
        {
            if (itemManager == null)
            {
                Debug.LogWarning(
                    "[EventRewardExecutor] ItemManager is null.");

                return;
            }

            BlessPoolSO pool =
                targetData as BlessPoolSO;

            if (pool == null)
            {
                Debug.LogWarning(
                    "[EventRewardExecutor] Invalid blessing pool reward.");

                return;
            }

            BlessSO blessing =
                pool.GetRandomBlessing();

            if (blessing == null)
            {
                Debug.LogWarning(
                    $"[EventRewardExecutor] Blessing pool is empty. pool={pool.name}");

                return;
            }

            GiveBlessing(blessing);

            Debug.Log(
                $"[EventRewardExecutor] Blessing granted from pool. pool={pool.name} blessing={blessing.name}");
        }

        private void UnlockRoute(
            string tag,
            int value)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                Debug.LogWarning(
                    "[EventRewardExecutor] Route tag is empty.");

                return;
            }

            StageManager stageManager =
                StageManager.Instance;

            if (stageManager == null)
            {
                Debug.LogWarning(
                    "[EventRewardExecutor] StageManager is null.");

                return;
            }

            StageRuntimeData runtimeData =
                stageManager.RuntimeData;

            if (runtimeData == null)
            {
                Debug.LogWarning(
                    "[EventRewardExecutor] RuntimeData is null.");

                return;
            }

            float multiplier =
                value <= 0
                    ? 1.5f
                    : value;

            runtimeData.AddTagModifier(
                tag,
                multiplier);

            Debug.Log(
                $"[EventRewardExecutor] Route unlocked. tag={tag} multiplier={multiplier}");
        }

        private void RevealHiddenNode(
            ScriptableObject targetData)
        {
            RoundNodeSO nodeSO =
                targetData as RoundNodeSO;

            if (nodeSO == null)
            {
                Debug.LogWarning(
                    "[EventRewardExecutor] Invalid hidden node reward.");

                return;
            }

            StageManager stageManager =
                StageManager.Instance;

            if (stageManager == null)
            {
                Debug.LogWarning(
                    "[EventRewardExecutor] StageManager is null.");

                return;
            }

            StageGraph graph =
                stageManager.RuntimeData.currentGraph;

            if (graph == null)
            {
                Debug.LogWarning(
                    "[EventRewardExecutor] StageGraph is null.");

                return;
            }

            RoundNode runtimeNode =
                graph.GetNode(nodeSO.nodeId);

            if (runtimeNode == null)
            {
                Debug.LogWarning(
                    $"[EventRewardExecutor] Hidden node not found. nodeId={nodeSO.nodeId}");

                return;
            }

            runtimeNode.Reveal();

            Debug.Log(
                $"[EventRewardExecutor] Hidden node revealed. node={nodeSO.nodeId}");
        }

        private void GiveAIFunction(
            ScriptableObject targetData)
        {
            if (itemManager == null)
            {
                Debug.LogWarning(
                    "[EventRewardExecutor] ItemManager is null.");

                return;
            }

            AIFunctionSO function =
                targetData as AIFunctionSO;

            if (function == null)
            {
                Debug.LogWarning(
                    "[EventRewardExecutor] Invalid AI function reward.");

                return;
            }

            itemManager.AddAIFunction(function);
        }

        private void GiveBlessing(
            ScriptableObject targetData)
        {
            BlessSO blessing =
                targetData as BlessSO;

            if (blessing == null)
            {
                Debug.LogWarning(
                    "[EventRewardExecutor] Invalid blessing reward.");

                return;
            }

            if (BlessManager.Instance == null)
            {
                Debug.LogWarning(
                    "[EventRewardExecutor] BlessManager is null.");

                return;
            }

            BlessManager.Instance.AddBless(blessing);

            Debug.Log(
                $"[EventRewardExecutor] Blessing granted. blessing={blessing.name}");
        }
    }
}