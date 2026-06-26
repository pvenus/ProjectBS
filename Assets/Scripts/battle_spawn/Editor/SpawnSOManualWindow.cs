#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Character;

public sealed class SpawnSOManualWindow : EditorWindow
{
    [Serializable]
    private class TempSquadGroup
    {
        public int order;
        public CharacterSO character;
        public SpawnPattern pattern;
        public Vector2 localOffset;
        public float localRotation;
        public float slotInterval;
        public int quantity;

        public TempSquadGroup()
        {
            order = 0;
            character = null;
            pattern = null;
            localOffset = Vector2.zero;
            localRotation = 0f;
            slotInterval = 0f;
            quantity = 1;
        }
    }

    private int activeTab = 0;
    private readonly string[] tabHeaders = { "Pattern Creator", "Squad Creator", "Formation Creator" };
    private Vector2 scrollPos;

    private SpawnNpcPoolSO npcPoolAsset;
    private SpawnContentPoolSO contentPoolAsset;
    private SpawnPatternPoolSO patternPoolAsset;
    private string baseOutputFolder = "Assets/Scripts/battle_spawn/Resource/Generated";

    // --- A. Pattern Creator State ---
    private int patternTypeTab = 0; // 0: Fixed Pattern, 1: Random Pattern
    private readonly string[] patternTypeTabHeaders = { "Fixed Pattern", "Random Pattern" };
    
    private string newPatternId = "pattern_custom_1";
    private string newPatternDisplayName = "신규 커스텀 패턴";
    private PatternTargetType manualTargetType = PatternTargetType.Squad;
    
    // (1) Fixed Pattern Params
    private LayoutGenerationType genType = LayoutGenerationType.Rectangle;
    private int rectRows = 3;
    private int rectCols = 3;
    private float spacingX = 2f;
    private float spacingY = 2f;
    private int circleCount = 6;
    private float circleRadius = 4f;
    private int triRows = 3;
    private float triSpacing = 2f;
    private string rowPatternInput = "3,2,3";
    private float rowSpacing = 2f;
    private float genRotation = 0f;
    private float genScale = 1f;

    // (2) Random Pattern Params
    private SpawnAreaShape randShape = SpawnAreaShape.Circle;
    private Vector2 randAreaSize = new Vector2(4f, 4f); // Circle: X=radius, Rectangle: X=width, Y=height

    private bool showPatternPreview = true;

    // --- B. Squad Creator State ---
    private string newSquadId = "squad.custom.squad_1";
    private float newSquadGroupInterval = 2.0f;
    private List<TempSquadGroup> tempSquadGroups = new List<TempSquadGroup>();
    private bool showSquadPreview = true;
    private float squadPreviewScale = 1.0f;

    // --- C. Formation Creator State ---
    private string newFormationId = "formation.custom.formation_1";
    private float newFormationSlotInterval = 1.5f;
    private SpawnSquadSO newFormationSquadSO;
    private SpawnPattern newFormationPatternSO; // SpawnPatternSO -> SpawnPattern
    private int newFormationQuantity = 1; // RandomPattern인 경우 소환 개수
    private bool showFormationPreview = true;
    private float formationPreviewScale = 1.0f;

    [MenuItem("BS/Spawn/Spawn SO Manual Window")]
    public static void ShowWindow()
    {
        GetWindow<SpawnSOManualWindow>("SO Manual Creator");
    }

    private void OnEnable()
    {
        FindDefaultPoolAssets();
    }

    private void FindDefaultPoolAssets()
    {
        string[] npcGuids = AssetDatabase.FindAssets("t:SpawnNpcPoolSO");
        if (npcGuids.Length > 0)
            npcPoolAsset = AssetDatabase.LoadAssetAtPath<SpawnNpcPoolSO>(AssetDatabase.GUIDToAssetPath(npcGuids[0]));

        string[] contentGuids = AssetDatabase.FindAssets("t:SpawnContentPoolSO");
        if (contentGuids.Length > 0)
            contentPoolAsset = AssetDatabase.LoadAssetAtPath<SpawnContentPoolSO>(AssetDatabase.GUIDToAssetPath(contentGuids[0]));

        string[] patternGuids = AssetDatabase.FindAssets("t:SpawnPatternPoolSO");
        if (patternGuids.Length > 0)
            patternPoolAsset = AssetDatabase.LoadAssetAtPath<SpawnPatternPoolSO>(AssetDatabase.GUIDToAssetPath(patternGuids[0]));
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        GUI.backgroundColor = new Color(0.2f, 0.45f, 0.75f, 1f);
        activeTab = GUILayout.Toolbar(activeTab, tabHeaders, GUILayout.Height(30));
        GUI.backgroundColor = Color.white;
        EditorGUILayout.Space();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        switch (activeTab)
        {
            case 0:
                DrawManualPatternTab();
                break;
            case 1:
                DrawManualSquadTab();
                break;
            case 2:
                DrawManualFormationTab();
                break;
        }

        EditorGUILayout.EndScrollView();
    }

