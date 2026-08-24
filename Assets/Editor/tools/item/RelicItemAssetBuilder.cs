using System;
using System.Collections.Generic;
using System.IO;
using Effect;
using Item;
using ResourceTools.Effect;
using ResourceTools.Helper;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Item
{
    public static class RelicItemAssetBuilder
    {
        private const string CurrentRelicJsonRoot =
            "Assets/Contents/Item/json";

        private const string CurrentRelicSoRoot =
            "Assets/Contents/Item/so";

        [Serializable]
        public class RelicItemJson
        {
            public string relicId;
            public string nameKo;
            public string descriptionKo;
            public string icon;
            public ColorJson themeColor;
            public string rarity;
            public string category;
            public string subCategory;
            public bool hidden;
            public bool developerOnly;
        }

        [Serializable]
        public class ColorJson
        {
            public float r = 1f;
            public float g = 1f;
            public float b = 1f;
            public float a = 1f;
        }

        [MenuItem("Assets/Item/Relic Item Generator", false, 1998)]
        private static void GenerateSelectedRelic()
        {
            string jsonPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!IsRelicJsonPath(jsonPath))
            {
                Debug.LogError(
                    "[RelicItemAssetBuilder] Select an item.relic.*.json file.");
                return;
            }

            GenerateFromJsonPath(jsonPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Item/Relic Item Generator", true)]
        private static bool ValidateGenerateSelectedRelic()
        {
            return IsRelicJsonPath(
                AssetDatabase.GetAssetPath(Selection.activeObject));
        }

        [MenuItem(
            "Assets/Item/Generate All Relic Item SO From Folder",
            false,
            1999)]
        private static void GenerateAllRelicsFromSelectedFolder()
        {
            string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrWhiteSpace(folderPath) ||
                !AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError(
                    "[RelicItemAssetBuilder] Select a folder in the Project window first.");
                return;
            }

            string[] jsonPaths = Directory.GetFiles(
                folderPath,
                "item.relic.*.json",
                SearchOption.TopDirectoryOnly);
            Array.Sort(jsonPaths, StringComparer.Ordinal);

            int generatedCount = 0;
            for (int i = 0; i < jsonPaths.Length; i++)
            {
                string jsonPath = jsonPaths[i].Replace("\\", "/");
                if (GenerateFromJsonPath(jsonPath) != null)
                {
                    generatedCount++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(
                $"[RelicItemAssetBuilder] Folder generation completed. " +
                $"folder={folderPath}, generated={generatedCount}, " +
                $"output={CurrentRelicSoRoot}");
        }

        [MenuItem(
            "Assets/Item/Generate All Relic Item SO From Folder",
            true)]
        private static bool ValidateGenerateAllRelicsFromSelectedFolder()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return !string.IsNullOrWhiteSpace(path) &&
                   AssetDatabase.IsValidFolder(path);
        }

        public static RelicSO GenerateFromJsonPath(string jsonPath)
        {
            if (!IsRelicJsonPath(jsonPath))
            {
                Debug.LogError(
                    $"[RelicItemAssetBuilder] Invalid relic json path. path={jsonPath}");
                return null;
            }

            RelicSO relic = CreateOrUpdate(
                File.ReadAllText(jsonPath),
                CurrentRelicSoRoot,
                CurrentRelicSoRoot);

            if (relic != null)
            {
                BuildItemStrings(jsonPath);
            }

            return relic;
        }

        private static bool IsRelicJsonPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) ||
                !path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string fileName = Path.GetFileName(path);
            return fileName.StartsWith(
                "item.relic.",
                StringComparison.OrdinalIgnoreCase);
        }

        public static RelicSO CreateOrUpdate(
            string relicJson,
            string relicOutputFolder,
            string effectOutputFolder)
        {
            if (string.IsNullOrWhiteSpace(relicJson))
            {
                Debug.LogError("[RelicItemAssetBuilder] relic json is required.");
                return null;
            }

            RelicItemJson data =
                JsonUtility.FromJson<RelicItemJson>(relicJson);

            List<string> effectEntries =
                ExtractObjectArray(
                    relicJson,
                    "effectEntries");

            if (!Validate(data, effectEntries))
            {
                return null;
            }

            EnsureFolder(relicOutputFolder);
            EnsureFolder(effectOutputFolder);

            List<EffectEntrySO> entryAssets = new();

            for (int i = 0; i < effectEntries.Count; i++)
            {
                EffectEntrySO entry =
                    EffectEntryAssetBuilder.CreateOrUpdate(
                        effectEntries[i],
                        effectOutputFolder);

                if (entry == null)
                {
                    Debug.LogError(
                        $"[RelicItemAssetBuilder] Failed to build effect entry. relicId={data.relicId}, index={i}");
                    return null;
                }

                entryAssets.Add(entry);
            }

            string assetPath =
                $"{relicOutputFolder}/{SanitizeFileName(data.relicId)}.asset";

            RelicSO relic =
                AssetDatabase.LoadAssetAtPath<RelicSO>(assetPath);

            if (relic == null)
            {
                relic = ScriptableObject.CreateInstance<RelicSO>();
                AssetDatabase.CreateAsset(relic, assetPath);
            }

            Apply(
                relic,
                data,
                entryAssets);

            EditorUtility.SetDirty(relic);
            AssetDatabase.SaveAssetIfDirty(relic);
            AssetDatabase.SaveAssets();

            return relic;
        }

        [MenuItem(
            "Tools/ProjectBS/Items/Build Current Relics From JSON",
            false,
            2100)]
        private static void BuildCurrentRelicsFromJsonMenu()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Build Current Relics",
                "This creates or updates the current RelicSO, EffectSO, and " +
                "EffectEntrySO assets under Assets/Contents/Item/so from " +
                "the 10 JSON files under Assets/Contents/Item/json.\n\n" +
                "JSON files are not modified.\n\n" +
                "Continue?",
                "Build",
                "Cancel");

            if (!confirmed)
            {
                return;
            }

            BuildCurrentRelicsFromJsonBatch();
        }

        [MenuItem(
            "Tools/ProjectBS/Items/Validate Current Relics",
            false,
            2101)]
        private static void ValidateCurrentRelicsMenu()
        {
            ValidateCurrentRelics();
        }

        public static void BuildCurrentRelicsFromJsonBatch()
        {
            string[] jsonPaths =
                Directory.GetFiles(
                    CurrentRelicJsonRoot,
                    "item.relic.*.json",
                    SearchOption.TopDirectoryOnly);

            Array.Sort(
                jsonPaths,
                StringComparer.Ordinal);

            if (jsonPaths.Length != 10)
            {
                throw new InvalidOperationException(
                    $"[RelicItemAssetBuilder] Expected 10 normalized relic JSON files, found {jsonPaths.Length}.");
            }

            List<string> generatedRelicPaths = new();
            List<string> generatedEffectAssets = new();

            foreach (string jsonPath in jsonPaths)
            {
                string relicJson =
                    File.ReadAllText(jsonPath);

                RelicItemJson data =
                    JsonUtility.FromJson<RelicItemJson>(relicJson);

                if (data == null
                    || string.IsNullOrWhiteSpace(data.relicId))
                {
                    throw new InvalidOperationException(
                        $"[RelicItemAssetBuilder] Invalid relic JSON. path={jsonPath}");
                }

                List<string> effectEntryJsons =
                    ExtractObjectArray(
                        relicJson,
                        "effectEntries");

                if (!Validate(data, effectEntryJsons))
                {
                    throw new InvalidOperationException(
                        $"[RelicItemAssetBuilder] Validation failed. relicId={data.relicId}");
                }

                RelicSO relic = CreateOrUpdate(
                    relicJson,
                    CurrentRelicSoRoot,
                    CurrentRelicSoRoot);

                if (relic == null)
                {
                    throw new InvalidOperationException(
                        $"[RelicItemAssetBuilder] Failed to build current RelicSO. relicId={data.relicId}");
                }

                generatedRelicPaths.Add(
                    AssetDatabase.GetAssetPath(relic));

                BuildItemStrings(jsonPath);

                foreach (EffectEntrySO entry in relic.effectEntries)
                {
                    generatedEffectAssets.Add(
                        AssetDatabase.GetAssetPath(entry));

                    if (entry.EffectSO != null)
                    {
                        generatedEffectAssets.Add(
                            AssetDatabase.GetAssetPath(entry.EffectSO));
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.ForceReserializeAssets(
                generatedRelicPaths,
                ForceReserializeAssetsOptions.ReserializeAssets);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ValidateCurrentRelics();

            Debug.Log(
                $"[RelicItemAssetBuilder] Built current relics. relicCount={generatedRelicPaths.Count}, effectAssetRefs={generatedEffectAssets.Count}, output={CurrentRelicSoRoot}");
        }

        private static void BuildItemStrings(string jsonPath)
        {
            ResourceTools.ItemStringBuilder.BuildResult result =
                ResourceTools.ItemStringBuilder.BuildFromJsonPath(jsonPath);

            foreach (string error in result.errors)
            {
                Debug.LogError(
                    $"[RelicItemAssetBuilder] String build error: {error}");
            }

            foreach (string warning in result.warnings)
            {
                Debug.LogWarning(
                    $"[RelicItemAssetBuilder] String build warning: {warning}");
            }
        }

        public static void ValidateCurrentRelics()
        {
            Dictionary<string, RelicSO> relicsById =
                LoadCurrentRelicsById();

            string[] jsonPaths =
                Directory.GetFiles(
                    CurrentRelicJsonRoot,
                    "item.relic.*.json",
                    SearchOption.TopDirectoryOnly);

            if (jsonPaths.Length != 10)
            {
                throw new InvalidOperationException(
                    $"[RelicItemAssetBuilder] Expected 10 normalized relic JSON files, found {jsonPaths.Length}.");
            }

            foreach (string jsonPath in jsonPaths)
            {
                RelicItemJson data =
                    JsonUtility.FromJson<RelicItemJson>(
                        File.ReadAllText(jsonPath));

                if (data == null
                    || string.IsNullOrWhiteSpace(data.relicId)
                    || !relicsById.TryGetValue(data.relicId, out RelicSO relic)
                    || relic == null)
                {
                    throw new InvalidOperationException(
                        $"[RelicItemAssetBuilder] Missing current relic. path={jsonPath}");
                }

                string expectedRelicPath =
                    $"{CurrentRelicSoRoot}/{SanitizeFileName(data.relicId)}.asset";

                if (!string.Equals(
                        AssetDatabase.GetAssetPath(relic),
                        expectedRelicPath,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"[RelicItemAssetBuilder] Current RelicSO is outside the approved output path. relicId={data.relicId}");
                }

                List<string> effectEntryJsons =
                    ExtractObjectArray(
                        File.ReadAllText(jsonPath),
                        "effectEntries");

                if (relic.effectEntries == null
                    || relic.effectEntries.Count != effectEntryJsons.Count)
                {
                    throw new InvalidOperationException(
                        $"[RelicItemAssetBuilder] Current relic EffectEntry count does not match JSON. relicId={data.relicId}, json={effectEntryJsons.Count}, asset={relic.effectEntries?.Count ?? 0}");
                }

                foreach (EffectEntrySO entry in relic.effectEntries)
                {
                    if (entry == null || entry.EffectSO == null)
                    {
                        throw new InvalidOperationException(
                            $"[RelicItemAssetBuilder] Current relic has broken effect entry. relicId={data.relicId}");
                    }

                    string entryPath = AssetDatabase.GetAssetPath(entry);
                    string effectPath = AssetDatabase.GetAssetPath(entry.EffectSO);

                    if (!entryPath.StartsWith(CurrentRelicSoRoot + "/", StringComparison.Ordinal)
                        || !effectPath.StartsWith(CurrentRelicSoRoot + "/", StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            $"[RelicItemAssetBuilder] Current relic references an Effect asset outside the approved output path. relicId={data.relicId}");
                    }
                }
            }

            Debug.Log(
                $"[RelicItemAssetBuilder] Current relic validation passed. relicCount={jsonPaths.Length}, output={CurrentRelicSoRoot}");
        }

        private static Dictionary<string, RelicSO> LoadCurrentRelicsById()
        {
            Dictionary<string, RelicSO> result = new();

            string[] guids =
                AssetDatabase.FindAssets(
                    "t:RelicSO",
                    new[] { CurrentRelicSoRoot });

            foreach (string guid in guids)
            {
                string path =
                    AssetDatabase.GUIDToAssetPath(guid);

                RelicSO relic =
                    AssetDatabase.LoadAssetAtPath<RelicSO>(path);

                if (relic == null
                    || string.IsNullOrWhiteSpace(relic.relicId))
                {
                    continue;
                }

                if (!relic.relicId.StartsWith(
                        "item.relic.",
                        StringComparison.Ordinal))
                {
                    continue;
                }

                result[relic.relicId] = relic;
            }

            return result;
        }

        private static void Apply(
            RelicSO relic,
            RelicItemJson data,
            List<EffectEntrySO> entries)
        {
            SerializedObject serializedObject =
                new SerializedObject(relic);

            SetString(serializedObject, "relicId", data.relicId);
            SetObject(serializedObject, "icon", SpriteHelper.FindSpriteByName(data.icon));
            SetColor(serializedObject, "themeColor", ToColor(data.themeColor));
            SetEnum(serializedObject, "rarity", data.rarity);
            SetString(serializedObject, "category", data.category);
            SetString(serializedObject, "subCategory", data.subCategory);
            SetBool(serializedObject, "hidden", data.hidden);
            SetBool(serializedObject, "developerOnly", data.developerOnly);
            SetObjectArray(serializedObject, "effectEntries", entries);

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static bool Validate(
            RelicItemJson data,
            List<string> effectEntries)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.relicId))
            {
                Debug.LogError("[RelicItemAssetBuilder] relicId is required.");
                return false;
            }

            if (!data.relicId.StartsWith("item.relic.", StringComparison.Ordinal))
            {
                Debug.LogError($"[RelicItemAssetBuilder] relicId must start with item.relic. relicId={data.relicId}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(data.nameKo)
                || string.IsNullOrWhiteSpace(data.descriptionKo))
            {
                Debug.LogError(
                    $"[RelicItemAssetBuilder] nameKo and descriptionKo are required localization inputs. relicId={data.relicId}");
                return false;
            }

            if (!Enum.TryParse(data.rarity, true, out RelicRarity _))
            {
                Debug.LogError($"[RelicItemAssetBuilder] Invalid rarity. relicId={data.relicId}, rarity={data.rarity}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(data.icon))
            {
                Debug.LogError($"[RelicItemAssetBuilder] icon is required. relicId={data.relicId}");
                return false;
            }

            if (SpriteHelper.FindSpriteByName(data.icon) == null)
            {
                Debug.LogError($"[RelicItemAssetBuilder] icon Sprite could not be resolved. relicId={data.relicId}, icon={data.icon}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(data.category)
                || string.IsNullOrWhiteSpace(data.subCategory))
            {
                Debug.LogError($"[RelicItemAssetBuilder] category and subCategory are required. relicId={data.relicId}");
                return false;
            }

            if (!IsValidColor(data.themeColor))
            {
                Debug.LogError($"[RelicItemAssetBuilder] themeColor must be 0..1. relicId={data.relicId}");
                return false;
            }

            if (effectEntries == null || effectEntries.Count == 0)
            {
                Debug.LogError($"[RelicItemAssetBuilder] effectEntries are required. relicId={data.relicId}");
                return false;
            }

            return true;
        }

        private static List<string> ExtractObjectArray(
            string json,
            string propertyName)
        {
            List<string> result = new();
            string arrayJson = ExtractJsonValue(json, propertyName);

            if (string.IsNullOrWhiteSpace(arrayJson) || arrayJson[0] != '[')
            {
                return result;
            }

            bool inString = false;
            bool escape = false;
            int depth = 0;
            int objectStart = -1;

            for (int i = 1; i < arrayJson.Length - 1; i++)
            {
                char c = arrayJson[i];

                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (c == '\\')
                {
                    escape = true;
                    continue;
                }

                if (c == '"')
                {
                    inString = !inString;
                    continue;
                }

                if (inString)
                {
                    continue;
                }

                if (c == '{')
                {
                    if (depth == 0)
                    {
                        objectStart = i;
                    }

                    depth++;
                }
                else if (c == '}')
                {
                    depth--;

                    if (depth == 0 && objectStart >= 0)
                    {
                        result.Add(arrayJson.Substring(objectStart, i - objectStart + 1));
                        objectStart = -1;
                    }
                }
            }

            return result;
        }

        private static string ExtractJsonValue(
            string json,
            string propertyName)
        {
            string key = $"\"{propertyName}\"";
            int keyIndex = json.IndexOf(key, StringComparison.Ordinal);

            if (keyIndex < 0)
            {
                return null;
            }

            int colonIndex = json.IndexOf(':', keyIndex + key.Length);

            if (colonIndex < 0)
            {
                return null;
            }

            int valueStart = colonIndex + 1;

            while (valueStart < json.Length && char.IsWhiteSpace(json[valueStart]))
            {
                valueStart++;
            }

            if (valueStart >= json.Length)
            {
                return null;
            }

            char first = json[valueStart];

            if (first == '[')
            {
                return ExtractBalanced(json, valueStart, '[', ']');
            }

            if (first == '{')
            {
                return ExtractBalanced(json, valueStart, '{', '}');
            }

            return null;
        }

        private static string ExtractBalanced(
            string json,
            int startIndex,
            char open,
            char close)
        {
            int depth = 0;
            bool inString = false;
            bool escape = false;

            for (int i = startIndex; i < json.Length; i++)
            {
                char c = json[i];

                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (c == '\\')
                {
                    escape = true;
                    continue;
                }

                if (c == '"')
                {
                    inString = !inString;
                    continue;
                }

                if (inString)
                {
                    continue;
                }

                if (c == open)
                {
                    depth++;
                }
                else if (c == close)
                {
                    depth--;

                    if (depth == 0)
                    {
                        return json.Substring(startIndex, i - startIndex + 1);
                    }
                }
            }

            return null;
        }

        private static bool IsValidColor(
            ColorJson color)
        {
            return color != null
                && Is01(color.r)
                && Is01(color.g)
                && Is01(color.b)
                && Is01(color.a);
        }

        private static bool Is01(float value)
        {
            return value >= 0f && value <= 1f;
        }

        private static Color ToColor(
            ColorJson color)
        {
            return color != null
                ? new Color(color.r, color.g, color.b, color.a)
                : Color.white;
        }

        private static void SetString(
            SerializedObject serializedObject,
            string propertyName,
            string value)
        {
            SerializedProperty property =
                serializedObject.FindProperty(propertyName);

            if (property != null)
            {
                property.stringValue = value ?? string.Empty;
            }
        }

        private static void SetBool(
            SerializedObject serializedObject,
            string propertyName,
            bool value)
        {
            SerializedProperty property =
                serializedObject.FindProperty(propertyName);

            if (property != null)
            {
                property.boolValue = value;
            }
        }

        private static void SetEnum(
            SerializedObject serializedObject,
            string propertyName,
            string value)
        {
            SerializedProperty property =
                serializedObject.FindProperty(propertyName);

            if (property != null
                && Enum.TryParse(value, true, out RelicRarity rarity))
            {
                property.enumValueIndex = (int)rarity / 100;
            }
        }

        private static void SetColor(
            SerializedObject serializedObject,
            string propertyName,
            Color value)
        {
            SerializedProperty property =
                serializedObject.FindProperty(propertyName);

            if (property != null)
            {
                property.colorValue = value;
            }
        }

        private static void SetObject(
            SerializedObject serializedObject,
            string propertyName,
            UnityEngine.Object value)
        {
            SerializedProperty property =
                serializedObject.FindProperty(propertyName);

            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        private static void SetObjectArray(
            SerializedObject serializedObject,
            string propertyName,
            List<EffectEntrySO> values)
        {
            SerializedProperty property =
                serializedObject.FindProperty(propertyName);

            if (property == null || !property.isArray)
            {
                return;
            }

            property.arraySize = values.Count;

            for (int i = 0; i < values.Count; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static void EnsureFolder(
            string folderPath)
        {
            folderPath = folderPath.Replace("\\", "/");

            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string[] parts = folderPath.Split('/');
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

        private static string SanitizeFileName(
            string value)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalid, '_');
            }

            return value;
        }
    }
}
