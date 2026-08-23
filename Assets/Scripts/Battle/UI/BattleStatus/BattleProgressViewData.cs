using UnityEngine;

namespace Battle.UI.BattleStatus
{
    public sealed class BattleProgressViewData
    {
        public string BattleName { get; }
        public int CurrentWave { get; }
        public int TotalWave { get; }
        public float RemainingTimeSeconds { get; }
        public float ElapsedTimeSeconds { get; }
        public int RemainingEnemyCount { get; }

        public BattleProgressViewData(
            string battleName,
            int currentWave,
            int totalWave,
            float remainingTimeSeconds,
            float elapsedTimeSeconds,
            int remainingEnemyCount)
        {
            BattleName = battleName ?? string.Empty;
            TotalWave = Mathf.Max(0, totalWave);
            CurrentWave =
                TotalWave <= 0
                    ? 0
                    : Mathf.Clamp(currentWave, 0, TotalWave);
            RemainingTimeSeconds =
                BattleStatusValueUtility.ToNonNegativeFinite(
                    remainingTimeSeconds);
            ElapsedTimeSeconds =
                BattleStatusValueUtility.ToNonNegativeFinite(
                    elapsedTimeSeconds);
            RemainingEnemyCount = Mathf.Max(0, remainingEnemyCount);
        }
    }

    internal static class BattleStatusValueUtility
    {
        public static float ToNonNegativeFinite(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                return 0f;
            }

            return Mathf.Max(0f, value);
        }
    }
}
