using Session;
using UnityEngine;

namespace Battle
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        private BattleSession battleSession;

        public BattleSession BattleSession => battleSession;

        private void Awake()
        {
            if (Instance != null
                && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            GameSession gameSession =
                GameSession.Instance;

            if (gameSession == null)
            {
                Debug.LogError(
                    "[BattleManager] GameSession not found.");

                return;
            }

            battleSession =
                gameSession.BattleSession;

            if (battleSession == null)
            {
                Debug.LogError(
                    "[BattleManager] BattleSession not found.");
            }
        }

        public void EndBattle()
        {
            if (battleSession == null)
            {
                Debug.LogError(
                    "[BattleManager] BattleSession is null.");

                return;
            }

            battleSession.EndBattle();
        }
    }
}