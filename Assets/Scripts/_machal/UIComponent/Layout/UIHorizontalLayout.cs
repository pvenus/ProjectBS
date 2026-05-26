using UnityEngine;
using System.Collections.Generic;

public class UIHorizontalLayout : MonoBehaviour
{
    public float spacing = 200f;
    public float startOffsetX = 0f;

    /// <summary>
    /// 지정된 자식 개수에 맞춰 가로 배치 목표 좌표를 계산하여 반환합니다.
    /// 애니메이션 기반 배치와 충돌하지 않도록 실제 Transform을 조작하지는 않습니다.
    /// </summary>
    public List<Vector2> CalculatePositions(int childCount)
    {
        List<Vector2> positions = new List<Vector2>();
        if (childCount <= 0) return positions;

        // 전체 가로 길이를 계산하여 중앙 정렬이 되도록 시작 X 좌표를 구함
        float totalWidth = (childCount - 1) * spacing;
        float startX = startOffsetX - (totalWidth / 2f);

        for (int i = 0; i < childCount; i++)
        {
            float x = startX + (i * spacing);
            positions.Add(new Vector2(x, 0f));
        }

        return positions;
    }
}
