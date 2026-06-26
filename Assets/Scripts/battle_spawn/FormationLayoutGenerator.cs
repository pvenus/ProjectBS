using System.Collections.Generic;
using UnityEngine;

public enum LayoutGenerationType
{
    Rectangle,
    Circle,
    Triangle,
    RowPattern
}

public static class FormationLayoutGenerator
{
    public static List<Vector2> GenerateRectangle(int rows, int cols, float spacingX, float spacingY)
    {
        List<Vector2> list = new List<Vector2>();
        if (rows <= 0 || cols <= 0) return list;

        float startX = -(cols - 1) * spacingX * 0.5f;
        float startY = -(rows - 1) * spacingY * 0.5f;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float x = startX + c * spacingX;
                float y = startY + r * spacingY;
                list.Add(new Vector2(x, y));
            }
        }
        return list;
    }

    public static List<Vector2> GenerateCircle(int count, float radius)
    {
        List<Vector2> list = new List<Vector2>();
        if (count <= 0) return list;

        float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float angleRad = (i * angleStep) * Mathf.Deg2Rad;
            float x = radius * Mathf.Cos(angleRad);
            float y = radius * Mathf.Sin(angleRad);
            list.Add(new Vector2(x, y));
        }
        return list;
    }

    public static List<Vector2> GenerateTriangle(int rows, float spacing)
    {
        List<Vector2> list = new List<Vector2>();
        if (rows <= 0) return list;

        // 정삼각형 높이비 적용 (sqrt(3)/2 = 0.8660254f)
        float rowHeight = spacing * 0.8660254f;
        float totalHeight = (rows - 1) * rowHeight;
        float startY = totalHeight * 0.5f;

        for (int r = 0; r < rows; r++)
        {
            int colsInRow = r + 1;
            float startX = -(colsInRow - 1) * spacing * 0.5f;
            float y = startY - r * rowHeight;

            for (int c = 0; c < colsInRow; c++)
            {
                float x = startX + c * spacing;
                list.Add(new Vector2(x, y));
            }
        }
        return list;
    }

    public static List<Vector2> GenerateRowPattern(List<int> groupCounts, float spacing)
    {
        List<Vector2> list = new List<Vector2>();
        if (groupCounts == null || groupCounts.Count == 0) return list;

        float totalHeight = (groupCounts.Count - 1) * spacing;
        float startY = totalHeight * 0.5f;

        for (int r = 0; r < groupCounts.Count; r++)
        {
            int countInRow = groupCounts[r];
            if (countInRow <= 0) continue;

            float startX = -(countInRow - 1) * spacing * 0.5f;
            float y = startY - r * spacing;

            for (int c = 0; c < countInRow; c++)
            {
                float x = startX + c * spacing;
                list.Add(new Vector2(x, y));
            }
        }
        return list;
    }
}
