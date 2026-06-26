#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Character;

public sealed class SpawnSOFromJsonWindow : EditorWindow
{
    // --- JSON 파싱 및 데이터 래핑용 클래스 ---
    [Serializable]
    private class JsonPatternListWrapper
    {
        public List<JsonSpawnPattern> patterns;
    }

    [Serializable]
    private class JsonSquadListWrapper
    {
        public List<JsonSpawnSquad> squads;
    }

    [Serializable]
    private class JsonFormationListWrapper
    {
        public List<JsonSpawnFormation> formations;
    }

    [Serializable]
    private class JsonSpawnSequence
    {
        public string sequenceId;
        public string displayName;
        public string repeatMode;
        public int loopStartOrder;
        public List<JsonSpawnSequenceStep> steps;
    }

    [Serializable]
    private class JsonSpawnSequenceStep
    {
        public int order;
        public float startDelay;
        public string contentId;
        public string completionMode;
    }

    [Serializable]
    private class JsonSequenceListWrapper
    {
        public List<JsonSpawnSequence> sequences;
    }

    [Serializable]
    private class JsonAllInOnePreset
    {
        public List<JsonSpawnPattern> patterns;
        public List<JsonSpawnSquad> squads;
        public List<JsonSpawnFormation> formations;
        public List<JsonSpawnSequence> sequences;
    }

    private List<UnityEngine.Object> patternJsonFiles = new List<UnityEngine.Object>();
    private PatternTargetType batchTargetType = PatternTargetType.Squad;
    private List<UnityEngine.Object> squadJsonFiles = new List<UnityEngine.Object>();
    private List<UnityEngine.Object> formationJsonFiles = new List<UnityEngine.Object>();
    private List<UnityEngine.Object> sequenceJsonFiles = new List<UnityEngine.Object>();
    private UnityEngine.Object singlePresetJsonFile; // 일괄 프리셋용

    private SpawnNpcPoolSO npcPoolAsset;
    private SpawnContentPoolSO contentPoolAsset;
    private SpawnPatternPoolSO patternPoolAsset;

    private string baseOutputFolder = "Assets/Scripts/battle_spawn/Resource/Generated";
    private Vector2 scrollPos;

    [MenuItem("BS/Spawn/Spawn SO FromJson Window")]
    public static void ShowWindow()
    {
        GetWindow<SpawnSOFromJsonWindow>("SO FromJson Converter");
    }

    private void OnEnable()
    {
        FindDefaultPoolAssets();
    }

