using UnityEngine;
using UnityEngine.SceneManagement;

namespace Session
{
    public class GameSession : MonoBehaviour
    {
        public static GameSession Instance { get; private set; }

        [Header("Sessions")]
        public StageSession StageSession;

        public BattleSession BattleSession;

        [Header("Debug")]
        [SerializeField] private bool enableBattleSceneTest;

        [SerializeField] private KeyCode battleTestKey = KeyCode.F1;
        [SerializeField] private KeyCode returnStageTestKey = KeyCode.F2;

        [SerializeField] private string battleSceneName = "BattleScene";

        private void Awake()
        {
            if (Instance != null
                && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void Update()
        {
            if (!enableBattleSceneTest)
            {
                return;
            }

            if (Input.GetKeyDown(battleTestKey))
            {
                if (BattleSession == null)
                {
                    Debug.LogError(
                        "[GameSession] BattleSession is null.");

                    return;
                }

                BattleSession.BeginBattle(
                    "debug_battle",
                    battleSceneName,
                    SceneManager.GetActiveScene().name);

                return;
            }

            if (Input.GetKeyDown(returnStageTestKey))
            {
                if (BattleSession == null)
                {
                    Debug.LogError(
                        "[GameSession] BattleSession is null.");

                    return;
                }

                BattleSession.EndBattle();
            }
        }

        private void Initialize()
        {
            StageSession ??= new StageSession();
            BattleSession ??= new BattleSession();
        }
    }
}