    // --- (A) Pattern Creator ---
    private void DrawManualPatternTab()
    {
        GUI.backgroundColor = new Color(0.95f, 0.95f, 0.95f);
        EditorGUILayout.BeginVertical("box");
        GUI.backgroundColor = Color.white;

        EditorGUILayout.LabelField("Pattern Creator (수동 패턴 생성)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        patternTypeTab = GUILayout.Toolbar(patternTypeTab, patternTypeTabHeaders, GUILayout.Height(20));
        EditorGUILayout.Space();

        newPatternId = EditorGUILayout.TextField("패턴 ID", newPatternId);
        newPatternDisplayName = EditorGUILayout.TextField("표시 이름", newPatternDisplayName);
        manualTargetType = (PatternTargetType)EditorGUILayout.EnumPopup("패턴 용도 타입", manualTargetType);
        
        EditorGUILayout.Space();

        if (patternTypeTab == 0)
        {
            // Fixed Pattern 파라미터
            genType = (LayoutGenerationType)EditorGUILayout.EnumPopup("배치 형태", genType);
            switch (genType)
            {
                case LayoutGenerationType.Rectangle:
                    rectRows = EditorGUILayout.IntField("행 (Rows)", rectRows);
                    rectCols = EditorGUILayout.IntField("열 (Cols)", rectCols);
                    spacingX = EditorGUILayout.FloatField("간격 X (Spacing X)", spacingX);
                    spacingY = EditorGUILayout.FloatField("간격 Y (Spacing Y)", spacingY);
                    break;
                case LayoutGenerationType.Circle:
                    circleCount = EditorGUILayout.IntField("개수 (Count)", circleCount);
                    circleRadius = EditorGUILayout.FloatField("반경 (Radius)", circleRadius);
                    break;
                case LayoutGenerationType.Triangle:
                    triRows = EditorGUILayout.IntField("행 (Rows)", triRows);
                    triSpacing = EditorGUILayout.FloatField("간격 (Spacing)", triSpacing);
                    break;
                case LayoutGenerationType.RowPattern:
                    rowPatternInput = EditorGUILayout.TextField("행별 개수 (예: 3,2,3)", rowPatternInput);
                    rowSpacing = EditorGUILayout.FloatField("간격 (Spacing)", rowSpacing);
                    break;
            }

            genRotation = EditorGUILayout.Slider("생성 회전각 (도)", genRotation, 0f, 360f);
            genScale = EditorGUILayout.FloatField("생성 스케일", genScale);
        }
        else
        {
            // Random Pattern 파라미터
            randShape = (SpawnAreaShape)EditorGUILayout.EnumPopup("영역 형태 (Shape)", randShape);
            if (randShape == SpawnAreaShape.Circle)
            {
                float radius = randAreaSize.x;
                radius = EditorGUILayout.FloatField("원형 반경 (Radius)", radius);
                randAreaSize = new Vector2(radius, radius);
            }
            else
            {
                Vector2 size = randAreaSize;
                size.x = EditorGUILayout.FloatField("가로 크기 (Width)", size.x);
                size.y = EditorGUILayout.FloatField("세로 크기 (Height)", size.y);
                randAreaSize = size;
            }
            genScale = EditorGUILayout.FloatField("생성 스케일", genScale);
        }

        EditorGUILayout.Space();

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Bake Pattern SO (에셋 저장)", GUILayout.Height(30)))
        {
            BakeManualPatternAsset();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        showPatternPreview = EditorGUILayout.Foldout(showPatternPreview, "실시간 배치 미리보기 (2D Preview)");
        if (showPatternPreview)
        {
            DrawManualPatternPreview();
        }
    }

    private List<Vector2> CalculateManualCoords()
    {
        List<Vector2> coords = null;
        switch (genType)
        {
            case LayoutGenerationType.Rectangle:
                coords = FormationLayoutGenerator.GenerateRectangle(rectRows, rectCols, spacingX, spacingY);
                break;
            case LayoutGenerationType.Circle:
                coords = FormationLayoutGenerator.GenerateCircle(circleCount, circleRadius);
                break;
            case LayoutGenerationType.Triangle:
                coords = FormationLayoutGenerator.GenerateTriangle(triRows, triSpacing);
                break;
            case LayoutGenerationType.RowPattern:
                List<int> parsedCounts = new List<int>();
                string[] splits = rowPatternInput.Split(',');
                for (int i = 0; i < splits.Length; i++)
                {
                    if (int.TryParse(splits[i].Trim(), out int count))
                    {
                        parsedCounts.Add(count);
                    }
                }
                coords = FormationLayoutGenerator.GenerateRowPattern(parsedCounts, rowSpacing);
                break;
        }
        return coords ?? new List<Vector2>();
    }

    private void DrawManualPatternPreview()
    {
        Rect rect = GUILayoutUtility.GetRect(200, 250);

        EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 1f));
        Handles.color = Color.gray;
        Handles.DrawWireCube(rect.center, rect.size);

        Handles.color = new Color(0.25f, 0.25f, 0.25f, 0.5f);
        Handles.DrawLine(new Vector2(rect.x, rect.center.y), new Vector2(rect.xMax, rect.center.y));
        Handles.DrawLine(new Vector2(rect.center.x, rect.y), new Vector2(rect.center.x, rect.yMax));

        if (patternTypeTab == 0)
        {
            List<Vector2> coords = CalculateManualCoords();
            if (coords.Count == 0)
            {
                GUI.color = Color.red;
                GUI.Label(new Rect(rect.x + 10, rect.y + 10, 180, 20), "좌표 데이터가 비어있습니다.", EditorStyles.miniLabel);
                GUI.color = Color.white;
                return;
            }

            float maxAbs = 1f;
            foreach (var c in coords)
            {
                maxAbs = Mathf.Max(maxAbs, Mathf.Abs(c.x * genScale));
                maxAbs = Mathf.Max(maxAbs, Mathf.Abs(c.y * genScale));
            }
            maxAbs *= 1.3f;

            float ratio = Mathf.Min(rect.width, rect.height) * 0.5f / maxAbs;
            Vector2 drawCenter = rect.center;

            // 원점
            EditorGUI.DrawRect(new Rect(drawCenter.x - 3, drawCenter.y - 3, 6, 6), Color.cyan);

            for (int i = 0; i < coords.Count; i++)
            {
                Vector2 coord = coords[i];
                Vector2 scaled = coord * genScale;

                Vector2 rotated = SpawnCoordinateUtility.Rotate(scaled, genRotation);

                float lookAngle = genRotation;
                if (genType == LayoutGenerationType.Circle && coord != Vector2.zero)
                {
                    lookAngle = SpawnCoordinateUtility.GetRotationFromLookVector(coord) + genRotation;
                }
                lookAngle = (lookAngle % 360f + 360f) % 360f;

                Vector2 screenOffset = new Vector2(rotated.x * ratio, -rotated.y * ratio);
                Vector2 drawPos = drawCenter + screenOffset;

                EditorGUI.DrawRect(new Rect(drawPos.x - 4, drawPos.y - 4, 8, 8), Color.green);

                Vector2 lookVec = SpawnCoordinateUtility.GetLookVector(lookAngle);
                Vector2 dirVec = new Vector2(lookVec.x, -lookVec.y) * 12f;
                Handles.color = Color.yellow;
                Handles.DrawLine(drawPos, drawPos + dirVec);
            }

            GUI.color = Color.white;
            GUI.Label(new Rect(rect.x + 5, rect.y + 5, 230, 20), "Cyan: Origin, Green: Slot, Yellow: Looking Dir", EditorStyles.miniLabel);
        }
        else
        {
            Vector2 scaledAreaSize = randAreaSize * genScale;
            float maxAbs = Mathf.Max(1f, Mathf.Max(scaledAreaSize.x, scaledAreaSize.y)) * 1.3f;
            float ratio = Mathf.Min(rect.width, rect.height) * 0.5f / maxAbs;
            Vector2 drawCenter = rect.center;

            // 원점
            EditorGUI.DrawRect(new Rect(drawCenter.x - 3, drawCenter.y - 3, 6, 6), Color.cyan);

            Handles.color = Color.green;
            if (randShape == SpawnAreaShape.Circle)
            {
                float radiusPixels = scaledAreaSize.x * ratio;
                Handles.DrawWireDisc(drawCenter, Vector3.forward, radiusPixels);
            }
            else if (randShape == SpawnAreaShape.Rectangle)
            {
                float w = scaledAreaSize.x * ratio;
                float h = scaledAreaSize.y * ratio;
                Handles.DrawWireCube(drawCenter, new Vector3(w, h, 0f));
            }

            GUI.color = Color.white;
            GUI.Label(new Rect(rect.x + 5, rect.y + 5, 230, 20), $"Green Outline: Random Area ({randShape})", EditorStyles.miniLabel);
        }
    }

