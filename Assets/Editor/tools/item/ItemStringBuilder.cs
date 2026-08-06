

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ResourceTools
{
    public static class ItemStringBuilder
    {
        private const string DefaultCsvPath = "Assets/Resources/string/item_string.csv";

        public static BuildResult BuildFromJsonPath(
            string jsonPath,
            string csvPath = DefaultCsvPath)
        {
            BuildResult result = new();
            jsonPath = NormalizeAssetPath(jsonPath);
            csvPath = NormalizeAssetPath(csvPath);

            string fullJsonPath = ToFullPath(jsonPath);
            if (!File.Exists(fullJsonPath))
            {
                result.errors.Add($"Json file not found. path={jsonPath}");
                return result;
            }

            string json = File.ReadAllText(fullJsonPath);
            StrategicSkillItemStringJson data = JsonUtility.FromJson<StrategicSkillItemStringJson>(json);
            if (data == null || string.IsNullOrWhiteSpace(data.strategicSkillItemId))
            {
                result.errors.Add($"Invalid item string json. path={jsonPath}");
                return result;
            }

            List<StringEntry> entries = ExtractEntries(data);
            if (entries.Count == 0)
            {
                result.warnings.Add($"No item strings found. path={jsonPath}");
                return result;
            }

            EnsureFolder(Path.GetDirectoryName(csvPath)?.Replace("\\", "/"));
            UpsertCsv(csvPath, entries, result);

            AssetDatabase.ImportAsset(csvPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();

            Debug.Log(
                $"[ItemStringBuilder] Updated item strings. Added={result.addedCount}, Updated={result.updatedCount}, Csv={csvPath}");

            return result;
        }

        public static BuildResult BuildFromJsonPaths(
            IEnumerable<string> jsonPaths,
            string csvPath = DefaultCsvPath)
        {
            BuildResult merged = new();
            if (jsonPaths == null)
            {
                return merged;
            }

            foreach (string jsonPath in jsonPaths)
            {
                BuildResult result = BuildFromJsonPath(jsonPath, csvPath);
                merged.addedCount += result.addedCount;
                merged.updatedCount += result.updatedCount;
                merged.errors.AddRange(result.errors);
                merged.warnings.AddRange(result.warnings);
            }

            return merged;
        }

        private static List<StringEntry> ExtractEntries(StrategicSkillItemStringJson data)
        {
            List<StringEntry> entries = new();
            AddEntry(entries, data.strategicSkillItemId, "name", data.nameKo);
            AddEntry(entries, data.strategicSkillItemId, "description", data.descriptionKo);
            return entries;
        }

        private static void AddEntry(
            List<StringEntry> entries,
            string mainKey,
            string subKey,
            string ko)
        {
            if (string.IsNullOrWhiteSpace(mainKey)
                || string.IsNullOrWhiteSpace(subKey)
                || string.IsNullOrWhiteSpace(ko))
            {
                return;
            }

            entries.Add(new StringEntry
            {
                mainKey = mainKey,
                subKey = subKey,
                ko = ko
            });
        }

        private static void UpsertCsv(
            string csvPath,
            List<StringEntry> entries,
            BuildResult result)
        {
            string fullCsvPath = ToFullPath(csvPath);
            List<CsvRow> rows = ReadCsvRows(fullCsvPath);

            if (rows.Count == 0)
            {
                rows.Add(new CsvRow
                {
                    mainKey = "main_key",
                    subKey = "sub_key",
                    ko = "ko",
                    en = "en"
                });
            }

            foreach (StringEntry entry in entries)
            {
                CsvRow existing = rows.FirstOrDefault(row =>
                    row.mainKey == entry.mainKey
                    && row.subKey == entry.subKey);

                if (existing == null)
                {
                    rows.Add(new CsvRow
                    {
                        mainKey = entry.mainKey,
                        subKey = entry.subKey,
                        ko = entry.ko,
                        en = string.Empty
                    });
                    result.addedCount++;
                }
                else
                {
                    if (existing.ko != entry.ko)
                    {
                        existing.ko = entry.ko;
                        result.updatedCount++;
                    }
                }
            }

            File.WriteAllLines(fullCsvPath, rows.Select(ToCsvLine));
        }

        private static List<CsvRow> ReadCsvRows(string fullCsvPath)
        {
            List<CsvRow> rows = new();
            if (!File.Exists(fullCsvPath))
            {
                return rows;
            }

            string[] lines = File.ReadAllLines(fullCsvPath);
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                List<string> cells = ParseCsvLine(line);
                rows.Add(new CsvRow
                {
                    mainKey = cells.Count > 0 ? cells[0] : string.Empty,
                    subKey = cells.Count > 1 ? cells[1] : string.Empty,
                    ko = cells.Count > 2 ? cells[2] : string.Empty,
                    en = cells.Count > 3 ? cells[3] : string.Empty
                });
            }

            return rows;
        }

        private static string ToCsvLine(CsvRow row)
        {
            return string.Join(",", new[]
            {
                EscapeCsv(row.mainKey),
                EscapeCsv(row.subKey),
                EscapeCsv(row.ko),
                EscapeCsv(row.en)
            });
        }

        private static List<string> ParseCsvLine(string line)
        {
            List<string> cells = new();
            if (line == null)
            {
                return cells;
            }

            bool inQuote = false;
            string current = string.Empty;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuote && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current += '"';
                        i++;
                    }
                    else
                    {
                        inQuote = !inQuote;
                    }
                }
                else if (c == ',' && !inQuote)
                {
                    cells.Add(current);
                    current = string.Empty;
                }
                else
                {
                    current += c;
                }
            }

            cells.Add(current);
            return cells;
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            bool shouldQuote = value.Contains(',')
                               || value.Contains('"')
                               || value.Contains('\n')
                               || value.Contains('\r');

            string escaped = value.Replace("\"", "\"\"");
            return shouldQuote ? $"\"{escaped}\"" : escaped;
        }

        private static void EnsureFolder(string assetFolder)
        {
            assetFolder = NormalizeAssetPath(assetFolder);
            if (string.IsNullOrWhiteSpace(assetFolder)
                || AssetDatabase.IsValidFolder(assetFolder))
            {
                return;
            }

            string[] parts = assetFolder.Split('/');
            string current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static string ToFullPath(string assetPath)
        {
            assetPath = NormalizeAssetPath(assetPath);
            if (Path.IsPathRooted(assetPath))
            {
                return assetPath;
            }

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            return Path.Combine(projectRoot ?? string.Empty, assetPath);
        }

        private static string NormalizeAssetPath(string path)
        {
            return string.IsNullOrWhiteSpace(path)
                ? string.Empty
                : path.Replace("\\", "/").Trim();
        }

        [Serializable]
        private class StrategicSkillItemStringJson
        {
            public string strategicSkillItemId;
            public string nameKo;
            public string descriptionKo;
        }

        private class StringEntry
        {
            public string mainKey;
            public string subKey;
            public string ko;
        }

        private class CsvRow
        {
            public string mainKey;
            public string subKey;
            public string ko;
            public string en;
        }

        public class BuildResult
        {
            public int addedCount;
            public int updatedCount;
            public readonly List<string> errors = new();
            public readonly List<string> warnings = new();
        }
    }
}