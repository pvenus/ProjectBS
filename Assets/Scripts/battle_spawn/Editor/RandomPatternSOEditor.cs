#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RandomPatternSO))]
public sealed class RandomPatternSOEditor : Editor
{
    private bool showPreview = true;
    private float modifyScale = 1f;

    public override void OnInspectorGUI()
    {
        RandomPatternSO pattern = (RandomPatternSO)target;
        if (pattern == null) return;

        serializedObject.Update();

        EditorGUILayout.LabelField("랜덤 배치 영역 정보 (Random Pattern Area)", EditorStyles.boldLabel);
        
        SerializedProperty shapeProp = serializedObject.FindProperty("shape");
        SerializedProperty areaSizeProp = serializedObject.FindProperty("areaSize");

        EditorGUILayout.PropertyField(shapeProp, new GUIContent("영역 형태 (Shape)"));
        
        if ((SpawnAreaShape)shapeProp.enumValueIndex == SpawnAreaShape.Circle)
        {
            float radius = areaSizeProp.vector2Value.x;
            radius = EditorGUILayout.FloatField("원형 반경 (Radius)", radius);
            areaSizeProp.vector2Value = new Vector2(radius, radius);
        }
        else
        {
            Vector2 size = areaSizeProp.vector2Value;
            size.x = EditorGUILayout.FloatField("가로 크기 (Width)", size.x);
            size.y = EditorGUILayout.FloatField("세로 크기 (Height)", size.y);
            areaSizeProp.vector2Value = size;
        }

        EditorGUILayout.Space();

        // 배치 수정 도구 (Scale만 지원)
        GUI.backgroundColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        EditorGUILayout.BeginVertical("box");
        GUI.backgroundColor = Color.white;

        EditorGUILayout.LabelField("배치 수정 도구 (Layout Modifiers)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        modifyScale = EditorGUILayout.FloatField("스케일 배수 조정", modifyScale);

        EditorGUILayout.Space();

        if (GUILayout.Button("스케일 즉시 반영 (Bake)"))
        {
            pattern.ScaleAreaSize(modifyScale);
            EditorUtility.SetDirty(pattern);
            AssetDatabase.SaveAssets();
            modifyScale = 1f;
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        // 3. 배치 미리보기 (2D Preview)
        showPreview = EditorGUILayout.Foldout(showPreview, "현재 영역 미리보기 (2D Preview)");
        if (showPreview)
        {
            DrawAreaPreview(pattern);
        }

        EditorGUILayout.Space();

        // 4. 변경 예정 배치 미리보기 (Modified Preview)
        if (modifyScale != 1f)
        {
            EditorGUILayout.LabelField("변경 예정 영역 미리보기 (Modified Preview)", EditorStyles.boldLabel);
            DrawModifiedPreview(pattern);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawAreaPreview(RandomPatternSO pattern)
    {
        Rect rect = GUILayoutUtility.GetRect(200, 200);

        EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 1f));
        Handles.color = Color.gray;
        Handles.DrawWireCube(rect.center, rect.size);

        Handles.color = new Color(0.25f, 0.25f, 0.25f, 0.5f);
        Handles.DrawLine(new Vector2(rect.x, rect.center.y), new Vector2(rect.xMax, rect.center.y));
        Handles.DrawLine(new Vector2(rect.center.x, rect.y), new Vector2(rect.center.x, rect.yMax));

        float maxAbs = Mathf.Max(1f, Mathf.Max(pattern.AreaSize.x, pattern.AreaSize.y)) * 1.3f;
        float ratio = Mathf.Min(rect.width, rect.height) * 0.5f / maxAbs;
        Vector2 center = rect.center;

        // 원점
        EditorGUI.DrawRect(new Rect(center.x - 2, center.y - 2, 4, 4), Color.cyan);

        Handles.color = Color.green;
        if (pattern.Shape == SpawnAreaShape.Circle)
        {
            float radiusPixels = pattern.AreaSize.x * ratio;
            Handles.DrawWireDisc(center, Vector3.forward, radiusPixels);
        }
        else if (pattern.Shape == SpawnAreaShape.Rectangle)
        {
            float w = pattern.AreaSize.x * ratio;
            float h = pattern.AreaSize.y * ratio;
            Handles.DrawWireCube(center, new Vector3(w, h, 0f));
        }

        GUI.color = Color.white;
        GUI.Label(new Rect(rect.x + 5, rect.y + 5, 230, 20), $"Green Outline: Random Area ({pattern.Shape})", EditorStyles.miniLabel);
    }

    private void DrawModifiedPreview(RandomPatternSO pattern)
    {
        Rect rect = GUILayoutUtility.GetRect(200, 200);

        EditorGUI.DrawRect(rect, new Color(0.15f, 0.08f, 0.08f, 1f));
        Handles.color = Color.gray;
        Handles.DrawWireCube(rect.center, rect.size);

        Handles.color = new Color(0.35f, 0.2f, 0.2f, 0.5f);
        Handles.DrawLine(new Vector2(rect.x, rect.center.y), new Vector2(rect.xMax, rect.center.y));
        Handles.DrawLine(new Vector2(rect.center.x, rect.y), new Vector2(rect.center.x, rect.yMax));

        Vector2 simAreaSize = pattern.AreaSize * modifyScale;
        float maxAbs = Mathf.Max(1f, Mathf.Max(simAreaSize.x, simAreaSize.y)) * 1.3f;
        float ratio = Mathf.Min(rect.width, rect.height) * 0.5f / maxAbs;
        Vector2 center = rect.center;

        // 원점
        EditorGUI.DrawRect(new Rect(center.x - 2, center.y - 2, 4, 4), Color.cyan);

        Handles.color = Color.red; // 가상 수정은 적색
        if (pattern.Shape == SpawnAreaShape.Circle)
        {
            float radiusPixels = simAreaSize.x * ratio;
            Handles.DrawWireDisc(center, Vector3.forward, radiusPixels);
        }
        else if (pattern.Shape == SpawnAreaShape.Rectangle)
        {
            float w = simAreaSize.x * ratio;
            float h = simAreaSize.y * ratio;
            Handles.DrawWireCube(center, new Vector3(w, h, 0f));
        }

        GUI.color = Color.white;
        GUI.Label(new Rect(rect.x + 5, rect.y + 5, 230, 20), $"Red Outline: Proposed Random Area ({pattern.Shape})", EditorStyles.miniLabel);
    }
}
#endif
