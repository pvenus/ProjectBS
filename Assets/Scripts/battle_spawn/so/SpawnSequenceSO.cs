using System;
using System.Collections.Generic;
using UnityEngine;

public enum SpawnSequenceRepeatMode
{
    Once,
    Infinite
}

public enum SpawnStepCompletionMode
{
    AfterSpawnCompleted,
    AfterSpawnedEnemiesDefeated
}

[Serializable]
public sealed class SpawnSequenceStep
{
    [SerializeField] private int order;
    [Min(0f)]
    [SerializeField] private float startDelay;
    [SerializeField] private SpawnContentSO content;
    [SerializeField] private SpawnStepCompletionMode completionMode;

    public int Order => order;
    public float StartDelay => startDelay;
    public SpawnContentSO Content => content;
    public SpawnStepCompletionMode CompletionMode => completionMode;

    public SpawnSequenceStep(
        int order, 
        float startDelay, 
        SpawnContentSO content, 
        SpawnStepCompletionMode completionMode)
    {
        this.order = order;
        this.startDelay = startDelay;
        this.content = content;
        this.completionMode = completionMode;
    }
}

[CreateAssetMenu(fileName = "SpawnSequence", menuName = "BS/Spawn/SpawnSequence")]
public class SpawnSequenceSO : ScriptableObject
{
    [SerializeField] private string sequenceId;
    [SerializeField] private string displayName;

    [SerializeField] private SpawnSequenceRepeatMode repeatMode = SpawnSequenceRepeatMode.Once;
    [SerializeField] private int loopStartOrder = 0;

    [SerializeField] private List<SpawnSequenceStep> steps = new List<SpawnSequenceStep>();

    public string SequenceId => sequenceId;
    public string DisplayName => displayName;
    
    public SpawnSequenceRepeatMode RepeatMode => repeatMode;
    public int LoopStartOrder => loopStartOrder;

    public IReadOnlyList<SpawnSequenceStep> Steps => steps;

    public void Initialize(
        string sequenceId, 
        string displayName, 
        SpawnSequenceRepeatMode repeatMode,
        int loopStartOrder)
    {
        this.sequenceId = sequenceId;
        this.displayName = displayName;
        this.repeatMode = repeatMode;
        this.loopStartOrder = loopStartOrder;
        this.steps = new List<SpawnSequenceStep>();
    }

    public void AddStep(SpawnSequenceStep step)
    {
        steps.Add(step);
    }

    public List<string> Validate()
    {
        List<string> errors = new List<string>();

        if (string.IsNullOrEmpty(sequenceId))
        {
            errors.Add("Sequence ID가 비어있습니다.");
        }

        if (steps == null || steps.Count == 0)
        {
            errors.Add("스텝(Steps) 목록이 비어있습니다.");
            return errors;
        }

        bool hasLoopStartOrder = false;

        for (int i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            if (step == null)
            {
                errors.Add($"스텝 리스트 {i}번째 요소가 null입니다.");
                continue;
            }

            if (step.Order < 0)
            {
                errors.Add($"[Order {step.Order}] 실행 순서(Order)가 음수입니다.");
            }

            if (step.StartDelay < 0f)
            {
                errors.Add($"[Order {step.Order}] 시작 딜레이(startDelay)가 음수입니다: {step.StartDelay}");
            }

            if (step.Content == null)
            {
                errors.Add($"[Order {step.Order}] 할당된 SpawnContentSO가 없습니다.");
            }

            if (step.Order == loopStartOrder)
            {
                hasLoopStartOrder = true;
            }
        }

        if (repeatMode == SpawnSequenceRepeatMode.Infinite && !hasLoopStartOrder)
        {
            errors.Add($"Infinite 반복 모드이지만 loopStartOrder({loopStartOrder})에 해당하는 스텝이 없습니다.");
        }

        return errors;
    }
}
