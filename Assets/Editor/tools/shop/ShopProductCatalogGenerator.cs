#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Item;
using Shop;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Shop
{
    public static class ShopProductCatalogGenerator
    {
        private const string ItemSoFolder = "Assets/Contents/Item/so";
        private const string OutputRoot = "Assets/Contents/Shop/so";
        private const string LegacyOutputRoot = "Assets/Resources/shop/product";

        [Serializable]
        private sealed class ShopCatalogJson
        {
            public string catalogId;
            public List<ShopPoolJson> pools = new();
        }

        [Serializable]
        private sealed class ShopPoolJson
        {
            public string poolId;
            public string outputFolder;
            public bool allowDuplicate;
            public bool includeDisabledEntries;
            public List<ShopProductJson> products = new();
        }

        [Serializable]
        private sealed class ShopProductJson
        {
            public string productId;
            public string productType;
            public string rewardId;
            public int price = 100;
            public int weight = 100;
            public bool uniqueProduct;
            public string[] tags = Array.Empty<string>();
        }

        [MenuItem("Assets/Shop/Generate Products From Catalog Json", false, 2000)]
        public static void GenerateSelected()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!IsJsonPath(path))
            {
                Debug.LogError("[ShopProductCatalogGenerator] Select a shop catalog json file.");
                return;
            }

            GenerateFromJsonPath(path);
        }

        [MenuItem("Assets/Shop/Generate Products From Catalog Json Folder", false, 2001)]
        public static void GenerateSelectedFolder()
        {
            string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrWhiteSpace(folderPath) ||
                !AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError("[ShopProductCatalogGenerator] Select a folder containing shop catalog json files.");
                return;
            }

            string[] paths = Directory.GetFiles(folderPath, "*.json", SearchOption.TopDirectoryOnly);
            Array.Sort(paths, StringComparer.Ordinal);

            int generated = 0;
            int skipped = 0;
            int failed = 0;

            foreach (string rawPath in paths)
            {
                string path = rawPath.Replace("\\", "/");
                ShopCatalogJson catalog = ReadCatalog(path, false);
                if (catalog == null || string.IsNullOrWhiteSpace(catalog.catalogId))
                {
                    skipped++;
                    continue;
                }

                if (GenerateCatalog(path, catalog))
                {
                    generated++;
                }
                else
                {
                    failed++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(
                $"[ShopProductCatalogGenerator] Folder generation completed. " +
                $"folder={folderPath}, generated={generated}, failed={failed}, skipped={skipped}");
        }

        [MenuItem("Assets/Shop/Generate Products From Catalog Json", true)]
        private static bool ValidateGenerateSelected()
        {
            return IsJsonPath(AssetDatabase.GetAssetPath(Selection.activeObject));
        }

        [MenuItem("Assets/Shop/Generate Products From Catalog Json Folder", true)]
        private static bool ValidateGenerateSelectedFolder()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return !string.IsNullOrWhiteSpace(path) && AssetDatabase.IsValidFolder(path);
        }

        public static bool GenerateFromJsonPath(string jsonPath)
        {
            ShopCatalogJson catalog = ReadCatalog(jsonPath, true);
            if (catalog == null)
            {
                return false;
            }

            bool success = GenerateCatalog(jsonPath, catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return success;
        }

        private static ShopCatalogJson ReadCatalog(string path, bool logErrors)
        {
            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<ShopCatalogJson>(json);
            }
            catch (Exception exception)
            {
                if (logErrors)
                {
                    Debug.LogError($"[ShopProductCatalogGenerator] Failed to parse catalog. path={path}\n{exception}");
                }

                return null;
            }
        }

        private static bool GenerateCatalog(string jsonPath, ShopCatalogJson catalog)
        {
            if (catalog == null || string.IsNullOrWhiteSpace(catalog.catalogId))
            {
                Debug.LogError($"[ShopProductCatalogGenerator] catalogId is required. path={jsonPath}");
                return false;
            }

            if (catalog.pools == null || catalog.pools.Count == 0)
            {
                Debug.LogError($"[ShopProductCatalogGenerator] At least one pool is required. catalog={catalog.catalogId}");
                return false;
            }

            var seenPoolIds = new HashSet<string>(StringComparer.Ordinal);
            var seenProductIds = new HashSet<string>(StringComparer.Ordinal);
            int productCount = 0;

            foreach (ShopPoolJson poolData in catalog.pools)
            {
                if (!ValidatePool(poolData, seenPoolIds))
                {
                    return false;
                }

                string outputFolder = ResolveOutputFolder(poolData.outputFolder);
                EnsureFolder(outputFolder);
                var products = new List<ShopProductSO>();

                foreach (ShopProductJson productData in poolData.products)
                {
                    if (!ValidateProduct(productData, seenProductIds, out ShopProductType productType))
                    {
                        return false;
                    }

                    ScriptableObject reward = LoadReward(productData.rewardId, productType);
                    if (reward == null)
                    {
                        return false;
                    }

                    ShopProductSO product = CreateOrUpdateProduct(
                        outputFolder,
                        productData,
                        productType,
                        reward);
                    products.Add(product);
                    productCount++;
                }

                CreateOrUpdatePool(outputFolder, poolData, products);
            }

            Debug.Log(
                $"[ShopProductCatalogGenerator] Catalog generated. " +
                $"catalog={catalog.catalogId}, pools={catalog.pools.Count}, products={productCount}");
            return true;
        }

        private static bool ValidatePool(ShopPoolJson data, HashSet<string> seenPoolIds)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.poolId))
            {
                Debug.LogError("[ShopProductCatalogGenerator] poolId is required.");
                return false;
            }

            if (!seenPoolIds.Add(data.poolId))
            {
                Debug.LogError($"[ShopProductCatalogGenerator] Duplicate poolId: {data.poolId}");
                return false;
            }

            if (data.products == null)
            {
                Debug.LogError($"[ShopProductCatalogGenerator] products is required. pool={data.poolId}");
                return false;
            }

            return true;
        }

        private static bool ValidateProduct(
            ShopProductJson data,
            HashSet<string> seenProductIds,
            out ShopProductType productType)
        {
            productType = ShopProductType.None;

            if (data == null || string.IsNullOrWhiteSpace(data.productId) ||
                string.IsNullOrWhiteSpace(data.rewardId))
            {
                Debug.LogError("[ShopProductCatalogGenerator] productId and rewardId are required.");
                return false;
            }

            if (!seenProductIds.Add(data.productId))
            {
                Debug.LogError($"[ShopProductCatalogGenerator] Duplicate productId: {data.productId}");
                return false;
            }

            if (!Enum.TryParse(data.productType, true, out productType) ||
                (productType != ShopProductType.Relic &&
                 productType != ShopProductType.StrategicSkillItem))
            {
                Debug.LogError(
                    $"[ShopProductCatalogGenerator] Unsupported productType. " +
                    $"product={data.productId}, type={data.productType}");
                return false;
            }

            if (data.price < 0 || data.weight < 0)
            {
                Debug.LogError($"[ShopProductCatalogGenerator] price and weight must be non-negative. product={data.productId}");
                return false;
            }

            return true;
        }

        private static ScriptableObject LoadReward(string rewardId, ShopProductType productType)
        {
            string path = $"{ItemSoFolder}/{rewardId}.asset";
            ScriptableObject reward = productType switch
            {
                ShopProductType.Relic => AssetDatabase.LoadAssetAtPath<RelicSO>(path),
                ShopProductType.StrategicSkillItem => AssetDatabase.LoadAssetAtPath<StrategicSkillItemSO>(path),
                _ => null,
            };

            if (reward == null)
            {
                Debug.LogError(
                    $"[ShopProductCatalogGenerator] Reward SO not found or has wrong type. " +
                    $"rewardId={rewardId}, expected={productType}, path={path}");
            }

            return reward;
        }

        private static ShopProductSO CreateOrUpdateProduct(
            string folder,
            ShopProductJson data,
            ShopProductType productType,
            ScriptableObject reward)
        {
            string path = $"{folder}/[{data.productId}] ShopProductSO.asset";
            string legacyPath =
                $"{LegacyOutputRoot}/{Path.GetFileName(folder)}/[{data.productId}] ShopProductSO.asset";
            MoveLegacyAssetIfNeeded(legacyPath, path);

            ShopProductSO product = AssetDatabase.LoadAssetAtPath<ShopProductSO>(path);
            if (product == null)
            {
                product = ScriptableObject.CreateInstance<ShopProductSO>();
                AssetDatabase.CreateAsset(product, path);
            }

            product.productId = data.productId;
            product.productType = productType;
            product.price = data.price;
            product.weight = data.weight;
            product.uniqueProduct = data.uniqueProduct;
            product.tags = data.tags ?? Array.Empty<string>();
            product.rewardData ??= new ShopRewardData();
            product.rewardData.rewardType = productType == ShopProductType.Relic
                ? ShopRewardType.Relic
                : ShopRewardType.StrategicSkillItem;
            product.rewardData.reward = reward;

            EditorUtility.SetDirty(product);
            return product;
        }

        private static void CreateOrUpdatePool(
            string folder,
            ShopPoolJson data,
            List<ShopProductSO> products)
        {
            string path = $"{folder}/[{data.poolId}]Shop Item Pool SO.asset";
            string legacyPath =
                $"{LegacyOutputRoot}/{Path.GetFileName(folder)}/[{data.poolId}]Shop Item Pool SO.asset";
            MoveLegacyAssetIfNeeded(legacyPath, path);

            ShopItemPoolSO pool = AssetDatabase.LoadAssetAtPath<ShopItemPoolSO>(path);
            if (pool == null)
            {
                pool = ScriptableObject.CreateInstance<ShopItemPoolSO>();
                AssetDatabase.CreateAsset(pool, path);
            }

            pool.poolId = data.poolId;
            pool.products = products;
            pool.allowDuplicate = data.allowDuplicate;
            pool.includeDisabledEntries = data.includeDisabledEntries;
            EditorUtility.SetDirty(pool);
        }

        private static void MoveLegacyAssetIfNeeded(string legacyPath, string contentPath)
        {
            UnityEngine.Object contentAsset = AssetDatabase.LoadMainAssetAtPath(contentPath);
            if (contentAsset != null)
            {
                return;
            }

            UnityEngine.Object legacyAsset = AssetDatabase.LoadMainAssetAtPath(legacyPath);
            if (legacyAsset == null)
            {
                return;
            }

            string error = AssetDatabase.MoveAsset(legacyPath, contentPath);
            if (!string.IsNullOrEmpty(error))
            {
                throw new InvalidOperationException(
                    $"Failed to migrate legacy shop asset. " +
                    $"from={legacyPath}, to={contentPath}, error={error}");
            }

            Debug.Log(
                $"[ShopProductCatalogGenerator] Migrated legacy asset. " +
                $"from={legacyPath}, to={contentPath}");
        }

        private static string ResolveOutputFolder(string value)
        {
            string child = string.IsNullOrWhiteSpace(value)
                ? "common"
                : value.Trim().Replace("\\", "/").Trim('/');
            return $"{OutputRoot}/{child}";
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folder)?.Replace("\\", "/");
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }

        private static bool IsJsonPath(string path)
        {
            return !string.IsNullOrWhiteSpace(path) &&
                   path.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
        }
    }
}
#endif
