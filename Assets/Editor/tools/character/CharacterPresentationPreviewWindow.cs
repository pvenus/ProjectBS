using System;
using System.Collections.Generic;
using System.Text;
using Character;
using Presentation;
using Stat;
using UnityEditor;
using UnityEngine;

public sealed class CharacterPresentationPreviewWindow : EditorWindow
{
    private const string ApprovedJsonRoot = "Assets/Resources/character/json";

    [Serializable]
    private sealed class CharacterJsonData
    {
        public string characterId;
        public string name;
        public string characterType;
        public string job;
        public List<CharacterStatJsonData> baseStats = new();
    }

    [Serializable]
    private sealed class CharacterStatJsonData
    {
        public string statType;
        public float value;
    }

    private CharacterSO selectedCharacter;
    private TextAsset sourceJson;
    private CharacterJsonData sourceData;
    private readonly Dictionary<string, string> previewStrings =
        new(StringComparer.Ordinal);
    private Vector2 rawJsonScroll;
    private Vector2 inspectionScroll;
    private Vector2 playerScroll;

    [MenuItem("Tools/ProjectBS/Presentation/Open Character Data Preview")]
    private static void OpenWindow()
    {
        CharacterPresentationPreviewWindow window =
            GetWindow<CharacterPresentationPreviewWindow>();
        window.titleContent = new GUIContent("Character Presentation");
        window.SetCharacter(Selection.activeObject as CharacterSO);
        window.Show();
    }

    [MenuItem("Assets/ProjectBS/Presentation/Preview Selected Character", false, 2100)]
    private static void OpenSelectedCharacter()
    {
        CharacterPresentationPreviewWindow window =
            GetWindow<CharacterPresentationPreviewWindow>();
        window.titleContent = new GUIContent("Character Presentation");
        window.SetCharacter(Selection.activeObject as CharacterSO);
        window.Show();
    }

    [MenuItem("Assets/ProjectBS/Presentation/Preview Selected Character", true)]
    private static bool ValidateOpenSelectedCharacter()
    {
        return Selection.activeObject is CharacterSO;
    }

