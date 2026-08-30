using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Battle;
using ResourceTools.Stage;
using Shop;
using Shrine;
using Stage;
using UnityEditor;
using UnityEngine;

namespace StageEditor
{
    /// <summary>
    /// Choice execution JSON 계약과 SO ID 해석을 실제 AssetDatabase 저장/재로드로 검증한다.
    /// 모든 테스트 에셋과 JSON은 finally에서 제거한다.
    /// </summary>
    public static class StageChoiceExecutionBuilderSelfTest
    {
        private const string TestAssetRoot =
            "Assets/__StageChoiceExecutionBuilderSelfTest";
        private const string ReferenceFolder =
            TestAssetRoot + "/References";
        private const string OutputFolder =
            TestAssetRoot + "/Output";

        private const string BattleId =
            "selftest.choice.execution.battle";
        private const string PoolId =
            "selftest.choice.execution.pool";
        private const string ShrineConfigId =
            "selftest.choice.execution.shrine.config";
        private const string ShrineGodId =
            "selftest.choice.execution.shrine.god";
        private const string BattleEventId =
            "selftest.choice.execution.event";
        private const string BattleNodeId =
            "selftest.choice.execution.node";
        private const string BattleReservationId =
            "selftest.choice.execution.reservation";
        private const string BattleResultId =
            "selftest.choice.execution.result";

        [MenuItem(
            "Tools/Stage/Choice Execution Tests/Run Builder Fixtures")]
        public static void RunFromMenu()
        {
            RunAll();
            Debug.Log("Choice execution builder fixture tests passed.");
        }

        public static void RunAllBatch()
        {
            RunAll();
        }

        public static void RunBattleTupleBatch()
        {
            string tempFolder = Path.Combine(
                Path.GetTempPath(),
                "ProjectBS_BattleTupleBuilderSelfTest");
            CleanupAssets();
            CleanupDirectory(tempFolder);
            try
            {
                Directory.CreateDirectory(tempFolder);
                CreateReferenceAssets();
                string jsonPath = WriteFixture(
                    tempFolder,
                    "battle-tuple.json",
                    CreateSingleChoiceJson(
                        "\"executionConfig\":{\"type\":\"Battle\","
                        + "\"battle\":{\"battleId\":\"" + BattleId
                        + "\",\"eventId\":\"" + BattleEventId
                        + "\",\"nodeId\":\"" + BattleNodeId
                        + "\",\"sourcePopupId\":\"" + BattleNodeId
                        + "\",\"reservationId\":\"" + BattleReservationId
                        + "\",\"choiceId\":\"fixture.choice\""
                        + ",\"expectedVictoryResultId\":\"" + BattleResultId
                        + "\"}}"));
                PopupEventBuilder.BuildResult result =
                    PopupEventBuilder.BuildFromJsonPath(
                        jsonPath,
                        OutputFolder + "/BattleTuple");
                var popup = (PopupEventSO)result.StartEvent;
                var data = (BattleExecutionData)popup.choices[0].executionConfig.data;
                Ensure(data.battle?.BattleId == BattleId
                    && data.eventId == BattleEventId
                    && data.nodeId == BattleNodeId
                    && data.sourcePopupId == BattleNodeId
                    && data.reservationId == BattleReservationId
                    && data.choiceId == "fixture.choice"
                    && data.expectedVictoryResultId == BattleResultId,
                    "Battle completion identity tuple did not round-trip.");
            }
            finally
            {
                CleanupAssets();
                CleanupDirectory(tempFolder);
                AssetDatabase.Refresh();
            }
        }

