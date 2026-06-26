using System.Collections.Generic;
using UnityEngine;

public sealed class EventSpawnHandler : MonoBehaviour
{
    private class ActiveRunnerState
    {
        public SpawnContentRunner Runner { get; }
        public SpawnExecutionRuntime Runtime { get; }

        public ActiveRunnerState(SpawnContentRunner runner, SpawnExecutionRuntime runtime)
        {
            Runner = runner;
            Runtime = runtime;
        }
    }

    private readonly List<ActiveRunnerState> activeRunners = new List<ActiveRunnerState>();

    // 씬 또는 외부 컴포넌트에서 이 메서드를 직접 호출하여 이벤트를 소환으로 변환
    public SpawnExecutionRuntime TriggerEventSpawn(SpawnContentSO content, Vector3 position, float rotation)
    {
        if (content == null)
        {
            Debug.LogError("[EventSpawnHandler] Event Spawn 실패: content가 null입니다.");
            return null;
        }

        SpawnRequest request = new SpawnRequest(content, position, rotation);
        SpawnContentRunner runner = new SpawnContentRunner();
        SpawnExecutionRuntime runtime = runner.Run(request);

        activeRunners.Add(new ActiveRunnerState(runner, runtime));
        
        Debug.Log($"[EventSpawnHandler] 이벤트 스폰 핸들러를 통한 소환 개시: '{content.ContentId}'");
        return runtime;
    }

    private void Update()
    {
        for (int i = activeRunners.Count - 1; i >= 0; i--)
        {
            var state = activeRunners[i];
            if (state.Runner.IsCompleted)
            {
                activeRunners.RemoveAt(i);
            }
            else
            {
                state.Runner.Tick(Time.deltaTime);
            }
        }
    }
}
