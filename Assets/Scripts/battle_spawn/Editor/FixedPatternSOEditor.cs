#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FixedPatternSO))]
public sealed class FixedPatternSOEditor : Editor
{
    private bool showPreview = true;

    // 배치 수정 및 제어 도구 상태 값
    private float modifyRotation = 0f;
    private float modifyScale = 1f;
    private LookDirectionType lookDirType = LookDirectionType.AxisY;
    private bool flipDirection = false;

    public override void OnInspectorGUI()
    {
        FixedPatternSO pattern = (FixedPatternSO)target;
        if (pattern == null) return;

        serializedObject.Update();

        // 1. 슬롯 배치 정보 표시
        EditorGUILayout.LabelField("고정 배치 정보 (Fixed Pattern Slots)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox($"수량: {pattern.Slots?.Count ?? 0}", MessageType.Info);
        
        SerializedProperty slotsProp = serializedObject.FindProperty("slots");
        EditorGUILayout.PropertyField(slotsProp, new GUIContent("배치 슬롯 목록"), true);

        EditorGUILayout.Space();

        // 2. 배치 수정 및 제어 도구
        GUI.backgroundColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        EditorGUILayout.BeginVertical("box");
        GUI.backgroundColor = Color.white;

        EditorGUILayout.LabelField("배치 수정 및 제어 도구 (Layout Modifiers)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        modifyRotation = EditorGUILayout.Slider("회전각 조정 (도)", modifyRotation, 0f, 360f);
        modifyScale = EditorGUILayout.FloatField("스케일 배수 조정", modifyScale);

        EditorGUILayout.Space();

        lookDirType = (LookDirectionType)EditorGUILayout.EnumPopup("바라보는 방향 기준", lookDirType);
        flipDirection = EditorGUILayout.Toggle("반대 방향 (Flip)", flipDirection);

        EditorGUILayout.Space();

        if (GUILayout.Button("변동사항 일괄 적용 (Bake)", GUILayout.Height(30)))
        {
            pattern.ApplyModifiers(modifyRotation, modifyScale, lookDirType, flipDirection);
            EditorUtility.SetDirty(pattern);
            AssetDatabase.SaveAssets();
            
            // 적용 후 값 초기화
            modifyRotation = 0f;
            modifyScale = 1f;
            lookDirType = LookDirectionType.AxisY;
            flipDirection = false;
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        // 3. 배치 미리보기 (2D Preview)
        showPreview = EditorGUILayout.Foldout(showPreview, "현재 배치 미리보기 (2D Preview)");
        if (showPreview)
        {
            DrawSlotsPreview(pattern);
        }

        EditorGUILayout.Space();

        // 4. 변경 예정 배치 미리보기 (Modified Preview)
        bool hasChanges = modifyRotation != 0f || 
                          modifyScale != 1f || 
                          lookDirType != LookDirectionType.AxisY || 
                          flipDirection;

        if (hasChanges && pattern.Slots != null && pattern.Slots.Count > 0)
        {
            EditorGUILayout.LabelField("변경 예정 배치 미리보기 (Modified Preview)", EditorStyles.boldLabel);
            DrawModifiedPreview(pattern);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSlotsPreview(FixedPatternSO pattern)
    {
        Rect rect = GUILayoutUtility.GetRect(200, 200);

        EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 1f));
        Handles.color = Color.gray;
        Handles.DrawWireCube(rect.center, rect.size);

        Handles.color = new Color(0.25f, 0.25f, 0.25f, 0.5f);
        Handles.DrawLine(new Vector2(rect.x, rect.center.y), new Vector2(rect.xMax, rect.center.y));
        Handles.DrawLine(new Vector2(rect.center.x, rect.y), new Vector2(rect.center.x, rect.yMax));

        if (pattern.Slots == null || pattern.Slots.Count == 0)
        {
            GUI.Label(new Rect(rect.x + 10, rect.y + 10, 180, 20), "표시할 배치 데이터가 없습니다.", EditorStyles.miniLabel);
            return;
        }

        float maxAbsFixed = 1f;
        foreach (var slot in pattern.Slots)
        {
            if (slot == null) continue;
            maxAbsFixed = Mathf.Max(maxAbsFixed, Mathf.Abs(slot.LocalPosition.x));
            maxAbsFixed = Mathf.Max(maxAbsFixed, Mathf.Abs(slot.LocalPosition.y));
        }
        maxAbsFixed *= 1.3f;

        float ratioFixed = Mathf.Min(rect.width, rect.height) * 0.5f / maxAbsFixed;
        Vector2 centerFixed = rect.center;

        // 원점
        EditorGUI.DrawRect(new Rect(centerFixed.x - 3, centerFixed.y - 3, 6, 6), Color.cyan);

        for (int i = 0; i < pattern.Slots.Count; i++)
        {
            var slot = pattern.Slots[i];
            if (slot == null) continue;

            Vector2 screenOffset = new Vector2(slot.LocalPosition.x * ratioFixed, -slot.LocalPosition.y * ratioFixed);
            Vector2 drawPos = centerFixed + screenOffset;

            // 슬롯 녹색 점
            EditorGUI.DrawRect(new Rect(drawPos.x - 4, drawPos.y - 4, 8, 8), Color.green);

            // 방향 노란색 선
            Vector2 lookVec = SpawnCoordinateUtility.GetLookVector(slot.LocalRotation);
            Vector2 dirVec = new Vector2(lookVec.x, -lookVec.y) * 12f;
            Handles.color = Color.yellow;
            Handles.DrawLine(drawPos, drawPos + dirVec);
        }

        GUI.color = Color.white;
        GUI.Label(new Rect(rect.x + 5, rect.y + 5, 230, 20), "Cyan: Origin, Green: Slot, Yellow: Looking Dir", EditorStyles.miniLabel);
    }

    private void DrawModifiedPreview(FixedPatternSO pattern)
    {
        Rect rect = GUILayoutUtility.GetRect(200, 200);

        EditorGUI.DrawRect(rect, new Color(0.15f, 0.08f, 0.08f, 1f));
        Handles.color = Color.gray;
        Handles.DrawWireCube(rect.center, rect.size);

        Handles.color = new Color(0.35f, 0.2f, 0.2f, 0.5f);
        Handles.DrawLine(new Vector2(rect.x, rect.center.y), new Vector2(rect.xMax, rect.center.y));
        Handles.DrawLine(new Vector2(rect.center.x, rect.y), new Vector2(rect.center.x, rect.yMax));

        List<SpawnPatternSlot> simSlots = new List<SpawnPatternSlot>();
        float maxAbs = 1f;

        foreach (var slot in pattern.Slots)
        {
            if (slot == null) continue;
            Vector2 originalPos = slot.LocalPosition;

            // 스케일
            Vector2 scaled = originalPos * modifyScale;

            // 회전
            Vector2 rotated = SpawnCoordinateUtility.Rotate(scaled, modifyRotation);

            // 방향
            float simRot = slot.LocalRotation + modifyRotation;
            if (lookDirType != LookDirectionType.AxisY || flipDirection)
            {
                simRot = SpawnCoordinateUtility.CalculateDirectionAngle(rotated, lookDirType, flipDirection);
            }
            simRot = (simRot % 360f + 360f) % 360f;

            simSlots.Add(new SpawnPatternSlot(rotated, simRot));

            maxAbs = Mathf.Max(maxAbs, Mathf.Abs(rotated.x));
            maxAbs = Mathf.Max(maxAbs, Mathf.Abs(rotated.y));
        }

        maxAbs *= 1.3f;
        float simRatio = Mathf.Min(rect.width, rect.height) * 0.5f / maxAbs;
        Vector2 drawCenter = rect.center;

        // 원점
        EditorGUI.DrawRect(new Rect(drawCenter.x - 3, drawCenter.y - 3, 6, 6), Color.cyan);

        foreach (var slot in simSlots)
        {
            Vector2 screenOffset = new Vector2(slot.LocalPosition.x * simRatio, -slot.LocalPosition.y * simRatio);
            Vector2 drawPos = drawCenter + screenOffset;

            // 수정 예정 점 (적색)
            EditorGUI.DrawRect(new Rect(drawPos.x - 4, drawPos.y - 4, 8, 8), Color.red);

            // 수정 예정 방향선 (황색)
            Vector2 lookVec = SpawnCoordinateUtility.GetLookVector(slot.LocalRotation);
            Vector2 dirVec = new Vector2(lookVec.x, -lookVec.y) * 12f;
            Handles.color = Color.yellow;
            Handles.DrawLine(drawPos, drawPos + dirVec);
        }

        GUI.color = Color.white;
        GUI.Label(new Rect(rect.x + 5, rect.y + 5, 230, 20), "Red Dots: Proposed Layout, Yellow: Proposed Dir", EditorStyles.miniLabel);
    }
}
#endif
