using UnityEngine;
using UnityEngine.Events;

public class StageRuntimeMono : MonoBehaviour
{
    public enum StageState
    {
        Ready,
        Playing,
        Success,
        Fail
    }

    [Header("Stage")]
    [SerializeField, Min(1f)] private float stageDurationSeconds = 300f;
    [SerializeField] private bool autoStartOnAwake = true;

    [Header("References")]
    [SerializeField] private StatMono towerStat;
    [SerializeField] private NpcSpawnMono npcSpawner;

    [Header("Wave")]
    [SerializeField, Min(1f)] private float waveIntervalSeconds = 30f;
    [SerializeField] private bool notifyWaveByLog = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onStageStarted;
    [SerializeField] private UnityEvent onStageSucceeded;
    [SerializeField] private UnityEvent onStageFailed;
    [SerializeField] private UnityEvent<int> onWaveStarted;

    [Header("Debug")]
    [SerializeField] private bool debugLog = true;

    private StageState _state = StageState.Ready;
    private float _elapsedTime;
    private float _remainingTime;
    private int _currentWaveIndex;
    private float _nextWaveTime;
    private bool _hasInitializedWaveSchedule;

    public StageState CurrentState => _state;
    public float ElapsedTime => _elapsedTime;
    public float RemainingTime => _remainingTime;
    public float StageDurationSeconds => stageDurationSeconds;
    public int CurrentWaveIndex => _currentWaveIndex;
    public bool IsPlaying => _state == StageState.Playing;
    public bool IsFinished => _state == StageState.Success || _state == StageState.Fail;

    private void Awake()
    {
        _remainingTime = stageDurationSeconds;

        if (autoStartOnAwake)
            StartStage();
    }

    private void Update()
    {
        if (_state != StageState.Playing)
            return;

        UpdateStageTimer();

        if (CheckFailCondition())
        {
            FinishStageFail();
            return;
        }

        if (CheckSuccessCondition())
        {
            FinishStageSuccess();
            return;
        }

        UpdateWaveSchedule();
    }

    public void StartStage()
    {
        _state = StageState.Playing;
        _elapsedTime = 0f;
        _remainingTime = stageDurationSeconds;
        _currentWaveIndex = 0;
        _nextWaveTime = 0f;
        _hasInitializedWaveSchedule = false;

        if (debugLog)
            Debug.Log($"[StageRuntime] Stage started duration={stageDurationSeconds:0.0}s");

        onStageStarted?.Invoke();
    }

    public void ResetStage()
    {
        _state = StageState.Ready;
        _elapsedTime = 0f;
        _remainingTime = stageDurationSeconds;
        _currentWaveIndex = 0;
        _nextWaveTime = 0f;
        _hasInitializedWaveSchedule = false;

        if (debugLog)
            Debug.Log("[StageRuntime] Stage reset");
    }

    public void FinishStageSuccess()
    {
        if (IsFinished)
            return;

        _state = StageState.Success;
        _remainingTime = 0f;

        if (debugLog)
            Debug.Log($"[StageRuntime] Stage success elapsed={_elapsedTime:0.00}s wave={_currentWaveIndex}");

        onStageSucceeded?.Invoke();
    }

    public void FinishStageFail()
    {
        if (IsFinished)
            return;

        _state = StageState.Fail;

        if (debugLog)
            Debug.Log($"[StageRuntime] Stage failed elapsed={_elapsedTime:0.00}s wave={_currentWaveIndex}");

        onStageFailed?.Invoke();
    }

    private void UpdateStageTimer()
    {
        _elapsedTime += Time.deltaTime;
        _remainingTime = Mathf.Max(0f, stageDurationSeconds - _elapsedTime);
    }

    private bool CheckSuccessCondition()
    {
        return _elapsedTime >= stageDurationSeconds;
    }

    private bool CheckFailCondition()
    {
        if (towerStat == null)
            return false;

        return towerStat.CurrentHp <= 0f || towerStat.IsDead;
    }

    private void UpdateWaveSchedule()
    {
        if (!_hasInitializedWaveSchedule)
        {
            _hasInitializedWaveSchedule = true;
            _nextWaveTime = 0f;
        }

        while (_elapsedTime >= _nextWaveTime)
        {
            StartNextWave();
            _nextWaveTime += Mathf.Max(1f, waveIntervalSeconds);
        }
    }

    private void StartNextWave()
    {
        _currentWaveIndex++;

        if (notifyWaveByLog || debugLog)
            Debug.Log($"[StageRuntime] Wave start index={_currentWaveIndex} elapsed={_elapsedTime:0.00}s");

        onWaveStarted?.Invoke(_currentWaveIndex);

        // Current NpcSpawnMono runs autonomously. This stage runtime keeps the wave timeline
        // and can later be extended to actively push wave configs into the spawner.
        if (npcSpawner == null)
            return;
    }
}