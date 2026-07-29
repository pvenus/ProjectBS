using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Battle;
using Shop;
using Shrine;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Stage
{
    /// <summary>
    /// Choice 실행 JSON의 문자열 ID를 기존 SO 참조로 해석한다.
    /// 검색 실패와 중복 ID는 잘못된 실행 대상을 숨기지 않도록 Import를 중단한다.
    /// </summary>
    public static class StageChoiceExecutionAssetResolver
    {
        public static BattleSO ResolveBattle(string battleId)
        {
            return ResolveUnique<BattleSO>(
                battleId,
                "BattleSO",
                battle => battle.BattleId);
        }

        public static List<ShopItemPoolSO> ResolveShopPools(
            IReadOnlyList<string> poolIds)
        {
            if (poolIds == null || poolIds.Count == 0)
            {
                throw new InvalidDataException(
                    "SHOP_POOL_IDS_REQUIRED: "
                    + "At least one ShopItemPoolSO poolId is required.");
            }

            List<ShopItemPoolSO> pools = new(poolIds.Count);

            for (int i = 0; i < poolIds.Count; i++)
            {
                pools.Add(
                    ResolveUnique<ShopItemPoolSO>(
                        poolIds[i],
                        $"ShopItemPoolSO pools[{i}]",
                        pool => pool.poolId));
            }

            return pools;
        }

        public static ShrineConfigSO ResolveShrineConfig(
            string configId)
        {
            return ResolveUnique<ShrineConfigSO>(
                configId,
                "ShrineConfigSO",
                config => config.ConfigId);
        }

        public static ShrineGodSO ResolveShrineGod(string godId)
        {
            return ResolveUnique<ShrineGodSO>(
                godId,
                "ShrineGodSO",
                god => god.GodId);
        }

        private static T ResolveUnique<T>(
            string id,
            string assetLabel,
            Func<T, string> getId)
            where T : ScriptableObject
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new InvalidDataException(
                    $"ASSET_ID_REQUIRED: {assetLabel} ID is required.");
            }

            List<T> matches = AssetDatabase
                .FindAssets($"t:{typeof(T).Name}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => path, StringComparer.Ordinal)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(
                    asset => asset != null
                             && string.Equals(
                                 getId(asset),
                                 id,
                                 StringComparison.Ordinal))
                .ToList();

            if (matches.Count == 0)
            {
                throw new InvalidDataException(
                    $"ASSET_NOT_FOUND: {assetLabel} ID '{id}' was not found.");
            }

            if (matches.Count > 1)
            {
                string paths = string.Join(
                    ", ",
                    matches.Select(AssetDatabase.GetAssetPath));

                throw new InvalidDataException(
                    $"ASSET_ID_DUPLICATE: {assetLabel} ID '{id}' "
                    + $"matched {matches.Count} assets: {paths}");
            }

            return matches[0];
        }
    }
}
