using System;
using UnityEngine;

[AddComponentMenu("BS/Spawn/Spawn Sequence Runner Component")]
public sealed class SpawnSequenceRunnerComponent : MonoBehaviour
{
    [SerializeField] private SpawnSequenceSO sequence;
    [SerializeField] private bool playOnStart = true;

    private SpawnSequenceRunner _runner;
    private SpawnSequenceRuntime _runtime;

    private void Start()
    {
        if (playOnStart && sequence != null)
        {
            PlaySequence();
        }
    }

    public void PlaySequence()
    {
        if (sequence == null)
        {
            Debug.LogWarning("[SpawnSequenceRunnerComponent] PlaySequence가 호출되었으나 sequence가 할당되어 있지 않습니다.");
            return;
        }

        _runner = new SpawnSequenceRunner();
        _runtime = new SpawnSequenceRuntime(sequence);

        // 씬 상의 anchor 좌표 결정 (부착된 오브젝트 위치 기준)
        Vector3 anchorPos = transform.position;

        foreach (var stepRuntime in _runtime.StepRuntimes)
        {
            if (stepRuntime != null)
            {
                stepRuntime.AnchorPosition = anchorPos;
            }
        }

        _runner.StartSequence(_runtime, () =>
        {
            Debug.Log($"[SpawnSequenceRunnerComponent] 시퀀스 '{sequence.SequenceId}' 실행 완료!");
        });
    }

    private void Update()
    {
        if (_runner != null && _runtime != null && _runtime.IsRunning)
        {
            _runner.Tick(Time.deltaTime);
        }
    }

    private void OnDestroy()
    {
        if (_runner != null)
        {
            _runner.StopSequence();
        }
    }
}