        public static void RunEvent34NextEventBatch()
        {
            string tempFolder = Path.Combine(
                Path.GetTempPath(), "ProjectBS_Event34NextEventBuilderSelfTest");
            CleanupAssets();
            CleanupDirectory(tempFolder);
            try
            {
                Directory.CreateDirectory(tempFolder);
                string parentEvent = "event.act1.random_event.34.half_vein_map";
                string parentNode = "node.act1.random_event.34.half_vein_map.intro";
                string childEvent = parentEvent + ".followup.unstable_vein";
                string childNode = "node.act1.random_event.34.half_vein_map.followup.unstable_vein.intro";
                string typed = "{\"startNodeId\":\"" + parentNode + "\",\"nodes\":["
                    + "{\"nodeId\":\"" + parentNode + "\",\"choices\":[{\"choiceId\":\"choice.parent\","
                    + "\"executionConfig\":{\"type\":\"NextEvent\",\"nextEvent\":{"
                    + "\"parentEventId\":\"" + parentEvent + "\",\"parentNodeId\":\"" + parentNode + "\","
                    + "\"parentChoiceId\":\"choice.parent\",\"parentResultId\":\"result.parent\","
                    + "\"parentReservationId\":\"reservation.parent\",\"childEventId\":\"" + childEvent + "\","
                    + "\"childNodeId\":\"" + childNode + "\",\"childReservationId\":\"reservation.child\"}}}]},"
                    + "{\"nodeId\":\"" + childNode + "\",\"choices\":[]}]}";
                PopupEventBuilder.BuildResult result = PopupEventBuilder.BuildFromJsonPath(
                    WriteFixture(tempFolder, "event34.json", typed), OutputFolder + "/Event34");
                var popup = (PopupEventSO)result.StartEvent;
                var data = (NextEventExecutionData)popup.choices[0].executionConfig.data;
                Ensure(data.nextEvent?.eventId == childNode
                    && data.parentEventId == parentEvent
                    && data.parentNodeId == parentNode
                    && data.parentChoiceId == "choice.parent"
                    && data.parentResultId == "result.parent"
                    && data.parentReservationId == "reservation.parent"
                    && data.childEventId == childEvent
                    && data.childNodeId == childNode
                    && data.childReservationId == "reservation.child",
                    "Event34 typed NextEvent identity did not round-trip.");
                Ensure(ChoiceExecutionConfigValidator.Validate(
                    popup.choices[0].executionConfig).Count == 0,
                    "Event34 typed NextEvent identity failed runtime validation.");
            }
            finally
            {
                CleanupAssets();
                CleanupDirectory(tempFolder);
                AssetDatabase.Refresh();
            }
        }

        public static void RunAll()
        {
            string tempFolder = Path.Combine(
                Path.GetTempPath(),
                "ProjectBS_ChoiceExecutionBuilderSelfTest");

            CleanupAssets();
            CleanupDirectory(tempFolder);

            try
            {
                Directory.CreateDirectory(tempFolder);
                CreateReferenceAssets();
                VerifyNormalFixtures(tempFolder);
                VerifyErrorFixtures(tempFolder);
            }
            finally
            {
                CleanupAssets();
                CleanupDirectory(tempFolder);
                AssetDatabase.Refresh();
            }
        }


