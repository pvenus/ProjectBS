#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Character;

[CustomEditor(typeof(SpawnSequenceSO))]
public class SpawnSequenceSOEditor : Editor
{
    private bool showPreview = true;

    public override void OnInspectorGUI()
    {
        SpawnSequenceSO sequence = (SpawnSequenceSO)target;

        serializedObject.Update();

        EditorGUILayout.LabelField("시퀀스 정보", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("sequenceId"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"));

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("반복 설정", EditorStyles.boldLabel);
        SerializedProperty repeatModeProp = serializedObject.FindProperty("repeatMode");
        EditorGUILayout.PropertyField(repeatModeProp);
        if (repeatModeProp.enumValueIndex == (int)SpawnSequenceRepeatMode.Infinite)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loopStartOrder"));
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("시퀀스 스텝 (패턴 및 몬스터 실행 정보)", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("steps"), true);

        EditorGUILayout.Space();

        List<string> errors = sequence.Validate();
        if (errors.Count > 0)
        {
            EditorGUILayout.LabelField("데이터 유효성 검증 오류", EditorStyles.boldLabel);
            for (int i = 0; i < errors.Count; i++)
            {
                EditorGUILayout.HelpBox(errors[i], MessageType.Error);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("데이터 검증 통과 (이상 없음)", MessageType.Info);
        }

        EditorGUILayout.Space();

        showPreview = EditorGUILayout.Foldout(showPreview, "시퀀스 통합 미리보기 (2D Preview)");
        if (showPreview)
        {
            DrawSequencePreview(sequence);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSequencePreview(SpawnSequenceSO sequence)
    {
        Rect rect = GUILayoutUtility.GetRect(200, 300);

        EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f, 1f));
        Handles.color = Color.gray;
        Handles.DrawWireCube(rect.center, rect.size);

        Handles.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        Handles.DrawLine(new Vector2(rect.x, rect.center.y), new Vector2(rect.xMax, rect.center.y));
        Handles.DrawLine(new Vector2(rect.center.x, rect.y), new Vector2(rect.center.x, rect.yMax));

        float maxAbs = 1f;
        Dictionary<SpawnSequenceStep, SpawnPlan> stepPlans = new Dictionary<SpawnSequenceStep, SpawnPlan>();

        if (sequence.Steps != null)
        {
            foreach (var step in sequence.Steps)
            {
                if (step == null || step.Content == null) continue;
                try
                {
                    SpawnPlan plan = SpawnContentResolver.Resolve(new SpawnRequest(step.Content, Vector3.zero, 0f));
                    stepPlans[step] = plan;
                    
                    float mockStepOffsetX = (step.Order - 1) * 6f;
                    foreach (var cmd in plan.Commands)
                    {
                        float posX = cmd.Position.x + mockStepOffsetX;
                        float posY = cmd.Position.y;
                        maxAbs = Mathf.Max(maxAbs, Mathf.Abs(posX));
                        maxAbs = Mathf.Max(maxAbs, Mathf.Abs(posY));
                    }
                }
                catch
                {
                    // Ignore resolution errors for max scale calculation
                }
            }
        }
        
        maxAbs *= 1.4f;
        float ratio = Mathf.Min(rect.width, rect.height) * 0.5f / maxAbs;

        if (sequence.Steps != null)
        {
            foreach (var step in sequence.Steps)
            {
                if (step == null || step.Content == null) continue;

                float mockStepOffsetX = (step.Order - 1) * 6f;
                Vector2 scaleOffset = new Vector2(mockStepOffsetX * ratio, 0f);
                Vector2 drawAnchorCenter = rect.center + scaleOffset;

                // Step Pivot
                EditorGUI.DrawRect(new Rect(drawAnchorCenter.x - 3, drawAnchorCenter.y - 3, 6, 6), Color.green);

                if (!stepPlans.TryGetValue(step, out SpawnPlan plan))
                {
                    try
                    {
                        plan = SpawnContentResolver.Resolve(new SpawnRequest(step.Content, Vector3.zero, 0f));
                    }
                    catch (Exception ex)
                    {
                        GUI.color = Color.red;
                        GUI.Label(new Rect(drawAnchorCenter.x - 50, drawAnchorCenter.y + 10, 100, 40), $"오류:\n{ex.Message}", EditorStyles.miniLabel);
                        GUI.color = Color.white;
                        continue;
                    }
                }

                if (plan == null || plan.Commands.Count == 0) continue;

                Color color = GetOrderColor(step.Order);
                foreach (var cmd in plan.Commands)
                {
                    Vector2 screenOffset = new Vector2(cmd.Position.x * ratio, -cmd.Position.y * ratio);
                    Vector2 drawPos = drawAnchorCenter + screenOffset;

                    // Draw monster dot
                    EditorGUI.DrawRect(new Rect(drawPos.x - 4, drawPos.y - 4, 8, 8), color);

                    // Draw direction line (yellow)
                    Vector2 lookVec = SpawnCoordinateUtility.GetLookVector(cmd.Rotation);
                    Vector2 dirVec = new Vector2(lookVec.x, -lookVec.y) * 12f;
                    Handles.color = Color.yellow;
                    Handles.DrawLine(drawPos, drawPos + dirVec);

                    // Label
                    string labelText = $"[{step.Order}-{cmd.StartTime:F1}s] {GetNpcDisplayName(cmd.Character)}";
                    DrawLabel(drawPos, labelText);
                }
            }
        }

        GUI.color = Color.white;
        GUI.Label(new Rect(rect.x + 5, rect.y + 5, 250, 20), "Green dots: Mock Step Pivots (Spaced)", EditorStyles.miniLabel);
    }

    private string GetNpcDisplayName(CharacterSO npc)
    {
        if (npc == null) return "Unknown";
        try
        {
            string displayName = npc.DisplayName;
            if (!string.IsNullOrEmpty(displayName)) return displayName;
        }
        catch
        {
        }
        return !string.IsNullOrEmpty(npc.CharacterId) ? npc.CharacterId : npc.name;
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

    private Color GetOrderColor(int order)
    {
        switch (order % 6)
        {
            case 0: return Color.green;
            case 1: return Color.red;
            case 2: return new Color(0.2f, 0.6f, 1f, 1f); // Light blue
            case 3: return Color.yellow;
            case 4: return Color.magenta;
            case 5: return new Color(1f, 0.5f, 0f, 1f); // Orange
            default: return Color.white;
        }
    }
}
#endif