    private void FindDefaultPoolAssets()
    {
        if (npcPoolAsset == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:SpawnNpcPoolSO");
            if (guids.Length > 0)
                npcPoolAsset = AssetDatabase.LoadAssetAtPath<SpawnNpcPoolSO>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
        if (contentPoolAsset == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:SpawnContentPoolSO");
            if (guids.Length > 0)
                contentPoolAsset = AssetDatabase.LoadAssetAtPath<SpawnContentPoolSO>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
        if (patternPoolAsset == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:SpawnPatternPoolSO");
            if (guids.Length > 0)
                patternPoolAsset = AssetDatabase.LoadAssetAtPath<SpawnPatternPoolSO>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Spawn SO FromJson Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // --- 공통 풀 에셋 설정 ---
        GUI.backgroundColor = new Color(0.95f, 0.97f, 1.0f);
        EditorGUILayout.BeginVertical("box");
        GUI.backgroundColor = Color.white;
        EditorGUILayout.LabelField("1. 공통 에셋 및 풀 설정", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        npcPoolAsset = (SpawnNpcPoolSO)EditorGUILayout.ObjectField("NPC 풀 에셋 (NpcPoolSO)", npcPoolAsset, typeof(SpawnNpcPoolSO), false);
        contentPoolAsset = (SpawnContentPoolSO)EditorGUILayout.ObjectField("콘텐츠 풀 에셋 (ContentPoolSO)", contentPoolAsset, typeof(SpawnContentPoolSO), false);
        patternPoolAsset = (SpawnPatternPoolSO)EditorGUILayout.ObjectField("패턴 풀 에셋 (PatternPoolSO)", patternPoolAsset, typeof(SpawnPatternPoolSO), false);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("기본 출력 루트 폴더", EditorStyles.miniBoldLabel);
        EditorGUILayout.BeginHorizontal();
        baseOutputFolder = EditorGUILayout.TextField(baseOutputFolder);
        if (GUILayout.Button("Browse...", GUILayout.Width(75)))
        {
            string selectedFolder = EditorUtility.OpenFolderPanel("출력 루트 폴더 선택", Application.dataPath, "");
            if (!string.IsNullOrEmpty(selectedFolder))
            {
                if (selectedFolder.StartsWith(Application.dataPath))
                {
                    baseOutputFolder = "Assets" + selectedFolder.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("경고", "프로젝트(Assets) 외부 폴더는 선택할 수 없습니다.", "확인");
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // --- PART A: 개별 JSON 변환 기능 ---
        GUI.backgroundColor = new Color(0.95f, 0.95f, 0.95f);
        EditorGUILayout.BeginVertical("box");
        GUI.backgroundColor = Color.white;
        EditorGUILayout.LabelField("2. 개별 JSON 변환 도구 (Individual Convert)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // (1) 패턴 JSON 변환
        EditorGUILayout.BeginVertical("helpBox");
        EditorGUILayout.LabelField("■ 패턴 JSON 파일 변환", EditorStyles.boldLabel);
        batchTargetType = (PatternTargetType)EditorGUILayout.EnumPopup("패턴 타깃 용도", batchTargetType);
        
        Rect patDrag = GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true));
        GUI.Box(patDrag, "여기에 패턴 JSON 드롭", EditorStyles.helpBox);
        HandleDragAndDrop(patDrag, patternJsonFiles);

        for (int i = 0; i < patternJsonFiles.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            patternJsonFiles[i] = EditorGUILayout.ObjectField($"패턴 JSON {i + 1}", patternJsonFiles[i], typeof(UnityEngine.Object), false);
            if (GUILayout.Button("제거", GUILayout.Width(45))) { patternJsonFiles.RemoveAt(i); i--; }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("슬롯 추가")) patternJsonFiles.Add(null);
        if (GUILayout.Button("모두 비우기")) patternJsonFiles.Clear();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Generate Patterns (패턴 개별 변환 실행)", GUILayout.Height(35)))
        {
            GenerateIndividualPatterns();
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // (2) 콘텐츠 (스쿼드 / 포메이션) JSON 변환
        EditorGUILayout.BeginVertical("helpBox");
        EditorGUILayout.LabelField("■ 스쿼드 & 포메이션 JSON 파일 변환", EditorStyles.boldLabel);
        
        Rect squadDrag = GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true));
        GUI.Box(squadDrag, "여기에 Squad JSON 드롭", EditorStyles.helpBox);
        HandleDragAndDrop(squadDrag, squadJsonFiles);
        for (int i = 0; i < squadJsonFiles.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            squadJsonFiles[i] = EditorGUILayout.ObjectField($"스쿼드 JSON {i + 1}", squadJsonFiles[i], typeof(UnityEngine.Object), false);
            if (GUILayout.Button("제거", GUILayout.Width(45))) { squadJsonFiles.RemoveAt(i); i--; }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        Rect formDrag = GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true));
        GUI.Box(formDrag, "여기에 Formation JSON 드롭", EditorStyles.helpBox);
        HandleDragAndDrop(formDrag, formationJsonFiles);
        for (int i = 0; i < formationJsonFiles.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            formationJsonFiles[i] = EditorGUILayout.ObjectField($"포메이션 JSON {i + 1}", formationJsonFiles[i], typeof(UnityEngine.Object), false);
            if (GUILayout.Button("제거", GUILayout.Width(45))) { formationJsonFiles.RemoveAt(i); i--; }
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("스쿼드 추가")) squadJsonFiles.Add(null);
        if (GUILayout.Button("포메이션 추가")) formationJsonFiles.Add(null);
        if (GUILayout.Button("모두 비우기")) { squadJsonFiles.Clear(); formationJsonFiles.Clear(); }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Generate Contents (스쿼드/포메이션 개별 변환 실행)", GUILayout.Height(35)))
        {
            GenerateIndividualContents();
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // (3) 시퀀스 JSON 변환
        EditorGUILayout.BeginVertical("helpBox");
        EditorGUILayout.LabelField("■ 시퀀스(Sequence) JSON 파일 변환", EditorStyles.boldLabel);
        
        Rect seqDrag = GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true));
        GUI.Box(seqDrag, "여기에 시퀀스 JSON 드롭", EditorStyles.helpBox);
        HandleDragAndDrop(seqDrag, sequenceJsonFiles);

        for (int i = 0; i < sequenceJsonFiles.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            sequenceJsonFiles[i] = EditorGUILayout.ObjectField($"시퀀스 JSON {i + 1}", sequenceJsonFiles[i], typeof(UnityEngine.Object), false);
            if (GUILayout.Button("제거", GUILayout.Width(45))) { sequenceJsonFiles.RemoveAt(i); i--; }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("슬롯 추가")) sequenceJsonFiles.Add(null);
        if (GUILayout.Button("모두 비우기")) sequenceJsonFiles.Clear();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Generate Sequences (시퀀스 개별 변환 실행)", GUILayout.Height(35)))
        {
            GenerateIndividualSequences();
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // --- PART B: 단일 JSON 파일에서 일괄 생성 (Bake All) ---
        GUI.backgroundColor = new Color(0.9f, 0.95f, 0.9f);
        EditorGUILayout.BeginVertical("box");
        GUI.backgroundColor = Color.white;
        EditorGUILayout.LabelField("3. 통합 JSON 프리셋 일괄 변환 (Bake All from Single JSON)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        singlePresetJsonFile = EditorGUILayout.ObjectField("All-in-one JSON 파일", singlePresetJsonFile, typeof(UnityEngine.Object), false);

        EditorGUILayout.Space();
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Bake All from Single JSON (일괄 프리셋 생성)", GUILayout.Height(35)))
        {
            BakeAllFromSingleJson();
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.EndScrollView();
    }

    private void GenerateIndividualPatterns()
    {
        string sub = (batchTargetType == PatternTargetType.Squad) ? "Squads" : "Formations";
        string targetDir = $"{baseOutputFolder}/Patterns/{sub}";
        if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

        int count = 0;
        foreach (var file in patternJsonFiles)
        {
            if (file == null) continue;
            string text = File.ReadAllText(AssetDatabase.GetAssetPath(file));
            List<JsonSpawnPattern> plist = ParseJsonList<JsonSpawnPattern, JsonPatternListWrapper>(text, "patterns", w => w.patterns);
            foreach (var pat in plist)
            {
                var asset = SpawnPatternBuilder.Build(pat, targetDir);
                if (asset != null) count++;
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        RefreshPatternPool();
        EditorUtility.DisplayDialog("완료", $"총 {count}개의 패턴 에셋을 생성/갱신했습니다.", "확인");
    }

    private void GenerateIndividualContents()
    {
        if (npcPoolAsset == null || patternPoolAsset == null)
        {
            EditorUtility.DisplayDialog("오류", "NPC 풀과 패턴 풀 에셋 지정이 필요합니다.", "확인");
            return;
        }

        Dictionary<string, CharacterSO> npcDict = BuildNpcDictionary();
        Dictionary<string, SpawnPattern> patDict = GetActivePatternsDictionary();
        Dictionary<string, SpawnSquadSO> squadDict = LoadSquadsDictionary($"{baseOutputFolder}/SpawnContents");

        int squadCount = 0;
        foreach (var file in squadJsonFiles)
        {
            if (file == null) continue;
            string text = File.ReadAllText(AssetDatabase.GetAssetPath(file));
            List<JsonSpawnSquad> slist = ParseJsonList<JsonSpawnSquad, JsonSquadListWrapper>(text, "squads", w => w.squads);
            foreach (var s in slist)
            {
                var asset = SpawnContentBuilder.BuildSquad(s, $"{baseOutputFolder}/SpawnContents", npcDict, patDict);
                if (asset != null)
                {
                    squadCount++;
                    squadDict[asset.ContentId] = asset;
                }
            }
        }

        int formCount = 0;
        foreach (var file in formationJsonFiles)
        {
            if (file == null) continue;
            string text = File.ReadAllText(AssetDatabase.GetAssetPath(file));
            List<JsonSpawnFormation> flist = ParseJsonList<JsonSpawnFormation, JsonFormationListWrapper>(text, "formations", w => w.formations);
            foreach (var f in flist)
            {
                var asset = SpawnContentBuilder.BuildFormation(f, $"{baseOutputFolder}/SpawnContents", squadDict, patDict);
                if (asset != null) formCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        RefreshContentPool();
        EditorUtility.DisplayDialog("완료", $"Squad {squadCount}개, Formation {formCount}개 에셋을 생성/갱신했습니다.", "확인");
    }

    private void GenerateIndividualSequences()
    {
        string targetDir = $"{baseOutputFolder}/Sequences";
        if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

        Dictionary<string, SpawnContentSO> contentDict = LoadAllContentsDictionary();

        int count = 0;
        foreach (var file in sequenceJsonFiles)
        {
            if (file == null) continue;
            string text = File.ReadAllText(AssetDatabase.GetAssetPath(file));
            List<JsonSpawnSequence> seqs = ParseJsonList<JsonSpawnSequence, JsonSequenceListWrapper>(text, "sequences", w => w.sequences);
            foreach (var seq in seqs)
            {
                if (BuildSequenceSO(seq, targetDir, contentDict) != null) count++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료", $"Sequence {count}개 에셋을 생성/갱신했습니다.", "확인");
    }

    private void BakeAllFromSingleJson()
    {
        if (singlePresetJsonFile == null)
        {
            EditorUtility.DisplayDialog("오류", "All-in-one JSON 프리셋 파일을 할당해주세요.", "확인");
            return;
        }

        string jsonPath = AssetDatabase.GetAssetPath(singlePresetJsonFile);
        string jsonText = File.ReadAllText(jsonPath);

        JsonAllInOnePreset preset = null;
        try
        {
            preset = UnityEngine.JsonUtility.FromJson<JsonAllInOnePreset>(jsonText);
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("오류", $"JSON 파싱 실패:\n{ex.Message}", "확인");
            return;
        }

        if (preset == null)
        {
            EditorUtility.DisplayDialog("오류", "JSON 데이터가 비어있거나 올바른 형식이 아닙니다.", "확인");
            return;
        }

        // 1. 전체 NPC CharacterSO 룩업 캐시 구축
        Dictionary<string, CharacterSO> npcDict = BuildNpcDictionary();

        // 2. 패턴 생성
        Dictionary<string, SpawnPattern> patternDict = new Dictionary<string, SpawnPattern>();
        if (preset.patterns != null)
        {
            foreach (var pat in preset.patterns)
            {
                if (string.IsNullOrEmpty(pat.patternId)) continue;
                bool isForm = pat.patternId.Contains("formation") || pat.patternId.Contains("format");
                string subFolder = isForm ? "Formations" : "Squads";
                string targetDir = $"{baseOutputFolder}/Patterns/{subFolder}";
                if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

                SpawnPattern patAsset = SpawnPatternBuilder.Build(pat, targetDir);
                if (patAsset != null)
                {
                    patternDict[patAsset.name] = patAsset;
                }
            }
        }

        // 3. 스쿼드 생성
        Dictionary<string, SpawnSquadSO> squadDict = new Dictionary<string, SpawnSquadSO>();
        if (preset.squads != null)
        {
            string contentDir = $"{baseOutputFolder}/SpawnContents";
            foreach (var sqd in preset.squads)
            {
                if (string.IsNullOrEmpty(sqd.contentId)) continue;
                SpawnSquadSO squadAsset = SpawnContentBuilder.BuildSquad(sqd, contentDir, npcDict, patternDict);
                if (squadAsset != null)
                {
                    squadDict[squadAsset.ContentId] = squadAsset;
                }
            }
        }

        // 4. 포메이션 생성
        Dictionary<string, SpawnFormationSO> formationDict = new Dictionary<string, SpawnFormationSO>();
        if (preset.formations != null)
        {
            string contentDir = $"{baseOutputFolder}/SpawnContents";
            foreach (var form in preset.formations)
            {
                if (string.IsNullOrEmpty(form.contentId)) continue;
                SpawnFormationSO formAsset = SpawnContentBuilder.BuildFormation(form, contentDir, squadDict, patternDict);
                if (formAsset != null)
                {
                    formationDict[formAsset.ContentId] = formAsset;
                }
            }
        }

        // 5. 시퀀스 생성
        Dictionary<string, SpawnContentSO> combinedContents = new Dictionary<string, SpawnContentSO>();
        foreach (var kv in squadDict) combinedContents[kv.Key] = kv.Value;
        foreach (var kv in formationDict) combinedContents[kv.Key] = kv.Value;

        int seqCount = 0;
        if (preset.sequences != null)
        {
            string seqDir = $"{baseOutputFolder}/Sequences";
            if (!Directory.Exists(seqDir)) Directory.CreateDirectory(seqDir);

            foreach (var seq in preset.sequences)
            {
                if (BuildSequenceSO(seq, seqDir, combinedContents) != null) seqCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        RefreshPatternPool();
        RefreshContentPool();

        EditorUtility.DisplayDialog("Bake All 완료", 
            $"통합 JSON에서 일괄 생성을 수행했습니다!\n" +
            $"- 패턴: {patternDict.Count}개\n" +
            $"- 스쿼드: {squadDict.Count}개\n" +
            $"- 포메이션: {formationDict.Count}개\n" +
            $"- 시퀀스: {seqCount}개", "확인");
    }

    private void HandleDragAndDrop(Rect dropArea, List<UnityEngine.Object> fileList)
    {
        Event currentEvent = Event.current;
        if (!dropArea.Contains(currentEvent.mousePosition)) return;

        if (currentEvent.type == EventType.DragUpdated)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            currentEvent.Use();
        }
        else if (currentEvent.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj != null)
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    if (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!fileList.Contains(obj))
                        {
                            fileList.Add(obj);
                        }
                    }
                }
            }
            currentEvent.Use();
        }
    }

    /// <summary>
    /// JSON 텍스트를 파싱해 항목 리스트를 반환합니다.
    /// 배열([]), 래퍼 오브젝트({\'listKey\':[...]}), 단일 오브젝트({...}) 세 가지 형태를 모두 지원합니다.
    /// </summary>
    private List<TItem> ParseJsonList<TItem, TWrapper>(
        string text,
        string listKey,
        Func<TWrapper, List<TItem>> listSelector)
        where TItem : class, new()
        where TWrapper : class, new()
    {
        List<TItem> results = new List<TItem>();
        text = text.Trim();
        if (text.Length > 0 && text[0] == '\uFEFF') text = text.Substring(1).Trim();

        if (text.StartsWith("["))
        {
            string wrappedJson = $"{{\"{listKey}\":" + text + "}";
            try
            {
                var wrapper = UnityEngine.JsonUtility.FromJson<TWrapper>(wrappedJson);
                var list = wrapper != null ? listSelector(wrapper) : null;
                if (list != null) results.AddRange(list);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SpawnSOFromJsonWindow] {listKey} JSON 배열 파싱 실패: {ex.Message}");
            }
        }
        else if (text.StartsWith("{"))
        {
            if (text.Contains($"\"{listKey}\""))
            {
                try
                {
                    var wrapper = UnityEngine.JsonUtility.FromJson<TWrapper>(text);
                    var list = wrapper != null ? listSelector(wrapper) : null;
                    if (list != null) results.AddRange(list);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SpawnSOFromJsonWindow] {listKey} JSON 리스트 파싱 실패: {ex.Message}");
                }
            }
            else
            {
                try
                {
                    var singleItem = UnityEngine.JsonUtility.FromJson<TItem>(text);
                    if (singleItem != null) results.Add(singleItem);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SpawnSOFromJsonWindow] {listKey} 단일 JSON 파싱 실패: {ex.Message}");
                }
            }
        }
        return results;
    }

    /// <summary>
    /// JsonSpawnSequence 데이터로부터 SpawnSequenceSO 에셋을 생성하거나 갱신합니다.
    /// </summary>
    private SpawnSequenceSO BuildSequenceSO(
        JsonSpawnSequence seq,
        string targetDir,
        Dictionary<string, SpawnContentSO> contentDict)
    {
        if (string.IsNullOrEmpty(seq.sequenceId)) return null;

        string path = $"{targetDir}/{seq.sequenceId}.asset";
        SpawnSequenceSO asset = AssetDatabase.LoadAssetAtPath<SpawnSequenceSO>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<SpawnSequenceSO>();
            AssetDatabase.CreateAsset(asset, path);
        }

        SpawnSequenceRepeatMode repeatMode = SpawnSequenceRepeatMode.Once;
        if (!string.IsNullOrEmpty(seq.repeatMode) && seq.repeatMode.Equals("Infinite", StringComparison.OrdinalIgnoreCase))
            repeatMode = SpawnSequenceRepeatMode.Infinite;

        asset.Initialize(seq.sequenceId, seq.displayName, repeatMode, seq.loopStartOrder);

        if (seq.steps != null)
        {
            foreach (var stepVal in seq.steps)
            {
                SpawnContentSO cAsset = null;
                if (!string.IsNullOrEmpty(stepVal.contentId))
                    contentDict.TryGetValue(stepVal.contentId, out cAsset);

                SpawnStepCompletionMode mode = SpawnStepCompletionMode.AfterSpawnCompleted;
                if (!string.IsNullOrEmpty(stepVal.completionMode) && stepVal.completionMode.Equals("AfterSpawnedEnemiesDefeated", StringComparison.OrdinalIgnoreCase))
                    mode = SpawnStepCompletionMode.AfterSpawnedEnemiesDefeated;

                asset.AddStep(new SpawnSequenceStep(stepVal.order, stepVal.startDelay, cAsset, mode));
            }
        }

        EditorUtility.SetDirty(asset);
        return asset;
    }

    private Dictionary<string, CharacterSO> BuildNpcDictionary()
    {
        Dictionary<string, CharacterSO> npcDict = new Dictionary<string, CharacterSO>();
        if (npcPoolAsset != null && npcPoolAsset.Npcs != null)
        {
            foreach (var n in npcPoolAsset.Npcs)
            {
                if (n != null && !npcDict.ContainsKey(n.CharacterId)) npcDict.Add(n.CharacterId, n);
            }
        }
        string[] npcGuids = AssetDatabase.FindAssets("t:CharacterSO");
        foreach (var guid in npcGuids)
        {
            CharacterSO c = AssetDatabase.LoadAssetAtPath<CharacterSO>(AssetDatabase.GUIDToAssetPath(guid));
            if (c != null && !string.IsNullOrEmpty(c.CharacterId) && !npcDict.ContainsKey(c.CharacterId))
            {
                npcDict.Add(c.CharacterId, c);
            }
        }
        return npcDict;
    }

    private Dictionary<string, SpawnPattern> GetActivePatternsDictionary()
    {
        Dictionary<string, SpawnPattern> patDict = new Dictionary<string, SpawnPattern>();
        if (patternPoolAsset != null && patternPoolAsset.Patterns != null)
        {
            foreach (var p in patternPoolAsset.Patterns)
            {
                if (p == null) continue;
                string path = AssetDatabase.GetAssetPath(p);
                SpawnPattern so = AssetDatabase.LoadAssetAtPath<SpawnPattern>(path);
                if (so != null && !patDict.ContainsKey(so.name)) patDict.Add(so.name, so);
            }
        }
        return patDict;
    }

    private Dictionary<string, SpawnSquadSO> LoadSquadsDictionary(string contentDir)
    {
        Dictionary<string, SpawnSquadSO> squadDict = new Dictionary<string, SpawnSquadSO>();
        if (Directory.Exists(contentDir))
        {
            string[] guids = AssetDatabase.FindAssets("t:SpawnSquadSO", new[] { contentDir });
            foreach (var guid in guids)
            {
                SpawnSquadSO s = AssetDatabase.LoadAssetAtPath<SpawnSquadSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (s != null && !squadDict.ContainsKey(s.ContentId)) squadDict.Add(s.ContentId, s);
            }
        }
        return squadDict;
    }

    private Dictionary<string, SpawnContentSO> LoadAllContentsDictionary()
    {
        Dictionary<string, SpawnContentSO> contentDict = new Dictionary<string, SpawnContentSO>();
        string[] guids = AssetDatabase.FindAssets("t:SpawnContentSO");
        foreach (var guid in guids)
        {
            SpawnContentSO c = AssetDatabase.LoadAssetAtPath<SpawnContentSO>(AssetDatabase.GUIDToAssetPath(guid));
            if (c != null && !contentDict.ContainsKey(c.ContentId)) contentDict.Add(c.ContentId, c);
        }
        return contentDict;
    }

    private void RefreshPatternPool()
    {
        if (patternPoolAsset != null)
        {
            patternPoolAsset.CollectAllPatterns();
            EditorUtility.SetDirty(patternPoolAsset);
        }
    }

    private void RefreshContentPool()
    {
        if (contentPoolAsset != null)
        {
            contentPoolAsset.CollectAllContents();
            EditorUtility.SetDirty(contentPoolAsset);
        }
    }

}
#endif
