using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Stage
{
    /// <summary>
    /// Converts stage story json text fields into stage_string.csv rows.
    ///
    /// CSV format:
    /// main_key,sub_key,ko,en
    ///
    /// Key rule:
    /// - Stage title:        main_key={stageNodeId}, sub_key=title
    /// - Stage summary/body: main_key={stageNodeId}, sub_key=body
    /// - Node body:          main_key={nodeId}, sub_key=body
    /// - Choice label:       main_key={choiceId}, sub_key=label
    /// - Choice description: main_key={choiceId}, sub_key=description
    /// - Choice result:      main_key={choiceId}, sub_key=result
    ///
    /// Stored CSV rule:
    /// - main_key and sub_key are stored separately.
    /// - ko is overwritten from json.
    /// - en and any other existing columns are preserved when the same main_key/sub_key row already exists.
    /// </summary>
    public static class StageStringBuilder
    {
        private const string DefaultCsvPath = "Assets/Resources/string/stage_string.csv";

        public sealed class BuildResult
        {
            public string csvPath;
            public int addedCount;
            public int updatedCount;
            public int skippedCount;
            public readonly List<string> warnings = new();
        }

        private readonly struct StringEntry
        {
            public readonly string mainKey;
            public readonly string subKey;
            public readonly string ko;

            public StringEntry(string mainKey, string subKey, string ko)
            {
                this.mainKey = mainKey;
                this.subKey = subKey;
                this.ko = ko;
            }
        }

        [Serializable]
        private sealed class StageStringJsonRoot
        {
            public string stageNodeId;
            public string roundNodeId;
            public string nodeId;
            public string actId;
            public string episodeId;
            public string titleKo;
            public string summaryKo;
            public string bodyKo;
            public string textKo;
            public List<StageStringNodeJson> nodes;
        }

        [Serializable]
        private sealed class StageStringNodeJson
        {
            public string nodeId;
            public string textKo;
            public string bodyKo;
            public List<StageStringChoiceJson> choices;
        }

        [Serializable]
        private sealed class StageStringChoiceJson
        {
            public string choiceId;
            public string textKo;
            public string labelKo;
            public string descriptionKo;
            public string resultKo;
        }

        private sealed class CsvRow
        {
            public readonly Dictionary<string, string> columns = new(StringComparer.OrdinalIgnoreCase);
        }

        public static BuildResult BuildFromJsonPath(string jsonPath)
        {
            return BuildFromJsonPath(jsonPath, DefaultCsvPath);
        }

        public static BuildResult BuildFromJsonPath(string jsonPath, string csvPath)
        {
            if (string.IsNullOrWhiteSpace(jsonPath))
            {
                throw new ArgumentException("jsonPath is null or empty.", nameof(jsonPath));
            }

            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException($"Stage json not found: {jsonPath}", jsonPath);
            }

            if (string.IsNullOrWhiteSpace(csvPath))
            {
                csvPath = DefaultCsvPath;
            }

            var jsonText = File.ReadAllText(jsonPath);
            var root = JsonUtility.FromJson<StageStringJsonRoot>(jsonText);
            if (root == null)
            {
                throw new InvalidDataException($"Failed to parse stage json: {jsonPath}");
            }

            var entries = ExtractEntries(root);
            return UpsertEntries(csvPath, entries);
        }

        public static BuildResult BuildFromJsonFolder(string jsonFolderPath, bool includeSubFolders = true)
        {
            return BuildFromJsonFolder(jsonFolderPath, DefaultCsvPath, includeSubFolders);
        }

        public static BuildResult BuildFromJsonFolder(string jsonFolderPath, string csvPath, bool includeSubFolders = true)
        {
            var result = new BuildResult { csvPath = csvPath };

            if (string.IsNullOrWhiteSpace(jsonFolderPath))
            {
                throw new ArgumentException("jsonFolderPath is null or empty.", nameof(jsonFolderPath));
            }

            if (!Directory.Exists(jsonFolderPath))
            {
                throw new DirectoryNotFoundException($"Stage json folder not found: {jsonFolderPath}");
            }

            var option = includeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var jsonFiles = Directory.GetFiles(jsonFolderPath, "*.json", option)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (jsonFiles.Count == 0)
            {
                result.warnings.Add($"No json files found: {jsonFolderPath}");
                return result;
            }

            var entries = new Dictionary<string, StringEntry>(StringComparer.OrdinalIgnoreCase);

            foreach (var jsonPath in jsonFiles)
            {
                try
                {
                    var jsonText = File.ReadAllText(jsonPath);
                    var root = JsonUtility.FromJson<StageStringJsonRoot>(jsonText);
                    if (root == null)
                    {
                        result.warnings.Add($"Failed to parse json: {jsonPath}");
                        continue;
                    }

                    foreach (var entry in ExtractEntries(root))
                    {
                        entries[MakeRowKey(entry.mainKey, entry.subKey)] = entry;
                    }
                }
                catch (Exception e)
                {
                    result.warnings.Add($"Failed to read json: {jsonPath} / {e.Message}");
                }
            }

            var upsertResult = UpsertEntries(csvPath, entries.Values.ToList());
            upsertResult.warnings.AddRange(result.warnings);
            return upsertResult;
        }

        private static BuildResult UpsertEntries(string csvPath, IReadOnlyCollection<StringEntry> koEntries)
        {
            var result = new BuildResult { csvPath = csvPath };

            if (string.IsNullOrWhiteSpace(csvPath))
            {
                csvPath = DefaultCsvPath;
                result.csvPath = csvPath;
            }

            EnsureFolder(Path.GetDirectoryName(csvPath));

            var headers = new List<string> { "main_key", "sub_key", "ko", "en" };
            var rows = new List<CsvRow>();
            var rowByKey = new Dictionary<string, CsvRow>(StringComparer.OrdinalIgnoreCase);

            if (File.Exists(csvPath))
            {
                ReadExistingCsv(csvPath, headers, rows, rowByKey, result);
            }

            EnsureHeader(headers, "main_key");
            EnsureHeader(headers, "sub_key");
            EnsureHeader(headers, "ko");
            EnsureHeader(headers, "en");

            foreach (var entry in koEntries
                         .OrderBy(e => e.mainKey, StringComparer.OrdinalIgnoreCase)
                         .ThenBy(e => e.subKey, StringComparer.OrdinalIgnoreCase))
            {
                var mainKey = entry.mainKey?.Trim();
                var subKey = entry.subKey?.Trim();
                var ko = entry.ko ?? string.Empty;

                if (string.IsNullOrWhiteSpace(mainKey) || string.IsNullOrWhiteSpace(subKey))
                {
                    result.skippedCount++;
                    continue;
                }

                var rowKey = MakeRowKey(mainKey, subKey);
                if (rowByKey.TryGetValue(rowKey, out var row))
                {
                    row.columns["ko"] = ko;
                    result.updatedCount++;
                }
                else
                {
                    row = new CsvRow();
                    foreach (var header in headers)
                    {
                        row.columns[header] = string.Empty;
                    }

                    row.columns["main_key"] = mainKey;
                    row.columns["sub_key"] = subKey;
                    row.columns["ko"] = ko;
                    row.columns["en"] = string.Empty;
                    rows.Add(row);
                    rowByKey[rowKey] = row;
                    result.addedCount++;
                }
            }

            WriteCsv(csvPath, headers, rows);
            AssetDatabase.ImportAsset(csvPath);
            AssetDatabase.Refresh();

            return result;
        }

        private static List<StringEntry> ExtractEntries(StageStringJsonRoot root)
        {
            var entries = new Dictionary<string, StringEntry>(StringComparer.OrdinalIgnoreCase);
            var rootId = ResolveRootId(root);

            if (!string.IsNullOrWhiteSpace(rootId))
            {
                AddEntry(entries, rootId, "title", root.titleKo);
                AddEntry(entries, rootId, "body", FirstNonEmpty(root.summaryKo, root.bodyKo, root.textKo));
            }

            if (root.nodes == null)
            {
                return entries.Values.ToList();
            }

            foreach (var node in root.nodes)
            {
                if (node == null || string.IsNullOrWhiteSpace(node.nodeId))
                {
                    continue;
                }

                AddEntry(entries, node.nodeId, "body", FirstNonEmpty(node.bodyKo, node.textKo));

                if (node.choices == null)
                {
                    continue;
                }

                foreach (var choice in node.choices)
                {
                    if (choice == null || string.IsNullOrWhiteSpace(choice.choiceId))
                    {
                        continue;
                    }

                    AddEntry(entries, choice.choiceId, "label", FirstNonEmpty(choice.labelKo, choice.textKo));
                    AddEntry(entries, choice.choiceId, "description", choice.descriptionKo);
                    AddEntry(entries, choice.choiceId, "result", choice.resultKo);
                }
            }

            return entries.Values.ToList();
        }

        private static string ResolveRootId(StageStringJsonRoot root)
        {
            if (!string.IsNullOrWhiteSpace(root.stageNodeId))
            {
                return root.stageNodeId;
            }

            if (!string.IsNullOrWhiteSpace(root.roundNodeId))
            {
                return root.roundNodeId;
            }

            if (!string.IsNullOrWhiteSpace(root.nodeId))
            {
                return root.nodeId;
            }

            if (!string.IsNullOrWhiteSpace(root.actId))
            {
                return root.actId;
            }

            if (!string.IsNullOrWhiteSpace(root.episodeId))
            {
                return root.episodeId;
            }

            return null;
        }

        private static void AddEntry(Dictionary<string, StringEntry> entries, string mainKey, string subKey, string ko)
        {
            if (string.IsNullOrWhiteSpace(mainKey) || string.IsNullOrWhiteSpace(subKey) || string.IsNullOrWhiteSpace(ko))
            {
                return;
            }

            var entry = new StringEntry(mainKey.Trim(), subKey.Trim(), ko.Trim());
            entries[MakeRowKey(entry.mainKey, entry.subKey)] = entry;
        }

        private static string FirstNonEmpty(params string[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }

        private static bool TryNormalizeLegacyKey(
            string mainKey,
            string subKey,
            out string normalizedMainKey,
            out string normalizedSubKey)
        {
            normalizedMainKey = mainKey?.Trim();
            normalizedSubKey = subKey?.Trim();

            if (!string.IsNullOrWhiteSpace(normalizedSubKey)
                || string.IsNullOrWhiteSpace(normalizedMainKey))
            {
                return false;
            }

            string[] suffixes =
            {
                ".title",
                ".body",
                ".label",
                ".description",
                ".result",
                ".summary"
            };

            foreach (string suffix in suffixes)
            {
                if (!normalizedMainKey.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                normalizedMainKey = normalizedMainKey.Substring(
                    0,
                    normalizedMainKey.Length - suffix.Length);
                normalizedSubKey = suffix.Substring(1);

                if (string.Equals(normalizedSubKey, "summary", StringComparison.OrdinalIgnoreCase))
                {
                    normalizedSubKey = "body";
                }

                return true;
            }

            return false;
        }

        private static string MakeRowKey(string mainKey, string subKey)
        {
            return $"{mainKey?.Trim()}::{subKey?.Trim()}";
        }

        private static void ReadExistingCsv(
            string csvPath,
            List<string> headers,
            List<CsvRow> rows,
            Dictionary<string, CsvRow> rowByKey,
            BuildResult result)
        {
            var lines = ParseCsvRecords(File.ReadAllText(csvPath, Encoding.UTF8));
            if (lines.Count == 0)
            {
                return;
            }

            var parsedHeader = ParseCsvLine(lines[0]);
            if (parsedHeader.Count > 0)
            {
                headers.Clear();
                headers.AddRange(parsedHeader);
            }

            for (var i = 1; i < lines.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }

                var values = ParseCsvLine(lines[i]);
                var row = new CsvRow();

                for (var h = 0; h < headers.Count; h++)
                {
                    row.columns[headers[h]] = h < values.Count ? values[h] : string.Empty;
                }

                var mainKey = row.columns.TryGetValue("main_key", out var key) ? key : string.Empty;
                var subKey = row.columns.TryGetValue("sub_key", out var sub) ? sub : string.Empty;

                if (TryNormalizeLegacyKey(mainKey, subKey, out var normalizedMainKey, out var normalizedSubKey))
                {
                    row.columns["main_key"] = normalizedMainKey;
                    row.columns["sub_key"] = normalizedSubKey;
                    mainKey = normalizedMainKey;
                    subKey = normalizedSubKey;
                }

                if (string.IsNullOrWhiteSpace(mainKey) || string.IsNullOrWhiteSpace(subKey))
                {
                    result.warnings.Add($"CSV row has empty main_key/sub_key. line={i + 1}");
                    continue;
                }

                var rowKey = MakeRowKey(mainKey, subKey);
                if (rowByKey.ContainsKey(rowKey))
                {
                    result.warnings.Add($"Duplicated CSV key. Later row ignored. main_key={mainKey}, sub_key={subKey}, line={i + 1}");
                    continue;
                }

                rows.Add(row);
                rowByKey[rowKey] = row;
            }
        }

        private static void WriteCsv(string csvPath, List<string> headers, List<CsvRow> rows)
        {
            var builder = new StringBuilder();
            builder.AppendLine(string.Join(",", headers.Select(EscapeCsv)));

            foreach (var row in rows
                         .OrderBy(r => r.columns.TryGetValue("main_key", out var key) ? key : string.Empty, StringComparer.OrdinalIgnoreCase)
                         .ThenBy(r => r.columns.TryGetValue("sub_key", out var subKey) ? subKey : string.Empty, StringComparer.OrdinalIgnoreCase))
            {
                var values = headers.Select(header =>
                    row.columns.TryGetValue(header, out var value) ? value : string.Empty);
                builder.AppendLine(string.Join(",", values.Select(EscapeCsv)));
            }

            File.WriteAllText(csvPath, builder.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        private static List<string> ParseCsvRecords(string csvText)
        {
            var records = new List<string>();
            if (string.IsNullOrEmpty(csvText))
            {
                return records;
            }

            var current = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < csvText.Length; i++)
            {
                var c = csvText[i];

                if (c == '"')
                {
                    current.Append(c);

                    if (inQuotes && i + 1 < csvText.Length && csvText[i + 1] == '"')
                    {
                        current.Append(csvText[i + 1]);
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }

                    continue;
                }

                if ((c == '\n' || c == '\r') && !inQuotes)
                {
                    if (c == '\r' && i + 1 < csvText.Length && csvText[i + 1] == '\n')
                    {
                        i++;
                    }

                    if (current.Length > 0)
                    {
                        records.Add(current.ToString());
                        current.Clear();
                    }

                    continue;
                }

                current.Append(c);
            }

            if (current.Length > 0)
            {
                records.Add(current.ToString());
            }

            return records;
        }

        private static List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            if (line == null)
            {
                return result;
            }

            var current = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString());
            return result;
        }

        private static string EscapeCsv(string value)
        {
            value ??= string.Empty;

            // CSV를 사람이 직접 열거나 StringManager가 단순 라인 단위로 읽어도 깨지지 않도록
            // 실제 개행은 파일에는 \n 문자열로 저장한다.
            value = value
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Replace("\n", "\\n");

            var mustQuote = value.Contains(',') || value.Contains('"');
            if (!mustQuote)
            {
                return value;
            }

            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        private static void EnsureHeader(List<string> headers, string header)
        {
            if (!headers.Any(h => string.Equals(h, header, StringComparison.OrdinalIgnoreCase)))
            {
                headers.Add(header);
            }
        }

        private static void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                return;
            }

            folderPath = folderPath.Replace('\\', '/').TrimEnd('/');
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parts = folderPath.Split('/');
            if (parts.Length == 0 || parts[0] != "Assets")
            {
                throw new ArgumentException($"Unity asset folder must start with Assets: {folderPath}");
            }

            var current = "Assets";
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
