using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Presentation
{
    public sealed class PresentationTextFormatter
    {
        private readonly Func<string, string> labelResolver;
        private readonly Func<string, string> tokenResolver;
        private readonly bool usePlayerDisplayCatalog;
        private readonly bool allowGeneratedFallback;

        public PresentationTextFormatter(
            Func<string, string> labelResolver = null,
            Func<string, string> tokenResolver = null,
            bool usePlayerDisplayCatalog = false,
            bool allowGeneratedFallback = true)
        {
            this.labelResolver = labelResolver;
            this.tokenResolver = tokenResolver;
            this.usePlayerDisplayCatalog = usePlayerDisplayCatalog;
            this.allowGeneratedFallback = allowGeneratedFallback;
        }

        public static PresentationTextFormatter CreatePlayerFormatter(
            Func<string, string> localizedTextResolver)
        {
            return new PresentationTextFormatter(
                localizedTextResolver,
                localizedTextResolver,
                usePlayerDisplayCatalog: true,
                allowGeneratedFallback: false);
        }

        public string FormatLabel(string key)
        {
            string resolved = labelResolver?.Invoke(key);
            if (!string.IsNullOrWhiteSpace(resolved))
            {
                return resolved;
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            if (!allowGeneratedFallback)
            {
                return string.Empty;
            }

            string[] segments = key.Split('.');
            string last = segments.Length > 0 ? segments[^1] : key;
            return SplitPascalCase(last);
        }

        public string FormatClassification(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            string localizationKey = usePlayerDisplayCatalog
                ? PresentationDisplayCatalog.GetTagLabelKey(key)
                : key;
            if (usePlayerDisplayCatalog
                && string.IsNullOrWhiteSpace(localizationKey))
            {
                return string.Empty;
            }

            string resolved = tokenResolver?.Invoke(localizationKey);
            if (!string.IsNullOrWhiteSpace(resolved))
            {
                return resolved;
            }

            if (!allowGeneratedFallback)
            {
                return string.Empty;
            }

            string[] segments = key.Split('.');
            return SplitPascalCase(segments[^1]);
        }

        public string FormatGroupLabel(string key)
        {
            return FormatCatalogLabel(
                PresentationDisplayCatalog.GetGroupLabelKey(key),
                key);
        }

        public string FormatEntryLabel(string key)
        {
            return FormatCatalogLabel(
                PresentationDisplayCatalog.GetEntryLabelKey(key),
                key);
        }

        public string FormatEntryValues(PresentationEntryData entry)
        {
            if (entry == null || entry.Values.Count == 0)
            {
                return string.Empty;
            }

            if (!usePlayerDisplayCatalog)
            {
                return FormatValues(entry.Values);
            }

            List<string> parts = new();
            foreach (PresentationValueData value in entry.Values)
            {
                string formatted = FormatEntryValue(entry.Key, value);
                if (!string.IsNullOrWhiteSpace(formatted))
                {
                    parts.Add(formatted);
                }
            }

            string combined = string.Join(" + ", parts);
            string formatKey = PresentationDisplayCatalog.GetValueFormatKey(entry.Key);
            if (string.IsNullOrWhiteSpace(formatKey)
                || string.IsNullOrWhiteSpace(combined))
            {
                return combined;
            }

            string format = labelResolver?.Invoke(formatKey);
            if (string.IsNullOrWhiteSpace(format))
            {
                return allowGeneratedFallback ? combined : string.Empty;
            }

            try
            {
                return string.Format(CultureInfo.CurrentCulture, format, combined);
            }
            catch (FormatException)
            {
                return allowGeneratedFallback ? combined : string.Empty;
            }
        }

        public string FormatValues(IReadOnlyList<PresentationValueData> values)
        {
            if (values == null || values.Count == 0)
            {
                return string.Empty;
            }

            List<string> parts = new();
            foreach (PresentationValueData value in values)
            {
                string formatted = FormatValue(value);
                if (!string.IsNullOrWhiteSpace(formatted))
                {
                    parts.Add(formatted);
                }
            }

            return string.Join(" + ", parts);
        }

        public string FormatValue(PresentationValueData value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value.Kind == PresentationValueKind.Token)
            {
                string resolved = tokenResolver?.Invoke(value.Token);
                return !string.IsNullOrWhiteSpace(resolved)
                    ? resolved
                    : SplitPascalCase(value.Token);
            }

            double displayValue = value.NumericValue;
            string suffix = string.Empty;
            switch (value.Unit)
            {
                case PresentationValueUnit.Ratio:
                    displayValue *= 100d;
                    suffix = "%";
                    break;
                case PresentationValueUnit.Percent:
                    suffix = "%";
                    break;
                case PresentationValueUnit.Seconds:
                    suffix = "s";
                    break;
                case PresentationValueUnit.Meters:
                    suffix = "m";
                    break;
                case PresentationValueUnit.Count:
                    suffix = "x";
                    break;
                case PresentationValueUnit.Degrees:
                    suffix = "°";
                    break;
                case PresentationValueUnit.MetersPerSecond:
                    suffix = "m/s";
                    break;
                case PresentationValueUnit.DegreesPerSecond:
                    suffix = "°/s";
                    break;
            }

            string number = displayValue.ToString("0.##", CultureInfo.InvariantCulture);
            return value.Unit == PresentationValueUnit.Count
                ? $"{number}{suffix}"
                : $"{number}{suffix}";
        }

        private string FormatEntryValue(
            string entryKey,
            PresentationValueData value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value.Kind != PresentationValueKind.Token)
            {
                return FormatValue(value);
            }

            if (PresentationDisplayCatalog.IsEntryTokenLocalizedText(entryKey))
            {
                return value.Token;
            }

            string tokenKey = PresentationDisplayCatalog.GetEntryTokenKey(
                entryKey,
                value.Token);
            if (string.IsNullOrWhiteSpace(tokenKey))
            {
                return allowGeneratedFallback
                    ? SplitPascalCase(value.Token)
                    : string.Empty;
            }

            string resolved = tokenResolver?.Invoke(tokenKey);
            if (!string.IsNullOrWhiteSpace(resolved))
            {
                return resolved;
            }

            return allowGeneratedFallback
                ? SplitPascalCase(value.Token)
                : string.Empty;
        }

        private string FormatCatalogLabel(
            string localizationKey,
            string rawKey)
        {
            if (!usePlayerDisplayCatalog)
            {
                return FormatLabel(rawKey);
            }

            if (string.IsNullOrWhiteSpace(localizationKey))
            {
                return string.Empty;
            }

            string resolved = labelResolver?.Invoke(localizationKey);
            if (!string.IsNullOrWhiteSpace(resolved))
            {
                return resolved;
            }

            return allowGeneratedFallback
                ? FormatLabel(rawKey)
                : string.Empty;
        }

        public string FormatPlainText(ContentPresentationData content)
        {
            if (content == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new();
            builder.AppendLine(string.IsNullOrWhiteSpace(content.Identity?.DisplayName)
                ? content.Identity?.ContentId ?? string.Empty
                : content.Identity.DisplayName);
            builder.AppendLine($"ID: {content.Identity?.ContentId}");
            builder.AppendLine($"Status: {content.Status}");

            if (!string.IsNullOrWhiteSpace(content.Description))
            {
                builder.AppendLine(content.Description);
            }

            if (content.ClassificationKeys.Count > 0)
            {
                List<string> tags = new();
                foreach (string key in content.ClassificationKeys)
                {
                    tags.Add(FormatClassification(key));
                }
                builder.AppendLine($"Tags: {string.Join(", ", tags)}");
            }

            foreach (PresentationGroupData group in content.Groups)
            {
                if (group == null)
                {
                    continue;
                }

                builder.AppendLine();
                string groupLabel = usePlayerDisplayCatalog
                    ? FormatGroupLabel(group.Key)
                    : FormatLabel(group.Key);
                if (string.IsNullOrWhiteSpace(groupLabel))
                {
                    continue;
                }

                builder.AppendLine($"[{groupLabel}]");
                if (!string.IsNullOrWhiteSpace(group.SourceContentId))
                {
                    builder.AppendLine($"Source ID: {group.SourceContentId}");
                }
                if (!string.IsNullOrWhiteSpace(group.Description))
                {
                    builder.AppendLine(group.Description);
                }

                foreach (PresentationEntryData entry in group.Entries)
                {
                    if (entry != null)
                    {
                        string entryLabel = usePlayerDisplayCatalog
                            ? FormatEntryLabel(entry.Key)
                            : FormatLabel(entry.Key);
                        string entryValues = usePlayerDisplayCatalog
                            ? FormatEntryValues(entry)
                            : FormatValues(entry.Values);
                        if (!string.IsNullOrWhiteSpace(entryLabel)
                            && !string.IsNullOrWhiteSpace(entryValues))
                        {
                            builder.AppendLine($"- {entryLabel}: {entryValues}");
                        }
                    }
                }
            }

            return builder.ToString().TrimEnd();
        }

        private static string SplitPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            StringBuilder builder = new(value.Length + 8);
            for (int index = 0; index < value.Length; index++)
            {
                char current = value[index];
                if (index > 0
                    && char.IsUpper(current)
                    && !char.IsWhiteSpace(value[index - 1])
                    && !char.IsUpper(value[index - 1]))
                {
                    builder.Append(' ');
                }
                builder.Append(current == '_' ? ' ' : current);
            }

            return builder.ToString();
        }
    }
}
