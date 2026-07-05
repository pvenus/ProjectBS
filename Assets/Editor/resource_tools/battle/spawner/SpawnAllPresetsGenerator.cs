#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SpawnAllPresetsGenerator
{
    private const string AssetMenuRoot = "Assets/Battle/Spawner";
    private const string BaseGeneratedFolder = "Assets/Resources/battle/spawner/Generated";
    private const string SequencesJsonPath = "Assets/Resources/battle/spawner/Jsons/sequence_presets.json";

    [MenuItem("BS/Spawn/Generate All Preset Assets")]
    public static void GenerateAllPresets()
    {
        GenerateFromJsonPath(SequencesJsonPath);
    }

    [MenuItem(AssetMenuRoot + "/Generate SpawnSequenceSO From Json", false, 2000)]
    public static void GenerateFromSelectedJson()
    {
        UnityEngine.Object selected = Selection.activeObject;

        if (selected == null)
        {
            Debug.LogWarning("[SpawnAllPresetsGenerator] Select a spawner json file first.");
            return;
        }

        string jsonPath = AssetDatabase.GetAssetPath(selected);

        if (string.IsNullOrEmpty(jsonPath) ||
            !jsonPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning($"[SpawnAllPresetsGenerator] Selected asset is not json. path={jsonPath}");
            return;
        }

        GenerateFromJsonPath(jsonPath);
    }

    [MenuItem(AssetMenuRoot + "/Generate SpawnSequenceSO From Json", true)]
    public static bool ValidateGenerateFromSelectedJson()
    {
        string jsonPath = AssetDatabase.GetAssetPath(Selection.activeObject);

        return !string.IsNullOrEmpty(jsonPath) &&
            jsonPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
    }

    [MenuItem(AssetMenuRoot + "/Generate SpawnSequenceSO From Json Folder", false, 2001)]
    public static void GenerateFromSelectedFolder()
    {
        string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);

        if (string.IsNullOrEmpty(selectedPath) ||
            !AssetDatabase.IsValidFolder(selectedPath))
        {
            Debug.LogWarning("[SpawnAllPresetsGenerator] Select a folder that contains spawner json files first.");
            return;
        }

        GenerateFromPath(selectedPath);
    }

    [MenuItem(AssetMenuRoot + "/Generate SpawnSequenceSO From Json Folder", true)]
    public static bool ValidateGenerateFromSelectedFolder()
    {
        string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);

        return !string.IsNullOrEmpty(selectedPath) &&
            AssetDatabase.IsValidFolder(selectedPath);
    }

    public static IReadOnlyList<SpawnSequenceSO> GenerateFromPath(
        string inputPath,
        bool includeSubFolders = true)
    {
        List<SpawnSequenceSO> results = new();
        IReadOnlyList<string> jsonFiles = CollectSpawnerJsonFiles(inputPath, includeSubFolders);

        foreach (string jsonFile in jsonFiles)
        {
            IReadOnlyList<SpawnSequenceSO> generated = GenerateFromJsonPath(jsonFile, false);
            results.AddRange(generated);
        }

        Debug.Log($"[SpawnAllPresetsGenerator] Folder generation completed. Path={inputPath}, JsonFiles={jsonFiles.Count}, Sequences={results.Count}");

        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("완료", $"스포너 JSON 폴더 변환이 완료되었습니다.\nJsonFiles={jsonFiles.Count}\nSequences={results.Count}", "확인");
        }

        return results;
    }

    public static IReadOnlyList<SpawnSequenceSO> GenerateFromJsonPath(
        string jsonPath,
        bool showDialog = true)
    {
        Debug.Log("[SpawnAllPresetsGenerator] 프리셋 자동 생성 프로세스 시작...");

        List<SpawnSequenceSO> generatedSequences = new();

        if (string.IsNullOrEmpty(jsonPath) ||
            !jsonPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogError($"[SpawnAllPresetsGenerator] Invalid sequence json path. path={jsonPath}");
            return generatedSequences;
        }

        string spawnContentOutputFolder = $"{BaseGeneratedFolder}/SpawnContents";
        if (!Directory.Exists(spawnContentOutputFolder)) Directory.CreateDirectory(spawnContentOutputFolder);

        string sequencesOutputFolder = $"{BaseGeneratedFolder}/Sequences";
        if (!Directory.Exists(sequencesOutputFolder)) Directory.CreateDirectory(sequencesOutputFolder);

        int sequenceCount = 0;
        if (!File.Exists(jsonPath))
        {
            Debug.LogWarning($"[SpawnAllPresetsGenerator] 시퀀스 JSON 파일이 없습니다: {jsonPath}");
        }
        else
        {
            string sequencesText = File.ReadAllText(jsonPath);
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
                        generatedSequences.Add(sequenceAsset);
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

        if (showDialog && !Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("완료", "시퀀스 프리셋과 inline 스폰 컨텐츠 에셋이 성공적으로 생성/갱신되었습니다!", "확인");
        }

        return generatedSequences;
    }

    public static IReadOnlyList<string> CollectSpawnerJsonFiles(
        string inputPath,
        bool includeSubFolders = true)
    {
        List<string> result = new();

        if (string.IsNullOrEmpty(inputPath))
        {
            return result;
        }

        string normalizedInputPath = inputPath.Replace("\\", "/");

        if (File.Exists(normalizedInputPath))
        {
            if (IsSpawnerJson(normalizedInputPath))
            {
                result.Add(normalizedInputPath);
            }

            return result;
        }

        if (!Directory.Exists(normalizedInputPath))
        {
            return result;
        }

        SearchOption option = includeSubFolders
            ? SearchOption.AllDirectories
            : SearchOption.TopDirectoryOnly;

        foreach (string file in Directory.GetFiles(normalizedInputPath, "*.json", option))
        {
            string assetPath = file.Replace("\\", "/");
            if (IsSpawnerJson(assetPath))
            {
                result.Add(assetPath);
            }
        }

        result.Sort(StringComparer.OrdinalIgnoreCase);
        return result;
    }

    public static bool IsSpawnerJson(string jsonPath)
    {
        if (string.IsNullOrEmpty(jsonPath) ||
            !jsonPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ||
            !File.Exists(jsonPath))
        {
            return false;
        }

        try
        {
            string json = File.ReadAllText(jsonPath);

            if (string.IsNullOrWhiteSpace(json) ||
                (!json.Contains("\"sequenceId\"") && !json.Contains("\"sequences\"")))
            {
                return false;
            }

            return ParseSequenceJson(json).Count > 0;
        }
        catch
        {
            return false;
        }
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

                content = ReloadContentAsset(content);

                asset.AddStep(new SpawnSequenceStep(
                    step.order,
                    step.startDelay,
                    content,
                    ParseCompletionMode(step.completionMode)));
            }
        }

        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssetIfDirty(asset);
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

        SpawnSequenceSO savedAsset = AssetDatabase.LoadAssetAtPath<SpawnSequenceSO>(assetPath);
        if (savedAsset == null)
        {
            Debug.LogError($"[SpawnAllPresetsGenerator] Sequence '{seq.sequenceId}' 저장 후 재로드에 실패했습니다. path={assetPath}");
            return asset;
        }

        ValidateSavedSequence(savedAsset, seq.sequenceId);
        return savedAsset;
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

    private static SpawnContentSO ReloadContentAsset(SpawnContentSO content)
    {
        if (content == null)
        {
            return null;
        }

        string contentPath = AssetDatabase.GetAssetPath(content);
        if (string.IsNullOrEmpty(contentPath))
        {
            return content;
        }

        AssetDatabase.ImportAsset(contentPath, ImportAssetOptions.ForceUpdate);
        SpawnContentSO reloaded = AssetDatabase.LoadAssetAtPath<SpawnContentSO>(contentPath);
        return reloaded != null ? reloaded : content;
    }

    private static void ValidateSavedSequence(SpawnSequenceSO sequence, string sequenceId)
    {
        if (sequence == null || sequence.Steps == null)
        {
            Debug.LogError($"[SpawnAllPresetsGenerator] Sequence 저장 검증 실패. sequenceId={sequenceId}");
            return;
        }

        for (int i = 0; i < sequence.Steps.Count; i++)
        {
            SpawnSequenceStep step = sequence.Steps[i];
            SpawnSquadSO squad = step?.Content as SpawnSquadSO;

            if (squad == null)
            {
                continue;
            }

            if (squad.Groups == null || squad.Groups.Count == 0)
            {
                Debug.LogError($"[SpawnAllPresetsGenerator] Sequence '{sequenceId}' step order {step.Order}의 SpawnSquadSO groups가 비어 있습니다. contentId={squad.ContentId}, path={AssetDatabase.GetAssetPath(squad)}");
            }
        }
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
