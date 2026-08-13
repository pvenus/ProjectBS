using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

using Battle;
using Character;
using Shop;
using Shrine;
using Stage;

namespace ResourceTools.Stage
{
    /// <summary>
    /// Builds PopupEventSO node assets from a story JSON file.
    ///
    /// Intended usage from another editor generator:
    ///     PopupEventBuilder.BuildFromJsonPath(jsonPath, outputFolder);
    ///
    /// Runtime SO rule:
    /// - JSON keeps nodeId / choice execution config (nextPopupId).
    /// - Generated SO keeps flow only in ChoiceExecutionConfig.
    /// - Text is not stored in SO. eventId / choiceId are used as StringManager keys.
    /// </summary>
    public static class PopupEventBuilder
    {
        private const string DefaultOutputFolder = "Assets/Resources/stage/popup_events";
        private const string DefaultMainImageFolder = "Assets/Resources/stage_new/popup_png";
        private const string MainImageSuffix = ".main";

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
            public string episodeId;
            public string chapterId;
            public int actNumber;
            public int episodeNumber;
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
            public List<PopupEventChoiceJson> choices;
        }

        [Serializable]
        private sealed class PopupEventChoiceJson
        {
            public string choiceId;
            public string textKo;
            public string valueTag;
            public ChoiceExecutionConfigJson executionConfig;
            public List<PopupEventChoiceConditionJson> visibleConditions;
            public List<PopupEventRewardJson> rewards;
        }

        [Serializable]
        private sealed class ChoiceExecutionConfigJson
        {
            public string type;
            public NextEventExecutionJson nextEvent;
            public BattleExecutionJson battle;
            public ShopExecutionJson shop;
            public ShrineExecutionJson shrine;
            public CompleteEventExecutionJson completeEvent;
        }

        [Serializable]
        private sealed class NextEventExecutionJson
        {
            public string nextPopupId;
        }

        [Serializable]
        private sealed class BattleExecutionJson
        {
            public string battleId;
        }

        [Serializable]
        private sealed class ShopExecutionJson
        {
            public string shopType;
            public List<string> poolIds;
            public int itemCount;
        }

        [Serializable]
        private sealed class ShrineExecutionJson
        {
            public string configId;
            public string godId;
        }

        [Serializable]
        private sealed class CompleteEventExecutionJson
        {
        }

        [Serializable]
        private sealed class PopupEventChoiceConditionJson
        {
            public string conditionType;
            public string targetId;
            public int value;
            public string tag;
            public bool invert;
        }

        [Serializable]
        private sealed class PopupEventRewardJson
        {
            public string rewardType;
            public string rewardId;
            public string targetId;
            public int amount;
            public int value;
            public string tag;
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

            EnsureFolderExists(outputFolder);

            string jsonText = File.ReadAllText(jsonPath);
            if (string.IsNullOrWhiteSpace(jsonText))
            {
                result.warnings.Add($"Json file is empty: {jsonPath}");
                return result;
            }

            PopupEventJsonRoot root = JsonUtility.FromJson<PopupEventJsonRoot>(jsonText);
            if (root == null || root.nodes == null || root.nodes.Count == 0)
            {
                result.warnings.Add($"No nodes found in json: {jsonPath}");
                return result;
            }

            Type eventType = FindType("Stage.PopupEventSO") ?? FindType("PopupEventSO");
            if (eventType == null)
            {
                throw new InvalidOperationException("Could not find PopupEventSO type in loaded assemblies.");
            }