    private void OnSelectionChange()
    {
        if (Selection.activeObject is CharacterSO character)
        {
            SetCharacter(character);
            Repaint();
        }
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        CharacterSO nextCharacter = (CharacterSO)EditorGUILayout.ObjectField(
            "Character",
            selectedCharacter,
            typeof(CharacterSO),
            false);
        if (EditorGUI.EndChangeCheck())
        {
            SetCharacter(nextCharacter);
        }

        if (selectedCharacter == null)
        {
            EditorGUILayout.HelpBox(
                "Select a CharacterSO. The tool reads only its matching JSON under " +
                $"{ApprovedJsonRoot}.",
                MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField(
            "Character ID",
            selectedCharacter.CharacterId);
        EditorGUILayout.LabelField(
            "Identity localization key",
            $"{selectedCharacter.LocalizationMainKey}.name");
        EditorGUILayout.LabelField(
            "JSON",
            sourceJson != null
                ? AssetDatabase.GetAssetPath(sourceJson)
                : $"Missing: {BuildExpectedJsonPath(selectedCharacter)}");

        DrawComparisonStatus();

        CharacterPresentationResolver resolver = new();
        ContentPresentationData inspection = resolver.Resolve(
            selectedCharacter,
            PresentationContext.Preview);
        ContentPresentationData player = resolver.ResolveForPlayerDisplay(
            selectedCharacter,
            PresentationContext.Preview);
        player = ReplacePlayerIdentity(player);

        PresentationTextFormatter inspectionFormatter = new();
        PresentationTextFormatter playerFormatter =
            PresentationTextFormatter.CreatePlayerFormatter(ResolvePreviewLabel);

        string rawJson = sourceJson != null
            ? sourceJson.text
            : "Matching approved JSON was not found.";
        string inspectionText = BuildInspectionText(
            inspectionFormatter.FormatPlainText(inspection));
        string playerText = playerFormatter.FormatPlainText(player);

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        float columnWidth = Mathf.Max(260f, (position.width - 32f) / 3f);
        rawJsonScroll = DrawTextColumn(
            "Original JSON",
            rawJson,
            rawJsonScroll,
            columnWidth);
        inspectionScroll = DrawTextColumn(
            "SO Inspection (all)",
            inspectionText,
            inspectionScroll,
            columnWidth);
        playerScroll = DrawTextColumn(
            "Player UI (filtered)",
            playerText,
            playerScroll,
            columnWidth);
        EditorGUILayout.EndHorizontal();
    }

    private void SetCharacter(CharacterSO character)
    {
        selectedCharacter = character;
        sourceJson = null;
        sourceData = null;
        LoadPreviewStrings();
        rawJsonScroll = Vector2.zero;
        inspectionScroll = Vector2.zero;
        playerScroll = Vector2.zero;

        if (selectedCharacter == null)
        {
            return;
        }

        sourceJson = AssetDatabase.LoadAssetAtPath<TextAsset>(
            BuildExpectedJsonPath(selectedCharacter));
        if (sourceJson != null)
        {
            sourceData = JsonUtility.FromJson<CharacterJsonData>(sourceJson.text);
        }
    }

    private void DrawComparisonStatus()
    {
        if (sourceJson == null || sourceData == null)
        {
            EditorGUILayout.HelpBox(
                "The approved source JSON could not be loaded. SO/UI comparison is unavailable.",
                MessageType.Error);
            return;
        }

        List<string> mismatches = FindJsonSoMismatches();
        if (mismatches.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "JSON -> CharacterSO imported fields match. " +
                "The JSON name is compared visually because the SO name is resolved through StringManager.",
                MessageType.Info);
            return;
        }

        EditorGUILayout.HelpBox(
            "JSON / CharacterSO mismatch:\n- " + string.Join("\n- ", mismatches),
            MessageType.Warning);
    }

    private List<string> FindJsonSoMismatches()
    {
        List<string> result = new();
        if (sourceData == null || selectedCharacter == null)
        {
            return result;
        }

        if (!string.Equals(
                sourceData.characterId,
                selectedCharacter.CharacterId,
                StringComparison.Ordinal))
        {
            result.Add(
                $"characterId JSON='{sourceData.characterId}' SO='{selectedCharacter.CharacterId}'");
        }

        string localizedName = ResolvePreviewLabel(
            selectedCharacter.LocalizationMainKey);
        if (!string.Equals(
                sourceData.name,
                localizedName,
                StringComparison.Ordinal))
        {
            result.Add(
                $"name JSON='{sourceData.name}' StringManager='{localizedName}'");
        }

        if (!Enum.TryParse(
                sourceData.characterType,
                true,
                out CharacterType sourceType)
            || sourceType != selectedCharacter.CharacterType)
        {
            result.Add(
                $"characterType JSON='{sourceData.characterType}' SO='{selectedCharacter.CharacterType}'");
        }

        if (!Enum.TryParse(
                sourceData.job,
                true,
                out CharacterJob sourceJob)
            || sourceJob != selectedCharacter.Job)
        {
            result.Add(
                $"job JSON='{sourceData.job}' SO='{selectedCharacter.Job}'");
        }

        IReadOnlyList<StatEntry> soStats = selectedCharacter.BaseStats;
        List<CharacterStatJsonData> jsonStats =
            sourceData.baseStats ?? new List<CharacterStatJsonData>();
        if (jsonStats.Count != soStats.Count)
        {
            result.Add($"baseStats count JSON={jsonStats.Count} SO={soStats.Count}");
        }

        int compareCount = Mathf.Min(jsonStats.Count, soStats.Count);
        for (int index = 0; index < compareCount; index++)
        {
            CharacterStatJsonData jsonStat = jsonStats[index];
            StatEntry soStat = soStats[index];
            if (jsonStat == null || soStat == null)
            {
                result.Add($"baseStats[{index}] null mismatch");
                continue;
            }

            bool typeMatches = Enum.TryParse(
                    jsonStat.statType,
                    true,
                    out StatType sourceStatType)
                && sourceStatType == soStat.statType;
            if (!typeMatches || !Mathf.Approximately(jsonStat.value, soStat.value))
            {
                result.Add(
                    $"baseStats[{index}] JSON='{jsonStat.statType}:{jsonStat.value}' " +
                    $"SO='{soStat.statType}:{soStat.value}'");
            }
        }

        return result;
    }

    private string BuildInspectionText(string presentationText)
    {
        StringBuilder builder = new();
        builder.AppendLine(presentationText);
        builder.AppendLine();
        builder.AppendLine("[SO-only system data]");
        builder.AppendLine($"Animation clips: {selectedCharacter.AnimationClips.Count}");
        builder.AppendLine($"Skill references: {selectedCharacter.Skills.Count}");

        for (int index = 0; index < selectedCharacter.Skills.Count; index++)
        {
            CharacterSkillEntry entry = selectedCharacter.Skills[index];
            builder.AppendLine(
                $"- skills[{index}] slotKey='{entry?.slotKey}' " +
                $"skill='{entry?.skillSo?.EquipmentId}'");
        }

        return builder.ToString().TrimEnd();
    }

    private static Vector2 DrawTextColumn(
        string title,
        string text,
        Vector2 scroll,
        float width)
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(width));
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        scroll = EditorGUILayout.BeginScrollView(scroll);
        GUIStyle style = new(EditorStyles.textArea)
        {
            wordWrap = false,
        };
        EditorGUILayout.TextArea(
            text ?? string.Empty,
            style,
            GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        return scroll;
    }

