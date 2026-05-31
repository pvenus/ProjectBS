using System.Collections.Generic;
using UnityEngine;

namespace String
{
    public enum LanguageType
    {
        Korean,
        English,
        Japanese
    }
    /// <summary>
    /// Resources/string/string_table.csv 파일을 읽어 문자열을 제공하는 매니저.
    ///
    /// CSV 형식:
    /// main_key,sub_key,ko,en,ja
    /// fireball,name,화염구,Fireball,ファイアボール
    /// fireball,desc,지정 위치에 화염 피해를 줍니다.,Deals fire damage at target location.,指定位置に火炎ダメージ
    /// </summary>
    public class StringManager : MonoBehaviour
    {
        private const string DefaultResourcePath = "string/string_table";
        private const string DefaultLanguage = "ko";

        public static StringManager Instance { get; private set; }

        [SerializeField] private string resourcePath = DefaultResourcePath;
        [SerializeField] private LanguageType currentLanguage = LanguageType.Korean;
        [SerializeField] private bool loadOnAwake = true;
        [SerializeField] private bool logMissingKey;

        private readonly Dictionary<string, Dictionary<string, string>> table =
            new Dictionary<string, Dictionary<string, string>>();

        private readonly List<string> languages =
            new List<string>();

        public LanguageType CurrentLanguage => currentLanguage;
        public IReadOnlyList<string> Languages => languages;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (loadOnAwake)
            {
                Load();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SetLanguage(LanguageType language)
        {
            currentLanguage = language;
        }

        public void Load()
        {
            TextAsset textAsset =
                Resources.Load<TextAsset>(resourcePath);

            if (textAsset == null)
            {
                Debug.LogWarning(
                    $"[StringManager] CSV not found. path=Resources/{resourcePath}.csv");
                return;
            }

            LoadFromCsv(textAsset.text);
        }

        public void LoadFromCsv(string csvText)
        {
            table.Clear();
            languages.Clear();

            if (string.IsNullOrWhiteSpace(csvText))
            {
                return;
            }

            string[] lines =
                csvText.Replace("\r\n", "\n")
                    .Replace('\r', '\n')
                    .Split('\n');

            if (lines.Length <= 0)
            {
                return;
            }

            List<string> headers =
                ParseCsvLine(lines[0]);

            if (headers.Count < 3)
            {
                Debug.LogWarning(
                    "[StringManager] Invalid CSV header. Expected: main_key,sub_key,ko,en,ja");
                return;
            }

            for (int i = 2; i < headers.Count; i++)
            {
                string language = headers[i].Trim();

                if (!string.IsNullOrEmpty(language))
                {
                    languages.Add(language);
                }
            }

            for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex];

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                List<string> columns =
                    ParseCsvLine(line);

                if (columns.Count < 2)
                {
                    continue;
                }

                string mainKey = columns[0].Trim();
                string subKey = columns[1].Trim();

                if (string.IsNullOrEmpty(mainKey)
                    || string.IsNullOrEmpty(subKey))
                {
                    continue;
                }

                string fullKey = BuildKey(mainKey, subKey);

                if (!table.TryGetValue(fullKey, out Dictionary<string, string> localizedValues))
                {
                    localizedValues = new Dictionary<string, string>();
                    table[fullKey] = localizedValues;
                }

                for (int headerIndex = 2; headerIndex < headers.Count; headerIndex++)
                {
                    string language = headers[headerIndex].Trim();

                    if (string.IsNullOrEmpty(language))
                    {
                        continue;
                    }

                    string value = headerIndex < columns.Count
                        ? columns[headerIndex]
                        : string.Empty;

                    localizedValues[language] = value;
                }
            }
        }

        public string Get(string mainKey, string subKey)
        {
            return Get(mainKey, subKey, GetLanguageCode(currentLanguage));
        }

        public string Get(string mainKey, string subKey, string language)
        {
            if (string.IsNullOrWhiteSpace(mainKey)
                || string.IsNullOrWhiteSpace(subKey))
            {
                return string.Empty;
            }

            string fullKey = BuildKey(mainKey, subKey);

            if (!table.TryGetValue(fullKey, out Dictionary<string, string> localizedValues))
            {
                return Missing(fullKey);
            }

            string resolvedLanguage = string.IsNullOrWhiteSpace(language)
                ? GetLanguageCode(currentLanguage)
                : language.Trim();

            if (localizedValues.TryGetValue(resolvedLanguage, out string value)
                && !string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (resolvedLanguage != DefaultLanguage
                && localizedValues.TryGetValue(DefaultLanguage, out string fallbackValue)
                && !string.IsNullOrEmpty(fallbackValue))
            {
                return fallbackValue;
            }

            return Missing(fullKey);
        }

        public string GetByFullKey(string fullKey)
        {
            if (string.IsNullOrWhiteSpace(fullKey))
            {
                return string.Empty;
            }

            string[] parts = fullKey.Split('.');

            if (parts.Length < 2)
            {
                return Missing(fullKey);
            }

            string subKey = parts[parts.Length - 1];
            string mainKey = fullKey.Substring(
                0,
                fullKey.Length - subKey.Length - 1);

            return Get(mainKey, subKey);
        }

        public bool Contains(string mainKey, string subKey)
        {
            return table.ContainsKey(BuildKey(mainKey, subKey));
        }

        private string Missing(string key)
        {
            if (logMissingKey)
            {
                Debug.LogWarning(
                    $"[StringManager] Missing string key. key={key}, language={GetLanguageCode(currentLanguage)}");
            }

            return key;
        }

        private string GetLanguageCode(LanguageType language)
        {
            switch (language)
            {
                case LanguageType.English:
                    return "en";

                case LanguageType.Japanese:
                    return "ja";

                default:
                    return "ko";
            }
        }

        private string BuildKey(string mainKey, string subKey)
        {
            return $"{mainKey.Trim()}.{subKey.Trim()}";
        }

        private List<string> ParseCsvLine(string line)
        {
            List<string> result = new List<string>();

            if (line == null)
            {
                return result;
            }

            System.Text.StringBuilder builder =
                new System.Text.StringBuilder();

            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    bool escapedQuote =
                        inQuotes
                        && i + 1 < line.Length
                        && line[i + 1] == '"';

                    if (escapedQuote)
                    {
                        builder.Append('"');
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
                    result.Add(builder.ToString());
                    builder.Clear();
                    continue;
                }

                builder.Append(c);
            }

            result.Add(builder.ToString());

            return result;
        }
    }
}