            foreach (var node in root.nodes)
            {
                if (node == null || string.IsNullOrWhiteSpace(node.nodeId))
                {
                    continue;
                }

                string assetPath = $"{outputFolder}/{node.nodeId}.asset";
                ScriptableObject eventAsset = AssetDatabase.LoadAssetAtPath(assetPath, eventType) as ScriptableObject;

                bool isNew = false;
                if (eventAsset == null)
                {
                    eventAsset = ScriptableObject.CreateInstance(eventType);
                    AssetDatabase.CreateAsset(eventAsset, assetPath);
                    isNew = true;
                }

                SetMemberValue(eventAsset, "eventId", node.nodeId);
                SetMemberValue(eventAsset, "id", node.nodeId);

                TryAssignMainImageSprite(eventAsset, node.nodeId);

                EditorUtility.SetDirty(eventAsset);

                result.eventsById[node.nodeId] = eventAsset;

                if (isNew)
                {
                    result.createdAssetPaths.Add(assetPath);
                }
                else
                {
                    result.updatedAssetPaths.Add(assetPath);
                }
            }

            foreach (var node in root.nodes)
            {
                if (node == null || string.IsNullOrWhiteSpace(node.nodeId))
                {
                    continue;
                }

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

                TrySetVisibleConditions(
                    choice,
                    choiceJson.visibleConditions,
                    result,
                    node.nodeId,
                    choiceJson.choiceId);

                ChoiceExecutionConfig executionConfig =
                    BuildChoiceExecutionConfig(
                        choiceJson,
                        result,
                        node.nodeId);
                SetMemberValue(
                    choice,
                    "executionConfig",
                    executionConfig);

                TrySetRewards(
                    choice,
                    choiceJson.rewards,
                    result,
                    node.nodeId,
                    choiceJson.choiceId);

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

            Type listType = typeof(List<>).MakeGenericType(choiceType);
            System.Collections.IList list = (System.Collections.IList)Activator.CreateInstance(listType);
            foreach (var item in choiceObjects)
            {
                list.Add(item);
            }

            return list;
        }

        private static ChoiceExecutionConfig BuildChoiceExecutionConfig(
            PopupEventChoiceJson choiceJson,
            BuildResult result,
            string nodeId)
        {
            string choiceId = choiceJson?.choiceId;
            ChoiceExecutionConfigJson configJson = choiceJson?.executionConfig;
            if (configJson == null || string.IsNullOrWhiteSpace(configJson.type))
            {
                throw CreateChoiceImportException(
                    "EXECUTION_TYPE_REQUIRED",
                    nodeId,
                    choiceId,
                    "Choice executionConfig.type is required.");
            }

            if (!Enum.TryParse(configJson.type, true, out ChoiceExecutionType executionType))
            {
                throw CreateChoiceImportException(
                    "INVALID_EXECUTION_TYPE",
                    nodeId,
                    choiceId,
                    $"Unsupported ChoiceExecutionType: '{configJson.type}'");
            }

            ValidateExecutionConfigPayload(
                configJson,
                executionType,
                nodeId,
                choiceId);

            ChoiceExecutionData executionData = executionType switch
            {
                ChoiceExecutionType.NextEvent =>
                    BuildNextEventExecutionData(
                        configJson.nextEvent,
                        result,
                        nodeId,
                        choiceId),
                ChoiceExecutionType.Battle =>
                    BuildBattleExecutionData(
                        configJson.battle,
                        result,
                        nodeId,
                        choiceId),
                ChoiceExecutionType.Shop =>
                    BuildShopExecutionData(
                        configJson.shop,
                        nodeId,
                        choiceId),
                ChoiceExecutionType.Shrine =>
                    BuildShrineExecutionData(
                        configJson.shrine,
                        result,
                        nodeId,
                        choiceId),
                ChoiceExecutionType.CompleteEvent =>
                    BuildCompleteEventExecutionData(),
                _ => throw CreateChoiceImportException(
                    "UNSUPPORTED_EXECUTION_TYPE",
                    nodeId,
                    choiceId,
                    $"Unhandled ChoiceExecutionType: '{executionType}'")
            };

            var config = ChoiceExecutionDataFactory.CreateConfig(executionType);
            config.data = executionData;
            return config;
        }

