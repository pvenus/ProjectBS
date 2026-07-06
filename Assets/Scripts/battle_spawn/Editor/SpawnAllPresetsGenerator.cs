#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SpawnAllPresetsGenerator
{
    private const string BaseGeneratedFolder = "Assets/Scripts/battle_spawn/Resource/Generated";
    private const string SequencesJsonPath = "Assets/Scripts/battle_spawn/Resource/Jsons/sequence_presets.json";

    [MenuItem("BS/Spawn/Generate All Preset Assets")]
    public static void GenerateAllPresets()
    {
        Debug.Log("[SpawnAllPresetsGenerator] 프리셋 자동 생성 프로세스 시작...");

        string spawnContentOutputFolder = $"{BaseGeneratedFolder}/SpawnContents";
        if (!Directory.Exists(spawnContentOutputFolder)) Directory.CreateDirectory(spawnContentOutputFolder);

        string sequencesOutputFolder = $"{BaseGeneratedFolder}/Sequences";
        if (!Directory.Exists(sequencesOutputFolder)) Directory.CreateDirectory(sequencesOutputFolder);

        int sequenceCount = 0;
        if (!File.Exists(SequencesJsonPath))
        {
            Debug.LogWarning($"[SpawnAllPresetsGenerator] 시퀀스 JSON 파일이 없습니다: {SequencesJsonPath}");
        }
        else
        {
            string sequencesText = File.ReadAllText(SequencesJsonPath);
            List<JsonSpawnSequence> jsonSequences = ParseSequenceJson(sequencesText);
            foreach (JsonSpawnSequence seq in jsonSequences)
            {
                if (string.IsNullOrEmpty(seq.sequenceId)) continue;
                try
                {
                    SpawnSequenceSO sequenceAsset = BuildSequence(seq, sequencesOutputFolder, spawnContentOutputFolder);
                    if (sequenceAsset != null)
                    {
                        sequenceCount++;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SpawnAllPresetsGenerator] 시퀀스 '{seq.sequenceId}' 생성 중 에러: {ex.Message}");
                }
            }
        }
        Debug.Log($"[SpawnAllPresetsGenerator] SpawnSequence SO {sequenceCount}개 생성/갱신 완료.");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 수집 가능한 전체 ContentPoolSO 갱신
        CollectAllPools();

        EditorUtility.DisplayDialog("완료", "시퀀스 프리셋과 inline 스폰 컨텐츠 에셋이 성공적으로 생성/갱신되었습니다!", "확인");
    }

    private static void CollectAllPools()
    {
        // 씬 내 또는 프로젝트 리소스 내 ContentPoolSO 수집 및 리프레시
        string[] poolGuids = AssetDatabase.FindAssets("t:SpawnContentPoolSO");
        foreach (var guid in poolGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SpawnContentPoolSO pool = AssetDatabase.LoadAssetAtPath<SpawnContentPoolSO>(path);
            if (pool != null)
            {
                pool.CollectAllContents();
                EditorUtility.SetDirty(pool);
            }
        }

    }

    // --- JSON 파싱 Helper 클래스 및 메서드 ---
    [Serializable]
    public class JsonSpawnSequence
    {
        public string sequenceId;
        public string displayName;
        public string repeatMode;
        public int loopStartOrder;
        public List<JsonSpawnSequenceStep> steps;
    }
    [Serializable]
    public class JsonSpawnSequenceStep
    {
        public int order;
        public float startDelay;
        public string contentId;
        public JsonSpawnSquad content;
        public string completionMode;
    }
    [Serializable]
    public class JsonSequenceListWrapper { public List<JsonSpawnSequence> sequences; }

    private static SpawnSequenceSO BuildSequence(
        JsonSpawnSequence seq,
        string targetDir,
        string spawnContentOutputFolder)
    {
        if (seq == null || string.IsNullOrEmpty(seq.sequenceId)) return null;

        string assetPath = $"{targetDir}/{seq.sequenceId}.asset";
        SpawnSequenceSO asset = AssetDatabase.LoadAssetAtPath<SpawnSequenceSO>(assetPath);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<SpawnSequenceSO>();
            AssetDatabase.CreateAsset(asset, assetPath);
        }

        SpawnSequenceRepeatMode repeatMode = ParseRepeatMode(seq.repeatMode);
        asset.Initialize(seq.sequenceId, seq.displayName, repeatMode, seq.loopStartOrder);

        if (seq.steps != null)
        {
            foreach (JsonSpawnSequenceStep step in seq.steps)
            {
                SpawnContentSO content = BuildStepContent(seq.sequenceId, step, spawnContentOutputFolder);

                if (content == null)
                {
                    Debug.LogError($"[SpawnAllPresetsGenerator] Sequence '{seq.sequenceId}' step order {step.order}의 content를 생성할 수 없습니다.");
                    return null;
                }

                asset.AddStep(new SpawnSequenceStep(
                    step.order,
                    step.startDelay,
                    content,
                    ParseCompletionMode(step.completionMode)));
            }
        }

        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static SpawnContentSO BuildStepContent(
        string sequenceId,
        JsonSpawnSequenceStep step,
        string spawnContentOutputFolder)
    {
        if (step.content != null && !string.IsNullOrEmpty(step.content.contentId))
        {
            return SpawnContentBuilder.BuildSquad(step.content, spawnContentOutputFolder, null);
        }

        if (!string.IsNullOrEmpty(step.contentId))
        {
            Debug.LogError($"[SpawnAllPresetsGenerator] Sequence '{sequenceId}' step order {step.order}가 legacy contentId '{step.contentId}'를 사용합니다. sequence_presets.json에는 content를 inline으로 넣어야 합니다.");
            return null;
        }

        Debug.LogError($"[SpawnAllPresetsGenerator] Sequence '{sequenceId}' step order {step.order}에 content가 없습니다.");
        return null;
    }

    private static SpawnSequenceRepeatMode ParseRepeatMode(string value)
    {
        if (!string.IsNullOrEmpty(value) && Enum.TryParse(value, true, out SpawnSequenceRepeatMode mode))
        {
            return mode;
        }

        return SpawnSequenceRepeatMode.Once;
    }

    private static SpawnStepCompletionMode ParseCompletionMode(string value)
    {
        if (!string.IsNullOrEmpty(value) && Enum.TryParse(value, true, out SpawnStepCompletionMode mode))
        {
            return mode;
        }

        return SpawnStepCompletionMode.AfterSpawnCompleted;
    }

    private static List<JsonSpawnSequence> ParseSequenceJson(string text)
    {
        List<JsonSpawnSequence> results = new List<JsonSpawnSequence>();
        text = text.Trim();
        if (text.Length > 0 && text[0] == '\uFEFF') text = text.Substring(1).Trim();

        if (text.StartsWith("["))
        {
            string wrapped = "{\"sequences\":" + text + "}";
            var wrapper = JsonUtility.FromJson<JsonSequenceListWrapper>(wrapped);
            if (wrapper != null && wrapper.sequences != null) results.AddRange(wrapper.sequences);
        }
        else
        {
            var wrapper = JsonUtility.FromJson<JsonSequenceListWrapper>(text);
            if (wrapper != null && wrapper.sequences != null) results.AddRange(wrapper.sequences);
        }
        return results;
    }
}
#endif
