#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Character;

[CustomEditor(typeof(SpawnFormationSO))]
public sealed class SpawnFormationSOEditor : Editor
{
    private bool showPreview = true;

    public override void OnInspectorGUI()
    {
        SpawnFormationSO formation = (SpawnFormationSO)target;
        if (formation == null) return;

        serializedObject.Update();

        EditorGUILayout.LabelField("포메이션 설정", EditorStyles.boldLabel);
        
        // Content ID 표시
        EditorGUILayout.LabelField("Content ID", formation.ContentId);
        EditorGUILayout.Space();

        // 1. Squad SO 설정
        SerializedProperty squadProp = serializedObject.FindProperty("squad");
        EditorGUILayout.PropertyField(squadProp, new GUIContent("하위 스쿼드 (SpawnSquadSO)"));

        // 2. Pattern SO 설정
        SerializedProperty patternProp = serializedObject.FindProperty("pattern");
        EditorGUILayout.PropertyField(patternProp, new GUIContent("포메이션 배치 패턴 (SpawnPatternSO)"));

        // 3. Slot Interval 설정
        SerializedProperty slotIntervalProp = serializedObject.FindProperty("slotInterval");
        EditorGUILayout.PropertyField(slotIntervalProp, new GUIContent("슬롯 간 소환 지연 (Slot Interval)"));

        EditorGUILayout.Space();

        // 2D 미리보기 렌더링
        showPreview = EditorGUILayout.Foldout(showPreview, "포메이션 3계층 배치 미리보기 (2D Preview)");
        if (showPreview)
        {
            DrawFormationPreview(formation);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawFormationPreview(SpawnFormationSO formation)
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
        SpawnRequest req = new SpawnRequest(formation, Vector3.zero, 0f);
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

        // 소환 명령별 2D 도식화 그리기
        foreach (var cmd in plan.Commands)
        {
            // 상대 좌표 스크린 좌표 오프셋으로 변환
            Vector2 screenOffset = new Vector2(cmd.Position.x * ratio, -cmd.Position.y * ratio);
            Vector2 drawPos = drawCenter + screenOffset;

            // 점 그리기 (몬스터) - 기본 녹색
            EditorGUI.DrawRect(new Rect(drawPos.x - 4, drawPos.y - 4, 8, 8), Color.green);

            // 방향선 그리기 (노란색)
            Vector2 lookVec = SpawnCoordinateUtility.GetLookVector(cmd.Rotation);
            Vector2 dirVec = new Vector2(lookVec.x, -lookVec.y) * 12f;
            Handles.color = Color.yellow;
            Handles.DrawLine(drawPos, drawPos + dirVec);

            // 타이밍/이름 라벨 표시
            string labelText = $"[{cmd.StartTime:F1}s] {GetCharacterDisplayName(cmd.Character)}";
            DrawLabel(drawPos, labelText);
        }

        GUI.color = Color.white;
        GUI.Label(new Rect(rect.x + 5, rect.y + 5, 250, 20), $"총 {plan.Commands.Count}마리 소환 (완료 시간: {plan.Commands.Max(c => c.StartTime):F2}초)", EditorStyles.miniLabel);
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
        EditorGUI.DrawRect(bgRect, new Color(0, 0, 0, 0.6f));
        GUI.Label(bgRect, text, style);
    }
}
#endif