        private static void ValidateExecutionConfigPayload(
            ChoiceExecutionConfigJson json,
            ChoiceExecutionType executionType,
            string nodeId,
            string choiceId)
        {
            bool hasNextEventPayload =
                !string.IsNullOrWhiteSpace(
                    json.nextEvent?.nextPopupId);
            bool hasBattlePayload =
                !string.IsNullOrWhiteSpace(
                    json.battle?.battleId);
            bool hasShopPayload =
                !string.IsNullOrWhiteSpace(
                    json.shop?.shopType)
                || json.shop?.poolIds?.Count > 0
                || json.shop?.itemCount != 0;
            bool hasShrinePayload =
                !string.IsNullOrWhiteSpace(
                    json.shrine?.configId)
                || !string.IsNullOrWhiteSpace(
                    json.shrine?.godId);

            bool payloadMatches = executionType switch
            {
                ChoiceExecutionType.NextEvent =>
                    !hasBattlePayload
                    && !hasShopPayload
                    && !hasShrinePayload,
                ChoiceExecutionType.Battle =>
                    !hasNextEventPayload
                    && !hasShopPayload
                    && !hasShrinePayload,
                ChoiceExecutionType.Shop =>
                    !hasNextEventPayload
                    && !hasBattlePayload
                    && !hasShrinePayload,
                ChoiceExecutionType.Shrine =>
                    !hasNextEventPayload
                    && !hasBattlePayload
                    && !hasShopPayload,
                ChoiceExecutionType.CompleteEvent =>
                    !hasNextEventPayload
                    && !hasBattlePayload
                    && !hasShopPayload
                    && !hasShrinePayload,
                _ => false
            };

            if (!payloadMatches)
            {
                throw CreateChoiceImportException(
                    "EXECUTION_PAYLOAD_MISMATCH",
                    nodeId,
                    choiceId,
                    $"{executionType} has a missing or conflicting payload.");
            }
        }

        private static ChoiceExecutionData BuildNextEventExecutionData(
            NextEventExecutionJson json,
            BuildResult result,
            string nodeId,
            string choiceId)
        {
            var data = new NextEventExecutionData();
            PopulateNextEvent(
                data,
                json,
                result,
                nodeId,
                choiceId);
            return data;
        }

        private static void PopulateNextEvent(
            NextEventExecutionData data,
            NextEventExecutionJson json,
            BuildResult result,
            string nodeId,
            string choiceId)
        {
            if (string.IsNullOrWhiteSpace(json?.nextPopupId))
            {
                throw CreateChoiceImportException(
                    "NEXT_EVENT_ID_REQUIRED",
                    nodeId,
                    choiceId,
                    "nextEvent.nextPopupId is required.");
            }

            ScriptableObject nextEvent = ResolveNextEvent(
                json.nextPopupId,
                result,
                nodeId,
                choiceId);

            if (nextEvent is not PopupEventSO popupEvent)
            {
                throw CreateChoiceImportException(
                    "NEXT_EVENT_NOT_FOUND",
                    nodeId,
                    choiceId,
                    $"PopupEventSO '{json.nextPopupId}' was not found.");
            }

            data.nextEvent = popupEvent;
        }

        private static ChoiceExecutionData BuildBattleExecutionData(
            BattleExecutionJson json,
            BuildResult result,
            string nodeId,
            string choiceId)
        {
            var data = new BattleExecutionData();
            PopulateBattle(
                data,
                json,
                result,
                nodeId,
                choiceId);
            return data;
        }

        private static void PopulateBattle(
            BattleExecutionData data,
            BattleExecutionJson json,
            BuildResult result,
            string nodeId,
            string choiceId)
        {
            if (string.IsNullOrWhiteSpace(json?.battleId))
            {
                throw CreateChoiceImportException(
                    "BATTLE_ID_REQUIRED",
                    nodeId,
                    choiceId,
                    "battle.battleId is required.");
            }

            data.battle =
                StageChoiceExecutionAssetResolver.ResolveBattle(
                    json.battleId);
        }