    private static string BuildExpectedJsonPath(CharacterSO character)
    {
        return character == null || string.IsNullOrWhiteSpace(character.CharacterId)
            ? string.Empty
            : $"{ApprovedJsonRoot}/{character.CharacterId}.json";
    }

    private ContentPresentationData ReplacePlayerIdentity(
        ContentPresentationData content)
    {
        if (content == null || content.Identity == null)
        {
            return content;
        }

        PresentationIdentityData identity = new(
            content.Identity.ContentId,
            ResolvePreviewLabel(selectedCharacter.LocalizationMainKey),
            content.Identity.Icon);
        return new ContentPresentationData(
            identity,
            content.Description,
            content.ClassificationKeys,
            content.Groups,
            content.Provenance,
            content.Status);
    }

    private void LoadPreviewStrings()
    {
        previewStrings.Clear();
        LoadPreviewCsv("Assets/Resources/string/character_string.csv");
        LoadPreviewCsv("Assets/Resources/string/presentation_string.csv");
    }

    private void LoadPreviewCsv(string path)
    {
        TextAsset csv = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        if (csv == null || string.IsNullOrWhiteSpace(csv.text))
        {
            return;
        }

        string[] lines = csv.text
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n');
        if (lines.Length == 0)
        {
            return;
        }

        List<string> headers = ParseCsvLine(lines[0]);
        int koreanIndex = headers.FindIndex(
            header => string.Equals(header.Trim(), "ko", StringComparison.Ordinal));
        if (koreanIndex < 0)
        {
            return;
        }

        for (int index = 1; index < lines.Length; index++)
        {
            if (string.IsNullOrWhiteSpace(lines[index]))
            {
                continue;
            }

            List<string> columns = ParseCsvLine(lines[index]);
            if (columns.Count <= koreanIndex
                || string.IsNullOrWhiteSpace(columns[0])
                || string.IsNullOrWhiteSpace(columns[1]))
            {
                continue;
            }

            previewStrings[$"{columns[0].Trim()}.{columns[1].Trim()}"] =
                columns[koreanIndex];
        }
    }

    private static List<string> ParseCsvLine(string line)
    {
        List<string> result = new();
        StringBuilder builder = new();
        bool inQuotes = false;

        for (int index = 0; index < line.Length; index++)
        {
            char current = line[index];
            if (current == '"')
            {
                bool escapedQuote = inQuotes
                    && index + 1 < line.Length
                    && line[index + 1] == '"';
                if (escapedQuote)
                {
                    builder.Append('"');
                    index++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
                continue;
            }

            if (current == ',' && !inQuotes)
            {
                result.Add(builder.ToString());
                builder.Clear();
                continue;
            }

            builder.Append(current);
        }

        result.Add(builder.ToString());
        return result;
    }

    private string ResolvePreviewLabel(string localizationMainKey)
    {
        string resolved =
            PresentationLocalizedTextResolver.ResolveLabel(localizationMainKey);
        if (!string.IsNullOrWhiteSpace(resolved))
        {
            return resolved;
        }

        string fullKey = $"{localizationMainKey}.name";
        return previewStrings.TryGetValue(fullKey, out string preview)
            && !string.IsNullOrWhiteSpace(preview)
                ? preview
                : fullKey;
    }
}
