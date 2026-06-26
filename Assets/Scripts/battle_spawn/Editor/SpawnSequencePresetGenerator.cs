#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class SpawnSequencePresetGenerator
{
    [MenuItem("BS/Spawn/Generate Sequence Presets")]
    public static void GeneratePresets()
    {
        string outputFolder = "Assets/Scripts/battle_spawn/Resource/Generated/Sequences";
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        // Find the generated SpawnContentSO assets
        SpawnContentSO squadLine3 = LoadContent("squad.wolf.line.3");
        SpawnContentSO squadCircle4 = LoadContent("squad.wolf.circle.4");
        SpawnContentSO squadGrid4 = LoadContent("squad.wolf.grid.4");
        SpawnContentSO formationTriangleDouble = LoadContent("formation.wolf.triangle.double");
        SpawnContentSO formationLineTriple = LoadContent("formation.wolf.line.triple");

        if (squadLine3 == null || squadCircle4 == null || squadGrid4 == null || 
            formationTriangleDouble == null || formationLineTriple == null)
        {
            Debug.LogError("[SpawnSequencePresetGenerator] 필요한 SpawnContentSO 에셋을 찾을 수 없습니다. 먼저 Content SO Generator를 통해 분대/포메이션 에셋을 생성했는지 확인하세요.");
            return;
        }

        // 1. 고정 시퀀스 프리셋 (Fixed Sequence)
        SpawnSequenceSO fixedSeq = ScriptableObject.CreateInstance<SpawnSequenceSO>();
        fixedSeq.Initialize("seq.wolf.fixed", "늑대 고정 웨이브 시퀀스", SpawnSequenceRepeatMode.Once, 0);
        fixedSeq.AddStep(new SpawnSequenceStep(0, 0f, squadLine3, SpawnStepCompletionMode.AfterSpawnCompleted));
        fixedSeq.AddStep(new SpawnSequenceStep(1, 2.0f, formationTriangleDouble, SpawnStepCompletionMode.AfterSpawnCompleted));
        fixedSeq.AddStep(new SpawnSequenceStep(2, 3.0f, formationLineTriple, SpawnStepCompletionMode.AfterSpawnedEnemiesDefeated));

        string fixedPath = $"{outputFolder}/seq.wolf.fixed.asset";
        AssetDatabase.CreateAsset(fixedSeq, fixedPath);

        // 2. 반복 시퀀스 프리셋 (Looping Sequence)
        SpawnSequenceSO loopSeq = ScriptableObject.CreateInstance<SpawnSequenceSO>();
        loopSeq.Initialize("seq.wolf.loop", "늑대 순환 웨이브 시퀀스", SpawnSequenceRepeatMode.Infinite, 0);
        loopSeq.AddStep(new SpawnSequenceStep(0, 0f, squadCircle4, SpawnStepCompletionMode.AfterSpawnCompleted));
        loopSeq.AddStep(new SpawnSequenceStep(1, 1.5f, squadGrid4, SpawnStepCompletionMode.AfterSpawnCompleted));
        loopSeq.AddStep(new SpawnSequenceStep(2, 2.0f, formationTriangleDouble, SpawnStepCompletionMode.AfterSpawnCompleted));

        string loopPath = $"{outputFolder}/seq.wolf.loop.asset";
        AssetDatabase.CreateAsset(loopSeq, loopPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[SpawnSequencePresetGenerator] 시퀀스 프리셋 에셋 생성 성공:\n1. {fixedPath}\n2. {loopPath}");
    }

    private static SpawnContentSO LoadContent(string contentId)
    {
        string[] guids = AssetDatabase.FindAssets("t:SpawnContentSO");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SpawnContentSO asset = AssetDatabase.LoadAssetAtPath<SpawnContentSO>(path);
            if (asset != null && asset.ContentId == contentId)
            {
                return asset;
            }
        }
        return null;
    }
}
#endif
