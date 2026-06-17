using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Battle;

namespace ResourceTools.Stage
{
    /// <summary>
    /// Builds PopupEventSO node assets from a story JSON file.
    ///
    /// Intended usage from another editor generator:
    ///     PopupEventBuilder.BuildFromJsonPath(jsonPath, outputFolder);
    ///
    /// Runtime SO rule:
    /// - JSON keeps nodeId / nextNodeId.
    /// - Generated SO keeps only PopupEventSO references for nextEvent.
    /// - Text is not stored in SO. eventId / choiceId are used as StringManager keys.
    /// </summary>
    public static class PopupEventBuilder
    {
        private const string DefaultOutputFolder = "Assets/Resources/stage/popup_events";

        public sealed class BuildResult
        {
            public readonly Dictionary<string, ScriptableObject> eventsById = new();
            public readonly List<string> createdAssetPaths = new();
            public readonly List<string> updatedAssetPaths = new();
            public readonly List<string> warnings = new();

            public ScriptableObject StartEvent { get; internal set; }
            public string StartEventId { get; internal set; }
        }

        [Serializable]
        private sealed class PopupEventJsonRoot
        {
            public string actId;
            public string chapterId;
            public int actNumber;
            public string titleKo;
            public string summary;
            public string startNodeId;
            public List<PopupEventNodeJson> nodes;
        }

        [Serializable]
        private sealed class PopupEventNodeJson
        {
            public string nodeId;
            public string nodeType;
            public string locationId;
            public string speakerId;
            public string speakerNameKo;
            public string textKo;
            public string nextNodeId;
            public List<PopupEventChoiceJson> choices;
        }

        [Serializable]
        private sealed class PopupEventChoiceJson
        {
            public string choiceId;
            public string textKo;
            public string valueTag;
            public string nextNodeId;
            public List<PopupEventRewardJson> rewards;
        }

        [Serializable]
        private sealed class PopupEventRewardJson
        {
            public string rewardType;
            public string rewardId;
            public int amount;
            public int value;
            public string tag;
            public BattleJsonGenerator.BattleJson battle;
        }

        public static BuildResult BuildFromJsonPath(string jsonPath)
        {
            return BuildFromJsonPath(jsonPath, DefaultOutputFolder);
        }

        public static BuildResult BuildFromJsonPath(string jsonPath, string outputFolder)
        {
            var result = new BuildResult();

            if (string.IsNullOrWhiteSpace(jsonPath))
            {
                throw new ArgumentException("jsonPath is null or empty.", nameof(jsonPath));
            }

            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException($"Popup event json not found: {jsonPath}", jsonPath);
            }

            if (string.IsNullOrWhiteSpace(outputFolder))
            {
                outputFolder = DefaultOutputFolder;
            }

            EnsureFolder(outputFolder);

            var jsonText = File.ReadAllText(jsonPath);
            var root = JsonUtility.FromJson<PopupEventJsonRoot>(jsonText);
            if (root == null || root.nodes == null || root.nodes.Count == 0)
            {
                throw new InvalidDataException($"Popup event json has no nodes: {jsonPath}");
            }

            var popupEventType = FindTypeByName("PopupEventSO");
            if (popupEventType == null)
            {
                throw new InvalidOperationException("Could not find type PopupEventSO. Check assembly / namespace.");
            }

            if (!typeof(ScriptableObject).IsAssignableFrom(popupEventType))
            {
                throw new InvalidOperationException("PopupEventSO must inherit ScriptableObject.");
            }

            var validNodes = root.nodes
                .Where(n => !string.IsNullOrWhiteSpace(n.nodeId))
                .GroupBy(n => n.nodeId)
                .Select(g => g.First())
                .ToList();

            if (validNodes.Count != root.nodes.Count)
            {
                result.warnings.Add("Some nodes had empty or duplicated nodeId and were ignored.");
            }

            // 1) Create or update node assets first.
            foreach (var node in validNodes)
            {
                var assetPath = GetNodeAssetPath(outputFolder, node.nodeId);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                var created = false;

                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance(popupEventType);
                    AssetDatabase.CreateAsset(asset, assetPath);
                    created = true;
                }

                SetMemberValue(asset, "eventId", node.nodeId);
                SetMemberValue(asset, "id", node.nodeId);
                SetMemberValue(asset, "popupEventId", node.nodeId);

                // Optional compatibility fields. Missing fields are ignored.
                SetMemberValue(asset, "nodeType", node.nodeType);
                SetMemberValue(asset, "locationId", node.locationId);

                EditorUtility.SetDirty(asset);
                result.eventsById[node.nodeId] = asset;

                if (created)
                {
                    result.createdAssetPaths.Add(assetPath);
                }
                else
                {
                    result.updatedAssetPaths.Add(assetPath);
                }
            }

