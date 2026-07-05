using UnityEngine;

/// <summary>
/// SpawnContentSO 및 SpawnSequenceSO를 입력받아 실시간 소환 루틴을 처리하는 Spawner 컴포넌트
/// </summary>
public sealed class Spawner : MonoBehaviour
{
    [Header("Asset Configuration")]
    [SerializeField] private SpawnContentSO contentToSpawn;
    [SerializeField] private SpawnSequenceSO sequenceToSpawn;
    [SerializeField] private SpawnUnitBinding[] unitBindings;

    [Header("Position Configuration")]
    [SerializeField] private Transform anchorTransform;
    [SerializeField] private Vector3 anchorOffsetPosition = Vector3.zero;
    [SerializeField] private bool useCameraCenterAsFallback = true;

    private SpawnContentRunner _contentRunner;
    private SpawnSequenceRunner _sequenceRunner;

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        if (_contentRunner != null && !_contentRunner.IsCompleted)
        {
            _contentRunner.Tick(deltaTime);
        }

        if (_sequenceRunner != null)
        {
            _sequenceRunner.Tick(deltaTime);
        }
    }

    /// <summary>
    /// 인스펙터에 설정된 기본 에셋들을 대상 위치에 소환
    /// </summary>
    [ContextMenu("Spawn")]
    public void Spawn()
    {
        Vector3 pos = ResolveAnchorPosition();

        if (contentToSpawn != null)
        {
            Spawn(contentToSpawn, pos);
        }
        else if (sequenceToSpawn != null)
        {
            Spawn(sequenceToSpawn, pos);
        }
        else
        {
            Debug.LogWarning("[Spawner] 소환할 에셋(Content/Sequence)이 할당되지 않았습니다.");
        }
    }

    /// <summary>
    /// 인스펙터에 지정된 SpawnContentSO 소환 실행
    /// </summary>
    [ContextMenu("Spawn Content")]
    public void SpawnContent()
    {
        if (contentToSpawn == null)
        {
            Debug.LogError("[Spawner] contentToSpawn이 비어있습니다.");
            return;
        }
        Spawn(contentToSpawn, ResolveAnchorPosition());
    }

    /// <summary>
    /// 인스펙터에 지정된 SpawnSequenceSO 시퀀스 실행
    /// </summary>
    [ContextMenu("Spawn Sequence")]
    public void SpawnSequence()
    {
        if (sequenceToSpawn == null)
        {
            Debug.LogError("[Spawner] sequenceToSpawn이 비어있습니다.");
            return;
        }
        Spawn(sequenceToSpawn, ResolveAnchorPosition());
    }

    /// <summary>
    /// 지정된 SpawnContentSO와 월드 좌표를 기반으로 즉각 소환 루틴 실행 (NpcSpawnService로 소환 로직 위임)
    /// </summary>
    public void Spawn(SpawnContentSO content, Vector3 position)
    {
        if (content == null) return;

        Debug.Log($"[Spawner] SpawnContent 실행 시작: '{content.ContentId}', 위치: {position}");
        _contentRunner = NpcSpawnService.Instance.SpawnContent(content, position, null, CreateUnitResolver());
    }

    /// <summary>
    /// 지정된 SpawnSequenceSO와 월드 좌표를 기반으로 즉각 시퀀스 실행 (NpcSpawnService로 소환 로직 위임)
    /// </summary>
    public void Spawn(SpawnSequenceSO sequence, Vector3 position)
    {
        if (sequence == null) return;

        Debug.Log($"[Spawner] SpawnSequence 실행 시작: '{sequence.SequenceId}', 위치: {position}");

        StopSequenceRunner();

        _sequenceRunner = NpcSpawnService.Instance.SpawnSequence(
            sequence, 
            position, 
            () => Debug.Log($"[Spawner] 시퀀스 완료: '{sequence.SequenceId}'"),
            CreateUnitResolver());
    }

    /// <summary>
    /// 현재 실행 중인 모든 소환 루틴 강제 중지 및 초기화
    /// </summary>
    [ContextMenu("Stop Spawns")]
    public void StopActiveSpawns()
    {
        _contentRunner = null;
        StopSequenceRunner();
        Debug.Log("[Spawner] 모든 실행 중인 소환 루틴이 중지되었습니다.");
    }

    private void StopSequenceRunner()
    {
        if (_sequenceRunner != null)
        {
            _sequenceRunner.StopSequence();
            _sequenceRunner = null;
        }
    }

    private Vector3 ResolveAnchorPosition()
    {
        if (anchorTransform != null)
        {
            return anchorTransform.position + anchorOffsetPosition;
        }

        if (anchorOffsetPosition != Vector3.zero)
        {
            return anchorOffsetPosition;
        }

        if (useCameraCenterAsFallback && Camera.main != null)
        {
            Vector3 camPos = Camera.main.transform.position;
            camPos.z = 0f;
            return camPos;
        }

        return Vector3.zero;
    }

    private ISpawnUnitResolver CreateUnitResolver()
    {
        return new SpawnUnitBindingResolver(unitBindings);
    }
}
