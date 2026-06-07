#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Character;
using Stat;
using UnityEditor;
using UnityEngine;

namespace ResourceTools
{
    /// <summary>
    /// CSV에 입력된 CharacterSO baseStats 값을 일괄 갱신하는 에디터 도구.
    ///
    /// CSV 연결 기준:
    /// - 필수 컬럼: characterId
    /// - characterId 값은 CharacterSO.characterId 값과 매칭한다.
    /// - 보조 매칭으로 CharacterSO asset 이름도 확인한다.
    ///
    /// CSV 예시:
    /// characterId,Attack,MaxHp,AttackSpeed,CritChance,CritDamage,MoveSpeed
    /// character_egg_ghost_1,10,100,1,90,50,1
    /// character.eodukshini_1,12,120,1,80,60,1
    /// </summary>
    public static class CharacterStatGenerator
    {
        private const string CharacterIdColumnName = "characterId";

        [MenuItem("Assets/Character/Import Character Stats From CSV", true)]
        private static bool ValidateImport()
        {
            UnityEngine.Object selectedObject = Selection.activeObject;

            if (selectedObject == null)
            {
                return false;
            }

            string path = AssetDatabase.GetAssetPath(selectedObject);
            return !string.IsNullOrEmpty(path)
                && path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);
        }

        [MenuItem("Assets/Character/Import Character Stats From CSV", false, 2100)]
        private static void Import()
        {
            UnityEngine.Object selectedObject = Selection.activeObject;

            if (selectedObject == null)
            {
                Debug.LogError("[CharacterStatGenerator] Select a CSV file first.");
                return;
            }

            string csvPath = AssetDatabase.GetAssetPath(selectedObject);
            ImportFromCsvPath(csvPath);
        }

