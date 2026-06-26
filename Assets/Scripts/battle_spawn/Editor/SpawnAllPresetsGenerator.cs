#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Character;

public static class SpawnAllPresetsGenerator
{
    private const string BaseGeneratedFolder = "Assets/Scripts/battle_spawn/Resource/Generated";
    private const string PatternsJsonPath = "Assets/Scripts/battle_spawn/Resource/Generated/Patterns/patterns_preset.json";
    private const string SquadsJsonPath = "Assets/Scripts/battle_spawn/Resource/Generated/SpawnContent/squads_preset.json";
    private const string FormationsJsonPath = "Assets/Scripts/battle_spawn/Resource/Generated/SpawnContent/formations_preset.json";

    [MenuItem("BS/Spawn/Generate All Preset Assets")]
    public static void GenerateAllPresets()
    {
        Debug.Log("[SpawnAllPresetsGenerator] 프리셋 자동 생성 프로세스 시작...");

        // 1. NPC CharacterSO 직접 로드 (squad.wolf.single 에셋에 사용되는 character.wolf.easy.1)
        string npcAssetPath = "Assets/Resources/character/NPC/Ash Wolf Black Female Guard/character_wolf_easy_1.asset";
        CharacterSO npcSO = AssetDatabase.LoadAssetAtPath<CharacterSO>(npcAssetPath);
        if (npcSO == null)
        {
            Debug.LogError($"[SpawnAllPresetsGenerator] NPC CharacterSO 에셋을 찾을 수 없습니다: '{npcAssetPath}'");
            EditorUtility.DisplayDialog("오류", $"NPC 에셋 로드 실패:\n{npcAssetPath}", "확인");
            return;
        }

        Dictionary<string, CharacterSO> npcPool = new Dictionary<string, CharacterSO>
        {
            { "character.wolf.easy.1", npcSO }
        };

        // 2. 패턴(SpawnPattern) SO 생성
        if (!File.Exists(PatternsJsonPath))
        {
            Debug.LogError($"[SpawnAllPresetsGenerator] 패턴 JSON 파일이 없습니다: {PatternsJsonPath}");
            return;
        }

        string patternsText = File.ReadAllText(PatternsJsonPath);
        List<JsonSpawnPattern> jsonPatterns = ParsePatternJson(patternsText);
        Dictionary<string, SpawnPattern> patternPool = new Dictionary<string, SpawnPattern>();

        string patternsOutputFolder = $"{BaseGeneratedFolder}/Patterns";
        if (!Directory.Exists(patternsOutputFolder)) Directory.CreateDirectory(patternsOutputFolder);

        foreach (var jp in jsonPatterns)
        {
            if (string.IsNullOrEmpty(jp.patternId)) continue;
            try
            {
                SpawnPattern patternAsset = SpawnPatternBuilder.Build(jp, patternsOutputFolder);
                if (patternAsset != null)
                {
                    patternPool[patternAsset.name] = patternAsset;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SpawnAllPresetsGenerator] 패턴 '{jp.patternId}' 생성 중 에러: {ex.Message}");
            }
        }
        Debug.Log($"[SpawnAllPresetsGenerator] SpawnPattern SO {patternPool.Count}개 생성/갱신 완료.");

        // 3. 스쿼드(SpawnSquadSO) SO 생성
        if (!File.Exists(SquadsJsonPath))
        {
            Debug.LogError($"[SpawnAllPresetsGenerator] 스쿼드 JSON 파일이 없습니다: {SquadsJsonPath}");
            return;
        }

        string squadsText = File.ReadAllText(SquadsJsonPath);
        List<JsonSpawnSquad> jsonSquads = ParseSquadJson(squadsText);
        Dictionary<string, SpawnSquadSO> squadPool = new Dictionary<string, SpawnSquadSO>();

        string spawnContentOutputFolder = $"{BaseGeneratedFolder}/SpawnContent";
        if (!Directory.Exists(spawnContentOutputFolder)) Directory.CreateDirectory(spawnContentOutputFolder);

        foreach (var js in jsonSquads)
        {
            if (string.IsNullOrEmpty(js.contentId)) continue;
            try
            {
                SpawnSquadSO squadAsset = SpawnContentBuilder.BuildSquad(js, spawnContentOutputFolder, npcPool, patternPool);
                if (squadAsset != null)
                {
                    squadPool[squadAsset.ContentId] = squadAsset;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SpawnAllPresetsGenerator] 스쿼드 '{js.contentId}' 생성 중 에러: {ex.Message}");
            }
        }
        Debug.Log($"[SpawnAllPresetsGenerator] SpawnSquad SO {squadPool.Count}개 생성/갱신 완료.");

        // 4. 포메이션(SpawnFormationSO) SO 생성
        if (!File.Exists(FormationsJsonPath))
        {
            Debug.LogError($"[SpawnAllPresetsGenerator] 포메이션 JSON 파일이 없습니다: {FormationsJsonPath}");
            return;
        }

        string formationsText = File.ReadAllText(FormationsJsonPath);
        List<JsonSpawnFormation> jsonFormations = ParseFormationJson(formationsText);
        Dictionary<string, SpawnFormationSO> formationPool = new Dictionary<string, SpawnFormationSO>();

        foreach (var jf in jsonFormations)
        {
            if (string.IsNullOrEmpty(jf.contentId)) continue;
            try
            {
                SpawnFormationSO formationAsset = SpawnContentBuilder.BuildFormation(jf, spawnContentOutputFolder, squadPool, patternPool);
                if (formationAsset != null)
                {
                    formationPool[formationAsset.ContentId] = formationAsset;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SpawnAllPresetsGenerator] 포메이션 '{jf.contentId}' 생성 중 에러: {ex.Message}");
            }
        }
        Debug.Log($"[SpawnAllPresetsGenerator] SpawnFormation SO {formationPool.Count}개 생성/갱신 완료.");

        // 5. 시퀀스(SpawnSequenceSO) 에셋 직접 생성
        string sequencesOutputFolder = $"{BaseGeneratedFolder}/Sequences";
        if (!Directory.Exists(sequencesOutputFolder)) Directory.CreateDirectory(sequencesOutputFolder);

        // 로드 확인
        SpawnContentSO formationLine5Single = GetContentFromPool(formationPool, "formation.wolf.line.5.single");
        SpawnContentSO formationCircle10Single = GetContentFromPool(formationPool, "formation.wolf.circle.10.single");
        SpawnContentSO formationCircle10Circle = GetContentFromPool(formationPool, "formation.wolf.circle.10.circle");
        SpawnContentSO formationLine5Trapezoid = GetContentFromPool(formationPool, "formation.wolf.line.5.trapezoid");

        if (formationLine5Single != null && formationCircle10Single != null && 
            formationCircle10Circle != null && formationLine5Trapezoid != null)
        {
            // 5-1. 고정 시퀀스 프리셋
            string fixedSeqId = "seq.wolf.preset.fixed";
            string fixedPath = $"{sequencesOutputFolder}/{fixedSeqId}.asset";
            SpawnSequenceSO fixedSeq = AssetDatabase.LoadAssetAtPath<SpawnSequenceSO>(fixedPath);
            if (fixedSeq == null)
            {
                fixedSeq = ScriptableObject.CreateInstance<SpawnSequenceSO>();
                AssetDatabase.CreateAsset(fixedSeq, fixedPath);
            }
            fixedSeq.Initialize(fixedSeqId, "늑대 고정 프리셋 시퀀스", SpawnSequenceRepeatMode.Once, 0);
            fixedSeq.AddStep(new SpawnSequenceStep(0, 0f, formationLine5Single, SpawnStepCompletionMode.AfterSpawnCompleted));
            fixedSeq.AddStep(new SpawnSequenceStep(1, 3.0f, formationCircle10Single, SpawnStepCompletionMode.AfterSpawnCompleted));
            fixedSeq.AddStep(new SpawnSequenceStep(2, 4.0f, formationCircle10Circle, SpawnStepCompletionMode.AfterSpawnedEnemiesDefeated));
            EditorUtility.SetDirty(fixedSeq);

            // 5-2. 루프 시퀀스 프리셋
            string loopSeqId = "seq.wolf.preset.loop";
            string loopPath = $"{sequencesOutputFolder}/{loopSeqId}.asset";
            SpawnSequenceSO loopSeq = AssetDatabase.LoadAssetAtPath<SpawnSequenceSO>(loopPath);
            if (loopSeq == null)
            {
                loopSeq = ScriptableObject.CreateInstance<SpawnSequenceSO>();
                AssetDatabase.CreateAsset(loopSeq, loopPath);
            }
            loopSeq.Initialize(loopSeqId, "늑대 루프 프리셋 시퀀스", SpawnSequenceRepeatMode.Infinite, 0);
            loopSeq.AddStep(new SpawnSequenceStep(0, 0f, formationLine5Trapezoid, SpawnStepCompletionMode.AfterSpawnCompleted));
            loopSeq.AddStep(new SpawnSequenceStep(1, 3.0f, formationCircle10Circle, SpawnStepCompletionMode.AfterSpawnCompleted));
            EditorUtility.SetDirty(loopSeq);

            Debug.Log("[SpawnAllPresetsGenerator] SpawnSequence SO 2개 생성/갱신 완료.");
        }
        else
        {
            Debug.LogError("[SpawnAllPresetsGenerator] 시퀀스 빌드에 필요한 포메이션 에셋들이 풀에 누락되었습니다.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 수집 가능한 전체 풀 에셋 갱신 (ContentPoolSO 및 PatternPoolSO 등)
        CollectAllPools();

        EditorUtility.DisplayDialog("완료", "모든 패턴, 스쿼드, 포메이션, 시퀀스 프리셋 에셋이 성공적으로 생성/갱신되었습니다!", "확인");
    }

    private static SpawnContentSO GetContentFromPool(Dictionary<string, SpawnFormationSO> pool, string id)
    {
        if (pool.TryGetValue(id, out var val)) return val;
        return null;
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

        string[] patternPoolGuids = AssetDatabase.FindAssets("t:SpawnPatternPoolSO");
        foreach (var guid in patternPoolGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SpawnPatternPoolSO pool = AssetDatabase.LoadAssetAtPath<SpawnPatternPoolSO>(path);
            if (pool != null)
            {
                pool.CollectAllPatterns();
                EditorUtility.SetDirty(pool);
            }
        }
    }

    // --- JSON 파싱 Helper 클래스 및 메서드 ---
    [Serializable]
    public class JsonPatternListWrapper { public List<JsonSpawnPattern> patterns; }
    [Serializable]
    public class JsonSquadListWrapper { public List<JsonSpawnSquad> squads; }
    [Serializable]
    public class JsonFormationListWrapper { public List<JsonSpawnFormation> formations; }

    private static List<JsonSpawnPattern> ParsePatternJson(string text)
    {
        List<JsonSpawnPattern> results = new List<JsonSpawnPattern>();
        text = text.Trim();
        if (text.Length > 0 && text[0] == '\uFEFF') text = text.Substring(1).Trim();

        if (text.StartsWith("["))
        {
            string wrapped = "{\"patterns\":" + text + "}";
            var wrapper = JsonUtility.FromJson<JsonPatternListWrapper>(wrapped);
            if (wrapper != null && wrapper.patterns != null) results.AddRange(wrapper.patterns);
        }
        else
        {
            var wrapper = JsonUtility.FromJson<JsonPatternListWrapper>(text);
            if (wrapper != null && wrapper.patterns != null) results.AddRange(wrapper.patterns);
        }
        return results;
    }

    private static List<JsonSpawnSquad> ParseSquadJson(string text)
    {
        List<JsonSpawnSquad> results = new List<JsonSpawnSquad>();
        text = text.Trim();
        if (text.Length > 0 && text[0] == '\uFEFF') text = text.Substring(1).Trim();

        if (text.StartsWith("["))
        {
            string wrapped = "{\"squads\":" + text + "}";
            var wrapper = JsonUtility.FromJson<JsonSquadListWrapper>(wrapped);
            if (wrapper != null && wrapper.squads != null) results.AddRange(wrapper.squads);
        }
        else
        {
            var wrapper = JsonUtility.FromJson<JsonSquadListWrapper>(text);
            if (wrapper != null && wrapper.squads != null) results.AddRange(wrapper.squads);
        }
        return results;
    }

    private static List<JsonSpawnFormation> ParseFormationJson(string text)
    {
        List<JsonSpawnFormation> results = new List<JsonSpawnFormation>();
        text = text.Trim();
        if (text.Length > 0 && text[0] == '\uFEFF') text = text.Substring(1).Trim();

        if (text.StartsWith("["))
        {
            string wrapped = "{\"formations\":" + text + "}";
            var wrapper = JsonUtility.FromJson<JsonFormationListWrapper>(wrapped);
            if (wrapper != null && wrapper.formations != null) results.AddRange(wrapper.formations);
        }
        else
        {
            var wrapper = JsonUtility.FromJson<JsonFormationListWrapper>(text);
            if (wrapper != null && wrapper.formations != null) results.AddRange(wrapper.formations);
        }
        return results;
    }
}
#endif
