using System;
using System.Collections.Generic;
using System.IO;
using Bless;
using Effect;
using Item;
using ResourceTools.Effect;
using ResourceTools.Item;
using Shrine;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Content
{
    public static class OwnedEffectSampleAssetBuilder
    {
        private const int ExpectedCountPerCategory = 10;

        private const string RelicSourceRoot =
            "Assets/Contents/Relic";

        private const string RelicOutputRoot =
            "Assets/Contents/Relic/Generated";

        private const string BlessSourceRoot =
            "Assets/Contents/Bless";

        private const string BlessOutputRoot =
            "Assets/Contents/Bless/Generated";

        [Serializable]
        private class GeneralBlessJson
        {
            public string blessingId;
            public string groupId;
            public string nameKo;
            public string descriptionKo;
            public string sourceAssetPath;
            public string sourceEffectAssetPath;
            public string iconGuid;
            public long iconFileId;
            public string category;
            public string godType;
            public string durationType;
            public int durationBattleCount = -1;
            public string[] tags;
        }

        [MenuItem(
            "Tools/ProjectBS/Contents/Build Relic + General Bless Samples",
            false,
            2200)]
        private static void BuildSamplesMenu()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Build owned-effect samples",
                "Create 10 RelicSO and 10 general BlessSO samples beside their JSON files. " +
                "EffectSO and EffectEntrySO assets remain under each Generated folder.\n\n" +
                "Legacy assets under Assets/Resources are read-only and are not modified.\n\n" +
                "Continue?",
                "Build",
                "Cancel");

            if (!confirmed)
            {
                return;
            }

            BuildSamplesBatch();
        }

        [MenuItem(
            "Tools/ProjectBS/Contents/Validate Relic + General Bless Samples",
            false,
            2201)]
        private static void ValidateSamplesMenu()
        {
            ValidateGeneratedSamples();
        }

        public static void BuildSamplesBatch()
        {
            EnsureFolder(RelicOutputRoot);
            EnsureFolder(BlessOutputRoot);

            List<string> generatedMainAssets = new();
            BuildRelicSamples(generatedMainAssets);
            BuildGeneralBlessSamples(generatedMainAssets);

            AssetDatabase.SaveAssets();
            AssetDatabase.ForceReserializeAssets(
                generatedMainAssets,
                ForceReserializeAssetsOptions.ReserializeAssets);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ValidateGeneratedSamples();

            Debug.Log(
                $"[OwnedEffectSampleAssetBuilder] Build passed. Relic={ExpectedCountPerCategory}, GeneralBless={ExpectedCountPerCategory}.");
        }

        public static void ValidateGeneratedSamples()
        {
            ValidateRelicSamples();
            ValidateGeneralBlessSamples();

            Debug.Log(
                $"[OwnedEffectSampleAssetBuilder] Validation passed. Relic={ExpectedCountPerCategory}, GeneralBless={ExpectedCountPerCategory}.");
        }

        private static void BuildRelicSamples(
            List<string> generatedMainAssets)
        {
            string[] jsonPaths = GetJsonPaths(
                RelicSourceRoot,
                "item.relic.*.json");

            RequireExpectedCount("Relic", jsonPaths);

            foreach (string jsonPath in jsonPaths)
            {
                string json = File.ReadAllText(jsonPath);
                RelicItemAssetBuilder.RelicItemJson data =
                    JsonUtility.FromJson<RelicItemAssetBuilder.RelicItemJson>(json);

                if (data == null || string.IsNullOrWhiteSpace(data.relicId))
                {
                    throw new InvalidOperationException(
                        $"[OwnedEffectSampleAssetBuilder] Invalid Relic JSON. json={jsonPath}");
                }

                RelocateGeneratedMainAsset(
                    RelicOutputRoot,
                    RelicSourceRoot,
                    data.relicId);

                RelicSO relic = RelicItemAssetBuilder.CreateOrUpdate(
                    json,
                    RelicSourceRoot,
                    RelicOutputRoot);

                if (relic == null || relic.relicId != data.relicId)
                {
                    throw new InvalidOperationException(
                        $"[OwnedEffectSampleAssetBuilder] Failed to build RelicSO. json={jsonPath}");
                }

                generatedMainAssets.Add(AssetDatabase.GetAssetPath(relic));
            }
        }

        private static void BuildGeneralBlessSamples(
            List<string> generatedMainAssets)
        {
            string[] jsonPaths = GetJsonPaths(
                BlessSourceRoot,
                "bless.*.json");

            RequireExpectedCount("General Bless", jsonPaths);

            foreach (string jsonPath in jsonPaths)
            {
                string json = File.ReadAllText(jsonPath);
                GeneralBlessJson data = JsonUtility.FromJson<GeneralBlessJson>(json);
                List<string> entryJsons = ExtractObjectArray(json, "effectEntries");

                ValidateGeneralBlessJson(data, entryJsons, jsonPath);

                Sprite icon = ResolveSprite(data.iconGuid, data.iconFileId);
                List<EffectEntrySO> entries = new();

                foreach (string entryJson in entryJsons)
                {
                    EffectEntrySO entry = EffectEntryAssetBuilder.CreateOrUpdate(
                        entryJson,
                        BlessOutputRoot);

                    if (entry == null || entry.EffectSO == null)
                    {
                        throw new InvalidOperationException(
                            $"[OwnedEffectSampleAssetBuilder] Failed to build Bless EffectEntrySO. blessingId={data.blessingId}");
                    }

                    entry.EffectSO.ApplyEditorData(
                        entry.EffectSO.EffectId,
                        icon,
                        entry.EffectSO.Config);
                    EditorUtility.SetDirty(entry.EffectSO);
                    entries.Add(entry);
                }

                RelocateGeneratedMainAsset(
                    BlessOutputRoot,
                    BlessSourceRoot,
                    data.blessingId);

                string assetPath =
                    $"{BlessSourceRoot}/{SanitizeFileName(data.blessingId)}.asset";

                BlessSO bless = AssetDatabase.LoadAssetAtPath<BlessSO>(assetPath);

                if (bless == null)
                {
                    bless = ScriptableObject.CreateInstance<BlessSO>();
                    AssetDatabase.CreateAsset(bless, assetPath);
                }

                ApplyGeneralBless(bless, data, icon, entries);
                EditorUtility.SetDirty(bless);
                AssetDatabase.SaveAssetIfDirty(bless);
                generatedMainAssets.Add(assetPath);
            }
        }

        private static void ValidateRelicSamples()
        {
            string[] jsonPaths = GetJsonPaths(
                RelicSourceRoot,
                "item.relic.*.json");

            RequireExpectedCount("Relic", jsonPaths);

            foreach (string jsonPath in jsonPaths)
            {
                string json = File.ReadAllText(jsonPath);
                RelicItemAssetBuilder.RelicItemJson data =
                    JsonUtility.FromJson<RelicItemAssetBuilder.RelicItemJson>(json);
                List<string> entryJsons = ExtractObjectArray(json, "effectEntries");
                string assetPath =
                    $"{RelicSourceRoot}/{SanitizeFileName(data.relicId)}.asset";
                RelicSO relic = AssetDatabase.LoadAssetAtPath<RelicSO>(assetPath);
                string oldGeneratedPath =
                    $"{RelicOutputRoot}/{SanitizeFileName(data.relicId)}.asset";

                if (relic == null
                    || relic.relicId != data.relicId
                    || AssetDatabase.LoadAssetAtPath<RelicSO>(oldGeneratedPath) != null)
                {
                    throw new InvalidOperationException(
                        $"[OwnedEffectSampleAssetBuilder] Missing generated RelicSO. relicId={data.relicId}");
                }

                if (relic.effectEntries == null || relic.effectEntries.Count != entryJsons.Count)
                {
                    throw new InvalidOperationException(
                        $"[OwnedEffectSampleAssetBuilder] Relic EffectEntry count mismatch. relicId={data.relicId}");
                }

                ValidateEntryPaths(relic.effectEntries, RelicOutputRoot, data.relicId);
            }
        }

        private static void ValidateGeneralBlessSamples()
        {
            string[] jsonPaths = GetJsonPaths(
                BlessSourceRoot,
                "bless.*.json");

            RequireExpectedCount("General Bless", jsonPaths);

            foreach (string jsonPath in jsonPaths)
            {
                string json = File.ReadAllText(jsonPath);
                GeneralBlessJson data = JsonUtility.FromJson<GeneralBlessJson>(json);
                List<string> entryJsons = ExtractObjectArray(json, "effectEntries");
                ValidateGeneralBlessJson(data, entryJsons, jsonPath);

                string assetPath =
                    $"{BlessSourceRoot}/{SanitizeFileName(data.blessingId)}.asset";
                BlessSO bless = AssetDatabase.LoadAssetAtPath<BlessSO>(assetPath);
                string oldGeneratedPath =
                    $"{BlessOutputRoot}/{SanitizeFileName(data.blessingId)}.asset";

                if (bless == null
                    || bless.BlessingId != data.blessingId
                    || bless.GroupId != data.groupId
                    || bless.EffectEntries.Count != entryJsons.Count
                    || bless.Icon != ResolveSprite(data.iconGuid, data.iconFileId)
                    || AssetDatabase.LoadAssetAtPath<BlessSO>(oldGeneratedPath) != null)
                {
                    throw new InvalidOperationException(
                        $"[OwnedEffectSampleAssetBuilder] Generated BlessSO does not match JSON. blessingId={data.blessingId}");
                }

                ValidateEntryPaths(bless.EffectEntries, BlessOutputRoot, data.blessingId);
            }
        }

        private static void ValidateGeneralBlessJson(
            GeneralBlessJson data,
            List<string> entryJsons,
            string jsonPath)
        {
            if (data == null
                || string.IsNullOrWhiteSpace(data.blessingId)
                || !data.blessingId.StartsWith("bless.", StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(data.groupId)
                || string.IsNullOrWhiteSpace(data.nameKo)
                || string.IsNullOrWhiteSpace(data.sourceAssetPath)
                || string.IsNullOrWhiteSpace(data.sourceEffectAssetPath)
                || string.IsNullOrWhiteSpace(data.iconGuid)
                || data.iconFileId == 0
                || entryJsons == null
                || entryJsons.Count == 0)
            {
                throw new InvalidOperationException(
                    $"[OwnedEffectSampleAssetBuilder] Invalid general Bless JSON. path={jsonPath}");
            }

            if (!File.Exists(data.sourceAssetPath)
                || !File.Exists(data.sourceEffectAssetPath))
            {
                throw new InvalidOperationException(
                    $"[OwnedEffectSampleAssetBuilder] Bless provenance source is missing. blessingId={data.blessingId}");
            }

            if (!Enum.TryParse(data.category, true, out BlessCategory _)
                || !Enum.TryParse(data.godType, true, out ShrineGodType _)
                || !Enum.TryParse(data.durationType, true, out BlessDurationType _)
                || ResolveSprite(data.iconGuid, data.iconFileId) == null)
            {
                throw new InvalidOperationException(
                    $"[OwnedEffectSampleAssetBuilder] Bless enum or icon is invalid. blessingId={data.blessingId}");
            }
        }

        private static void ApplyGeneralBless(
            BlessSO bless,
            GeneralBlessJson data,
            Sprite icon,
            List<EffectEntrySO> entries)
        {
            SerializedObject serializedObject = new SerializedObject(bless);

            SetString(serializedObject, "blessingId", data.blessingId);
            SetString(serializedObject, "groupId", data.groupId);
            SetObject(serializedObject, "icon", icon);
            SetEnumValue<BlessCategory>(serializedObject, "category", data.category);
            SetObjectArray(serializedObject, "effectEntries", entries);
            SetEnumValue<ShrineGodType>(serializedObject, "godType", data.godType);
            SetEnumValue<BlessDurationType>(serializedObject, "durationType", data.durationType);
            SetInt(serializedObject, "durationBattleCount", data.durationBattleCount);
            SetStringArray(serializedObject, "tags", data.tags);

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Sprite ResolveSprite(
            string guid,
            long localFileId)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (UnityEngine.Object asset in assets)
            {
                if (!(asset is Sprite sprite))
                {
                    continue;
                }

                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                        sprite,
                        out string assetGuid,
                        out long assetLocalFileId)
                    && assetGuid == guid
                    && assetLocalFileId == localFileId)
                {
                    return sprite;
                }
            }

            return null;
        }

        private static void RelocateGeneratedMainAsset(
            string generatedRoot,
            string targetRoot,
            string assetId)
        {
            string fileName = SanitizeFileName(assetId) + ".asset";
            string oldPath = $"{generatedRoot}/{fileName}";
            string targetPath = $"{targetRoot}/{fileName}";
            UnityEngine.Object oldAsset = AssetDatabase.LoadMainAssetAtPath(oldPath);

            if (oldAsset == null)
            {
                return;
            }

            if (AssetDatabase.LoadMainAssetAtPath(targetPath) != null)
            {
                throw new InvalidOperationException(
                    $"[OwnedEffectSampleAssetBuilder] Both old and target main assets exist. old={oldPath}, target={targetPath}");
            }

            string error = AssetDatabase.MoveAsset(oldPath, targetPath);

            if (!string.IsNullOrWhiteSpace(error))
            {
                throw new InvalidOperationException(
                    $"[OwnedEffectSampleAssetBuilder] Failed to relocate generated main asset. old={oldPath}, target={targetPath}, error={error}");
            }
        }

        private static void ValidateEntryPaths(
            IReadOnlyList<EffectEntrySO> entries,
            string outputRoot,
            string ownerId)
        {
            foreach (EffectEntrySO entry in entries)
            {
                if (entry == null || entry.EffectSO == null)
                {
                    throw new InvalidOperationException(
                        $"[OwnedEffectSampleAssetBuilder] Broken EffectEntry reference. ownerId={ownerId}");
                }

                string entryPath = AssetDatabase.GetAssetPath(entry);
                string effectPath = AssetDatabase.GetAssetPath(entry.EffectSO);

                if (!entryPath.StartsWith(outputRoot + "/", StringComparison.Ordinal)
                    || !effectPath.StartsWith(outputRoot + "/", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"[OwnedEffectSampleAssetBuilder] Effect assets are outside the approved Generated folder. ownerId={ownerId}");
                }
            }
        }

        private static string[] GetJsonPaths(
            string root,
            string pattern)
        {
            string[] paths = Directory.GetFiles(
                root,
                pattern,
                SearchOption.TopDirectoryOnly);

            Array.Sort(paths, StringComparer.Ordinal);
            return paths;
        }

        private static void RequireExpectedCount(
            string category,
            string[] jsonPaths)
        {
            if (jsonPaths.Length != ExpectedCountPerCategory)
            {
                throw new InvalidOperationException(
                    $"[OwnedEffectSampleAssetBuilder] Expected {ExpectedCountPerCategory} {category} JSON files, found {jsonPaths.Length}.");
            }
        }

        private static List<string> ExtractObjectArray(
            string json,
            string propertyName)
        {
            List<string> result = new List<string>();
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
                char character = arrayJson[i];

                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (character == '\\')
                {
                    escape = true;
                    continue;
                }

                if (character == '"')
                {
                    inString = !inString;
                    continue;
                }

                if (inString)
                {
                    continue;
                }

                if (character == '{')
                {
                    if (depth == 0)
                    {
                        objectStart = i;
                    }

                    depth++;
                }
                else if (character == '}')
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

            return valueStart < json.Length && json[valueStart] == '['
                ? ExtractBalanced(json, valueStart, '[', ']')
                : null;
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
                char character = json[i];

                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (character == '\\')
                {
                    escape = true;
                    continue;
                }

                if (character == '"')
                {
                    inString = !inString;
                    continue;
                }

                if (inString)
                {
                    continue;
                }

                if (character == open)
                {
                    depth++;
                }
                else if (character == close)
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

        private static void SetString(
            SerializedObject serializedObject,
            string propertyName,
            string value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property != null)
            {
                property.stringValue = value ?? string.Empty;
            }
        }

        private static void SetInt(
            SerializedObject serializedObject,
            string propertyName,
            int value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property != null)
            {
                property.intValue = value;
            }
        }

        private static void SetObject(
            SerializedObject serializedObject,
            string propertyName,
            UnityEngine.Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        private static void SetEnumValue<TEnum>(
            SerializedObject serializedObject,
            string propertyName,
            string value)
            where TEnum : struct
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property != null && Enum.TryParse(value, true, out TEnum parsed))
            {
                property.intValue = Convert.ToInt32(parsed);
            }
        }

        private static void SetObjectArray(
            SerializedObject serializedObject,
            string propertyName,
            IReadOnlyList<EffectEntrySO> values)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

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

        private static void SetStringArray(
            SerializedObject serializedObject,
            string propertyName,
            IReadOnlyList<string> values)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null || !property.isArray)
            {
                return;
            }

            int count = values != null ? values.Count : 0;
            property.arraySize = count;

            for (int i = 0; i < count; i++)
            {
                property.GetArrayElementAtIndex(i).stringValue = values[i] ?? string.Empty;
            }
        }

        private static void EnsureFolder(
            string folderPath)
        {
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