        private static ChoiceExecutionData BuildShopExecutionData(
            ShopExecutionJson json,
            string nodeId,
            string choiceId)
        {
            var data = new ShopExecutionData();
            PopulateShop(
                data,
                json,
                nodeId,
                choiceId);
            return data;
        }

        private static void PopulateShop(
            ShopExecutionData data,
            ShopExecutionJson json,
            string nodeId,
            string choiceId)
        {
            if (json == null || string.IsNullOrWhiteSpace(json.shopType))
            {
                throw CreateChoiceImportException(
                    "SHOP_TYPE_REQUIRED",
                    nodeId,
                    choiceId,
                    "shop.shopType is required.");
            }

            if (!Enum.TryParse(json.shopType, true, out ShopType shopType))
            {
                throw CreateChoiceImportException(
                    "INVALID_SHOP_TYPE",
                    nodeId,
                    choiceId,
                    $"Unsupported ShopType: '{json.shopType}'");
            }

            data.shopType = shopType;
            data.itemCount = json.itemCount;
            data.pools =
                StageChoiceExecutionAssetResolver.ResolveShopPools(
                    json.poolIds);
        }

        private static ChoiceExecutionData BuildShrineExecutionData(
            ShrineExecutionJson json,
            BuildResult result,
            string nodeId,
            string choiceId)
        {
            var data = new ShrineExecutionData();
            PopulateShrine(
                data,
                json,
                result,
                nodeId,
                choiceId);
            return data;
        }

        private static void PopulateShrine(
            ShrineExecutionData data,
            ShrineExecutionJson json,
            BuildResult result,
            string nodeId,
            string choiceId)
        {
            if (string.IsNullOrWhiteSpace(json?.configId))
            {
                throw CreateChoiceImportException(
                    "SHRINE_CONFIG_ID_REQUIRED",
                    nodeId,
                    choiceId,
                    "shrine.configId is required.");
            }

            if (string.IsNullOrWhiteSpace(json.godId))
            {
                throw CreateChoiceImportException(
                    "SHRINE_GOD_ID_REQUIRED",
                    nodeId,
                    choiceId,
                    "shrine.godId is required.");
            }

            data.config =
                StageChoiceExecutionAssetResolver.ResolveShrineConfig(
                    json.configId);
            data.god =
                StageChoiceExecutionAssetResolver.ResolveShrineGod(
                    json.godId);
        }

        private static ChoiceExecutionData BuildCompleteEventExecutionData()
        {
            return new CompleteEventExecutionData();
        }

        private static Exception CreateChoiceImportException(
            string errorCode,
            string nodeId,
            string choiceId,
            string message)
        {
            string formattedNodeId = string.IsNullOrWhiteSpace(nodeId)
                ? "<unknown>"
                : nodeId;
            string formattedChoiceId = string.IsNullOrWhiteSpace(choiceId)
                ? "<none>"
                : choiceId;

            return new InvalidOperationException(
                $"[{errorCode}] Node: {formattedNodeId}, Choice: {formattedChoiceId} - {message}");
        }

