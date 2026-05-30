using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Currency;
using Item;
using Session.SO;

namespace Session
{
    public class GameSession : MonoBehaviour
    {
        public static GameSession Instance { get; private set; }

        [Header("Sessions")]
        public StageSession StageSession;

        public BattleSession BattleSession;

        [Header("Start Profile")]
        [SerializeField] private StartProfileSO startProfile;
        [SerializeField, Min(0)] private int startProfileApplyDelayFrame = 1;

        private bool startProfileApplied;

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
            StartCoroutine(ApplyStartProfileDelayed());
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

        private IEnumerator ApplyStartProfileDelayed()
        {
            int delayFrame = Mathf.Max(0, startProfileApplyDelayFrame);

            for (int i = 0; i < delayFrame; i++)
            {
                yield return null;
            }

            ApplyStartProfileIfNeeded();
        }

        private void ApplyStartProfileIfNeeded()
        {
            if (startProfileApplied || startProfile == null)
            {
                return;
            }

            Initialize();

            StageSession.CurrencyRuntimeData ??= new CurrencyRutimeData();
            StageSession.RelicRuntimeData ??= new RelicRuntimeData();

            StageSession.CurrencyRuntimeData.gold =
                Mathf.Max(
                    0,
                    startProfile.StartGold);

            if (startProfile.StartRelics != null)
            {
                for (int i = 0; i < startProfile.StartRelics.Count; i++)
                {
                    RelicSO relic = startProfile.StartRelics[i];

                    if (relic == null)
                    {
                        continue;
                    }

                    if (ItemManager.Instance != null)
                    {
                        ItemManager.Instance.AddRelic(relic);
                    }
                }
            }

            startProfileApplied = true;
        }

        private void Initialize()
        {
            StageSession ??= new StageSession();
            BattleSession ??= new BattleSession();
        }
    }
}