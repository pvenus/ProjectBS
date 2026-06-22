


using System.IO;
using System.Text;
using System.Collections.Generic;

namespace ResourceTools.Skill
{
    /// <summary>
    /// Skill String CSV 생성용.
    /// 현재는 스킬 이름만 추출.
    /// </summary>
    public static class SkillStringBuilder
    {
        private const string CsvPath =
            "Assets/Resources/string/skill_string.csv";
        public class StringEntry
        {
            public string mainKey;
            public string subKey;
            public string ko;
            public string en;
        }

        public static void ExtractSkillName(
            string skillId,
            string skillNameKo)
        {
            List<StringEntry> entries = new();

            if (string.IsNullOrWhiteSpace(skillId) ||
                string.IsNullOrWhiteSpace(skillNameKo))
            {
                return;
            }

            entries.Add(
                new StringEntry
                {
                    mainKey = skillId,
                    subKey = "name",
                    ko = skillNameKo
                });

            SaveEntries(entries);
        }

        private static void SaveEntries(
            List<StringEntry> entries)
        {
            if (entries == null || entries.Count == 0)
            {
                return;
            }

            string folder = Path.GetDirectoryName(CsvPath);
            if (!string.IsNullOrWhiteSpace(folder) &&
                !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            List<StringEntry> mergedEntries = ReadExistingEntries();
            Dictionary<string, StringEntry> entryByKey = new();

            for (int i = 0; i < mergedEntries.Count; i++)
            {
                StringEntry entry = mergedEntries[i];
                if (entry == null)
                {
                    continue;
                }

                string key = MakeKey(entry.mainKey, entry.subKey);
                if (!entryByKey.ContainsKey(key))
                {
                    entryByKey.Add(key, entry);
                }
            }

            for (int i = 0; i < entries.Count; i++)
            {
                StringEntry entry = entries[i];
                if (entry == null ||
                    string.IsNullOrWhiteSpace(entry.mainKey) ||
                    string.IsNullOrWhiteSpace(entry.subKey))
                {
                    continue;
                }

                string key = MakeKey(entry.mainKey, entry.subKey);
                if (entryByKey.TryGetValue(key, out StringEntry existingEntry))
                {
                    existingEntry.ko = entry.ko;
                    continue;
                }

                StringEntry newEntry = new StringEntry
                {
                    mainKey = entry.mainKey,
                    subKey = entry.subKey,
                    ko = entry.ko,
                    en = entry.en
                };

                mergedEntries.Add(newEntry);
                entryByKey.Add(key, newEntry);
            }

            WriteEntries(mergedEntries);
        }

        private static List<StringEntry> ReadExistingEntries()
        {
            List<StringEntry> result = new();

            if (!File.Exists(CsvPath))
            {
                return result;
            }

            string[] lines = File.ReadAllLines(CsvPath, Encoding.UTF8);
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] columns = SplitCsvLine(line);
                if (columns.Length < 3)
                {
                    continue;
                }

                result.Add(
                    new StringEntry
                    {
                        mainKey = columns[0],
                        subKey = columns[1],
                        ko = columns[2],
                        en = columns.Length > 3 ? columns[3] : string.Empty
                    });
            }

            return result;
        }

        private static void WriteEntries(
            List<StringEntry> entries)
        {
            StringBuilder builder = new();
            builder.AppendLine("main_key,sub_key,ko,en");

            if (entries != null)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    StringEntry entry = entries[i];
                    if (entry == null)
                    {
                        continue;
                    }

                    builder.AppendLine(
                        $"{EscapeCsv(entry.mainKey)},{EscapeCsv(entry.subKey)},{EscapeCsv(entry.ko)},{EscapeCsv(entry.en)}");
                }
            }

            File.WriteAllText(CsvPath, builder.ToString(), Encoding.UTF8);
        }

        private static string MakeKey(
            string mainKey,
            string subKey)
        {
            return $"{mainKey}::{subKey}";
        }

        private static string[] SplitCsvLine(string line)
        {
            List<string> columns = new();
            StringBuilder current = new();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

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

                    continue;
                }

                if (c == ',' && !inQuotes)
                {
                    columns.Add(current.ToString());
                    current.Clear();
                    continue;
                }

                current.Append(c);
            }

            columns.Add(current.ToString());
            return columns.ToArray();
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            bool shouldQuote =
                value.Contains(",") ||
                value.Contains("\"") ||
                value.Contains("\n") ||
                value.Contains("\r");

            if (!shouldQuote)
            {
                return value;
            }

            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
    }
}