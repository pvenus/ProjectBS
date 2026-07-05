using System;
using System.Collections.Generic;
using UnityEngine;
using Character;

public sealed class SpawnContentRunner
{
    private SpawnExecutionRuntime runtime;
    private SpawnPlan plan;
    private int nextCommandIndex = 0;
    private float elapsedTime = 0f;
    private SpawnSequenceRuntime legacySequenceRuntime;
    private ISpawnUnitResolver unitResolver;

    // 레거시 호환 멤버
    private bool _isCompleted = false;
    public bool IsCompleted => runtime != null ? (runtime.IsSpawnCompleted || runtime.IsCancelled) : _isCompleted;

    public SpawnExecutionRuntime Run(SpawnRequest request)
    {
        return Run(request, null, null);
    }

    public SpawnExecutionRuntime Run(
        SpawnRequest request,
        SpawnSequenceRuntime legacySeqRuntime,
        ISpawnUnitResolver resolver = null)
    {
        this.runtime = new SpawnExecutionRuntime();
        this.nextCommandIndex = 0;
        this.elapsedTime = 0f;
        this.legacySequenceRuntime = legacySeqRuntime;
        this.unitResolver = resolver;
        this._isCompleted = false;

        if (request.Content == null)
        {
            runtime.SetSpawnCompleted();
            this._isCompleted = true;
            return runtime;
        }

        this.plan = SpawnContentResolver.Resolve(request);

        if (this.plan == null || this.plan.Commands.Count == 0)
        {
            runtime.SetSpawnCompleted();
            this._isCompleted = true;
        }

        return runtime;
    }

    public void Tick(float deltaTime)
    {
        if (runtime == null || runtime.IsSpawnCompleted || runtime.IsCancelled)
        {
            _isCompleted = true;
            return;
        }

        elapsedTime += deltaTime;

        while (nextCommandIndex < plan.Commands.Count)
        {
            var cmd = plan.Commands[nextCommandIndex];
            if (cmd.StartTime <= elapsedTime)
            {
                CharacterSO character = ResolveCharacter(cmd);
                if (character != null)
                {
                    try
                    {
                        GameObject spawnedNpc = NpcSpawnService.Instance.SpawnNpc(
                            character,
                            cmd.Position,
                            cmd.Rotation,
                            legacySequenceRuntime
                        );

                        if (spawnedNpc != null)
                        {
                            runtime.AddNpc(spawnedNpc);

                            if (legacySequenceRuntime != null)
                            {
                                legacySequenceRuntime.AddEnemyTracking(spawnedNpc.GetInstanceID());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[SpawnContentRunner] NPC 소환 오류: {character.CharacterId}. 에러: {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[SpawnContentRunner] UnitKey '{cmd.UnitKey}', Role '{cmd.Role}'에 매핑된 CharacterSO가 없습니다.");
                }
                nextCommandIndex++;
            }
            else
            {
                break;
            }
        }

        if (nextCommandIndex >= plan.Commands.Count)
        {
            runtime.SetSpawnCompleted();
            _isCompleted = true;
        }
    }

    // --- 레거시 호환용 ---
    public void Start(SpawnContentRuntime runtimeContent, SpawnSequenceRuntime sequenceRuntime)
    {
        Start(runtimeContent, sequenceRuntime, null);
    }

    public void Start(
        SpawnContentRuntime runtimeContent,
        SpawnSequenceRuntime sequenceRuntime,
        ISpawnUnitResolver resolver)
    {
        if (runtimeContent == null || runtimeContent.Content == null)
        {
            _isCompleted = true;
            return;
        }

        SpawnRequest request = new SpawnRequest(
            runtimeContent.Content,
            runtimeContent.AnchorPosition,
            0f
        );

        Run(request, sequenceRuntime, resolver);
    }

    private CharacterSO ResolveCharacter(SpawnCommand command)
    {
        if (unitResolver == null)
        {
            return null;
        }

        return unitResolver.Resolve(new SpawnUnitRequest(command.UnitKey, command.Role));
    }
}
