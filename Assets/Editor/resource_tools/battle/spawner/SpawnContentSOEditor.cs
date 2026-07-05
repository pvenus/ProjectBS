#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpawnContentSO), false)]
public class SpawnContentSOEditor : Editor
{
    private bool showPreview = true;
    private Dictionary<string, List<Vector2>> randomDotsCache = new Dictionary<string, List<Vector2>>();
    private string lastCacheKey = "";

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // 기본 필드 렌더링

        SpawnContentSO content = (SpawnContentSO)target;
        if (content == null) return;

        EditorGUILayout.Space();
        showPreview = EditorGUILayout.Foldout(showPreview, "스폰 패턴 배치 미리보기 (2D Preview)");
        if (showPreview)
        {
            DrawContentPreview(content);
        }
    }

    private void DrawContentPreview(SpawnContentSO content)
    {
        Rect rect = GUILayoutUtility.GetRect(200, 200);

        EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f, 1f));
        Handles.color = Color.gray;
        Handles.DrawWireCube(rect.center, rect.size);

        Handles.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        Handles.DrawLine(new Vector2(rect.x, rect.center.y), new Vector2(rect.xMax, rect.center.y));
        Handles.DrawLine(new Vector2(rect.center.x, rect.y), new Vector2(rect.center.x, rect.yMax));

        SpawnPatternData pattern = GetPatternOfContent(content);

        float maxAbs = CalculatePatternMaxAbs(pattern);
        if (maxAbs < 0.1f) maxAbs = 1f;
        maxAbs *= 1.5f;

        float ratio = Mathf.Min(rect.width, rect.height) * 0.5f / maxAbs;
        float widthRatio = ratio;
        float heightRatio = ratio;

        Vector2 drawCenter = rect.center;

        // 원점 표시 (Cyan)
        EditorGUI.DrawRect(new Rect(drawCenter.x - 2, drawCenter.y - 2, 4, 4), Color.cyan);

        string safeId = !string.IsNullOrEmpty(content.ContentId) ? content.ContentId : content.name;

        string currentKey = $"{safeId}_{pattern?.PatternId ?? "none"}";
        if (currentKey != lastCacheKey)
        {
            randomDotsCache.Clear();
            lastCacheKey = currentKey;
        }

        DrawPattern(
            content,
            pattern, 
            drawCenter, 
            widthRatio, 
            heightRatio, 
            safeId);

        GUI.color = Color.white;
    }

    private SpawnPatternData GetPatternOfContent(SpawnContentSO content)
    {
        if (content is SpawnSquadSO squad)
        {
            if (squad.HasFormationPattern)
            {
                return squad.FormationPattern;
            }

            if (squad.Groups != null && squad.Groups.Count > 0)
            {
                return squad.Groups[0].ToPatternData();
            }
        }
        return null;
    }

    private float CalculatePatternMaxAbs(SpawnPatternData pattern)
    {
        if (pattern == null || !pattern.HasPattern) return 1f;

        float max = 0f;
        if (pattern.PatternKind.IsFixedSlotKind())
        {
            FixedSpawnPatternConfig fixedConfig = pattern.FixedConfig;
            if (fixedConfig != null && fixedConfig.Slots != null)
            {
                foreach (var pos in fixedConfig.Slots)
                {
                    if (pos == null) continue;
                    max = Mathf.Max(max, Mathf.Abs(pos.LocalPosition.x));
                    max = Mathf.Max(max, Mathf.Abs(pos.LocalPosition.y));
                }
            }
        }
        else if (pattern.PatternKind.IsRandomAreaKind())
        {
            RandomSpawnPatternConfig randomConfig = pattern.RandomConfig;
            if (randomConfig != null)
            {
                max = Mathf.Max(randomConfig.AreaSize.x, randomConfig.AreaSize.y);
            }
        }
        return max;
    }

    private void DrawPattern(
        SpawnContentSO content,
        SpawnPatternData pattern,
        Vector2 pivot,
        float widthRatio,
        float heightRatio,
        string safeId)
    {
        if (pattern == null || !pattern.HasPattern)
        {
            EditorGUI.DrawRect(new Rect(pivot.x - 4, pivot.y - 4, 8, 8), Color.green);
            DrawLabel(pivot, "단일 소환 (Pattern: None)");
            return;
        }

        if (pattern.PatternKind.IsFixedSlotKind())
        {
            FixedSpawnPatternConfig fixedConfig = pattern.FixedConfig;
            if (fixedConfig != null && fixedConfig.Slots != null)
            {
                for (int i = 0; i < fixedConfig.Slots.Count; i++)
                {
                    var pos = fixedConfig.Slots[i];
                    if (pos == null) continue;

                    Vector2 childScreenOffset = new Vector2(pos.LocalPosition.x * widthRatio, -pos.LocalPosition.y * heightRatio);
                    Vector2 drawPos = pivot + childScreenOffset;

                    EditorGUI.DrawRect(new Rect(drawPos.x - 4, drawPos.y - 4, 8, 8), Color.green);
                    DrawLabel(drawPos, $"Slot {i + 1}");

                    float angleRad = pos.LocalRotation * Mathf.Deg2Rad;
                    Vector2 dirVec = new Vector2(-Mathf.Sin(angleRad), -Mathf.Cos(angleRad)) * 12f;
                    Handles.color = Color.yellow;
                    Handles.DrawLine(drawPos, drawPos + dirVec);
                }
            }
        }
        else if (pattern.PatternKind.IsRandomAreaKind())
        {
            RandomSpawnPatternConfig randomConfig = pattern.RandomConfig;
            if (randomConfig == null)
            {
                return;
            }

            Color color = Color.green;
            Handles.color = new Color(color.r, color.g, color.b, 0.5f);

            SpawnAreaShape shape = pattern.PatternKind.ResolveAreaShape(randomConfig.Shape);
            if (shape == SpawnAreaShape.Circle)
            {
                float pixelRadius = randomConfig.AreaSize.x * widthRatio;
                Handles.DrawWireDisc(pivot, Vector3.forward, pixelRadius);
            }
            else if (shape == SpawnAreaShape.Rectangle)
            {
                float w = randomConfig.AreaSize.x * widthRatio;
                float h = randomConfig.AreaSize.y * heightRatio;
                Handles.DrawWireCube(pivot, new Vector3(w, h, 0f));
            }

            int qty = 1;
            if (content is SpawnSquadSO squad && squad.Groups != null && squad.Groups.Count > 0)
            {
                if (squad.HasFormationPattern && pattern != null && pattern.PatternId == squad.FormationPatternId)
                {
                    qty = squad.FormationQuantity;
                }
                else
                {
                    var matched = squad.Groups.FirstOrDefault(g => g.PatternId == pattern.PatternId);
                    int groupQuantity = matched != null ? matched.Quantity : squad.Groups[0].Quantity;
                    qty = groupQuantity > 0 ? groupQuantity : squad.Quantity;
                }
            }
            qty = Mathf.Max(1, qty);

            string dictKey = $"rand_{safeId}_{pattern.PatternId ?? "temp"}";
            if (!randomDotsCache.TryGetValue(dictKey, out List<Vector2> dots))
            {
                dots = new List<Vector2>();
                System.Random rand = new System.Random(dictKey.GetHashCode());
                for (int d = 0; d < qty; d++)
                {
                    Vector2 localPos = Vector2.zero;
                    if (shape == SpawnAreaShape.Circle)
                    {
                        double angle = rand.NextDouble() * Math.PI * 2;
                        double r = Math.Sqrt(rand.NextDouble()) * randomConfig.AreaSize.x;
                        localPos = new Vector2((float)(Math.Cos(angle) * r), (float)(Math.Sin(angle) * r));
                    }
                    else if (shape == SpawnAreaShape.Rectangle)
                    {
                        float rx = (float)(rand.NextDouble() - 0.5) * randomConfig.AreaSize.x;
                        float ry = (float)(rand.NextDouble() - 0.5) * randomConfig.AreaSize.y;
                        localPos = new Vector2(rx, ry);
                    }
                    dots.Add(localPos);
                }
                randomDotsCache[dictKey] = dots;
            }

            foreach (var dot in dots)
            {
                Vector2 screenOffset = new Vector2(dot.x * widthRatio, -dot.y * heightRatio);
                Vector2 drawPos = pivot + screenOffset;

                EditorGUI.DrawRect(new Rect(drawPos.x - 3, drawPos.y - 3, 6, 6), new Color(0.2f, 1f, 0.2f, 0.7f));
            }
        }
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