            // 2) Connect choices and nextEvent references.
            foreach (var node in validNodes)
            {
                if (!result.eventsById.TryGetValue(node.nodeId, out var asset) || asset == null)
                {
                    continue;
                }

                var generatedChoices = BuildChoicesForNode(asset, node, result);
                if (!SetMemberValue(asset, "choices", generatedChoices))
                {
                    result.warnings.Add($"Could not set choices on node: {node.nodeId}. Field/property 'choices' not found or incompatible.");
                }

                EditorUtility.SetDirty(asset);
            }

            if (!string.IsNullOrWhiteSpace(root.startNodeId) && result.eventsById.TryGetValue(root.startNodeId, out var startEvent))
            {
                result.StartEvent = startEvent;
                result.StartEventId = root.startNodeId;
            }
            else
            {
                result.StartEventId = root.startNodeId;
                result.warnings.Add($"Start node not found: {root.startNodeId}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return result;
        }

        private static object BuildChoicesForNode(ScriptableObject eventAsset, PopupEventNodeJson node, BuildResult result)
        {
            var choicesMemberType = GetMemberType(eventAsset.GetType(), "choices");
            if (choicesMemberType == null)
            {
                return null;
            }

            var choiceType = GetListElementType(choicesMemberType);
            if (choiceType == null)
            {
                result.warnings.Add($"choices field is not a supported List<T> or array type on node: {node.nodeId}");
                return null;
            }

            var jsonChoices = new List<PopupEventChoiceJson>();

            if (node.choices != null && node.choices.Count > 0)
            {
                jsonChoices.AddRange(node.choices.Where(c => !string.IsNullOrWhiteSpace(c.choiceId)));
            }
            else if (!string.IsNullOrWhiteSpace(node.nextNodeId))
            {
                // Choice-less next is still represented as a single generated choice.
                jsonChoices.Add(new PopupEventChoiceJson
                {
                    choiceId = $"{node.nodeId}.next",
                    nextNodeId = node.nextNodeId
                });
            }

            var choiceObjects = jsonChoices.Select(choiceJson =>
            {
                var choice = CreateChoiceInstance(choiceType);
                if (choice == null)
                {
                    result.warnings.Add($"Failed to create choice instance for node: {node.nodeId}");
                    return null;
                }

                SetMemberValue(choice, "choiceId", choiceJson.choiceId);
                SetMemberValue(choice, "id", choiceJson.choiceId);
                SetMemberValue(choice, "valueTag", choiceJson.valueTag);

                var nextEvent = ResolveNextEvent(choiceJson.nextNodeId, result, node.nodeId, choiceJson.choiceId);
                SetMemberValue(choice, "nextEvent", nextEvent);

                var completesEventSet = SetMemberValue(choice, "completesEvent", true);
                if (!completesEventSet)
                {
                    SetMemberValue(choice, "isCompleteEvent", true);
                }

                TrySetRewards(choice, choiceJson.rewards, result, node.nodeId, choiceJson.choiceId);

                return choice;
            }).Where(c => c != null).ToList();

            if (choicesMemberType.IsArray)
            {
                var array = Array.CreateInstance(choiceType, choiceObjects.Count);
                for (var i = 0; i < choiceObjects.Count; i++)
                {
                    array.SetValue(choiceObjects[i], i);
                }

                return array;
            }

            var listType = typeof(List<>).MakeGenericType(choiceType);
            var list = (System.Collections.IList)Activator.CreateInstance(listType);
            foreach (var choice in choiceObjects)
            {
                list.Add(choice);
            }

            return list;
        }

        private static ScriptableObject ResolveNextEvent(string nextNodeId, BuildResult result, string nodeId, string choiceId)
        {
            if (string.IsNullOrWhiteSpace(nextNodeId))
            {
                return null;
            }

            if (result.eventsById.TryGetValue(nextNodeId, out var nextEvent))
            {
                return nextEvent;
            }

            result.warnings.Add($"Missing next node. node={nodeId}, choice={choiceId}, nextNodeId={nextNodeId}");
            return null;
        }

        private static void TrySetRewards(object choice, List<PopupEventRewardJson> rewards, BuildResult result, string nodeId, string choiceId)
        {
            if (rewards == null || rewards.Count == 0)
            {
                SetMemberValue(choice, "rewards", null);
                return;
            }

            var rewardsMemberType = GetMemberType(choice.GetType(), "rewards");
            if (rewardsMemberType == null)
            {
                return;
            }

            var rewardType = GetListElementType(rewardsMemberType);
            if (rewardType == null)
            {
                result.warnings.Add($"rewards field is not a supported List<T> or array type. node={nodeId}, choice={choiceId}");
                return;
            }

            var rewardObjects = rewards.Select((rewardJson, rewardIndex) =>
            {
                var reward = CreateChoiceInstance(rewardType);
                if (reward == null)
                {
                    return null;
                }

                SetMemberValue(reward, "rewardType", rewardJson.rewardType);
                SetMemberValue(reward, "type", rewardJson.rewardType);
                SetMemberValue(reward, "rewardId", rewardJson.rewardId);
                SetMemberValue(reward, "id", rewardJson.rewardId);
                SetMemberValue(reward, "amount", rewardJson.amount);
                SetMemberValue(reward, "value", rewardJson.value != 0 ? rewardJson.value : rewardJson.amount);
                SetMemberValue(reward, "tag", rewardJson.tag);

                BattleSO embeddedBattle = BuildEmbeddedBattle(rewardJson);
                if (embeddedBattle != null)
                {
                    SetMemberValue(reward, "targetData", embeddedBattle);
                }

                return reward;
            }).Where(r => r != null).ToList();

            object finalRewards;
            if (rewardsMemberType.IsArray)
            {
                var array = Array.CreateInstance(rewardType, rewardObjects.Count);
                for (var i = 0; i < rewardObjects.Count; i++)
                {
                    array.SetValue(rewardObjects[i], i);
                }

                finalRewards = array;
            }
            else
            {
                var listType = typeof(List<>).MakeGenericType(rewardType);
                var list = (System.Collections.IList)Activator.CreateInstance(listType);
                foreach (var reward in rewardObjects)
                {
                    list.Add(reward);
                }

                finalRewards = list;
            }

            SetMemberValue(choice, "rewards", finalRewards);
        }


        private static BattleSO BuildEmbeddedBattle(PopupEventRewardJson rewardJson)
        {
            if (rewardJson == null || rewardJson.battle == null)
            {
                return null;
            }

            return BattleJsonGenerator.GenerateFromData(rewardJson.battle);
        }

        private static object CreateChoiceInstance(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch
            {
                return null;
            }
        }

        private static Type FindTypeByName(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type found;
                try
                {
                    found = assembly.GetTypes().FirstOrDefault(t => t.Name == typeName || t.FullName == typeName);
                }
                catch (ReflectionTypeLoadException e)
                {
                    found = e.Types?.FirstOrDefault(t => t != null && (t.Name == typeName || t.FullName == typeName));
                }

                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static bool SetMemberValue(object target, string memberName, object value)
        {
            if (target == null || string.IsNullOrWhiteSpace(memberName))
            {
                return false;
            }

            var type = target.GetType();
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var field = type.GetField(memberName, flags);
            if (field != null)
            {
                if (TryConvertValue(value, field.FieldType, out var converted))
                {
                    field.SetValue(target, converted);
                    return true;
                }
            }

            var property = type.GetProperty(memberName, flags);
            if (property != null && property.CanWrite)
            {
                if (TryConvertValue(value, property.PropertyType, out var converted))
                {
                    property.SetValue(target, converted);
                    return true;
                }
            }

            return false;
        }

        private static Type GetMemberType(Type ownerType, string memberName)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var field = ownerType.GetField(memberName, flags);
            if (field != null)
            {
                return field.FieldType;
            }

            var property = ownerType.GetProperty(memberName, flags);
            if (property != null)
            {
                return property.PropertyType;
            }

            return null;
        }

        private static Type GetListElementType(Type type)
        {
            if (type == null)
            {
                return null;
            }

            if (type.IsArray)
            {
                return type.GetElementType();
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                return type.GetGenericArguments()[0];
            }

            return null;
        }

        private static bool TryConvertValue(object value, Type targetType, out object converted)
        {
            converted = null;

            if (targetType == null)
            {
                return false;
            }

            if (value == null)
            {
                if (!targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null)
                {
                    converted = null;
                    return true;
                }

                return false;
            }

            if (targetType.IsInstanceOfType(value))
            {
                converted = value;
                return true;
            }

            if (targetType.IsEnum && value is string enumText)
            {
                return Enum.TryParse(targetType, enumText, true, out converted);
            }

            try
            {
                converted = Convert.ChangeType(value, targetType);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string GetNodeAssetPath(string outputFolder, string nodeId)
        {
            var safeFileName = SanitizeFileName(nodeId) + ".asset";
            return CombineAssetPath(outputFolder, safeFileName);
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "popup_event";
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            var chars = value.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray();
            return new string(chars).Replace(':', '_').Replace('/', '_').Replace('\\', '_');
        }

        private static string CombineAssetPath(string folder, string fileName)
        {
            return (folder.TrimEnd('/', '\\') + "/" + fileName).Replace('\\', '/');
        }

        private static void EnsureFolder(string folderPath)
        {
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
