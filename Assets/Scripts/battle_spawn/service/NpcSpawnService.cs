using System;
using UnityEngine;
using Character;
using Character.Helper;

/// <summary>
/// NPC 소환을 처리하는 서비스 클래스
/// </summary>
public sealed class NpcSpawnService
{
    private static NpcSpawnService instance;
    public static NpcSpawnService Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new NpcSpawnService();
            }
            return instance;
        }
    }

    public GameObject SpawnNpc(
        CharacterSO characterSO, 
        Vector3 worldPosition, 
        float rotationZ, 
        SpawnSequenceRuntime sequenceRuntime)
    {
        if (characterSO == null)
        {
            Debug.LogError("[NpcSpawnService] characterSO가 null입니다.");
            return null;
        }

        GameObject spawnedGo = CharacterBuilder.CreateOrBuildNpcObject(
            null,
            characterSO.name,
            null,
            worldPosition,
            // Keep the root transform unrotated; facing is applied to AnimationMono below.
            Quaternion.identity,
            "Enemy",
            null,
            true);

        CharacterManager characterManager = spawnedGo.GetComponent<CharacterManager>();
        if (characterManager == null)
        {
            characterManager = spawnedGo.GetComponentInChildren<CharacterManager>();
        }

        if (characterManager == null)
        {
            characterManager = spawnedGo.AddComponent<CharacterManager>();
        }

        characterManager.InitializeFromSO(characterSO);
        ApplyInitialLookDirection(spawnedGo, rotationZ);

        if (sequenceRuntime != null)
        {
            sequenceRuntime.AddEnemyTracking(spawnedGo.GetInstanceID());
        }

        EnemyRegistry.Instance.RegisterEnemy(spawnedGo);

        return spawnedGo;
    }

    private static void ApplyInitialLookDirection(GameObject spawnedGo, float rotationZ)
    {
        if (spawnedGo == null)
        {
            return;
        }

        AnimationMono animationMono =
            spawnedGo.GetComponent<AnimationMono>()
            ?? spawnedGo.GetComponentInChildren<AnimationMono>();

        if (animationMono == null)
        {
            return;
        }

        Vector2 lookDirection = SpawnCoordinateUtility.GetLookVector(rotationZ);
        animationMono.SetDirectionFromVector(lookDirection);
    }

    /// <summary>
    /// SpawnContentSO 소환 실행 및 러너 반환 (위치 계산 및 실행은 러너에서 처리)
    /// </summary>
    public SpawnContentRunner SpawnContent(
        SpawnContentSO content,
        Vector3 position,
        SpawnSequenceRuntime sequenceRuntime = null,
        ISpawnUnitResolver unitResolver = null)
    {
        if (content == null)
        {
            Debug.LogError("[NpcSpawnService] SpawnContent: content가 null입니다.");
            return null;
        }

        var runtime = new SpawnContentRuntime(content)
        {
            AnchorPosition = position,
            AnchorOffset = Vector2.zero,
            IsCanvasCoordinate = false
        };

        var runner = new SpawnContentRunner();
        runner.Start(runtime, sequenceRuntime, unitResolver);
        return runner;
    }

    /// <summary>
    /// SpawnSequenceSO 시퀀스 실행 및 러너 반환 (시퀀스 제어 상태는 러너에서 관리)
    /// </summary>
    public SpawnSequenceRunner SpawnSequence(
        SpawnSequenceSO sequence,
        Vector3 position,
        Action onCompleted = null,
        ISpawnUnitResolver unitResolver = null)
    {
        if (sequence == null)
        {
            Debug.LogError("[NpcSpawnService] SpawnSequence: sequence가 null입니다.");
            return null;
        }

        var runtime = new SpawnSequenceRuntime(sequence);
        foreach (var stepRuntime in runtime.StepRuntimes)
        {
            if (stepRuntime != null)
            {
                stepRuntime.AnchorPosition = position;
                stepRuntime.AnchorOffset = Vector2.zero;
                stepRuntime.IsCanvasCoordinate = false;
            }
        }

        var runner = new SpawnSequenceRunner();
        runner.StartSequence(runtime, onCompleted, unitResolver);
        return runner;
    }
}
