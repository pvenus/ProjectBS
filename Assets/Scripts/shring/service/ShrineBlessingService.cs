using System.Collections.Generic;
using System.Linq;
using Bless;
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

        private readonly bool logDebug;

        public ShrineBlessingService(
            ShrineManager shrineManager,
            ShrineConfigSO config,
            bool logDebug)
        {
            this.shrineManager = shrineManager;
            this.config = config;
            this.logDebug = logDebug;
        }

        public List<BlessSO> GenerateBlessingCandidates(
            ShrineGodType godType,
            int count)
        {
            List<BlessSO> result = new();

            if (count <= 0)
            {
                return result;
            }

            List<BlessSO> pool =
                GetAvailableBlessingPool(godType);

            if (pool.Count <= 0)
            {
                return result;
            }

            int safeCount =
                Mathf.Min(count, pool.Count);

            List<BlessSO> workingPool =
                new(pool);

            for (int i = 0; i < safeCount; i++)
            {
                BlessSO picked =
                    PickWeightedBlessing(
                        workingPool,
                        godType);

                if (picked == null)
                {
                    break;
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

        public List<BlessSO> GenerateEnhancedBlessingCandidates(
            ShrineGodType godType,
            int count)
        {
            List<BlessSO> result = new();

            if (count <= 0)
            {
                return result;
            }

            List<BlessSO> pool =
                GetEnhancedBlessingPool(godType);

            if (pool.Count <= 0)
            {
                return result;
            }

            int safeCount =
                Mathf.Min(count, pool.Count);

            List<BlessSO> workingPool =
                new(pool);

            for (int i = 0; i < safeCount; i++)
            {
                BlessSO picked =
                    PickWeightedBlessing(
                        workingPool,
                        godType);

                if (picked == null)
                {
                    break;
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

        private List<BlessSO> GetAvailableBlessingPool(
            ShrineGodType godType)
        {
            List<BlessSO> result = new();

            if (config == null
                || BlessManager.Instance == null)
            {
                return result;
            }

            BlessPoolSO commonPool =
                BlessManager.Instance.CommonPool;

            if (commonPool == null)
            {
                return result;
            }

            int faithLevel =
                shrineManager.PlayerRuntimeData != null
                    ? shrineManager.PlayerRuntimeData.GetFaithLevel(godType)
                    : 0;

            for (int i = 0; i < commonPool.blessings.Count; i++)
            {
                BlessPoolSO.BlessPoolEntry entry =
                    commonPool.blessings[i];

                if (entry == null
                    || entry.blessing == null)
                {
                    continue;
                }

                if (entry.progressionStep != faithLevel)
                {
                    continue;
                }

                if (!entry.blessing.CanAppear(godType, faithLevel))
                {
                    continue;
                }

                result.Add(entry.blessing);
            }

            return result;
        }

        private List<BlessSO> GetEnhancedBlessingPool(
            ShrineGodType godType)
        {
            List<BlessSO> result = new();

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
                shrineManager.PlayerRuntimeData != null
                    ? shrineManager.PlayerRuntimeData.GetFaithLevel(godType)
                    : 0;

            List<BlessSO> blessings =
                god.GetAvailableBlessings(
                    faithLevel,
                    ShrineBlessingGroup.Enhanced);

            if (blessings == null)
            {
                return result;
            }

            for (int i = 0; i < blessings.Count; i++)
            {
                BlessSO blessing =
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

        private BlessSO PickWeightedBlessing(
            List<BlessSO> pool,
            ShrineGodType godType)
        {
            if (pool == null
                || pool.Count <= 0)
            {
                return null;
            }

            int faithLevel =
                shrineManager.PlayerRuntimeData != null
                    ? shrineManager.PlayerRuntimeData.GetFaithLevel(godType)
                    : 0;

            List<BlessPoolSO.BlessPoolEntry> candidates = new();

            for (int i = 0; i < pool.Count; i++)
            {
                BlessSO blessing = pool[i];

                if (blessing == null)
                {
                    continue;
                }

                BlessPoolSO commonPool =
                    BlessManager.Instance != null
                        ? BlessManager.Instance.CommonPool
                        : null;

                if (commonPool == null)
                {
                    continue;
                }

                BlessPoolSO.BlessPoolEntry entry =
                    commonPool.blessings
                        .Find(x => x != null
                                   && x.blessing == blessing
                                   && x.progressionStep == faithLevel);

                if (entry == null)
                {
                    continue;
                }

                candidates.Add(entry);
            }

            if (candidates.Count <= 0)
            {
                return null;
            }

            int totalWeight = 0;

            for (int i = 0; i < candidates.Count; i++)
            {
                totalWeight += Mathf.Max(1, candidates[i].weight);
            }

            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;

            for (int i = 0; i < candidates.Count; i++)
            {
                currentWeight += Mathf.Max(1, candidates[i].weight);

                if (randomValue < currentWeight)
                {
                    return candidates[i].blessing;
                }
            }

            return candidates[0].blessing;
        }
    }
}
