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
    public sealed class EventRewardExecutor
    {
        private readonly EventRewardContext context;
        private readonly Dictionary<PopupEventRewardType, IEventRewardHandler> handlers = new();

        public EventRewardExecutor(
            ItemManager itemManager,
            StatManager statManager)
        {
            context = new EventRewardContext(itemManager, statManager);
            RegisterDefaultHandlers();
        }

        public void Execute(List<PopupEventRewardData> rewards)
        {
            if (rewards == null || rewards.Count == 0)
            {
                return;
            }

            foreach (PopupEventRewardData reward in rewards)
            {
                ExecuteReward(reward);
            }
        }

        public void ExecuteReward(PopupEventRewardData reward)
        {
            if (reward == null || reward.rewardType == PopupEventRewardType.None)
            {
                return;
            }

            if (!handlers.TryGetValue(reward.rewardType, out IEventRewardHandler handler))
            {
                Debug.LogWarning($"[EventRewardExecutor] Unsupported reward type. type={reward.rewardType}");
                return;
            }

            handler.Execute(reward, context);
        }

        private void RegisterDefaultHandlers()
        {
            Register(new GoldRewardHandler());
            Register(new ReputationRewardHandler());

            Register(new RelicRewardHandler());
            Register(new RelicPoolRewardHandler());
            Register(new StrategicSkillItemRewardHandler());
            Register(new StrategicSkillItemPoolRewardHandler());
            Register(new AIFunctionRewardHandler());
            Register(new BlessingRewardHandler());
            Register(new BlessingPoolRewardHandler());

            Register(new RevealHiddenNodeRewardHandler());
            Register(new UnlockRouteRewardHandler());

            Register(new BattleRewardHandler(PopupEventRewardType.SpecialBattle));
            Register(new BattleRewardHandler(PopupEventRewardType.BossBattle));
        }

        private void Register(IEventRewardHandler handler)
        {
            if (handler == null)
            {
                return;
            }

            handlers[handler.RewardType] = handler;
        }
    }

    public sealed class EventRewardContext
    {
        public ItemManager ItemManager { get; }
        public StatManager StatManager { get; }

        public EventRewardContext(
            ItemManager itemManager,
            StatManager statManager)
        {
            ItemManager = itemManager;
            StatManager = statManager;
        }
    }

    public interface IEventRewardHandler
    {
        PopupEventRewardType RewardType { get; }
        void Execute(PopupEventRewardData reward, EventRewardContext context);
    }

    public abstract class EventRewardHandlerBase : IEventRewardHandler
    {
        public abstract PopupEventRewardType RewardType { get; }
        public abstract void Execute(PopupEventRewardData reward, EventRewardContext context);

        protected static bool TryGetTarget<T>(
            PopupEventRewardData reward,
            out T target)
            where T : ScriptableObject
        {
            target = reward != null
                ? reward.targetData as T
                : null;

            if (target != null)
            {
                return true;
            }

            Debug.LogWarning($"[EventRewardExecutor] Invalid reward target. type={reward?.rewardType}, expected={typeof(T).Name}");
            return false;
        }

        protected static bool TryGetItemManager(
            EventRewardContext context,
            out ItemManager itemManager)
        {
            itemManager = context?.ItemManager;
            if (itemManager != null)
            {
                return true;
            }

            Debug.LogWarning("[EventRewardExecutor] ItemManager is null.");
            return false;
        }

        protected static bool TryGetStatManager(
            EventRewardContext context,
            out StatManager statManager)
        {
            statManager = context?.StatManager;
            if (statManager != null)
            {
                return true;
            }

            Debug.LogWarning("[EventRewardExecutor] StatManager is null.");
            return false;
        }
    }

    public sealed class GoldRewardHandler : EventRewardHandlerBase
    {
        public override PopupEventRewardType RewardType => PopupEventRewardType.Gold;

        public override void Execute(PopupEventRewardData reward, EventRewardContext context)
        {
            Debug.Log($"[EventRewardExecutor] Gold changed. value={reward.value}");
        }
    }

    public sealed class ReputationRewardHandler : EventRewardHandlerBase
    {
        public override PopupEventRewardType RewardType => PopupEventRewardType.Reputation;

        public override void Execute(PopupEventRewardData reward, EventRewardContext context)
        {
            if (!TryGetStatManager(context, out StatManager statManager))
            {
                return;
            }

            statManager.AddStat(StatType.Reputation, reward.value);
        }
    }

    public sealed class RelicRewardHandler : EventRewardHandlerBase
    {
        public override PopupEventRewardType RewardType => PopupEventRewardType.Relic;

        public override void Execute(PopupEventRewardData reward, EventRewardContext context)
        {
            if (!TryGetItemManager(context, out ItemManager itemManager)
                || !TryGetTarget(reward, out RelicSO relic))
            {
                return;
            }

            itemManager.AddRelic(relic);
        }
    }

    public sealed class RelicPoolRewardHandler : EventRewardHandlerBase
    {
        public override PopupEventRewardType RewardType => PopupEventRewardType.RelicPool;

        public override void Execute(PopupEventRewardData reward, EventRewardContext context)
        {
            if (!TryGetItemManager(context, out ItemManager itemManager)
                || !TryGetTarget(reward, out RelicPoolSO pool))
            {
                return;
            }

            RelicSO relic = pool.GetRandomRelic();
            if (relic == null)
            {
                Debug.LogWarning($"[EventRewardExecutor] Relic pool is empty. pool={pool.name}");
                return;
            }

            itemManager.AddRelic(relic);
            Debug.Log($"[EventRewardExecutor] Relic granted from pool. pool={pool.name} relic={relic.name}");
        }
    }

    public sealed class StrategicSkillItemRewardHandler : EventRewardHandlerBase
    {
        public override PopupEventRewardType RewardType => PopupEventRewardType.StrategicSkillItem;

        public override void Execute(PopupEventRewardData reward, EventRewardContext context)
        {
            if (!TryGetItemManager(context, out ItemManager itemManager)
                || !TryGetTarget(reward, out StrategicSkillItemSO strategicSkillItem))
            {
                return;
            }

            itemManager.AddStrategicSkillItem(strategicSkillItem);
        }
    }

    public sealed class StrategicSkillItemPoolRewardHandler : EventRewardHandlerBase
    {
        public override PopupEventRewardType RewardType => PopupEventRewardType.StrategicSkillItemPool;

        public override void Execute(PopupEventRewardData reward, EventRewardContext context)
        {
            if (!TryGetItemManager(context, out ItemManager itemManager)
                || !TryGetTarget(reward, out StrategicSkillItemPoolSO pool))
            {
                return;
            }

            StrategicSkillItemSO strategicSkillItem = pool.GetRandomStrategicSkillItem();
            if (strategicSkillItem == null)
            {
                Debug.LogWarning($"[EventRewardExecutor] Strategic skill item pool is empty. pool={pool.name}");
                return;
            }

            itemManager.AddStrategicSkillItem(strategicSkillItem);
            Debug.Log($"[EventRewardExecutor] Strategic skill item granted from pool. pool={pool.name} item={strategicSkillItem.name}");
        }
    }

    public sealed class AIFunctionRewardHandler : EventRewardHandlerBase
    {
        public override PopupEventRewardType RewardType => PopupEventRewardType.AIFunction;

        public override void Execute(PopupEventRewardData reward, EventRewardContext context)
        {
            if (!TryGetItemManager(context, out ItemManager itemManager)
                || !TryGetTarget(reward, out AIFunctionSO function))
            {
                return;
            }

            itemManager.AddAIFunction(function);
        }
    }

    public sealed class BlessingRewardHandler : EventRewardHandlerBase
    {
        public override PopupEventRewardType RewardType => PopupEventRewardType.Blessing;

        public override void Execute(PopupEventRewardData reward, EventRewardContext context)
        {
            if (!TryGetTarget(reward, out BlessSO blessing))
            {
                return;
            }

            if (BlessManager.Instance == null)
            {
                Debug.LogWarning("[EventRewardExecutor] BlessManager is null.");
                return;
            }

            BlessManager.Instance.AddBless(blessing);
            Debug.Log($"[EventRewardExecutor] Blessing granted. blessing={blessing.name}");
        }
    }

    public sealed class BlessingPoolRewardHandler : EventRewardHandlerBase
    {
        public override PopupEventRewardType RewardType => PopupEventRewardType.BlessingPool;

        public override void Execute(PopupEventRewardData reward, EventRewardContext context)
        {
            if (!TryGetTarget(reward, out BlessPoolSO pool))
            {
                return;
            }

            BlessSO blessing = pool.GetRandomBlessing();
            if (blessing == null)
            {
                Debug.LogWarning($"[EventRewardExecutor] Blessing pool is empty. pool={pool.name}");
                return;
            }

            if (BlessManager.Instance == null)
            {
                Debug.LogWarning("[EventRewardExecutor] BlessManager is null.");
                return;
            }

            BlessManager.Instance.AddBless(blessing);
            Debug.Log($"[EventRewardExecutor] Blessing granted from pool. pool={pool.name} blessing={blessing.name}");
        }
    }

    public sealed class RevealHiddenNodeRewardHandler : EventRewardHandlerBase
    {
        public override PopupEventRewardType RewardType => PopupEventRewardType.RevealHiddenNode;

        public override void Execute(PopupEventRewardData reward, EventRewardContext context)
        {
            if (!TryGetTarget(reward, out RoundNodeSO nodeSO))
            {
                return;
            }

            StageManager stageManager = StageManager.Instance;
            if (stageManager == null)
            {
                Debug.LogWarning("[EventRewardExecutor] StageManager is null.");
                return;
            }

            StageGraph graph = stageManager.RuntimeData.currentGraph;
            if (graph == null)
            {
                Debug.LogWarning("[EventRewardExecutor] StageGraph is null.");
                return;
            }

            RoundNode runtimeNode = graph.GetNode(nodeSO.nodeId);
            if (runtimeNode == null)
            {
                Debug.LogWarning($"[EventRewardExecutor] Hidden node not found. nodeId={nodeSO.nodeId}");
                return;
            }

            runtimeNode.Reveal();
            Debug.Log($"[EventRewardExecutor] Hidden node revealed. node={nodeSO.nodeId}");
        }
    }

    public sealed class UnlockRouteRewardHandler : EventRewardHandlerBase
    {
        public override PopupEventRewardType RewardType => PopupEventRewardType.UnlockRoute;

        public override void Execute(PopupEventRewardData reward, EventRewardContext context)
        {
            if (string.IsNullOrWhiteSpace(reward.tag))
            {
                Debug.LogWarning("[EventRewardExecutor] Route tag is empty.");
                return;
            }

            StageManager stageManager = StageManager.Instance;
            if (stageManager == null)
            {
                Debug.LogWarning("[EventRewardExecutor] StageManager is null.");
                return;
            }

            StageRuntimeData runtimeData = stageManager.RuntimeData;
            if (runtimeData == null)
            {
                Debug.LogWarning("[EventRewardExecutor] RuntimeData is null.");
                return;
            }

            float multiplier = reward.value <= 0
                ? 1.5f
                : reward.value;

            runtimeData.AddTagModifier(reward.tag, multiplier);
            Debug.Log($"[EventRewardExecutor] Route unlocked. tag={reward.tag} multiplier={multiplier}");
        }
    }

    public sealed class BattleRewardHandler : EventRewardHandlerBase
    {
        public override PopupEventRewardType RewardType { get; }

        public BattleRewardHandler(PopupEventRewardType rewardType)
        {
            RewardType = rewardType;
        }

        public override void Execute(PopupEventRewardData reward, EventRewardContext context)
        {
            if (!TryGetTarget(reward, out BattleSO battleSO))
            {
                return;
            }

            GameSession gameSession = GameSession.Instance;
            if (gameSession == null)
            {
                Debug.LogWarning("[EventRewardExecutor] GameSession is null.");
                return;
            }

            if (gameSession.BattleSession == null)
            {
                Debug.LogWarning("[EventRewardExecutor] BattleSession is null.");
                return;
            }

            string currentSceneName = SceneManager.GetActiveScene().name;

            gameSession.BattleSession.BeginBattle(
                battleSO,
                "BattleScene",
                currentSceneName);

            Debug.Log($"[EventRewardExecutor] Battle reward started. type={RewardType} battle={battleSO.name}");
        }
    }
}