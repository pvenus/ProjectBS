#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class SpawnPresetMigrationTool
{
    [MenuItem("BS/Spawn/Migration Scan Tool")]
    public static void ScanAndReportLegacyData()
    {
        Debug.Log("[SpawnPresetMigrationTool] 스캔을 시작합니다...");

        int squadCount = 0;
        int squadLegacyDetected = 0;
        int formationCount = 0;
        int formationLegacyDetected = 0;

        StringBuilder report = new StringBuilder();
        report.AppendLine("=== Battle Spawn System 마이그레이션 대상 에셋 분석 리포트 ===");

        // 1. SpawnSquadSO 에셋 스캔
        string[] squadGuids = AssetDatabase.FindAssets("t:SpawnSquadSO");
        foreach (var guid in squadGuids)
        {
            squadCount++;
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SpawnSquadSO squad = AssetDatabase.LoadAssetAtPath<SpawnSquadSO>(path);
            if (squad == null) continue;

            bool isLegacy = false;
            List<string> details = new List<string>();

            // 구형 필드 검출
            if (squad.SpawnDelay > 0f)
            {
                isLegacy = true;
                details.Add($"레거시 spawnDelay 감지: {squad.SpawnDelay}초");
            }
            if (squad.Pattern != null)
            {
                isLegacy = true;
                details.Add($"레거시 legacyPattern(SpawnPattern) 참조 감지: {squad.Pattern.PatternId}");
            }
            if (squad.Npc != null)
            {
                isLegacy = true;
                details.Add($"레거시 legacyNpc(CharacterSO) 참조 감지: {squad.Npc.CharacterId}");
            }

            // 신규 groups가 아예 비어있는 경우도 마이그레이션 필요함
            if (squad.Groups == null || squad.Groups.Count == 0)
            {
                isLegacy = true;
                details.Add("신규 소환 그룹(Groups) 데이터가 비어있음");
            }

            if (isLegacy)
            {
                squadLegacyDetected++;
                report.AppendLine($"\n[Squad 마이그레이션 대상] {squad.name} (경로: {path})");
                foreach (var detail in details)
                {
                    report.AppendLine($"  - {detail}");
                }

                // 권장 매핑 방식 제시
                report.AppendLine("  * 권장 마이그레이션 조치:");
                if (squad.Pattern != null && squad.Npc != null)
                {
                    report.AppendLine($"    -> Order: 0, Character: {squad.Npc.name}, Pattern: {squad.Pattern.name} (SpawnPatternSO로 재변환 필요), Offset: (0,0), Rotation: 0, SlotInterval: 0");
                    report.AppendLine($"    -> Squad 자체의 GroupInterval 및 그룹 구성 수동 재정렬 권장");
                }
                else
                {
                    report.AppendLine("    -> 신규 에디터에서 소환 그룹(Groups) 리스트를 수동 구성해 주세요.");
                }
            }
        }

        // 2. SpawnFormationSO 에셋 스캔
        string[] formationGuids = AssetDatabase.FindAssets("t:SpawnFormationSO");
        foreach (var guid in formationGuids)
        {
            formationCount++;
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SpawnFormationSO formation = AssetDatabase.LoadAssetAtPath<SpawnFormationSO>(path);
            if (formation == null) continue;

            bool isLegacy = false;
            List<string> details = new List<string>();

            if (formation.SpawnDelay > 0f)
            {
                isLegacy = true;
                details.Add($"레거시 spawnDelay 감지: {formation.SpawnDelay}초 (신규 slotInterval로 마이그레이션 권장)");
            }
            if (formation.LegacyPattern != null)
            {
                isLegacy = true;
                details.Add($"레거시 legacyPattern(SpawnPattern) 참조 감지: {formation.LegacyPattern.PatternId}");
            }
            if (formation.Pattern == null)
            {
                isLegacy = true;
                details.Add("신규 포메이션 배치 패턴(pattern) 누락");
            }

            if (isLegacy)
            {
                formationLegacyDetected++;
                report.AppendLine($"\n[Formation 마이그레이션 대상] {formation.name} (경로: {path})");
                foreach (var detail in details)
                {
                    report.AppendLine($"  - {detail}");
                }

                report.AppendLine("  * 권장 마이그레이션 조치:");
                report.AppendLine($"    -> {formation.name} 에셋 인스펙터에서 하위 스쿼드 및 신규 배치 패턴(SpawnPatternSO)을 할당하고, Slot Interval을 {Mathf.Max(formation.SpawnDelay, formation.SlotInterval)}초로 재설정하세요.");
            }
        }

        report.AppendLine("\n=======================================================");
        report.AppendLine($"스캔 요약:");
        report.AppendLine($"- SpawnSquadSO: 총 {squadCount}개 에셋 중 {squadLegacyDetected}개 마이그레이션 필요");
        report.AppendLine($"- SpawnFormationSO: 총 {formationCount}개 에셋 중 {formationLegacyDetected}개 마이그레이션 필요");
        report.AppendLine("=======================================================");

        Debug.Log(report.ToString());
        EditorUtility.DisplayDialog("스캔 완료", $"스캔이 완료되었습니다!\n마이그레이션 대상 Squad: {squadLegacyDetected}개\n마이그레이션 대상 Formation: {formationLegacyDetected}개\n\n자세한 분석 내용은 Unity 콘솔 창(Console)을 확인해주세요.", "확인");
    }
}
#endif
