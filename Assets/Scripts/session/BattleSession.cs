using System;
using Party;
using Battle;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Session
{
    [Serializable]
    public class BattleSession
    {
        public bool IsBattleActive;

        public string BattleId;

        public string BattleSceneName;

        public string ReturnSceneName;
        public string LoadingSceneName = "LoadingScene";

        public PartyRuntimeData PartyRuntimeData = new();

        public BattleSO BattleSO;

        public BattleRuntime BattleRuntime;

        [Header("Stage Node Completion")]
        public string PendingStageNodeId;

        public string CompletedStageNodeId;

        public bool BeginBattle(
            BattleSO battleSO,
            string battleSceneName,
            string returnSceneName,
            string stageNodeId = null,
            Action beforeSceneLoad = null)
        {
            if (battleSO == null)
            {
                Debug.LogError(
                    "[BattleSession] BattleSO is null.");
                return false;
            }

            BattleSO = battleSO;

            bool started = BeginBattle(
                battleSO != null
                    ? battleSO.BattleId
                    : string.Empty,
                battleSceneName,
                returnSceneName,
                stageNodeId,
                beforeSceneLoad);

            if (!started)
            {
                BattleSO = null;
            }

            return started;
        }

        public bool BeginBattle(
            string battleId,
            string battleSceneName,
            string returnSceneName,
            string stageNodeId = null,
            Action beforeSceneLoad = null)
        {
            if (IsBattleActive)
            {
                Debug.LogWarning(
                    "[BattleSession] A battle is already active.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(battleSceneName))
            {
                Debug.LogError(
                    "[BattleSession] BattleSceneName is empty.");

                return false;
            }

            if (string.IsNullOrWhiteSpace(returnSceneName))
            {
                Debug.LogError(
                    "[BattleSession] ReturnSceneName is empty.");
                return false;
            }

            IsBattleActive = true;
            BattleId = battleId;
            BattleSceneName = battleSceneName;
            ReturnSceneName = returnSceneName;
            PendingStageNodeId = stageNodeId;
            BattleRuntime = null;

            beforeSceneLoad?.Invoke();
            SceneManager.LoadScene(LoadingSceneName);
            return true;
        }

        public void EndBattle()
        {
            QueueStageNodeCompletionIfCleared();
            IsBattleActive = false;

            if (string.IsNullOrEmpty(ReturnSceneName))
            {
                Debug.LogError(
                    "[BattleSession] ReturnSceneName is empty.");

                return;
            }

            BattleSceneName = ReturnSceneName;

            SceneManager.LoadScene(LoadingSceneName);
        }

        public void Clear()
        {
            IsBattleActive = false;

            BattleId = string.Empty;
            BattleSO = null;
            BattleRuntime = null;
            BattleSceneName = string.Empty;
            ReturnSceneName = string.Empty;
            PendingStageNodeId = string.Empty;
        }

        public bool QueueStageNodeCompletionIfCleared()
        {
            if (string.IsNullOrWhiteSpace(PendingStageNodeId))
            {
                return false;
            }

            if (BattleRuntime?.isCompleted != true)
            {
                Debug.LogWarning(
                    "[BattleSession] Stage node completion not queued. "
                    + "The battle has not completed. "
                    + $"nodeId={PendingStageNodeId}, "
                    + $"battleId={BattleId}.");
                return false;
            }

            CompletedStageNodeId = PendingStageNodeId;
            Debug.Log(
                "[BattleSession] Stage node completion queued. "
                + $"nodeId={CompletedStageNodeId}, "
                + $"battleId={BattleId}.");
            return true;
        }

        public bool TryGetCompletedStageNodeId(
            out string nodeId)
        {
            nodeId = CompletedStageNodeId;
            return !string.IsNullOrWhiteSpace(nodeId);
        }

        public bool ConsumeCompletedStageNodeId(
            string expectedNodeId)
        {
            if (string.IsNullOrWhiteSpace(expectedNodeId)
                || !string.Equals(
                    CompletedStageNodeId,
                    expectedNodeId,
                    StringComparison.Ordinal))
            {
                return false;
            }

            CompletedStageNodeId = string.Empty;
            return true;
        }
    }
}