        private static void CreateReferenceAssets()
        {
            EnsureAssetFolder(ReferenceFolder);

            BattleSO battle = ScriptableObject.CreateInstance<BattleSO>();
            battle.battleId = BattleId;
            AssetDatabase.CreateAsset(
                battle,
                ReferenceFolder + "/Battle.asset");

            ShopProductSO product =
                ScriptableObject.CreateInstance<ShopProductSO>();
            AssetDatabase.CreateAsset(
                product,
                ReferenceFolder + "/Product.asset");

            ShopItemPoolSO pool =
                ScriptableObject.CreateInstance<ShopItemPoolSO>();
            pool.poolId = PoolId;
            pool.products.Add(product);
            AssetDatabase.CreateAsset(
                pool,
                ReferenceFolder + "/Pool.asset");

            ShrineGodSO god =
                ScriptableObject.CreateInstance<ShrineGodSO>();
            SetSerializedString(god, "godId", ShrineGodId);
            SetSerializedInt(
                god,
                "godType",
                (int)ShrineGodType.Life);
            AssetDatabase.CreateAsset(
                god,
                ReferenceFolder + "/God.asset");

            ShrineConfigSO shrineConfig =
                ScriptableObject.CreateInstance<ShrineConfigSO>();
            SetSerializedString(
                shrineConfig,
                "configId",
                ShrineConfigId);
            SetSerializedObjectList(
                shrineConfig,
                "gods",
                god);
            AssetDatabase.CreateAsset(
                shrineConfig,
                ReferenceFolder + "/ShrineConfig.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void VerifyNormalFixtures(string tempFolder)
        {
            string jsonPath = WriteFixture(
                tempFolder,
                "normal.json",
                CreateNormalFixtureJson());

            PopupEventBuilder.BuildResult first =
                PopupEventBuilder.BuildFromJsonPath(
                    jsonPath,
                    OutputFolder + "/Normal");
            string firstSignature =
                GetExecutionSignature(first);

            Ensure(
                first.createdAssetPaths.Count == 2,
                "First fixture import must create two PopupEventSO assets.");
            EnsureNormalResult(first);

            PopupEventBuilder.BuildResult second =
                PopupEventBuilder.BuildFromJsonPath(
                    jsonPath,
                    OutputFolder + "/Normal");
            string secondSignature =
                GetExecutionSignature(second);

            Ensure(
                second.createdAssetPaths.Count == 0,
                "Reimport must not create duplicate PopupEventSO assets.");
            Ensure(
                firstSignature == secondSignature,
                "Reimport changed Choice execution data.");
            EnsureNormalResult(second);
        }

        private static void EnsureNormalResult(
            PopupEventBuilder.BuildResult result)
        {
            Ensure(
                result.StartEvent is PopupEventSO,
                "StartEvent was not resolved.");

            PopupEventSO startEvent =
                (PopupEventSO)result.StartEvent;

            Ensure(
                startEvent.choices != null
                && startEvent.choices.Count == 5,
                "Normal fixture must generate five choices.");

            Dictionary<ChoiceExecutionType, PopupEventChoice> choices =
                startEvent.choices.ToDictionary(
                    choice => choice.executionConfig.executionType);

            Ensure(
                choices.Count == 5,
                "Each normal fixture execution type must be unique.");

            NextEventExecutionData nextEvent =
                (NextEventExecutionData)choices[
                    ChoiceExecutionType.NextEvent]
                    .executionConfig.data;
            Ensure(
                nextEvent.nextEvent != null
                && nextEvent.nextEvent.eventId == "fixture.next",
                "NextEvent fixture target was not resolved.");

            BattleExecutionData battle =
                (BattleExecutionData)choices[
                    ChoiceExecutionType.Battle]
                    .executionConfig.data;
            Ensure(
                battle.battle != null
                && battle.battle.BattleId == BattleId,
                "Battle fixture reference was not resolved.");
            Ensure(
                choices[ChoiceExecutionType.Battle].rewards == null
                || choices[ChoiceExecutionType.Battle].rewards.Count == 0,
                "Legacy battle-entry reward was not removed.");

            ShopExecutionData shop =
                (ShopExecutionData)choices[
                    ChoiceExecutionType.Shop]
                    .executionConfig.data;
            Ensure(
                shop.shopType == ShopType.Rare
                && shop.itemCount == 3
                && shop.pools.Count == 1
                && shop.pools[0].poolId == PoolId,
                "Shop fixture data was not resolved.");

            ShrineExecutionData shrine =
                (ShrineExecutionData)choices[
                    ChoiceExecutionType.Shrine]
                    .executionConfig.data;
            Ensure(
                shrine.config != null
                && shrine.config.ConfigId == ShrineConfigId
                && shrine.god != null
                && shrine.god.GodId == ShrineGodId,
                "Shrine fixture references were not resolved.");

            Ensure(
                choices[ChoiceExecutionType.CompleteEvent]
                    .executionConfig.data
                    is CompleteEventExecutionData,
                "CompleteEvent fixture data was not generated.");

            foreach (PopupEventChoice choice in startEvent.choices)
            {
                List<string> errors =
                    ChoiceExecutionConfigValidator.Validate(
                        choice.executionConfig);
                Ensure(
                    errors.Count == 0,
                    $"{choice.choiceId} is invalid: "
                    + string.Join(" | ", errors));
            }
        }

        private static void VerifyErrorFixtures(string tempFolder)
        {
            ExpectFailure(
                tempFolder,
                "unknown-type",
                CreateSingleChoiceJson(
                    "\"executionConfig\":{\"type\":\"Unknown\"}"),
                "INVALID_EXECUTION_TYPE");

            ExpectFailure(
                tempFolder,
                "missing-id",
                CreateSingleChoiceJson(
                    "\"executionConfig\":{"
                    + "\"type\":\"Battle\",\"battle\":{}}"),
                "ASSET_ID_REQUIRED");

            ExpectFailure(
                tempFolder,
                "missing-asset",
                CreateSingleChoiceJson(
                    "\"executionConfig\":{"
                    + "\"type\":\"Battle\",\"battle\":{"
                    + "\"battleId\":\"missing.battle\"}}"),
                "ASSET_NOT_FOUND");

            BattleSO duplicate =
                ScriptableObject.CreateInstance<BattleSO>();
            duplicate.battleId = BattleId;
            AssetDatabase.CreateAsset(
                duplicate,
                ReferenceFolder + "/BattleDuplicate.asset");
            AssetDatabase.SaveAssets();

            try
            {
                ExpectFailure(
                    tempFolder,
                    "duplicate-id",
                    CreateSingleChoiceJson(
                        "\"executionConfig\":{"
                        + "\"type\":\"Battle\",\"battle\":{"
                        + $"\"battleId\":\"{BattleId}\"}}"),
                    "ASSET_ID_DUPLICATE");
            }
            finally
            {
                AssetDatabase.DeleteAsset(
                    ReferenceFolder + "/BattleDuplicate.asset");
            }

            ExpectFailure(
                tempFolder,
                "missing-config",
                CreateSingleChoiceJson(
                    "\"rewards\":[]"),
                "EXECUTION_CONFIG_REQUIRED");

            ExpectFailure(
                tempFolder,
                "payload-mismatch",
                CreateSingleChoiceJson(
                    "\"executionConfig\":{"
                    + "\"type\":\"Battle\","
                    + "\"shop\":{\"shopType\":\"Normal\","
                    + $"\"poolIds\":[\"{PoolId}\"],"
                    + "\"itemCount\":1}}"),
                "EXECUTION_PAYLOAD_MISMATCH");
        }

        private static void ExpectFailure(
            string tempFolder,
            string fixtureName,
            string json,
            string expectedCode)
        {
            string jsonPath = WriteFixture(
                tempFolder,
                fixtureName + ".json",
                json);
            string outputFolder =
                OutputFolder + "/Errors/" + fixtureName;

            try
            {
                PopupEventBuilder.BuildFromJsonPath(
                    jsonPath,
                    outputFolder);
            }
            catch (InvalidDataException exception)
            {
                Ensure(
                    exception.Message.IndexOf(
                        expectedCode,
                        StringComparison.Ordinal) >= 0,
                    $"{fixtureName} failed with an unexpected error: "
                    + exception.Message);
                return;
            }
            catch (InvalidOperationException exception)
            {
                Ensure(
                    exception.Message.IndexOf(
                        expectedCode,
                        StringComparison.Ordinal) >= 0,
                    $"{fixtureName} failed with an unexpected error: "
                    + exception.Message);
                return;
            }
            finally
            {
                if (AssetDatabase.IsValidFolder(outputFolder))
                {
                    AssetDatabase.DeleteAsset(outputFolder);
                }
            }

            throw new InvalidOperationException(
                $"{fixtureName} did not fail with {expectedCode}.");
        }

        private static string GetExecutionSignature(
            PopupEventBuilder.BuildResult result)
        {
            PopupEventSO startEvent =
                (PopupEventSO)result.StartEvent;

            return string.Join(
                "\n",
                startEvent.choices.Select(
                    choice =>
                        choice.choiceId
                        + ":"
                        + choice.executionConfig.executionType
                        + ":"
                        + GetDataSignature(
                            choice.executionConfig.data)));
        }

        private static string GetDataSignature(
            ChoiceExecutionData data)
        {
            return data switch
            {
                NextEventExecutionData next =>
                    next.nextEvent?.eventId ?? "<null>",
                BattleExecutionData battle =>
                    battle.battle?.BattleId ?? "<null>",
                ShopExecutionData shop =>
                    $"{shop.shopType}:{shop.itemCount}:"
                    + string.Join(
                        ",",
                        shop.pools.Select(pool => pool.poolId)),
                ShrineExecutionData shrine =>
                    $"{shrine.config?.ConfigId}:{shrine.god?.GodId}",
                CompleteEventExecutionData => "complete",
                _ => "<unknown>"
            };
        }

        private static string CreateNormalFixtureJson()
        {
            return "{"
                   + "\"startNodeId\":\"fixture.start\","
                   + "\"nodes\":["
                   + "{\"nodeId\":\"fixture.start\",\"choices\":["
                   + "{\"choiceId\":\"choice.next\","
                   + "\"executionConfig\":{\"type\":\"NextEvent\","
                   + "\"nextEvent\":{\"nextPopupId\":\"fixture.next\"}}},"
                   + "{\"choiceId\":\"choice.battle\","
                   + "\"executionConfig\":{\"type\":\"Battle\","
                   + "\"battle\":{\"battleId\":\""
                   + BattleId + "\"}},"
                   + "\"rewards\":[{\"rewardType\":\"SpecialBattle\","
                   + "\"rewardId\":\"" + BattleId + "\"}]},"
                   + "{\"choiceId\":\"choice.shop\","
                   + "\"executionConfig\":{\"type\":\"Shop\","
                   + "\"shop\":{\"shopType\":\"Rare\","
                   + "\"poolIds\":[\"" + PoolId + "\"],"
                   + "\"itemCount\":3}}},"
                   + "{\"choiceId\":\"choice.shrine\","
                   + "\"executionConfig\":{\"type\":\"Shrine\","
                   + "\"shrine\":{\"configId\":\""
                   + ShrineConfigId + "\",\"godId\":\""
                   + ShrineGodId + "\"}}},"
                   + "{\"choiceId\":\"choice.complete\","
                   + "\"executionConfig\":{\"type\":\"CompleteEvent\"}}"
                   + "]},"
                   + "{\"nodeId\":\"fixture.next\",\"choices\":[]}"
                   + "]}";
        }

        private static string CreateSingleChoiceJson(
            string choiceFields)
        {
            string nodes =
                "{\"nodeId\":\"fixture.start\",\"choices\":[{"
                + "\"choiceId\":\"fixture.choice\","
                + choiceFields
                + "}]}";

            return "{"
                   + "\"startNodeId\":\"fixture.start\","
                   + "\"nodes\":[" + nodes + "]}";
        }

        private static string WriteFixture(
            string tempFolder,
            string fileName,
            string json)
        {
            string path = Path.Combine(tempFolder, fileName);
            File.WriteAllText(path, json);
            return path;
        }

        private static void SetSerializedString(
            UnityEngine.Object target,
            string propertyName,
            string value)
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty property =
                serializedObject.FindProperty(propertyName);
            Ensure(
                property != null,
                $"{target.GetType().Name}.{propertyName} was not found.");
            property.stringValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSerializedInt(
            UnityEngine.Object target,
            string propertyName,
            int value)
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty property =
                serializedObject.FindProperty(propertyName);
            Ensure(
                property != null,
                $"{target.GetType().Name}.{propertyName} was not found.");
            property.intValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSerializedObjectList(
            UnityEngine.Object target,
            string propertyName,
            UnityEngine.Object value)
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty property =
                serializedObject.FindProperty(propertyName);
            Ensure(
                property != null && property.isArray,
                $"{target.GetType().Name}.{propertyName} was not found.");
            property.arraySize = 1;
            property.GetArrayElementAtIndex(0).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureAssetFolder(string folderPath)
        {
            string[] parts = folderPath.Split('/');
            string current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];

                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static void CleanupAssets()
        {
            if (AssetDatabase.IsValidFolder(TestAssetRoot))
            {
                AssetDatabase.DeleteAsset(TestAssetRoot);
            }
        }

        private static void CleanupDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        private static void Ensure(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
