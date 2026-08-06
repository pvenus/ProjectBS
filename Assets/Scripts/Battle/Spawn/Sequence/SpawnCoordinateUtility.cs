using UnityEngine;

public readonly struct SpawnTransform
{
    public Vector3 Position { get; }
    public float Rotation { get; }

    public SpawnTransform(Vector3 position, float rotation)
    {
        Position = position;
        Rotation = (rotation % 360f + 360f) % 360f;
    }
}

public static class SpawnCoordinateUtility
{
    // AxisY 반시계 방향 기준 2D 벡터 회전
    public static Vector2 Rotate(Vector2 localPosition, float parentRotation)
    {
        float rad = parentRotation * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(
            localPosition.x * cos - localPosition.y * sin,
            localPosition.x * sin + localPosition.y * cos
        );
    }

    // 각도에서 Look 방향 벡터 계산 (AxisY 기준)
    public static Vector2 GetLookVector(float rotation)
    {
        return Quaternion.Euler(0f, 0f, rotation) * Vector2.up;
    }

    // Look 방향 벡터에서 AxisY 기준 각도 계산
    public static float GetRotationFromLookVector(Vector2 direction)
    {
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return 0f;
        }
        return Vector2.SignedAngle(Vector2.up, direction.normalized);
    }

    // 계층 간 중첩 Transform 합성 규칙
    public static SpawnTransform Compose(SpawnTransform parent, Vector2 localPosition, float localRotation)
    {
        Vector2 worldOffset = Rotate(localPosition, parent.Rotation);
        return new SpawnTransform(
            parent.Position + new Vector3(worldOffset.x, worldOffset.y, 0f),
            parent.Rotation + localRotation
        );
    }

    // 배치 수정용 각도 계산 (AxisY 기준)
    public static float CalculateDirectionAngle(Vector2 pos, LookDirectionType lookDirType, bool flipDirection)
    {
        float angle = 0f;
        switch (lookDirType)
        {
            case LookDirectionType.AxisX:
                angle = flipDirection ? 90f : -90f;
                break;
            case LookDirectionType.AxisY:
                angle = flipDirection ? 180f : 0f;
                break;
            case LookDirectionType.Center:
                if (pos != Vector2.zero)
                {
                    Vector2 dir = flipDirection ? -pos : pos;
                    angle = GetRotationFromLookVector(dir);
                }
                else
                {
                    angle = 0f;
                }
                break;
        }
        return (angle % 360f + 360f) % 360f;
    }

    // --- 레거시 호환 및 UI Canvas 헬퍼 메서드들 ---
    public static Vector3 CalculatePlatoonBasePosition(
        Vector3 anchorWorldPos, 
        Vector2 patternAnchorOffset, 
        Vector2 patternPositionLocal)
    {
        return anchorWorldPos 
            + new Vector3(patternAnchorOffset.x, patternAnchorOffset.y, 0f) 
            + new Vector3(patternPositionLocal.x, patternPositionLocal.y, 0f);
    }

    public static Vector3 CalculateMonsterWorldPosition(
        Vector3 platoonBasePos, 
        float patternPositionRotation, 
        Vector2 platoonPositionLocal)
    {
        Vector2 rotated = Rotate(platoonPositionLocal, patternPositionRotation);
        return platoonBasePos + new Vector3(rotated.x, rotated.y, 0f);
    }

    public static Vector3 CalculateSingleMonsterWorldPosition(
        Vector3 anchorWorldPos,
        Vector2 patternAnchorOffset,
        Vector2 patternPositionLocal,
        float patternRotation = 0f,
        float patternScale = 1f,
        bool isCanvasCoordinate = false)
    {
        Vector2 scaledLocal = patternPositionLocal * patternScale;
        Vector2 rotatedLocal = Rotate(scaledLocal, patternRotation);

        if (isCanvasCoordinate)
        {
            Vector2 totalCanvasOffset = patternAnchorOffset + rotatedLocal;
            return CanvasToWorldPosition(totalCanvasOffset, anchorWorldPos, null);
        }
        return CalculatePlatoonBasePosition(anchorWorldPos, patternAnchorOffset, rotatedLocal);
    }

    public static Vector3 CanvasToWorldPosition(
        Vector2 canvasLocalPos, 
        Vector3 anchorWorldPos,
        Camera camera)
    {
        if (camera == null) camera = Camera.main;
        if (camera == null) return anchorWorldPos + new Vector3(canvasLocalPos.x, canvasLocalPos.y, 0f);

        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            RectTransform canvasRect = canvas.transform as RectTransform;
            if (canvasRect != null)
            {
                Vector2 canvasSize = canvasRect.sizeDelta;
                if (canvasSize.x > 0 && canvasSize.y > 0)
                {
                    float scaleX = Screen.width / canvasSize.x;
                    float scaleY = Screen.height / canvasSize.y;

                    float screenOffsetX = canvasLocalPos.x * scaleX;
                    float screenOffsetY = canvasLocalPos.y * scaleY;

                    Vector3 anchorScreenPos = camera.WorldToScreenPoint(anchorWorldPos);

                    Vector3 targetScreenPos = new Vector3(
                        anchorScreenPos.x + screenOffsetX,
                        anchorScreenPos.y + screenOffsetY,
                        anchorScreenPos.z
                    );

                    Vector3 targetWorldPos = camera.ScreenToWorldPoint(targetScreenPos);
                    targetWorldPos.z = 0f;
                    return targetWorldPos;
                }
            }
        }

        float virtualHeight = 1080f;
        float scale = Screen.height / virtualHeight;
        float fallbackScreenOffsetX = canvasLocalPos.x * scale;
        float fallbackScreenOffsetY = canvasLocalPos.y * scale;

        Vector3 fallbackAnchorScreenPos = camera.WorldToScreenPoint(anchorWorldPos);
        Vector3 fallbackTargetScreenPos = new Vector3(
            fallbackAnchorScreenPos.x + fallbackScreenOffsetX,
            fallbackAnchorScreenPos.y + fallbackScreenOffsetY,
            fallbackAnchorScreenPos.z
        );

        Vector3 fallbackWorldPos = camera.ScreenToWorldPoint(fallbackTargetScreenPos);
        fallbackWorldPos.z = 0f;
        return fallbackWorldPos;
    }
}