        private static void TrySetVisibleConditions(
            object choice,
            List<PopupEventChoiceConditionJson> conditions,
            BuildResult result,
            string nodeId,
            string choiceId)
        {
            if (conditions == null || conditions.Count == 0)
            {
                SetMemberValue(choice, "visibleConditions", null);
                return;
            }

            var conditionsMemberType = GetMemberType(choice.GetType(), "visibleConditions");
            if (conditionsMemberType == null)
            {
                return;
            }

            var conditionType = GetListElementType(conditionsMemberType);
            if (conditionType == null)
            {
                result.warnings.Add($"visibleConditions field is not a supported List<T> or array type. node={nodeId}, choice={choiceId}");
                return;
            }

            List<object> conditionObjects = conditions.Select(conditionJson =>
            {
                object condition = CreateChoiceInstance(conditionType);
                if (condition == null)
                {
                    return null;
                }

                SetMemberValue(condition, "conditionType", conditionJson.conditionType);
                SetMemberValue(condition, "targetId", conditionJson.targetId);
                SetMemberValue(condition, "value", conditionJson.value);
                SetMemberValue(condition, "tag", conditionJson.tag);
                SetMemberValue(condition, "invert", conditionJson.invert);

                return condition;
            }).Where(c => c != null).ToList();

            object finalConditions;
            if (conditionsMemberType.IsArray)
            {
                Array array = Array.CreateInstance(conditionType, conditionObjects.Count);
                for (int i = 0; i < conditionObjects.Count; i++)
                {
                    array.SetValue(conditionObjects[i], i);
                }

                finalConditions = array;
            }
            else
            {
                Type listType = typeof(List<>).MakeGenericType(conditionType);
                System.Collections.IList list = (System.Collections.IList)Activator.CreateInstance(listType);
                foreach (object condition in conditionObjects)
                {
                    list.Add(condition);
                }

                finalConditions = list;
            }

            SetMemberValue(choice, "visibleConditions", finalConditions);
        }

        private static ScriptableObject ResolveNextEvent(string nextPopupId, BuildResult result, string nodeId, string choiceId)
        {
            if (string.IsNullOrWhiteSpace(nextPopupId))
            {
                return null;
            }

            if (result.eventsById.TryGetValue(nextPopupId, out var nextEvent))
            {
                return nextEvent;
            }

            result.warnings.Add($"Missing next popup event. node={nodeId}, choice={choiceId}, nextPopupId={nextPopupId}");
            return null;
        }

        private static void TrySetRewards(object choice, List<PopupEventRewardJson> rewards, BuildResult result, string nodeId, string choiceId)
        {
            List<PopupEventRewardJson> payoutRewards = rewards?
                .Where(reward =>
                    !IsLegacyExecutionRewardType(reward?.rewardType))
                .ToList();

            int ignoredExecutionRewardCount =
                (rewards?.Count ?? 0) - (payoutRewards?.Count ?? 0);
            if (ignoredExecutionRewardCount > 0)
            {
                result.warnings.Add(
                    $"Ignored {ignoredExecutionRewardCount} legacy execution reward(s). "
                    + $"ChoiceExecutionConfig owns the transition. node={nodeId}, choice={choiceId}");
            }

            if (payoutRewards == null || payoutRewards.Count == 0)
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

            var rewardObjects = payoutRewards.Select((rewardJson, rewardIndex) =>
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
                SetMemberValue(reward, "targetId", rewardJson.targetId);
                SetMemberValue(reward, "amount", rewardJson.amount);
                SetMemberValue(reward, "value", rewardJson.value != 0 ? rewardJson.value : rewardJson.amount);
                SetMemberValue(reward, "tag", rewardJson.tag);

                ScriptableObject targetData = BuildRewardTargetData(rewardJson, result, nodeId, choiceId);
                if (targetData != null)
                {
                    SetMemberValue(reward, "targetData", targetData);
                }

                return reward;
            }).Where(r => r != null).ToList();

            if (rewardsMemberType.IsArray)
            {
                Array array = Array.CreateInstance(rewardType, rewardObjects.Count);
                for (int i = 0; i < rewardObjects.Count; i++)
                {
                    array.SetValue(rewardObjects[i], i);
                }

                SetMemberValue(choice, "rewards", array);
                return;
            }

            Type listType = typeof(List<>).MakeGenericType(rewardType);
            System.Collections.IList list = (System.Collections.IList)Activator.CreateInstance(listType);
            foreach (object reward in rewardObjects)
            {
                list.Add(reward);
            }

            SetMemberValue(choice, "rewards", list);
        }

        private static bool IsLegacyExecutionRewardType(
            string rewardType)
        {
            return rewardType != null
                && (rewardType.Equals(
                        "SpecialBattle",
                        StringComparison.OrdinalIgnoreCase)
                    || rewardType.Equals(
                        "BossBattle",
                        StringComparison.OrdinalIgnoreCase)
                    || rewardType.Equals(
                        "NextEvent",
                        StringComparison.OrdinalIgnoreCase));
        }

