using System.Collections.Generic;
using System.Linq;
using Shrine;
using UnityEngine;

namespace Bless
{
    public class BlessManager : MonoBehaviour
    {
        public static BlessManager Instance { get; private set; }

        [Header("Pools")]
        [SerializeField]
        private BlessPoolSO commonPool;

        [SerializeField]
        private List<BlessPoolSO> godPools = new();

        [Header("Settings")]
        [SerializeField]
        private int commonBlessingCount = 1;

        [SerializeField]
        private int godBlessingCount = 2;

        [Header("Runtime")]
        [SerializeField]
        private BlessRuntimeData runtimeData = new();

        public BlessRuntimeData RuntimeData => runtimeData;

        private void Awake()
        {
            if (Instance != null
                && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            Initialize();
        }

        public List<BlessSO> GenerateBlessings(
            ShrineGodType godType,
            int progressionStep)
        {
            List<BlessSO> result = new();

            AddCommonBlessings(result, progressionStep);
            AddGodBlessings(result, godType, progressionStep);

            return result;
        }

        private void AddCommonBlessings(
            List<BlessSO> result,
            int progressionStep)
        {
            if (commonPool == null)
            {
                return;
            }

            for (int i = 0;
                 i < commonBlessingCount;
                 i++)
            {
                BlessSO blessing =
                    commonPool.GetRandomBlessing(
                        ShrineGodType.None,
                        progressionStep,
                        result);

                if (blessing == null)
                {
                    continue;
                }

                if (result.Contains(blessing))
                {
                    continue;
                }

                result.Add(blessing);
            }
        }

        private void AddGodBlessings(
            List<BlessSO> result,
            ShrineGodType godType,
            int progressionStep)
        {
            foreach (BlessPoolSO pool in godPools)
            {
                if (pool == null)
                {
                    continue;
                }

                for (int i = 0;
                     i < godBlessingCount;
                     i++)
                {
                    BlessSO blessing =
                        pool.GetRandomBlessing(
                            godType,
                            progressionStep,
                            result);

                    if (blessing == null)
                    {
                        continue;
                    }

                    result.Add(blessing);
                }
            }
        }

        private void Initialize()
        {
            runtimeData = new BlessRuntimeData();
        }

        public void ResetRuntime()
        {
            Initialize();
        }
    }
}