    private void BakeManualPatternAsset()
    {
        if (string.IsNullOrEmpty(newPatternId))
        {
            EditorUtility.DisplayDialog("오류", "패턴 ID를 입력해주세요.", "확인");
            return;
        }

        string subDir = (manualTargetType == PatternTargetType.Squad) ? "Squads" : "Formations";
        string targetFolder = $"{baseOutputFolder}/Patterns/{subDir}";

        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        string assetPath = $"{targetFolder}/{newPatternId}.asset";

        if (patternTypeTab == 0)
        {
            List<Vector2> coords = CalculateManualCoords();
            if (coords.Count == 0)
            {
                EditorUtility.DisplayDialog("오류", "유효한 슬롯 좌표가 없습니다.", "확인");
                return;
            }

            FixedPatternSO asset = AssetDatabase.LoadAssetAtPath<FixedPatternSO>(assetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<FixedPatternSO>();
                AssetDatabase.CreateAsset(asset, assetPath);
            }

            List<SpawnPatternSlot> newSlots = new List<SpawnPatternSlot>();
            for (int i = 0; i < coords.Count; i++)
            {
                Vector2 coord = coords[i];
                Vector2 scaled = coord * genScale;

                Vector2 rotated = SpawnCoordinateUtility.Rotate(scaled, genRotation);

                float lookAngle = genRotation;
                if (genType == LayoutGenerationType.Circle && coord != Vector2.zero)
                {
                    lookAngle = SpawnCoordinateUtility.GetRotationFromLookVector(coord) + genRotation;
                }
                lookAngle = (lookAngle % 360f + 360f) % 360f;

                newSlots.Add(new SpawnPatternSlot(rotated, lookAngle));
            }

            asset.Initialize(newPatternId, newPatternDisplayName, newSlots);
            EditorUtility.SetDirty(asset);
            Selection.activeObject = asset;
        }
        else
        {
            RandomPatternSO asset = AssetDatabase.LoadAssetAtPath<RandomPatternSO>(assetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<RandomPatternSO>();
                AssetDatabase.CreateAsset(asset, assetPath);
            }

            Vector2 scaledAreaSize = randAreaSize * genScale;
            asset.Initialize(newPatternId, newPatternDisplayName, randShape, scaledAreaSize);
            EditorUtility.SetDirty(asset);
            Selection.activeObject = asset;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (patternPoolAsset != null)
        {
            patternPoolAsset.CollectAllPatterns();
            EditorUtility.SetDirty(patternPoolAsset);
        }

        EditorUtility.DisplayDialog("Bake 완료", $"패턴 '{newPatternId}'이 성공적으로 저장되었습니다!\n위치: {assetPath}", "확인");
    }

    // --- (B) Squad Creator ---
    private void DrawManualSquadTab()
    {
        GUI.backgroundColor = new Color(0.95f, 0.95f, 0.95f);
        EditorGUILayout.BeginVertical("box");
        GUI.backgroundColor = Color.white;

        EditorGUILayout.LabelField("Squad Creator (수동 분대 생성)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        newSquadId = EditorGUILayout.TextField("분대 Content ID", newSquadId);
        newSquadGroupInterval = EditorGUILayout.FloatField("Group Interval (그룹 간 대기시간)", newSquadGroupInterval);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("소환 그룹 목록 (Squad Groups)", EditorStyles.boldLabel);

        for (int i = 0; i < tempSquadGroups.Count; i++)
        {
            var g = tempSquadGroups[i];
            GUI.backgroundColor = new Color(0.9f, 0.93f, 0.97f);
            EditorGUILayout.BeginVertical("box");
            GUI.backgroundColor = Color.white;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"[Group {i + 1}]", EditorStyles.boldLabel);
            if (GUILayout.Button("▲", GUILayout.Width(25)) && i > 0)
            {
                var tmp = tempSquadGroups[i - 1];
                tempSquadGroups[i - 1] = g;
                tempSquadGroups[i] = tmp;
                break;
            }
            if (GUILayout.Button("▼", GUILayout.Width(25)) && i < tempSquadGroups.Count - 1)
            {
                var tmp = tempSquadGroups[i + 1];
                tempSquadGroups[i + 1] = g;
                tempSquadGroups[i] = tmp;
                break;
            }
            GUI.color = Color.red;
            if (GUILayout.Button("제거", GUILayout.Width(45)))
            {
                tempSquadGroups.RemoveAt(i);
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();

            g.order = EditorGUILayout.IntField("Order (실행 순서)", g.order);
            g.character = (CharacterSO)EditorGUILayout.ObjectField("Character (NPC)", g.character, typeof(CharacterSO), false);
            g.pattern = (SpawnPattern)EditorGUILayout.ObjectField("Pattern (SO)", g.pattern, typeof(SpawnPattern), false);
            
            if (g.pattern != null && g.pattern is RandomPatternSO)
            {
                g.quantity = EditorGUILayout.IntField("Random 소환 수량", g.quantity);
                if (g.quantity < 1) g.quantity = 1;
            }

            g.localOffset = EditorGUILayout.Vector2Field("Local Offset", g.localOffset);
            g.localRotation = EditorGUILayout.FloatField("Local Rotation (도)", g.localRotation);
            g.slotInterval = EditorGUILayout.FloatField("Slot Interval (슬롯 간 소환 지연)", g.slotInterval);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+ 소환 그룹 추가"))
        {
            int maxOrder = tempSquadGroups.Count > 0 ? tempSquadGroups.Max(g => g.order) + 10 : 0;
            tempSquadGroups.Add(new TempSquadGroup { order = maxOrder });
        }
        if (GUILayout.Button("Order 10 단위 정렬"))
        {
            tempSquadGroups = tempSquadGroups.OrderBy(x => x.order).ToList();
            int cur = 0;
            int lastOrd = -1;
            for (int i = 0; i < tempSquadGroups.Count; i++)
            {
                if (lastOrd != -1 && tempSquadGroups[i].order != lastOrd) cur += 10;
                lastOrd = tempSquadGroups[i].order;
                tempSquadGroups[i].order = cur;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Bake Squad SO (에셋 저장)", GUILayout.Height(30)))
        {
            BakeManualSquadAsset();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        showSquadPreview = EditorGUILayout.Foldout(showSquadPreview, "실시간 3계층 소환 타임라인 미리보기 (2D Preview)");
        if (showSquadPreview)
        {
            squadPreviewScale = EditorGUILayout.Slider("미리보기 줌(Zoom)", squadPreviewScale, 0.2f, 3.0f);
            DrawManualSquadPreview();
        }
    }

    private void DrawManualSquadPreview()
    {
        Rect rect = GUILayoutUtility.GetRect(200, 260);

        EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 1f));
        Handles.color = Color.gray;
        Handles.DrawWireCube(rect.center, rect.size);

        Handles.color = new Color(0.25f, 0.25f, 0.25f, 0.5f);
        Handles.DrawLine(new Vector2(rect.x, rect.center.y), new Vector2(rect.xMax, rect.center.y));
        Handles.DrawLine(new Vector2(rect.center.x, rect.y), new Vector2(rect.center.x, rect.yMax));

        if (tempSquadGroups.Count == 0)
        {
            GUI.Label(new Rect(rect.x + 10, rect.y + 10, 200, 20), "소환 그룹 목록이 비어 있습니다.", EditorStyles.miniLabel);
            return;
        }

        SpawnSquadSO tempSO = ScriptableObject.CreateInstance<SpawnSquadSO>();
        List<SpawnSquadGroup> converted = new List<SpawnSquadGroup>();
        foreach (var tg in tempSquadGroups)
        {
            converted.Add(new SpawnSquadGroup(tg.order, tg.character, tg.pattern, tg.localOffset, tg.localRotation, tg.slotInterval, tg.quantity));
        }
        tempSO.Initialize(newSquadId, newSquadGroupInterval, converted);

        SpawnPlan plan = null;
        try
        {
            plan = SpawnContentResolver.Resolve(new SpawnRequest(tempSO, Vector3.zero, 0f));
        }
        catch (Exception ex)
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(rect.x + 10, rect.y + 10, 200, 40), $"소환 데이터 연산 오류:\n{ex.Message}", EditorStyles.miniLabel);
            GUI.color = Color.white;
            return;
        }

        if (plan == null || plan.Commands.Count == 0)
        {
            GUI.Label(new Rect(rect.x + 10, rect.y + 10, 200, 20), "소환될 캐릭터 데이터가 없습니다.", EditorStyles.miniLabel);
            return;
        }

        float maxAbs = 1f;
        foreach (var cmd in plan.Commands)
        {
            maxAbs = Mathf.Max(maxAbs, Mathf.Abs(cmd.Position.x));
            maxAbs = Mathf.Max(maxAbs, Mathf.Abs(cmd.Position.y));
        }
        maxAbs *= 1.4f;

        float baseRatio = Mathf.Min(rect.width, rect.height) * 0.5f / maxAbs;
        float ratio = baseRatio * squadPreviewScale;
        Vector2 drawCenter = rect.center;

        // 원점
        EditorGUI.DrawRect(new Rect(drawCenter.x - 3, drawCenter.y - 3, 6, 6), Color.cyan);

        var orders = tempSquadGroups.Select(tg => tg.order).Distinct().OrderBy(o => o).ToList();

        foreach (var cmd in plan.Commands)
        {
            Vector2 screenOffset = new Vector2(cmd.Position.x * ratio, -cmd.Position.y * ratio);
            Vector2 drawPos = drawCenter + screenOffset;

            if (!rect.Contains(drawPos)) continue;

            Color dotColor = Color.green;
            var matchedGroup = tempSquadGroups.FirstOrDefault(tg => tg.character == cmd.Character);
            if (matchedGroup != null)
            {
                int colorIdx = orders.IndexOf(matchedGroup.order);
                dotColor = GetColorByIndex(colorIdx);
            }

            EditorGUI.DrawRect(new Rect(drawPos.x - 4, drawPos.y - 4, 8, 8), dotColor);

            float angleRad = cmd.Rotation * Mathf.Deg2Rad;
            Vector2 dirVec = new Vector2(-Mathf.Sin(angleRad), -Mathf.Cos(angleRad)) * 12f;
            Handles.color = Color.yellow;
            Handles.DrawLine(drawPos, drawPos + dirVec);

            string txt = $"[{cmd.StartTime:F1}s] {GetCharacterDisplayName(cmd.Character)}";
            DrawLabel(drawPos, txt);
        }

        GUI.color = Color.white;
        GUI.Label(new Rect(rect.x + 5, rect.y + 5, 260, 20), 
            $"총 {plan.Commands.Count}마리 (최종 소환 완료 시점: {plan.Commands.Max(c => c.StartTime):F2}초)", EditorStyles.miniLabel);
    }

    private void BakeManualSquadAsset()
    {
        if (string.IsNullOrEmpty(newSquadId))
        {
            EditorUtility.DisplayDialog("오류", "분대 Content ID를 입력해주세요.", "확인");
            return;
        }

        bool hasPattern = tempSquadGroups.Any(tg => tg.pattern != null);
        string sub = hasPattern ? "Squads" : "Singles";
        string targetDir = $"{baseOutputFolder}/SpawnContent/{sub}";
        if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

        string assetPath = $"{targetDir}/{newSquadId}.asset";
        SpawnSquadSO asset = AssetDatabase.LoadAssetAtPath<SpawnSquadSO>(assetPath);
        bool isNew = (asset == null);
        if (isNew)
        {
            asset = ScriptableObject.CreateInstance<SpawnSquadSO>();
            AssetDatabase.CreateAsset(asset, assetPath);
        }

        List<SpawnSquadGroup> converted = new List<SpawnSquadGroup>();
        foreach (var tg in tempSquadGroups)
        {
            converted.Add(new SpawnSquadGroup(tg.order, tg.character, tg.pattern, tg.localOffset, tg.localRotation, tg.slotInterval, tg.quantity));
        }

        asset.Initialize(newSquadId, newSquadGroupInterval, converted);
        EditorUtility.SetDirty(asset);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (contentPoolAsset != null)
        {
            contentPoolAsset.CollectAllContents();
            EditorUtility.SetDirty(contentPoolAsset);
        }

        Selection.activeObject = asset;
        EditorUtility.DisplayDialog("Bake 완료", $"스쿼드 '{newSquadId}'이(가) 저장되었습니다!\n위치: {assetPath}", "확인");
    }

    // --- (C) Formation Creator ---
    private void DrawManualFormationTab()
    {
        GUI.backgroundColor = new Color(0.95f, 0.95f, 0.95f);
        EditorGUILayout.BeginVertical("box");
        GUI.backgroundColor = Color.white;

        EditorGUILayout.LabelField("Formation Creator (수동 포메이션 생성)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        newFormationId = EditorGUILayout.TextField("포메이션 Content ID", newFormationId);
        newFormationSlotInterval = EditorGUILayout.FloatField("슬롯 간 소환 지연 (Slot Interval)", newFormationSlotInterval);

        newFormationSquadSO = (SpawnSquadSO)EditorGUILayout.ObjectField("하위 스쿼드 (SpawnSquadSO)", newFormationSquadSO, typeof(SpawnSquadSO), false);
        newFormationPatternSO = (SpawnPattern)EditorGUILayout.ObjectField("포메이션 배치 패턴 (SpawnPattern)", newFormationPatternSO, typeof(SpawnPattern), false);

        if (newFormationPatternSO != null && newFormationPatternSO is RandomPatternSO)
        {
            newFormationQuantity = EditorGUILayout.IntField("Random 소환 수량", newFormationQuantity);
            if (newFormationQuantity < 1) newFormationQuantity = 1;
        }

        EditorGUILayout.Space();
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Bake Formation SO (에셋 저장)", GUILayout.Height(30)))
        {
            BakeManualFormationAsset();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        showFormationPreview = EditorGUILayout.Foldout(showFormationPreview, "실시간 3계층 결합 배치 미리보기 (2D Preview)");
        if (showFormationPreview)
        {
            formationPreviewScale = EditorGUILayout.Slider("미리보기 줌(Zoom)", formationPreviewScale, 0.2f, 3.0f);
            DrawManualFormationPreview();
        }
    }

    private void DrawManualFormationPreview()
    {
        Rect rect = GUILayoutUtility.GetRect(200, 260);

        EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 1f));
        Handles.color = Color.gray;
        Handles.DrawWireCube(rect.center, rect.size);

        Handles.color = new Color(0.25f, 0.25f, 0.25f, 0.5f);
        Handles.DrawLine(new Vector2(rect.x, rect.center.y), new Vector2(rect.xMax, rect.center.y));
        Handles.DrawLine(new Vector2(rect.center.x, rect.y), new Vector2(rect.center.x, rect.yMax));

        if (newFormationSquadSO == null || newFormationPatternSO == null)
        {
            GUI.Label(new Rect(rect.x + 10, rect.y + 10, 220, 20), "스쿼드 및 패턴 에셋을 지정해주세요.", EditorStyles.miniLabel);
            return;
        }

        SpawnFormationSO tempSO = ScriptableObject.CreateInstance<SpawnFormationSO>();
        tempSO.Initialize(newFormationId, newFormationPatternSO, newFormationSquadSO, newFormationSlotInterval, newFormationQuantity);

        SpawnPlan plan = null;
        try
        {
            plan = SpawnContentResolver.Resolve(new SpawnRequest(tempSO, Vector3.zero, 0f));
        }
        catch (Exception ex)
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(rect.x + 10, rect.y + 10, 220, 40), $"포메이션 배치 연산 오류:\n{ex.Message}", EditorStyles.miniLabel);
            GUI.color = Color.white;
            return;
        }

        if (plan == null || plan.Commands.Count == 0)
        {
            GUI.Label(new Rect(rect.x + 10, rect.y + 10, 220, 20), "결합 소환할 데이터가 비어있습니다.", EditorStyles.miniLabel);
            return;
        }

        float maxAbs = 1f;
        foreach (var cmd in plan.Commands)
        {
            maxAbs = Mathf.Max(maxAbs, Mathf.Abs(cmd.Position.x));
            maxAbs = Mathf.Max(maxAbs, Mathf.Abs(cmd.Position.y));
        }
        maxAbs *= 1.4f;

        float baseRatio = Mathf.Min(rect.width, rect.height) * 0.5f / maxAbs;
        float ratio = baseRatio * formationPreviewScale;
        Vector2 drawCenter = rect.center;

        // 원점
        EditorGUI.DrawRect(new Rect(drawCenter.x - 3, drawCenter.y - 3, 6, 6), Color.cyan);

        foreach (var cmd in plan.Commands)
        {
            Vector2 screenOffset = new Vector2(cmd.Position.x * ratio, -cmd.Position.y * ratio);
            Vector2 drawPos = drawCenter + screenOffset;

            if (!rect.Contains(drawPos)) continue;

            EditorGUI.DrawRect(new Rect(drawPos.x - 4, drawPos.y - 4, 8, 8), Color.green);

            float angleRad = cmd.Rotation * Mathf.Deg2Rad;
            Vector2 dirVec = new Vector2(-Mathf.Sin(angleRad), -Mathf.Cos(angleRad)) * 12f;
            Handles.color = Color.yellow;
            Handles.DrawLine(drawPos, drawPos + dirVec);

            string labelText = $"[{cmd.StartTime:F1}s] {GetCharacterDisplayName(cmd.Character)}";
            DrawLabel(drawPos, labelText);
        }

        GUI.color = Color.white;
        GUI.Label(new Rect(rect.x + 5, rect.y + 5, 260, 20), 
            $"총 {plan.Commands.Count}마리 (최종 완료 시점: {plan.Commands.Max(c => c.StartTime):F2}초)", EditorStyles.miniLabel);
    }

