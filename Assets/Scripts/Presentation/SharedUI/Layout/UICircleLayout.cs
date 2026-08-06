using UnityEngine;
using System.Collections.Generic;

public class UICircleLayout : MonoBehaviour
{
    public float radius = 300f;
    public float angleOffset = 90f; // 90 is top

    /// <summary>
    /// 지정된 자식 개수에 맞춰 원형 배치 목표 좌표를 계산하여 반환합니다.
    /// 애니메이션 기반 배치와 충돌하지 않도록 실제 Transform을 조작하지는 않습니다.
    /// </summary>
    public List<Vector2> CalculatePositions(int childCount)
    {
        List<Vector2> positions = new List<Vector2>();
        if (childCount <= 0) return positions;

        float angleStep = 360f / childCount;

        for (int i = 0; i < childCount; i++)
        {
            float angle = angleOffset - (angleStep * i);
            float rad = angle * Mathf.Deg2Rad;
            
            float x = Mathf.Cos(rad) * radius;
            float y = Mathf.Sin(rad) * radius;

            positions.Add(new Vector2(x, y));
        }

        return positions;
    }
}
