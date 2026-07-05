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

            if (squad.SpawnDelay > 0f)
            {
                isLegacy = true;
                details.Add($"레거시 spawnDelay 감지: {squad.SpawnDelay}초");
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
                report.AppendLine("    -> 신규 에디터에서 소환 그룹(Groups) 리스트를 수동 구성해 주세요.");
            }
        }

        report.AppendLine("\n=======================================================");
        report.AppendLine($"스캔 요약:");
        report.AppendLine($"- SpawnSquadSO: 총 {squadCount}개 에셋 중 {squadLegacyDetected}개 마이그레이션 필요");
        report.AppendLine("- Formation 배치는 SpawnSquadSO.formationPattern으로 통합됨");
        report.AppendLine("=======================================================");

        Debug.Log(report.ToString());
        EditorUtility.DisplayDialog("스캔 완료", $"스캔이 완료되었습니다!\n마이그레이션 대상 Squad: {squadLegacyDetected}개\n\n자세한 분석 내용은 Unity 콘솔 창(Console)을 확인해주세요.", "확인");
    }
}
#endif