        private static ScriptableObject BuildRewardTargetData(
            PopupEventRewardJson rewardJson,
            BuildResult result,
            string nodeId,
            string choiceId)
        {
            if (rewardJson == null || string.IsNullOrWhiteSpace(rewardJson.rewardType))
            {
                return null;
            }

            string rewardType = rewardJson.rewardType.Trim();
            string targetId = rewardJson.targetId;
            if (string.IsNullOrWhiteSpace(targetId))
            {
                targetId = rewardJson.rewardId;
            }

            if (string.IsNullOrWhiteSpace(targetId))
            {
                return null;
            }

            if (rewardType.Equals("battle", StringComparison.OrdinalIgnoreCase) ||
                rewardType.Equals("SpecialBattle", StringComparison.OrdinalIgnoreCase))
            {
                return ResolveBattleData(targetId, result, nodeId, choiceId);
            }

            if (rewardType.Equals("party_candidate", StringComparison.OrdinalIgnoreCase) ||
                rewardType.Equals("partyMember", StringComparison.OrdinalIgnoreCase) ||
                rewardType.Equals("Character", StringComparison.OrdinalIgnoreCase))
            {
                return ResolveCharacterData(targetId, result, nodeId, choiceId);
            }

            if (rewardType.Equals("shrine_config", StringComparison.OrdinalIgnoreCase) ||
                rewardType.Equals("shrine", StringComparison.OrdinalIgnoreCase))
            {
                return ResolveShrineConfig(targetId, result, nodeId, choiceId);
            }

            return null;
        }

