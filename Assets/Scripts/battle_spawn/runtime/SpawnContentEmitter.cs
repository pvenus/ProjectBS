using UnityEngine;

public sealed class SpawnContentEmitter : MonoBehaviour
{
    [SerializeField] private SpawnContentSO content;
    private SpawnContentRunner runner;
    private SpawnExecutionRuntime runtime;

    public SpawnExecutionRuntime Runtime => runtime;

    [ContextMenu("Emit Spawn")]
    public void Emit()
    {
        if (content == null)
        {
            Debug.LogError($"[SpawnContentEmitter] Emit 실패: 소환할 SpawnContentSO가 지정되지 않았습니다.");
            return;
        }

        float rotZ = transform.eulerAngles.z;
        SpawnRequest request = new SpawnRequest(content, transform.position, rotZ);
        runner = new SpawnContentRunner();
        runtime = runner.Run(request);
        
        Debug.Log($"[SpawnContentEmitter] 소환 요청 시작. ContentId: '{content.ContentId}'");
    }

    private void Update()
    {
        if (runner != null && !runner.IsCompleted)
        {
            runner.Tick(Time.deltaTime);
        }
    }
}
