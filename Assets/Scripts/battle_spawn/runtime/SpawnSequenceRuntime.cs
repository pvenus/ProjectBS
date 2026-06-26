using System.Collections.Generic;
using UnityEngine;

public class SpawnContentRuntime
{
    public SpawnContentSO Content { get; }
    public Vector3 AnchorPosition { get; set; }
    public Vector2 AnchorOffset { get; set; }
    public bool IsCanvasCoordinate { get; set; }

    public SpawnContentRuntime(SpawnContentSO content)
    {
        Content = content;
        AnchorPosition = Vector3.zero;
        AnchorOffset = Vector2.zero;
        IsCanvasCoordinate = false;
    }
}

public class SpawnSequenceRuntime
{
    public SpawnSequenceSO OriginalSequence { get; private set; }
    public List<SpawnContentRuntime> StepRuntimes { get; private set; } = new List<SpawnContentRuntime>();
    
    public int CurrentOrder { get; set; }
    public int CurrentLoopCount { get; set; }
    
    public bool IsRunning { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsCancelled { get; set; }
    
    // 실행 중인 적의 Handle이나 ID 목록
    // 적 객체가 풀에 반환되거나 파괴되었는지 추적하기 위함
    public HashSet<int> AliveEnemyInstanceIds { get; private set; } = new HashSet<int>();
    
    public SpawnSequenceRuntime(SpawnSequenceSO sequenceSO)
    {
        OriginalSequence = sequenceSO;
        CurrentOrder = -1;
        CurrentLoopCount = 0;
        IsRunning = false;
        IsCompleted = false;
        IsCancelled = false;

        if (sequenceSO != null && sequenceSO.Steps != null)
        {
            foreach (var step in sequenceSO.Steps)
            {
                if (step != null && step.Content != null)
                {
                    StepRuntimes.Add(new SpawnContentRuntime(step.Content));
                }
                else
                {
                    StepRuntimes.Add(null);
                }
            }
        }
    }

    public void AddEnemyTracking(int instanceId)
    {
        AliveEnemyInstanceIds.Add(instanceId);
    }

    public void RemoveEnemyTracking(int instanceId)
    {
        AliveEnemyInstanceIds.Remove(instanceId);
    }
    
    public bool AreAllSpawnedEnemiesDefeated()
    {
        // HashSet에 남아있는 적이 0마리이면 모두 처치된 것으로 간주
        return AliveEnemyInstanceIds.Count == 0;
    }

    public void Cancel()
    {
        IsCancelled = true;
        IsRunning = false;
    }
}
