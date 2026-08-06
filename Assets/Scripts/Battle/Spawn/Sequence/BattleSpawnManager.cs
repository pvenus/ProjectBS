using System;
using Session;
using UnityEngine;

[AddComponentMenu("BS/Spawn/Battle Spawn Manager")]
public class BattleSpawnManager : MonoBehaviour
{
    public static BattleSpawnManager Instance { get; private set; }

    [Header("Sequence Data (For Fallback / Testing / Manual Assignment)")]
    [SerializeField] private SpawnSequenceSO spawnSequence;
    [SerializeField] private SpawnUnitBinding[] testUnitBindings;
    [SerializeField] private bool playOnStart = false;

    private BattleSession battleSession;
    private SpawnSequenceRunner sequenceRunner;
    private bool isInitialPrefabSpawned;

    // 소환 시퀀스 종료 시 외부(BattleManager 등)로 전파하기 위한 이벤트
    public event Action OnSequenceFinished;

    public BattleSession BattleSession => battleSession;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // BattleManager가 존재하지 않고 playOnStart가 활성화되어 있을 때만 독자 테스트 실행
        if (Battle.BattleManager.Instance == null && playOnStart)
        {
            InitializeForTest();
        }
    }

    private void OnDestroy()
    {
        StopSequence();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (sequenceRunner != null)
        {
            sequenceRunner.Tick(Time.deltaTime);
        }
    }

    /// <summary>
    /// 외부(예: BattleManager)에서 설정된 특정 소환 시퀀스를 재생할 때 호출합니다.
    /// </summary>
    public void PlaySequence(SpawnSequenceSO sequence)
    {
        PlaySequence(sequence, CreateTestUnitResolver());
    }

    public void PlaySequence(SpawnSequenceSO sequence, ISpawnUnitResolver unitResolver)
    {
        if (sequence == null)
        {
            Debug.LogError("[BattleSpawnManager] PlaySequence: 전달된 sequence가 null입니다.");
            return;
        }

        StopSequence();

        var runtime = new SpawnSequenceRuntime(sequence);
        Vector3 anchorPos = Vector3.zero;
        if (Camera.main != null)
        {
            anchorPos = Camera.main.transform.position;
            anchorPos.z = 0f;
        }

        foreach (var stepRuntime in runtime.StepRuntimes)
        {
            if (stepRuntime != null)
            {
                stepRuntime.AnchorPosition = anchorPos;
            }
        }

        sequenceRunner = new SpawnSequenceRunner();
        sequenceRunner.StartSequence(runtime, HandleSequenceFinished, unitResolver);
        Debug.Log($"[BattleSpawnManager] 신규 스폰 시퀀스 '{runtime.OriginalSequence.SequenceId}' 재생 시작");
    }

    /// <summary>
    /// 인스펙터에 할당된 spawnSequence를 재생합니다.
    /// </summary>
    [ContextMenu("Play Sequence")]
    public void PlaySequence()
    {
        if (spawnSequence == null)
        {
            Debug.LogWarning("[BattleSpawnManager] PlaySequence가 호출되었으나 spawnSequence가 할당되어 있지 않습니다.");
            return;
        }

        PlaySequence(spawnSequence);
    }

    /// <summary>
    /// 현재 진행 중인 소환 시퀀스를 중단합니다.
    /// </summary>
    public void StopSequence()
    {
        if (sequenceRunner != null)
        {
            sequenceRunner.StopSequence();
            sequenceRunner = null;
        }
    }

    private void HandleSequenceFinished()
    {
        Debug.Log("[BattleSpawnManager] 소환 시퀀스 완료.");
        OnSequenceFinished?.Invoke();

        // BattleManager가 씬에 없다면 독자 테스트 모드로 동작하고 있으므로 자체적으로 배틀 클리어 처리
        if (Battle.BattleManager.Instance == null)
        {
            CompleteBattleForTest();
        }
    }

    #region Test / Fallback Environment Logic (BattleManager가 없을 때 동작)

    private void InitializeForTest()
    {
        Debug.Log("[BattleSpawnManager] 독자 테스트 모드로 초기화를 수행합니다.");
        GameSession gameSession = GameSession.Instance;

        if (gameSession == null)
        {
            Debug.LogError("[BattleSpawnManager] [Test] GameSession이 씬에 존재하지 않습니다.");
            return;
        }

        battleSession = gameSession.BattleSession;

        if (battleSession == null)
        {
            Debug.LogError("[BattleSpawnManager] [Test] BattleSession이 GameSession 내부에 존재하지 않습니다.");
            return;
        }

        EnsureBattleRuntimeForTest();
        SpawnInitialPrefabsForTest();

        if (spawnSequence != null)
        {
            PlaySequence(spawnSequence);
        }
        else
        {
            Debug.LogWarning("[BattleSpawnManager] [Test] 구동할 SpawnSequenceSO가 세팅되지 않았습니다.");
        }
    }

    private void EnsureBattleRuntimeForTest()
    {
        if (battleSession.BattleSO == null)
        {
            Debug.LogWarning("[BattleSpawnManager] [Test] BattleSO가 BattleSession에 없습니다. 테스트를 위한 더미 런타임을 임시 생성합니다.");

            battleSession.BattleRuntime = new Battle.BattleRuntime
            {
                battleId = "dummy_spawn_test",
                battleName = "신규 스폰 시스템 테스트",
                victoryRule = Battle.BattleVictoryRule.ClearAllEnemies,
                survivalTimeSeconds = 0f,
                bossKilled = false,
                remainingEnemyCount = 0,
                isCompleted = false,
                elapsedTime = 0f
            };
            return;
        }

        battleSession.BattleRuntime = new Battle.BattleRuntime
        {
            battleId = battleSession.BattleSO.BattleId,
            battleName = battleSession.BattleSO.BattleName,
            victoryRule = battleSession.BattleSO.VictoryRule,
            survivalTimeSeconds = battleSession.BattleSO.SurvivalTimeSeconds,
            rewardExperience = battleSession.BattleSO.RewardExperience,
            relicDropPool = battleSession.BattleSO.RelicDropPool,
            normalRelicDropChance = battleSession.BattleSO.NormalRelicDropChance,
            bossRelicDropChance = battleSession.BattleSO.BossRelicDropChance,
            backgroundSprite = battleSession.BattleSO.BackgroundSprite,
            bossKilled = false,
            remainingEnemyCount = 0,
            isCompleted = false,
            elapsedTime = 0f
        };
    }

    private void SpawnInitialPrefabsForTest()
    {
        if (isInitialPrefabSpawned) return;

        Battle.BattleRuntime runtime = battleSession.BattleRuntime;
        if (runtime != null)
        {
            SpawnBackground(runtime);
        }

        isInitialPrefabSpawned = true;
    }

    private GameObject SpawnBackground(Battle.BattleRuntime runtime)
    {
        if (runtime.backgroundSprite != null)
        {
            GameObject backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(transform, false);

            SpriteRenderer renderer = backgroundObject.AddComponent<SpriteRenderer>();
            renderer.sprite = runtime.backgroundSprite;
            renderer.sortingOrder = -1000;

            return backgroundObject;
        }

        return null;
    }

    private void CompleteBattleForTest()
    {
        if (battleSession == null || battleSession.BattleRuntime == null) return;
        if (battleSession.BattleRuntime.isCompleted) return;

        battleSession.BattleRuntime.isCompleted = true;
        Debug.Log("[BattleSpawnManager] [Test] CompleteBattle - 배틀 완료!");

        battleSession.EndBattle();
    }

    private ISpawnUnitResolver CreateTestUnitResolver()
    {
        return new SpawnUnitBindingResolver(testUnitBindings);
    }

    #endregion
}
