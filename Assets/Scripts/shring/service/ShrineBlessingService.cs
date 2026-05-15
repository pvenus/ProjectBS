using System.Collections.Generic;
using UnityEngine;

namespace Shrine
{
    /// <summary>
    /// Shrine Blessing 생성 및 선택 로직 전담 서비스.
    /// Common Bless / God Bless 후보 생성과 가중치 선택을 담당한다.
    /// </summary>
    public class ShrineBlessingService
    {
        private readonly ShrineManager shrineManager;

        private readonly ShrineConfigSO config;

        private readonly ShrinePlayerRuntimeData playerRuntimeData;

        private readonly bool logDebug;

        public ShrineBlessingService(
            ShrineManager shrineManager,
            ShrineConfigSO config,
            ShrinePlayerRuntimeData playerRuntimeData,
            bool logDebug)
        {
            this.shrineManager = shrineManager;
            this.config = config;
            this.playerRuntimeData = playerRuntimeData;
            this.logDebug = logDebug;
        }

        public List<ShrineBlessingSO> GenerateBlessingCandidates(
            ShrineGodType godType,
            int count)
        {
            List<ShrineBlessingSO> result = new();

            if (count <= 0)
            {
                return result;
            }

            List<ShrineBlessingSO> pool =
                GetAvailableBlessingPool(godType);

            if (pool.Count <= 0)
            {
                return result;
            }

            int safeCount =
                Mathf.Min(count, pool.Count);

            List<ShrineBlessingSO> workingPool =
                new(pool);

            for (int i = 0; i < safeCount; i++)
            {
                ShrineBlessingSO picked =
                    PickWeightedBlessing(workingPool);

                if (picked == null)
                {
                    continue;
                }

                result.Add(picked);
                workingPool.Remove(picked);
            }

            if (logDebug)
            {
                Debug.Log(
                    $"[ShrineBlessingService] Blessing candidates generated. god={godType}, count={result.Count}");
            }

            return result;
        }

        public List<ShrineBlessingSO> GenerateEnhancedBlessingCandidates(
            ShrineGodType godType,
            int count)
        {
            List<ShrineBlessingSO> result = new();

            if (count <= 0)
            {
                return result;
            }

            List<ShrineBlessingSO> pool =
                GetEnhancedBlessingPool(godType);

            if (pool.Count <= 0)
            {
                return result;
            }

            int safeCount =
                Mathf.Min(count, pool.Count);

            List<ShrineBlessingSO> workingPool =
                new(pool);

            for (int i = 0; i < safeCount; i++)
            {
                ShrineBlessingSO picked =
                    PickWeightedBlessing(workingPool);

                if (picked == null)
                {
                    continue;
                }

                result.Add(picked);
                workingPool.Remove(picked);
            }

            if (logDebug)
            {
                Debug.Log(
                    $"[ShrineBlessingService] Enhanced blessing candidates generated. god={godType}, count={result.Count}");
            }

            return result;
        }

        private List<ShrineBlessingSO> GetAvailableBlessingPool(
            ShrineGodType godType)
        {
            List<ShrineBlessingSO> result = new();

            if (config == null)
            {
                return result;
            }

            if (config.blessingPool == null)
            {
                return result;
            }

            int faithLevel =
                playerRuntimeData != null
                    ? playerRuntimeData.GetFaithLevel(godType)
                    : 0;

            for (int i = 0; i < config.blessingPool.Count; i++)
            {
                ShrineBlessingSO blessing =
                    config.blessingPool[i];

                if (blessing == null)
                {
                    continue;
                }

                if (!blessing.CanAppear(godType, faithLevel))
                {
                    continue;
                }

                result.Add(blessing);
            }

            return result;
        }

        private List<ShrineBlessingSO> GetEnhancedBlessingPool(
            ShrineGodType godType)
        {
            List<ShrineBlessingSO> result = new();

            if (config == null)
            {
                return result;
            }

            ShrineGodSO god =
                config.GetGod(godType);

            if (god == null)
            {
                return result;
            }

            if (!god.HasEnhancedBlessings)
            {
                return result;
            }

            int faithLevel =
                playerRuntimeData != null
                    ? playerRuntimeData.GetFaithLevel(godType)
                    : 0;

            List<ShrineBlessingSO> blessings =
                god.GetAvailableBlessings(
                    faithLevel,
                    ShrineBlessingGroup.Enhanced);

            if (blessings == null)
            {
                return result;
            }

            for (int i = 0; i < blessings.Count; i++)
            {
                ShrineBlessingSO blessing =
                    blessings[i];

                if (blessing == null)
                {
                    continue;
                }

                if (!blessing.CanAppear(godType, faithLevel))
                {
                    continue;
                }

                result.Add(blessing);
            }

            return result;
        }

        private ShrineBlessingSO PickWeightedBlessing(
            List<ShrineBlessingSO> pool)
        {
            if (pool == null
                || pool.Count <= 0)
            {
                return null;
            }

            float totalWeight = 0f;

            for (int i = 0; i < pool.Count; i++)
            {
                ShrineBlessingSO blessing =
                    pool[i];

                if (blessing == null)
                {
                    continue;
                }

                totalWeight += Mathf.Max(0f, blessing.weight);
            }

            if (totalWeight <= 0f)
            {
                return pool[Random.Range(0, pool.Count)];
            }

            float randomValue =
                Random.Range(0f, totalWeight);

            float current = 0f;

            for (int i = 0; i < pool.Count; i++)
            {
                ShrineBlessingSO blessing =
                    pool[i];

                if (blessing == null)
                {
                    continue;
                }

                current += Mathf.Max(0f, blessing.weight);

                if (randomValue <= current)
                {
                    return blessing;
                }
            }

            return pool[pool.Count - 1];
        }
    }
}