        public static void ImportFromCsvPath(string csvPath)
        {
            if (string.IsNullOrEmpty(csvPath) || !csvPath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError($"[CharacterStatGenerator] Invalid csv path: {csvPath}");
                return;
            }

            string fullPath = ToFullPath(csvPath);

            if (!File.Exists(fullPath))
            {
                Debug.LogError($"[CharacterStatGenerator] CSV file not found: {csvPath}");
                return;
            }

            List<string[]> rows = ReadCsvRows(fullPath);

            if (rows.Count <= 1)
            {
                Debug.LogWarning($"[CharacterStatGenerator] CSV has no data rows: {csvPath}");
                return;
            }

            string[] headers = rows[0];
            int characterIdIndex = FindHeaderIndex(headers, CharacterIdColumnName);

            if (characterIdIndex < 0)
            {
                Debug.LogError($"[CharacterStatGenerator] Missing required column: {CharacterIdColumnName}");
                return;
            }

            Dictionary<string, CharacterSO> characterMap = BuildCharacterMap();

            int updatedCount = 0;
            int skippedCount = 0;

            for (int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
            {
                string[] row = rows[rowIndex];

                if (row == null || row.Length == 0)
                {
                    skippedCount++;
                    continue;
                }

                string characterId = GetCell(row, characterIdIndex);

                if (string.IsNullOrEmpty(characterId))
                {
                    skippedCount++;
                    continue;
                }

                if (!characterMap.TryGetValue(NormalizeKey(characterId), out CharacterSO characterSo) || characterSo == null)
                {
                    Debug.LogWarning($"[CharacterStatGenerator] CharacterSO not found. characterId={characterId}");
                    skippedCount++;
                    continue;
                }

                List<StatEntry> stats = BuildStatsFromRow(headers, row, characterIdIndex);
                SetField(characterSo, "baseStats", stats);
                EditorUtility.SetDirty(characterSo);
                updatedCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[CharacterStatGenerator] Complete. Updated {updatedCount} CharacterSO assets. Skipped {skippedCount} rows.");
        }

        private static Dictionary<string, CharacterSO> BuildCharacterMap()
        {
            Dictionary<string, CharacterSO> result = new();
            string[] guids = AssetDatabase.FindAssets("t:CharacterSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CharacterSO characterSo = AssetDatabase.LoadAssetAtPath<CharacterSO>(path);

                if (characterSo == null)
                {
                    continue;
                }

                string assetName = Path.GetFileNameWithoutExtension(path);
                AddCharacterMapEntry(result, assetName, characterSo);

                string characterId = GetFieldValue<string>(characterSo, "characterId");
                AddCharacterMapEntry(result, characterId, characterSo);
            }

            return result;
        }

        private static void AddCharacterMapEntry(
            Dictionary<string, CharacterSO> map,
            string key,
            CharacterSO characterSo)
        {
            if (string.IsNullOrEmpty(key) || characterSo == null)
            {
                return;
            }

            string normalizedKey = NormalizeKey(key);

            if (!map.ContainsKey(normalizedKey))
            {
                map.Add(normalizedKey, characterSo);
            }
        }

        private static List<StatEntry> BuildStatsFromRow(
            string[] headers,
            string[] row,
            int characterIdIndex)
        {
            List<StatEntry> stats = new();

            for (int i = 0; i < headers.Length; i++)
            {
                if (i == characterIdIndex)
                {
                    continue;
                }

                string header = headers[i]?.Trim();

                if (string.IsNullOrEmpty(header))
                {
                    continue;
                }

                string rawValue = GetCell(row, i);

                if (string.IsNullOrEmpty(rawValue))
                {
                    continue;
                }

                if (!TryParseStatType(header, out StatType statType))
                {
                    Debug.LogWarning($"[CharacterStatGenerator] Unknown StatType column: {header}");
                    continue;
                }

                if (!float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                {
                    Debug.LogWarning($"[CharacterStatGenerator] Invalid stat value. stat={header}, value={rawValue}");
                    continue;
                }

                stats.Add(new StatEntry
                {
                    statType = statType,
                    value = value
                });
            }

            return stats;
        }

        private static bool TryParseStatType(string rawName, out StatType statType)
        {
            statType = default;

            if (string.IsNullOrEmpty(rawName))
            {
                return false;
            }

            if (Enum.TryParse(rawName, true, out statType))
            {
                return true;
            }

            string normalizedInput = NormalizeKey(rawName);

            foreach (StatType candidate in Enum.GetValues(typeof(StatType)))
            {
                if (NormalizeKey(candidate.ToString()) == normalizedInput)
                {
                    statType = candidate;
                    return true;
                }
            }

            return false;
        }

        private static List<string[]> ReadCsvRows(string fullPath)
        {
            List<string[]> rows = new();
            string[] lines = File.ReadAllLines(fullPath);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                rows.Add(ParseCsvLine(line).ToArray());
            }

            return rows;
        }

        private static List<string> ParseCsvLine(string line)
        {
            List<string> cells = new();
            bool inQuotes = false;
            string current = string.Empty;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current += '"';
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    cells.Add(current.Trim());
                    current = string.Empty;
                }
                else
                {
                    current += c;
                }
            }

            cells.Add(current.Trim());
            return cells;
        }

        private static int FindHeaderIndex(string[] headers, string headerName)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                if (string.Equals(headers[i]?.Trim(), headerName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        private static string GetCell(string[] row, int index)
        {
            if (row == null || index < 0 || index >= row.Length)
            {
                return string.Empty;
            }

            return row[index]?.Trim() ?? string.Empty;
        }

        private static string NormalizeKey(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value
                    .Trim()
                    .Replace(".", string.Empty)
                    .Replace("_", string.Empty)
                    .Replace("-", string.Empty)
                    .Replace(" ", string.Empty)
                    .ToLowerInvariant();
        }

        private static T GetFieldValue<T>(object target, string fieldName)
        {
            if (target == null)
            {
                return default;
            }

            var field = target.GetType().GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);

            if (field == null)
            {
                return default;
            }

            object value = field.GetValue(target);

            if (value is T typedValue)
            {
                return typedValue;
            }

            return default;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            if (target == null)
            {
                return;
            }

            var field = target.GetType().GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);

            if (field == null)
            {
                Debug.LogWarning($"[CharacterStatGenerator] Field not found: {fieldName}");
                return;
            }

            field.SetValue(target, value);
        }

        private static string ToFullPath(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            return Path.Combine(projectRoot, assetPath);
        }
    }
}
#endif
