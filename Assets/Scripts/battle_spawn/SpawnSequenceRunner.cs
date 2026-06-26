using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class SpawnSequenceRunner
{
    private class ActiveStepState
    {
        public SpawnSequenceStep Step { get; }
        public SpawnContentRuntime ContentRuntime { get; }
        public SpawnContentRunner ContentRunner { get; }
        public SpawnExecutionRuntime ExecutionRuntime { get; set; }
        public float DelayTimer { get; set; }
        public bool SpawnStarted { get; set; }
        public bool SpawnCompleted { get; set; }
        public bool Completed { get; set; }

        public ActiveStepState(SpawnSequenceStep step, SpawnContentRuntime contentRuntime)
        {
            Step = step;
            ContentRuntime = contentRuntime;
            ContentRunner = new SpawnContentRunner();
            DelayTimer = 0f;
            SpawnStarted = false;
            SpawnCompleted = false;
            Completed = false;
        }
    }

    private SpawnSequenceRuntime _runtime;
    private Action _onSequenceCompleted;

    private List<List<int>> _groupedStepIndices = new List<List<int>>();
    private int _currentGroupIndex = 0;

    private List<ActiveStepState> _activeSteps = new List<ActiveStepState>();
    private bool _needsWaitEnemiesDefeated = false;

    public void StartSequence(SpawnSequenceRuntime runtime, Action onSequenceCompleted)
    {
        if (runtime == null)
        {
            Debug.LogError("[SpawnSequenceRunner] SpawnSequenceRuntime이 null입니다.");
            onSequenceCompleted?.Invoke();
            return;
        }

        Cleanup();

        _runtime = runtime;
        _onSequenceCompleted = onSequenceCompleted;
        _runtime.IsRunning = true;
        _runtime.IsCancelled = false;
        _runtime.IsCompleted = false;

        EnemyRegistry.Instance.OnEnemyDied += HandleEnemyDied;

        var groups = _runtime.OriginalSequence.Steps
            .Select((step, index) => new { step, index })
            .Where(x => x.step != null)
            .GroupBy(x => x.step.Order)
            .OrderBy(g => g.Key)
            .Select(g => g.Select(x => x.index).ToList())
            .ToList();

        _groupedStepIndices = groups;
        _currentGroupIndex = 0;
        _activeSteps.Clear();
        _needsWaitEnemiesDefeated = false;

        PrepareCurrentGroup();
    }

    private void PrepareCurrentGroup()
    {
        _activeSteps.Clear();
        _needsWaitEnemiesDefeated = false;

        if (_currentGroupIndex >= _groupedStepIndices.Count)
        {
            if (_runtime.OriginalSequence.RepeatMode == SpawnSequenceRepeatMode.Infinite)
            {
                _runtime.CurrentLoopCount++;
                int loopStartOrder = _runtime.OriginalSequence.LoopStartOrder;

                int nextIndex = -1;
                for (int i = 0; i < _groupedStepIndices.Count; i++)
                {
                    int firstIdx = _groupedStepIndices[i][0];
                    var step = _runtime.OriginalSequence.Steps[firstIdx];
                    if (step.Order == loopStartOrder)
                    {
                        nextIndex = i;
                        break;
                    }
                }

                if (nextIndex >= 0)
                {
                    _currentGroupIndex = nextIndex;
                }
                else
                {
                    _currentGroupIndex = 0;
                }
            }
            else
            {
                _runtime.IsRunning = false;
                _runtime.IsCompleted = true;
                Cleanup();
                _onSequenceCompleted?.Invoke();
                return;
            }
        }

        var indices = _groupedStepIndices[_currentGroupIndex];
        if (indices.Count > 0)
        {
            int order = _runtime.OriginalSequence.Steps[indices[0]].Order;
            _runtime.CurrentOrder = order;
        }

        foreach (int idx in indices)
        {
            var step = _runtime.OriginalSequence.Steps[idx];
            var stepRuntime = _runtime.StepRuntimes[idx];
            if (step != null && stepRuntime != null)
            {
                _activeSteps.Add(new ActiveStepState(step, stepRuntime));

                if (step.CompletionMode == SpawnStepCompletionMode.AfterSpawnedEnemiesDefeated)
                {
                    _needsWaitEnemiesDefeated = true;
                }
            }
        }
    }

    public void Tick(float deltaTime)
    {
        if (_runtime == null || !_runtime.IsRunning || _runtime.IsCancelled) return;

        bool allSpawnsCompleted = true;

        for (int i = 0; i < _activeSteps.Count; i++)
        {
            var state = _activeSteps[i];
            if (state.Completed) continue;

            if (!state.SpawnStarted)
            {
                state.DelayTimer += deltaTime;
                if (state.DelayTimer >= state.Step.StartDelay)
                {
                    state.SpawnStarted = true;
                    
                    SpawnRequest request = new SpawnRequest(
                        state.Step.Content,
                        state.ContentRuntime.AnchorPosition,
                        0f
                    );
                    state.ExecutionRuntime = state.ContentRunner.Run(request, _runtime);
                }
                else
                {
                    allSpawnsCompleted = false;
                    continue;
                }
            }

            if (!state.SpawnCompleted)
            {
                state.ContentRunner.Tick(deltaTime);
                if (state.ExecutionRuntime != null && state.ExecutionRuntime.IsSpawnCompleted)
                {
                    state.SpawnCompleted = true;
                }
                else
                {
                    allSpawnsCompleted = false;
                }
            }

            if (state.SpawnCompleted)
            {
                if (state.Step.CompletionMode == SpawnStepCompletionMode.AfterSpawnCompleted)
                {
                    state.Completed = true;
                }
                else if (state.Step.CompletionMode == SpawnStepCompletionMode.AfterSpawnedEnemiesDefeated)
                {
                    if (state.ExecutionRuntime != null && state.ExecutionRuntime.AreAllEnemiesDefeated)
                    {
                        state.Completed = true;
                    }
                    else
                    {
                        allSpawnsCompleted = false;
                    }
                }
            }
        }

        if (!allSpawnsCompleted) return;

        if (_needsWaitEnemiesDefeated)
        {
            bool allCompleted = true;
            foreach (var state in _activeSteps)
            {
                if (!state.Completed)
                {
                    if (state.Step.CompletionMode == SpawnStepCompletionMode.AfterSpawnedEnemiesDefeated)
                    {
                        if (state.ExecutionRuntime != null && !state.ExecutionRuntime.AreAllEnemiesDefeated)
                        {
                            allCompleted = false;
                        }
                        else
                        {
                            state.Completed = true;
                        }
                    }
                }
            }

            if (!allCompleted) return;
        }

        _currentGroupIndex++;
        PrepareCurrentGroup();
    }

    public void StopSequence()
    {
        if (_runtime != null)
        {
            _runtime.Cancel();
        }
        foreach (var s in _activeSteps)
        {
            if (s.ExecutionRuntime != null)
            {
                s.ExecutionRuntime.Cancel();
            }
        }
        _activeSteps.Clear();
        Cleanup();
    }

    private void Cleanup()
    {
        EnemyRegistry.Instance.OnEnemyDied -= HandleEnemyDied;
    }

    private void HandleEnemyDied(GameObject enemy)
    {
        if (enemy != null && _runtime != null)
        {
            _runtime.RemoveEnemyTracking(enemy.GetInstanceID());
        }
    }
}
