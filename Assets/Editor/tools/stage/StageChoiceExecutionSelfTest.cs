using System;
using System.Collections.Generic;
using Battle;
using Common.SO;
using Shop;
using Shrine;
using Stage;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StageEditor
{
    /// <summary>
    /// 별도 테스트 asmdef 없이 실행 가능한 Editor self-test.
    /// 테스트 데이터는 메모리에만 생성하며 프로젝트 에셋을 수정하지 않는다.
    /// </summary>
    public static class StageChoiceExecutionSelfTest
    {
        private const string SerializationTestAssetRoot =
            "Assets/__StageChoiceExecutionSerializationSelfTest";

        [MenuItem(
            "Tools/Stage/Choice Execution Tests/Run Data Tests")]
        public static void RunDataTests()
        {
            using TestContext context = new();
            RunDataTests(context);
            Debug.Log("Event Choice execution data tests passed.");
        }

        [MenuItem(
            "Tools/Stage/Choice Execution Tests/Run Serialization Tests")]
        public static void RunSerializationTests()
        {
            using TestContext context = new(true);
            List<ChoiceExecutionConfig> configs =
                CreateValidConfigs(context);

            VerifySerializationRoundTrip(context, configs);
            Debug.Log("Event Choice execution serialization tests passed.");
        }

        [MenuItem(
            "Tools/Stage/Choice Execution Tests/Run Icon Asset Tests")]
        public static void RunIconAssetTests()
        {
            VerifyIconAssets();
            Debug.Log("Choice execution icon asset tests passed.");
        }

        [MenuItem(
            "Tools/Stage/Choice Execution Tests/Run All")]
        public static void RunAll()
        {
            RunAllInternal();
            Debug.Log("All Event Choice execution self-tests passed.");
        }

        /// <summary>
        /// Unity batchmode -executeMethod 진입점.
        /// </summary>
        public static void RunAllBatch()
        {
            RunAllInternal();
        }

        private static void RunAllInternal()
        {
            using TestContext context = new(true);
            List<ChoiceExecutionConfig> configs =
                CreateValidConfigs(context);

            VerifyFactory();
            ValidateAll(configs);
            ValidateInvalidCases();
            VerifySerializationRoundTrip(context, configs);
            StageChoiceExecutionRouterSelfTest.RunAll();
            StageChoiceExecutionBuilderSelfTest.RunAll();
            VerifyIconAssets();
        }

        private static void RunDataTests(TestContext context)
        {
            List<ChoiceExecutionConfig> configs =
                CreateValidConfigs(context);

            VerifyFactory();
            ValidateAll(configs);
            ValidateInvalidCases();
        }

        private static void VerifyFactory()
        {
            Dictionary<ChoiceExecutionType, Type> expectedTypes = new()
            {
                {
                    ChoiceExecutionType.NextEvent,
                    typeof(NextEventExecutionData)
                },
                {
                    ChoiceExecutionType.Battle,
                    typeof(BattleExecutionData)
                },
                {
                    ChoiceExecutionType.Shop,
                    typeof(ShopExecutionData)
                },
                {
                    ChoiceExecutionType.Shrine,
                    typeof(ShrineExecutionData)
                },
                {
                    ChoiceExecutionType.CompleteEvent,
                    typeof(CompleteEventExecutionData)
                }
            };

            foreach (
                KeyValuePair<ChoiceExecutionType, Type> pair
                in expectedTypes)
            {
                ChoiceExecutionData data =
                    ChoiceExecutionDataFactory.Create(pair.Key);

                Ensure(
                    data != null && data.GetType() == pair.Value,
                    $"Factory returned an invalid type for {pair.Key}.");
            }

            Ensure(
                ChoiceExecutionDataFactory.Create(
                    ChoiceExecutionType.None) == null,
                "Factory must return null for None.");

            ChoiceExecutionConfig config =
                ChoiceExecutionDataFactory.CreateConfig(
                    ChoiceExecutionType.Battle);
            ChoiceExecutionData previousData = config.data;
            config.executionType = ChoiceExecutionType.Shop;

            bool replaced =
                ChoiceExecutionDataFactory.ReplaceDataIfNeeded(config);

            Ensure(replaced, "Factory did not replace mismatched data.");
            Ensure(
                config.data is ShopExecutionData
                && !ReferenceEquals(config.data, previousData),
                "Factory retained data from the previous execution type.");

            Ensure(
                !ChoiceExecutionDataFactory.ReplaceDataIfNeeded(config),
                "Factory replaced data that already matched its type.");
        }

        private static List<ChoiceExecutionConfig> CreateValidConfigs(
            TestContext context)
        {
            PopupEventSO nextEvent =
                context.Create<PopupEventSO>();
            BattleSO battle =
                context.Create<BattleSO>();
            ShopProductSO product =
                context.Create<ShopProductSO>();
            ShopItemPoolSO pool =
                context.Create<ShopItemPoolSO>();
            ShrineGodSO god =
                context.Create<ShrineGodSO>();
            ShrineConfigSO shrineConfig =
                context.Create<ShrineConfigSO>();

            pool.products.Add(product);
            ConfigureShrine(shrineConfig, god);

            return new List<ChoiceExecutionConfig>
            {
                new()
                {
                    executionType =
                        ChoiceExecutionType.NextEvent,
                    data = new NextEventExecutionData
                    {
                        nextEvent = nextEvent
                    }
                },
                new()
                {
                    executionType =
                        ChoiceExecutionType.Battle,
                    data = new BattleExecutionData
                    {
                        battle = battle
                    }
                },
                new()
                {
                    executionType =
                        ChoiceExecutionType.Shop,
                    data = new ShopExecutionData
                    {
                        pools = new List<ShopItemPoolSO> { pool },
                        itemCount = 1
                    }
                },
                new()
                {
                    executionType =
                        ChoiceExecutionType.Shrine,
                    data = new ShrineExecutionData
                    {
                        config = shrineConfig,
                        god = god
                    }
                },
                new()
                {
                    executionType =
                        ChoiceExecutionType.CompleteEvent,
                    data = new CompleteEventExecutionData()
                }
            };
        }

        private static void ConfigureShrine(
            ShrineConfigSO shrineConfig,
            ShrineGodSO god)
        {
            SerializedObject godObject = new(god);
            SerializedProperty godType =
                godObject.FindProperty("godType");

            Ensure(godType != null, "ShrineGodSO.godType was not found.");
            godType.intValue = (int)ShrineGodType.Life;
            godObject.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject configObject = new(shrineConfig);
            SerializedProperty gods =
                configObject.FindProperty("gods");

            Ensure(gods != null, "ShrineConfigSO.gods was not found.");
            gods.arraySize = 1;
            gods.GetArrayElementAtIndex(0).objectReferenceValue = god;
            configObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ValidateAll(
            IEnumerable<ChoiceExecutionConfig> configs)
        {
            foreach (ChoiceExecutionConfig config in configs)
            {
                List<string> errors =
                    ChoiceExecutionConfigValidator.Validate(config);

                Ensure(
                    errors.Count == 0,
                    $"{config.executionType} should be valid: "
                    + string.Join(" | ", errors));
            }
        }

        private static void ValidateInvalidCases()
        {
            EnsureInvalid(
                null,
                "CONFIG_NULL");

            EnsureInvalid(
                new ChoiceExecutionConfig
                {
                    executionType = ChoiceExecutionType.Battle,
                    data = null
                },
                "DATA_NULL");

            EnsureInvalid(
                new ChoiceExecutionConfig
                {
                    executionType = ChoiceExecutionType.Battle,
                    data = new ShopExecutionData()
                },
                "TYPE_MISMATCH");

            EnsureInvalid(
                new ChoiceExecutionConfig
                {
                    executionType = ChoiceExecutionType.NextEvent,
                    data = new NextEventExecutionData()
                },
                "NEXT_EVENT_NULL");

            EnsureInvalid(
                new ChoiceExecutionConfig
                {
                    executionType = ChoiceExecutionType.Shop,
                    data = new ShopExecutionData()
                },
                "SHOP_POOLS_EMPTY");

            EnsureInvalid(
                new ChoiceExecutionConfig
                {
                    executionType = ChoiceExecutionType.Shrine,
                    data = new ShrineExecutionData()
                },
                "SHRINE_CONFIG_NULL");
        }

        private static void VerifySerializationRoundTrip(
            TestContext context,
            IReadOnlyList<ChoiceExecutionConfig> configs)
        {
            PopupEventSO source = context.Create<PopupEventSO>();
            PopupEventSO copy = context.Create<PopupEventSO>();

            source.eventId = "self-test.source";

            for (int i = 0; i < configs.Count; i++)
            {
                source.choices.Add(
                    new PopupEventChoice
                    {
                        choiceId = $"self-test.choice.{i}",
                        executionConfig = configs[i]
                    });
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string json = EditorJsonUtility.ToJson(source);
            EditorJsonUtility.FromJsonOverwrite(json, copy);

            Ensure(
                copy.choices != null
                && copy.choices.Count == configs.Count,
                "PopupEventChoice list was not preserved.");

            for (int i = 0; i < configs.Count; i++)
            {
                ChoiceExecutionConfig expected = configs[i];
                ChoiceExecutionConfig actual =
                    copy.choices[i].executionConfig;

                Ensure(
                    actual != null,
                    $"executionConfig[{i}] was not preserved.");
                Ensure(
                    actual.executionType == expected.executionType,
                    $"executionType[{i}] changed after round-trip.");
                Ensure(
                    actual.data != null
                    && actual.data.GetType() == expected.data.GetType(),
                    $"SerializeReference type[{i}] was not preserved.");

                VerifyObjectReferences(expected.data, actual.data, i);
            }
        }

        private static void VerifyObjectReferences(
            ChoiceExecutionData expected,
            ChoiceExecutionData actual,
            int index)
        {
            switch (expected)
            {
                case NextEventExecutionData expectedNext:
                    Ensure(
                        ((NextEventExecutionData)actual).nextEvent
                        == expectedNext.nextEvent,
                        $"NextEvent reference[{index}] was not preserved.");
                    break;

                case BattleExecutionData expectedBattle:
                    Ensure(
                        ((BattleExecutionData)actual).battle
                        == expectedBattle.battle,
                        $"BattleSO reference[{index}] was not preserved.");
                    break;

                case ShopExecutionData expectedShop:
                    ShopExecutionData actualShop =
                        (ShopExecutionData)actual;

                    Ensure(
                        actualShop.pools != null
                        && actualShop.pools.Count
                        == expectedShop.pools.Count
                        && actualShop.pools[0] == expectedShop.pools[0],
                        $"Shop pool reference[{index}] was not preserved.");
                    break;

                case ShrineExecutionData expectedShrine:
                    ShrineExecutionData actualShrine =
                        (ShrineExecutionData)actual;

                    Ensure(
                        actualShrine.config == expectedShrine.config
                        && actualShrine.god == expectedShrine.god,
                        $"Shrine references[{index}] were not preserved.");
                    break;
            }
        }

        private static void EnsureInvalid(
            ChoiceExecutionConfig config,
            string expectedCode)
        {
            List<string> errors =
                ChoiceExecutionConfigValidator.Validate(config);

            bool found = errors.Exists(
                error => error.StartsWith(
                    expectedCode,
                    StringComparison.Ordinal));

            Ensure(
                found,
                $"Expected validation error {expectedCode}: "
                + string.Join(" | ", errors));
        }

        private static void VerifyIconAssets()
        {
            const string libraryPath =
                "Assets/Resources/library/NodeTypeIconLibrary.asset";
            NodeTypeIconLibrarySO library =
                AssetDatabase.LoadAssetAtPath<NodeTypeIconLibrarySO>(
                    libraryPath);

            Ensure(
                library != null,
                $"Node icon library was not found: {libraryPath}");
            Ensure(
                library.GetIcon(
                    NodeIconType.Battle) != null,
                "Battle icon is not assigned.");
            Ensure(
                library.GetIcon(
                    NodeIconType.Shop) != null,
                "Shop icon is not assigned.");
            Ensure(
                library.GetIcon(
                    NodeIconType.Story) != null,
                "Story icon is not assigned.");

            string[] shrineGodGuids =
                AssetDatabase.FindAssets(
                    "t:ShrineGodSO",
                    new[] { "Assets/Resources/shring" });

            Ensure(
                shrineGodGuids.Length > 0,
                "ShrineGodSO assets were not found.");

            foreach (string guid in shrineGodGuids)
            {
                string path =
                    AssetDatabase.GUIDToAssetPath(guid);
                ShrineGodSO god =
                    AssetDatabase.LoadAssetAtPath<ShrineGodSO>(
                        path);

                Ensure(
                    god != null && god.Icon != null,
                    $"Shrine icon is not assigned: {path}");
            }
        }

        private static void Ensure(
            bool condition,
            string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private sealed class TestContext : IDisposable
        {
            private readonly List<Object> objects = new();
            private readonly bool persistAssets;
            private int assetIndex;

            public TestContext(bool persistAssets = false)
            {
                this.persistAssets = persistAssets;

                if (!persistAssets)
                {
                    return;
                }

                if (AssetDatabase.IsValidFolder(
                        SerializationTestAssetRoot))
                {
                    AssetDatabase.DeleteAsset(
                        SerializationTestAssetRoot);
                }

                AssetDatabase.CreateFolder(
                    "Assets",
                    "__StageChoiceExecutionSerializationSelfTest");
            }

            public T Create<T>()
                where T : ScriptableObject
            {
                T instance = ScriptableObject.CreateInstance<T>();
                objects.Add(instance);

                if (persistAssets)
                {
                    string assetPath =
                        $"{SerializationTestAssetRoot}/"
                        + $"{assetIndex:D2}-{typeof(T).Name}.asset";
                    assetIndex++;
                    AssetDatabase.CreateAsset(instance, assetPath);
                }

                return instance;
            }

            public void Dispose()
            {
                if (persistAssets)
                {
                    AssetDatabase.DeleteAsset(
                        SerializationTestAssetRoot);
                    AssetDatabase.Refresh();
                    objects.Clear();
                    return;
                }

                for (int i = objects.Count - 1; i >= 0; i--)
                {
                    if (objects[i] != null)
                    {
                        Object.DestroyImmediate(objects[i]);
                    }
                }

                objects.Clear();
            }
        }
    }
}