    private void BakeManualFormationAsset()
    {
        if (string.IsNullOrEmpty(newFormationId))
        {
            EditorUtility.DisplayDialog("오류", "포메이션 Content ID를 입력해주세요.", "확인");
            return;
        }
        if (newFormationSquadSO == null)
        {
            EditorUtility.DisplayDialog("오류", "하위 스쿼드(SpawnSquadSO)가 지정되지 않았습니다.", "확인");
            return;
        }
        if (newFormationPatternSO == null)
        {
            EditorUtility.DisplayDialog("오류", "배치 패턴(SpawnPattern)이 지정되지 않았습니다.", "확인");
            return;
        }

        string targetDir = $"{baseOutputFolder}/SpawnContent/Formations";
        if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

        string assetPath = $"{targetDir}/{newFormationId}.asset";
        SpawnFormationSO asset = AssetDatabase.LoadAssetAtPath<SpawnFormationSO>(assetPath);
        bool isNew = (asset == null);
        if (isNew)
        {
            asset = ScriptableObject.CreateInstance<SpawnFormationSO>();
            AssetDatabase.CreateAsset(asset, assetPath);
        }

        asset.Initialize(newFormationId, newFormationPatternSO, newFormationSquadSO, newFormationSlotInterval, newFormationQuantity);
        EditorUtility.SetDirty(asset);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (contentPoolAsset != null)
        {
            contentPoolAsset.CollectAllContents();
            EditorUtility.SetDirty(contentPoolAsset);
        }

        Selection.activeObject = asset;
        EditorUtility.DisplayDialog("Bake 완료", $"포메이션 '{newFormationId}'이(가) 저장되었습니다!\n위치: {assetPath}", "확인");
    }

    private Color GetColorByIndex(int idx)
    {
        switch (idx % 6)
        {
            case 0: return Color.green;
            case 1: return new Color(0.2f, 0.6f, 1f, 1f); // Sky blue
            case 2: return Color.red;
            case 3: return Color.magenta;
            case 4: return new Color(1f, 0.5f, 0f, 1f); // Orange
            case 5: return Color.yellow;
            default: return Color.white;
        }
    }

    private string GetCharacterDisplayName(CharacterSO character)
    {
        if (character == null) return "None";
        return !string.IsNullOrEmpty(character.CharacterId) ? character.CharacterId : character.name;
    }

    private void DrawLabel(Vector2 pos, string text)
    {
        GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
        style.normal.textColor = Color.white;
        Vector2 size = style.CalcSize(new GUIContent(text));
        
        Rect bgRect = new Rect(pos.x + 5, pos.y - size.y / 2, size.x, size.y);
        EditorGUI.DrawRect(bgRect, new Color(0, 0, 0, 0.5f));
        GUI.Label(bgRect, text, style);
    }
}
#endif