        private static ScriptableObject ResolveBattleData(
            string battleId,
            BuildResult result,
            string nodeId,
            string choiceId)
        {
            Type battleDataType = FindType("Battle.BattleDataSO") ?? FindType("BattleDataSO");
            if (battleDataType == null)
            {
                return null;
            }

            string[] guids = AssetDatabase.FindAssets($"t:{battleDataType.Name}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath(path, battleDataType) as ScriptableObject;
                if (asset == null)
                {
                    continue;
                }

                string assetBattleId = GetMemberValue<string>(asset, "battleId")
                                     ?? GetMemberValue<string>(asset, "id")
                                     ?? asset.name;
                if (string.Equals(assetBattleId, battleId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(asset.name, battleId, StringComparison.OrdinalIgnoreCase))
                {
                    return asset;
                }
            }

            result.warnings.Add($"Missing battle asset. node={nodeId}, choice={choiceId}, battleId={battleId}");
            return null;
        }

        private static ScriptableObject ResolveCharacterData(
            string characterId,
            BuildResult result,
            string nodeId,
            string choiceId)
        {
            Type characterDataType = FindType("Character.CharacterDataSO") ?? FindType("CharacterDataSO");
            if (characterDataType == null)
            {
                return null;
            }

            string[] guids = AssetDatabase.FindAssets($"t:{characterDataType.Name}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath(path, characterDataType) as ScriptableObject;
                if (asset == null)
                {
                    continue;
                }

                string assetCharId = GetMemberValue<string>(asset, "characterId")
                                   ?? GetMemberValue<string>(asset, "id")
                                   ?? asset.name;
                if (string.Equals(assetCharId, characterId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(asset.name, characterId, StringComparison.OrdinalIgnoreCase))
                {
                    return asset;
                }
            }

            result.warnings.Add($"Missing character asset. node={nodeId}, choice={choiceId}, characterId={characterId}");
            return null;
        }

        private static ScriptableObject ResolveShrineConfig(
            string configId,
            BuildResult result,
            string nodeId,
            string choiceId)
        {
            Type shrineConfigType = FindType("Shrine.ShrineConfigSO") ?? FindType("ShrineConfigSO");
            if (shrineConfigType == null)
            {
                return null;
            }

            string[] guids = AssetDatabase.FindAssets($"t:{shrineConfigType.Name}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath(path, shrineConfigType) as ScriptableObject;
                if (asset == null)
                {
                    continue;
                }

                string assetConfigId = GetMemberValue<string>(asset, "configId")
                                     ?? GetMemberValue<string>(asset, "id")
                                     ?? asset.name;
                if (string.Equals(assetConfigId, configId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(asset.name, configId, StringComparison.OrdinalIgnoreCase))
                {
                    return asset;
                }
            }

            result.warnings.Add($"Missing shrine config asset. node={nodeId}, choice={choiceId}, configId={configId}");
            return null;
        }

        private static void TryAssignMainImageSprite(ScriptableObject eventAsset, string nodeId)
        {
            var mainImageMemberType = GetMemberType(eventAsset.GetType(), "mainImage");
            if (mainImageMemberType == null || !typeof(Sprite).IsAssignableFrom(mainImageMemberType))
            {
                return;
            }

            string imagePath = $"{DefaultMainImageFolder}/{nodeId}{MainImageSuffix}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
            if (sprite != null)
            {
                SetMemberValue(eventAsset, "mainImage", sprite);
            }
        }

        private static object CreateChoiceInstance(Type choiceType)
        {
            if (choiceType == null)
            {
                return null;
            }

            if (typeof(ScriptableObject).IsAssignableFrom(choiceType))
            {
                return ScriptableObject.CreateInstance(choiceType);
            }

            return Activator.CreateInstance(choiceType);
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

            foreach (Type iface in type.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return iface.GetGenericArguments()[0];
                }
            }

            return null;
        }

        private static MemberInfo GetMemberInfo(Type type, string name)
        {
            if (type == null || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase;
            FieldInfo field = type.GetField(name, flags);
            if (field != null)
            {
                return field;
            }

            PropertyInfo prop = type.GetProperty(name, flags);
            if (prop != null && prop.CanWrite)
            {
                return prop;
            }

            return null;
        }

        private static Type GetMemberType(Type type, string name)
        {
            MemberInfo member = GetMemberInfo(type, name);
            if (member is FieldInfo field)
            {
                return field.FieldType;
            }

            if (member is PropertyInfo prop)
            {
                return prop.PropertyType;
            }

            return null;
        }

        private static bool SetMemberValue(object target, string name, object value)
        {
            if (target == null || string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            MemberInfo member = GetMemberInfo(target.GetType(), name);
            if (member is FieldInfo field)
            {
                field.SetValue(target, ConvertValue(value, field.FieldType));
                return true;
            }

            if (member is PropertyInfo prop && prop.CanWrite)
            {
                prop.SetValue(target, ConvertValue(value, prop.PropertyType));
                return true;
            }

            return false;
        }

        private static T GetMemberValue<T>(object target, string name)
        {
            if (target == null || string.IsNullOrWhiteSpace(name))
            {
                return default;
            }

            MemberInfo member = GetMemberInfo(target.GetType(), name);
            if (member is FieldInfo field)
            {
                object raw = field.GetValue(target);
                return raw is T typed ? typed : default;
            }

            if (member is PropertyInfo prop && prop.CanRead)
            {
                object raw = prop.GetValue(target);
                return raw is T typed ? typed : default;
            }

            return default;
        }

        private static object ConvertValue(object value, Type targetType)
        {
            if (value == null)
            {
                return null;
            }

            Type valueType = value.GetType();
            if (targetType.IsAssignableFrom(valueType))
            {
                return value;
            }

            if (targetType.IsEnum && value is string strEnum)
            {
                return Enum.Parse(targetType, strEnum, true);
            }

            return Convert.ChangeType(value, targetType);
        }

        private static Type FindType(string typeName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetTypes().FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static void EnsureFolderExists(string folderPath)
        {
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
