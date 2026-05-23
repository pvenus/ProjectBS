using System;
using Party;
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

        public void BeginBattle(
            string battleId,
            string battleSceneName,
            string returnSceneName)
        {
            IsBattleActive = true;

            BattleId = battleId;
            BattleSceneName = battleSceneName;
            ReturnSceneName = returnSceneName;

            if (string.IsNullOrEmpty(BattleSceneName))
            {
                Debug.LogError(
                    "[BattleSession] BattleSceneName is empty.");

                return;
            }

            SceneManager.LoadScene(LoadingSceneName);
        }

        public void EndBattle()
        {
            IsBattleActive = false;

            if (string.IsNullOrEmpty(ReturnSceneName))
            {
                Debug.LogError(
                    "[BattleSession] ReturnSceneName is empty.");

                return;
            }

            BattleSceneName = ReturnSceneName;

            SceneManager.LoadScene(LoadingSceneName);

            Clear();
        }

        public void Clear()
        {
            IsBattleActive = false;

            BattleId = string.Empty;
            BattleSceneName = string.Empty;
            ReturnSceneName = string.Empty;
        }
    }
}