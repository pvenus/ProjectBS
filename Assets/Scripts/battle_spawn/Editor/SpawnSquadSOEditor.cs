#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpawnSquadSO))]
public sealed class SpawnSquadSOEditor : Editor
{
    private bool showPreview = true;

    public override void OnInspectorGUI()
    {
        SpawnSquadSO squad = (SpawnSquadSO)target;
        if (squad == null) return;

        serializedObject.Update();

        EditorGUILayout.LabelField("기본 정보", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Content ID", squad.ContentId);
        
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("부대 기준점 배치 (Formation Pattern)", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("formationPatternId"), new GUIContent("Formation Pattern ID"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("formationPatternDisplayName"), new GUIContent("Formation Pattern Display Name"));
        SerializedProperty formationKindProp = serializedObject.FindProperty("formationPatternKind");
        EditorGUILayout.PropertyField(formationKindProp, new GUIContent("Formation Pattern Kind"));
        EnsureFormationPatternConfig();
        SerializedProperty formationConfigProp = serializedObject.FindProperty("formationPatternConfig");
        if ((SpawnPatternKind)formationKindProp.enumValueIndex != SpawnPatternKind.None)
        {
            EditorGUILayout.PropertyField(formationConfigProp, new GUIContent("Formation Pattern Config"), true);
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("formationSlotInterval"), new GUIContent("Formation Slot Interval"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("formationQuantity"), new GUIContent("Formation Quantity"));

        EditorGUILayout.Space();

        // 1. Group Interval 설정
        SerializedProperty groupIntervalProp = serializedObject.FindProperty("groupInterval");
        EditorGUILayout.PropertyField(groupIntervalProp, new GUIContent("Group Interval (그룹 간 대기시간)"));
        SerializedProperty slotIntervalProp = serializedObject.FindProperty("slotInterval");
        EditorGUILayout.PropertyField(slotIntervalProp, new GUIContent("Default Slot Interval (기본 슬롯 간 지연)"));
        SerializedProperty quantityProp = serializedObject.FindProperty("quantity");
        EditorGUILayout.PropertyField(quantityProp, new GUIContent("Default Quantity (기본 소환 수량)"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("소환 그룹 목록 (Squad Groups)", EditorStyles.boldLabel);

        SerializedProperty groupsProp = serializedObject.FindProperty("groups");

        // Groups 리스트를 Order별로 묶어 GUI 렌더링하기 위해 데이터 수집
        var groupList = new List<GroupElementWrapper>();
        for (int i = 0; i < groupsProp.arraySize; i++)
        {
            SerializedProperty element = groupsProp.GetArrayElementAtIndex(i);
            int order = element.FindPropertyRelative("order").intValue;
            groupList.Add(new GroupElementWrapper(i, order, element));
        }

        // Order 오름차순 그룹화
        var grouped = groupList.GroupBy(g => g.Order).OrderBy(g => g.Key).ToList();

        // 현재 Order들의 시작 예정시간 시뮬레이션 계산
        float currentOrderStartTime = 0f;
        
        for (int groupIdx = 0; groupIdx < grouped.Count; groupIdx++)
        {
            var orderGroup = grouped[groupIdx];
            int orderVal = orderGroup.Key;

            // 소환 시간 계산 (Preview용 시간계산 재사용)
            float maxGroupDuration = 0f;
            foreach (var wrapper in orderGroup)
            {
                int groupQuantity = wrapper.Property.FindPropertyRelative("quantity").intValue;
                int slotCount = ResolvePatternSlotCount(wrapper.Property, groupQuantity, squad.Quantity);
                float groupSlotInterval = wrapper.Property.FindPropertyRelative("slotInterval").floatValue;
                float slotInt = groupSlotInterval > 0f ? groupSlotInterval : squad.SlotInterval;
                float duration = Mathf.Max(0f, (slotCount - 1) * slotInt);
                if (duration > maxGroupDuration) maxGroupDuration = duration;
            }

            // 시각적 그룹화 박스 렌더링
            GUI.backgroundColor = new Color(0.9f, 0.95f, 1.0f);
            EditorGUILayout.BeginVertical("box");
            GUI.backgroundColor = Color.white;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Order {orderVal} (시작 시간: {currentOrderStartTime:F2}초, 소환 기간: {maxGroupDuration:F2}초)", EditorStyles.boldLabel);
            
            // 편의 기능: Order 상하 조정
            if (GUILayout.Button("▲", GUILayout.Width(25)))
            {
                ShiftOrder(groupsProp, orderGroup.Select(x => x.Index).ToList(), -10);
                break;
            }
            if (GUILayout.Button("▼", GUILayout.Width(25)))
            {
                ShiftOrder(groupsProp, orderGroup.Select(x => x.Index).ToList(), 10);
                break;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // 내부 요소 렌더링
            foreach (var wrapper in orderGroup)
            {
                EditorGUILayout.BeginVertical("box");
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Group {wrapper.Index + 1}", EditorStyles.miniBoldLabel);
                GUI.color = Color.red;
                if (GUILayout.Button("제거", GUILayout.Width(50)))
                {
                    groupsProp.DeleteArrayElementAtIndex(wrapper.Index);
                    GUI.color = Color.white;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();

                SerializedProperty orderProp = wrapper.Property.FindPropertyRelative("order");
                orderProp.intValue = EditorGUILayout.IntField("Order", orderProp.intValue);

                EditorGUILayout.PropertyField(wrapper.Property.FindPropertyRelative("spawnUnitKey"), new GUIContent("Spawn Unit Key"));
                EditorGUILayout.PropertyField(wrapper.Property.FindPropertyRelative("spawnRole"), new GUIContent("Spawn Role"));
                
                EditorGUILayout.PropertyField(wrapper.Property.FindPropertyRelative("patternId"), new GUIContent("Pattern ID"));
                EditorGUILayout.PropertyField(wrapper.Property.FindPropertyRelative("patternDisplayName"), new GUIContent("Pattern Display Name"));
                SerializedProperty kindProp = wrapper.Property.FindPropertyRelative("patternKind");
                EditorGUILayout.PropertyField(kindProp, new GUIContent("Pattern Kind"));
                EnsurePatternConfig(wrapper.Property);
                SerializedProperty patternConfigProp = wrapper.Property.FindPropertyRelative("patternConfig");
                if ((SpawnPatternKind)kindProp.enumValueIndex != SpawnPatternKind.None)
                {
                    EditorGUILayout.PropertyField(patternConfigProp, new GUIContent("Pattern Config"), true);
                }
                SerializedProperty qtyProp = wrapper.Property.FindPropertyRelative("quantity");
                qtyProp.intValue = EditorGUILayout.IntField("Quantity Override (0=기본값)", qtyProp.intValue);
                if (qtyProp.intValue < 0) qtyProp.intValue = 0;

                EditorGUILayout.PropertyField(wrapper.Property.FindPropertyRelative("localOffset"), new GUIContent("Local Offset"));
                EditorGUILayout.PropertyField(wrapper.Property.FindPropertyRelative("localRotation"), new GUIContent("Local Rotation (도)"));
                EditorGUILayout.PropertyField(wrapper.Property.FindPropertyRelative("slotInterval"), new GUIContent("Slot Interval Override (0=기본값)"));

                EditorGUILayout.EndVertical();
            }

            // 편의 기능: 현재 Order에 그룹 추가
            if (GUILayout.Button($"+ Order {orderVal}에 새로운 소환 그룹 추가"))
            {
                int newIdx = groupsProp.arraySize;
                groupsProp.InsertArrayElementAtIndex(newIdx);
                SerializedProperty newEl = groupsProp.GetArrayElementAtIndex(newIdx);
                newEl.FindPropertyRelative("order").intValue = orderVal;
                ResetUnitProperties(newEl);
                ResetPatternProperties(newEl);
                newEl.FindPropertyRelative("slotInterval").floatValue = 0f;
                newEl.FindPropertyRelative("quantity").intValue = 0;
                newEl.FindPropertyRelative("localRotation").floatValue = 0f;
                newEl.FindPropertyRelative("localOffset").vector2Value = Vector2.zero;
                break;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            bool isLast = (groupIdx == grouped.Count - 1);
            if (!isLast)
            {
                currentOrderStartTime = currentOrderStartTime + maxGroupDuration + squad.GroupInterval;
            }
            else
            {
                currentOrderStartTime = currentOrderStartTime + maxGroupDuration;
            }
        }

        // 전체 단위 제어 버튼들
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+ 새로운 Order 그룹 추가"))
        {
            int maxOrder = grouped.Count > 0 ? grouped.Max(g => g.Key) + 10 : 0;
            int newIdx = groupsProp.arraySize;
            groupsProp.InsertArrayElementAtIndex(newIdx);
            SerializedProperty newEl = groupsProp.GetArrayElementAtIndex(newIdx);
            newEl.FindPropertyRelative("order").intValue = maxOrder;
            ResetUnitProperties(newEl);
            ResetPatternProperties(newEl);
            newEl.FindPropertyRelative("slotInterval").floatValue = 0f;
            newEl.FindPropertyRelative("quantity").intValue = 0;
            newEl.FindPropertyRelative("localRotation").floatValue = 0f;
            newEl.FindPropertyRelative("localOffset").vector2Value = Vector2.zero;
        }

        if (GUILayout.Button("Order 값 정규화 (10 단위 정렬)"))
        {
            NormalizeOrders(groupsProp);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 2D 미리보기 렌더링
        showPreview = EditorGUILayout.Foldout(showPreview, "스쿼드 전체 타임라인 미리보기 (2D Preview)");
        if (showPreview)
        {
            DrawSquadTimelinePreview(squad);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void ShiftOrder(SerializedProperty groupsProp, List<int> indices, int delta)
    {
        foreach (int idx in indices)
        {
            SerializedProperty el = groupsProp.GetArrayElementAtIndex(idx);
            SerializedProperty orderProp = el.FindPropertyRelative("order");
            orderProp.intValue = Mathf.Max(0, orderProp.intValue + delta);
        }
    }

    private int ResolvePatternSlotCount(SerializedProperty groupProp, int groupQuantity, int defaultQuantity)
    {
        if (groupProp == null)
        {
            return groupQuantity > 0 ? groupQuantity : Mathf.Max(1, defaultQuantity);
        }

        SerializedProperty kindProp = groupProp.FindPropertyRelative("patternKind");
        SpawnPatternKind kind = kindProp != null
            ? (SpawnPatternKind)kindProp.enumValueIndex
            : SpawnPatternKind.None;

        if (kind.IsFixedSlotKind())
        {
            SerializedProperty configProp = groupProp.FindPropertyRelative("patternConfig");
            SerializedProperty slotsProp = configProp != null ? configProp.FindPropertyRelative("slots") : null;
            int slotCount = slotsProp != null ? slotsProp.arraySize : 0;
            return groupQuantity > 1 ? groupQuantity : Mathf.Max(1, slotCount);
        }

        return groupQuantity > 0 ? groupQuantity : Mathf.Max(1, defaultQuantity);
    }

    private void ResetPatternProperties(SerializedProperty groupProp)
    {
        groupProp.FindPropertyRelative("patternId").stringValue = string.Empty;
        groupProp.FindPropertyRelative("patternDisplayName").stringValue = string.Empty;
        groupProp.FindPropertyRelative("patternKind").enumValueIndex = (int)SpawnPatternKind.None;
        SerializedProperty configProp = groupProp.FindPropertyRelative("patternConfig");
        if (configProp != null)
        {
            configProp.managedReferenceValue = null;
        }
    }

    private void ResetUnitProperties(SerializedProperty groupProp)
    {
        groupProp.FindPropertyRelative("spawnUnitKey").stringValue = string.Empty;
        groupProp.FindPropertyRelative("spawnRole").enumValueIndex = (int)SpawnUnitRole.Any;
    }

    private void EnsurePatternConfig(SerializedProperty groupProp)
    {
        SerializedProperty kindProp = groupProp.FindPropertyRelative("patternKind");
        SerializedProperty configProp = groupProp.FindPropertyRelative("patternConfig");
        if (kindProp == null || configProp == null)
        {
            return;
        }

        SpawnPatternKind kind = (SpawnPatternKind)kindProp.enumValueIndex;
        if (kind == SpawnPatternKind.None)
        {
            configProp.managedReferenceValue = null;
            return;
        }

        if (kind.IsFixedSlotKind() && !(configProp.managedReferenceValue is FixedSpawnPatternConfig))
        {
            configProp.managedReferenceValue = new FixedSpawnPatternConfig();
            return;
        }

        if (kind.IsRandomAreaKind() && !(configProp.managedReferenceValue is RandomSpawnPatternConfig))
        {
            configProp.managedReferenceValue = CreateDefaultRandomConfig(kind);
        }
    }

    private void EnsureFormationPatternConfig()
    {
        SerializedProperty kindProp = serializedObject.FindProperty("formationPatternKind");
        SerializedProperty configProp = serializedObject.FindProperty("formationPatternConfig");
        if (kindProp == null || configProp == null)
        {
            return;
        }

        SpawnPatternKind kind = (SpawnPatternKind)kindProp.enumValueIndex;
        if (kind == SpawnPatternKind.None)
        {
            configProp.managedReferenceValue = null;
            return;
        }

        if (kind.IsFixedSlotKind() && !(configProp.managedReferenceValue is FixedSpawnPatternConfig))
        {
            configProp.managedReferenceValue = new FixedSpawnPatternConfig();
            return;
        }

        if (kind.IsRandomAreaKind() && !(configProp.managedReferenceValue is RandomSpawnPatternConfig))
        {
            configProp.managedReferenceValue = CreateDefaultRandomConfig(kind);
        }
    }

    private static RandomSpawnPatternConfig CreateDefaultRandomConfig(SpawnPatternKind kind)
    {
        SpawnAreaShape shape = kind == SpawnPatternKind.RandomRectangle
            ? SpawnAreaShape.Rectangle
            : SpawnAreaShape.Circle;
        return new RandomSpawnPatternConfig(shape, new Vector2(1f, 1f));
    }

    private void NormalizeOrders(SerializedProperty groupsProp)
    {
        var list = new List<Tuple<int, SerializedProperty>>();
        for (int i = 0; i < groupsProp.arraySize; i++)
        {
            SerializedProperty el = groupsProp.GetArrayElementAtIndex(i);
            int order = el.FindPropertyRelative("order").intValue;
            list.Add(new Tuple<int, SerializedProperty>(order, el));
        }

        var sorted = list.OrderBy(x => x.Item1).ToList();
        int curOrderVal = 0;
        int lastOriginalOrder = -1;

        for (int i = 0; i < sorted.Count; i++)
        {
            int originalOrder = sorted[i].Item1;
            if (lastOriginalOrder != -1 && originalOrder != lastOriginalOrder)
            {
                curOrderVal += 10;
            }
            sorted[i].Item2.FindPropertyRelative("order").intValue = curOrderVal;
            lastOriginalOrder = originalOrder;
        }
    }

    private void DrawSquadTimelinePreview(SpawnSquadSO squad)
    {
        Rect rect = GUILayoutUtility.GetRect(200, 250);

        EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 1f));
        Handles.color = Color.gray;
        Handles.DrawWireCube(rect.center, rect.size);

        // 원점 안내선
        Handles.color = new Color(0.25f, 0.25f, 0.25f, 0.5f);
        Handles.DrawLine(new Vector2(rect.x, rect.center.y), new Vector2(rect.xMax, rect.center.y));
        Handles.DrawLine(new Vector2(rect.center.x, rect.y), new Vector2(rect.center.x, rect.yMax));

        // Resolver를 이용한 평탄화 소환 명령 생성
        SpawnRequest req = new SpawnRequest(squad, Vector3.zero, 0f);
        SpawnPlan plan = null;
        try
        {
            plan = SpawnContentResolver.Resolve(req);
        }
        catch (Exception ex)
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(rect.x + 10, rect.y + 10, 180, 50), $"Plan 생성 오류:\n{ex.Message}", EditorStyles.miniLabel);
            GUI.color = Color.white;
            return;
        }

        if (plan == null || plan.Commands.Count == 0)
        {
            GUI.Label(new Rect(rect.x + 10, rect.y + 10, 180, 20), "소환할 데이터가 비어있습니다.", EditorStyles.miniLabel);
            return;
        }

        // 최대 절대 거리 계산
        float maxAbs = 1f;
        foreach (var cmd in plan.Commands)
        {
            maxAbs = Mathf.Max(maxAbs, Mathf.Abs(cmd.Position.x));
            maxAbs = Mathf.Max(maxAbs, Mathf.Abs(cmd.Position.y));
        }
        maxAbs *= 1.4f;

        // 종횡비 왜곡 방지 1:1 스케일 비율
        float ratio = Mathf.Min(rect.width, rect.height) * 0.5f / maxAbs;

        // 원점 표시
        Vector2 drawCenter = rect.center;
        EditorGUI.DrawRect(new Rect(drawCenter.x - 3, drawCenter.y - 3, 6, 6), Color.cyan);

        // Order별 색상 매핑을 위한 그룹화
        var orders = squad.Groups.Select(g => g.Order).Distinct().OrderBy(o => o).ToList();

        // 소환 명령별 2D 도식화 그리기
        foreach (var cmd in plan.Commands)
        {
            // 상대 좌표 스크린 좌표 오프셋으로 변환
            Vector2 screenOffset = new Vector2(cmd.Position.x * ratio, -cmd.Position.y * ratio);
            Vector2 drawPos = drawCenter + screenOffset;

            // 그룹 Order 찾기 (색상 매핑용)
            int orderVal = 0;
            // cmd.StartTime 기반으로 어떤 Order에 속해 있는지 파악
            // 혹은 그냥 몬스터 이름을 토대로 인쇄
            
            // 색상 할당 (Order % 5 기준)
            Color dotColor = Color.green;
            if (squad.Groups.Count > 0)
            {
                var matchedGroup = squad.Groups.FirstOrDefault(g => g.SpawnUnitKey == cmd.UnitKey && g.SpawnRole == cmd.Role);
                if (matchedGroup != null)
                {
                    int colorIdx = orders.IndexOf(matchedGroup.Order);
                    dotColor = GetColorByIndex(colorIdx);
                }
            }

            // 점 그리기 (몬스터)
            EditorGUI.DrawRect(new Rect(drawPos.x - 4, drawPos.y - 4, 8, 8), dotColor);

            // 방향선 그리기 (노란색)
            Vector2 lookVec = SpawnCoordinateUtility.GetLookVector(cmd.Rotation);
            Vector2 dirVec = new Vector2(lookVec.x, -lookVec.y) * 12f;
            Handles.color = Color.yellow;
            Handles.DrawLine(drawPos, drawPos + dirVec);

            // 타이밍/이름 라벨 표시
            string labelText = $"[{cmd.StartTime:F1}s] {GetUnitDisplayName(cmd.UnitKey, cmd.Role)}";
            DrawLabel(drawPos, labelText);
        }

        GUI.color = Color.white;
        GUI.Label(new Rect(rect.x + 5, rect.y + 5, 250, 20), $"총 {plan.Commands.Count}마리 소환 (완료 시간: {plan.Commands.Max(c => c.StartTime):F2}초)", EditorStyles.miniLabel);
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

    private string GetUnitDisplayName(string unitKey, SpawnUnitRole role)
    {
        if (!string.IsNullOrEmpty(unitKey)) return unitKey;
        return role != SpawnUnitRole.Any ? role.ToString() : "None";
    }

    private void DrawLabel(Vector2 pos, string text)
    {
        GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
        style.normal.textColor = Color.white;
        Vector2 size = style.CalcSize(new GUIContent(text));
        
        Rect bgRect = new Rect(pos.x + 5, pos.y - size.y / 2, size.x, size.y);
        EditorGUI.DrawRect(bgRect, new Color(0, 0, 0, 0.6f));
        GUI.Label(bgRect, text, style);
    }

    private class GroupElementWrapper
    {
        public int Index { get; }
        public int Order { get; }
        public SerializedProperty Property { get; }

        public GroupElementWrapper(int index, int order, SerializedProperty property)
        {
            Index = index;
            Order = order;
            Property = property;
        }
    }
}
#endif
