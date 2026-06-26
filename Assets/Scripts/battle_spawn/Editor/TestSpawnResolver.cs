#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class TestSpawnResolver
{
    [MenuItem("BS/Spawn/Test Resolver Output")]
    public static void TestResolver()
    {
        // 1. 임시로 pattern.fixed.circle.6p 패턴과 squad.wolf.line.3 스쿼드 에셋을 로드합니다.
        string circlePatternPath = "Assets/Scripts/battle_spawn/Resource/Generated/Patterns/Squads/pattern.fixed.circle.6p.asset";
        string line3SquadPath = "Assets/Scripts/battle_spawn/Resource/Generated/SpawnContents/Squads/squad.wolf.line.3.asset";

        FixedPatternSO circlePat = AssetDatabase.LoadAssetAtPath<FixedPatternSO>(circlePatternPath);
        SpawnSquadSO line3Squad = AssetDatabase.LoadAssetAtPath<SpawnSquadSO>(line3SquadPath);

        if (circlePat == null)
        {
            Debug.LogError($"패턴 에셋 로드 실패: {circlePatternPath}");
            return;
        }
        if (line3Squad == null)
        {
            Debug.LogError($"스쿼드 에셋 로드 실패: {line3SquadPath}");
            return;
        }

        // 2. 임시 포메이션 생성
        SpawnFormationSO tempFormation = ScriptableObject.CreateInstance<SpawnFormationSO>();
        tempFormation.Initialize("formation.test", circlePat, line3Squad, 0.5f, 1);

        // 3. Resolver를 통해 좌표 생성
        SpawnRequest req = new SpawnRequest(tempFormation, Vector3.zero, 0f);
        SpawnPlan plan = SpawnContentResolver.Resolve(req);

        Debug.Log("==== [Test Resolver Output] ====");
        Debug.Log($"소환 명령 총 개수: {plan.Commands.Count}");

        // 각 슬롯별로 그룹화해서 좌표 출력
        // circle6p의 슬롯이 6개이므로, 결과는 18마리여야 함 (6슬롯 * 3마리)
        for (int s = 0; s < 6; s++)
        {
            var slots = circlePat.GetSlots();
            var circleSlot = slots[s];
            Debug.Log($"[포메이션 슬롯 {s}] 위치: {circleSlot.LocalPosition}, 회전: {circleSlot.LocalRotation}");

            for (int m = 0; m < 3; m++)
            {
                int cmdIdx = s * 3 + m;
                if (cmdIdx < plan.Commands.Count)
                {
                    var cmd = plan.Commands[cmdIdx];
                    Debug.Log($"  -> 몬스터 {m}: 위치 {cmd.Position}, 회전 {cmd.Rotation}");
                }
            }

            // 1행 3마리 몬스터의 일직선성(기울기) 검증
            if (s * 3 + 2 < plan.Commands.Count)
            {
                Vector3 p1 = plan.Commands[s * 3].Position;
                Vector3 p2 = plan.Commands[s * 3 + 1].Position;
                Vector3 p3 = plan.Commands[s * 3 + 2].Position;

                Vector3 v1 = p2 - p1;
                Vector3 v2 = p3 - p2;

                // 외적(Cross Product)의 크기를 구해 평행한지 확인
                Vector3 cross = Vector3.Cross(v1.normalized, v2.normalized);
                Debug.Log($"  -> 몬스터 1, 2, 3 일직선 외적 크기 (0에 가까우면 일직선): {cross.magnitude}");
            }
        }
    }

    [MenuItem("BS/Spawn/Test AxisY Circle Formation")]
    public static void TestAxisYCircleFormation()
    {
        FixedPatternSO line3Pattern = ScriptableObject.CreateInstance<FixedPatternSO>();
        line3Pattern.Initialize(
            "pattern.test.line3",
            "Test Line 3",
            new List<SpawnPatternSlot>
            {
                new SpawnPatternSlot(new Vector2(0f, -1f), 0f),
                new SpawnPatternSlot(Vector2.zero, 0f),
                new SpawnPatternSlot(new Vector2(0f, 1f), 0f),
            });

        SpawnSquadSO squad = ScriptableObject.CreateInstance<SpawnSquadSO>();
        squad.Initialize(
            "squad.test.line3",
            0f,
            new List<SpawnSquadGroup>
            {
                new SpawnSquadGroup(
                    0,
                    null,
                    line3Pattern,
                    Vector2.zero,
                    0f,
                    0f)
            });

        FixedPatternSO diamondFormationPattern = ScriptableObject.CreateInstance<FixedPatternSO>();
        diamondFormationPattern.Initialize(
            "pattern.test.axisy.diamond4",
            "AxisY Diamond 4",
            new List<SpawnPatternSlot>
            {
                new SpawnPatternSlot(new Vector2(0f, 5f), 0f),
                new SpawnPatternSlot(new Vector2(5f, 0f), -90f),
                new SpawnPatternSlot(new Vector2(0f, -5f), 180f),
                new SpawnPatternSlot(new Vector2(-5f, 0f), 90f),
            });

        SpawnFormationSO formation = ScriptableObject.CreateInstance<SpawnFormationSO>();
        formation.Initialize(
            "formation.test.axisy.diamond4",
            diamondFormationPattern,
            squad,
            0f,
            1);

        SpawnPlan plan = SpawnContentResolver.Resolve(
            new SpawnRequest(formation, Vector3.zero, 0f));

        Debug.Log("==== [AxisY Circle Formation Diagnostic] ====");
        Debug.Log($"Command Count: {plan.Commands.Count}");

        string[] slotNames = { "Top", "Right", "Bottom", "Left" };
        IReadOnlyList<SpawnCommand> commands = plan.Commands;

        for (int slotIndex = 0; slotIndex < slotNames.Length; slotIndex++)
        {
            int commandStartIndex = slotIndex * 3;
            if (commandStartIndex + 2 >= commands.Count)
            {
                Debug.LogError($"[{slotNames[slotIndex]}] Missing commands.");
                continue;
            }

            SpawnPatternSlot formationSlot =
                diamondFormationPattern.GetSlots()[slotIndex];

            SpawnCommand first = commands[commandStartIndex];
            SpawnCommand center = commands[commandStartIndex + 1];
            SpawnCommand last = commands[commandStartIndex + 2];

            Vector2 lineAxis = (last.Position - first.Position).normalized;
            Vector2 expectedAxis = SpawnCoordinateUtility.Rotate(Vector2.up, formationSlot.LocalRotation);
            Vector2 actualLook = SpawnCoordinateUtility.GetLookVector(center.Rotation);
            Vector2 expectedLook = SpawnCoordinateUtility.GetLookVector(formationSlot.LocalRotation);

            bool positionOk = ApproximatelySameDirection(lineAxis, expectedAxis);
            bool rotationOk = Mathf.Abs(Mathf.DeltaAngle(center.Rotation, formationSlot.LocalRotation)) <= 0.001f;
            bool lookOk = ApproximatelySameDirection(actualLook, expectedLook);

            Debug.Log(
                $"[{slotNames[slotIndex]}] " +
                $"FormationSlot pos={formationSlot.LocalPosition}, rot={formationSlot.LocalRotation} | " +
                $"LineAxis={lineAxis}, ExpectedAxis={expectedAxis}, PositionOK={positionOk} | " +
                $"FinalRotation={center.Rotation}, RotationOK={rotationOk}, LookOK={lookOk}");
        }

        bool allRotationsOk = commands
            .Select((cmd, index) => new { cmd, slotIndex = index / 3 })
            .All(x => Mathf.Abs(Mathf.DeltaAngle(
                x.cmd.Rotation,
                diamondFormationPattern.GetSlots()[x.slotIndex].LocalRotation)) <= 0.001f);

        Debug.Log($"[AxisY Circle Formation Diagnostic] All rotations match formation slots: {allRotationsOk}");
    }

    private static bool ApproximatelySameDirection(Vector2 lhs, Vector2 rhs)
    {
        if (lhs.sqrMagnitude <= 0.0001f || rhs.sqrMagnitude <= 0.0001f)
        {
            return false;
        }

        return Vector2.Dot(lhs.normalized, rhs.normalized) >= 0.999f;
    }
}
#endif
