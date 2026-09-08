using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Progression
{
    [Serializable]
    public sealed class BattleExperienceLevelConfig
    {
        public string schemaVersion = "battle_experience_level_v1";
        public float baseRequiredExperience = 20f;
        public float growthPerLevel = 5f;
        public float growthExponent = 1.15f;

        public float RequiredForLevel(int currentLevel)
        {
            int level = Mathf.Max(1, currentLevel);
            return Mathf.Max(1f, Mathf.Round(
                baseRequiredExperience
                + growthPerLevel * Mathf.Pow(level - 1, growthExponent)));
        }

        public static BattleExperienceLevelConfig Load()
        {
            TextAsset source = Resources.Load<TextAsset>(
                "battle/Progression/battle_experience_level_v1");
            if (source == null)
                return new BattleExperienceLevelConfig();

            BattleExperienceLevelConfig loaded =
                JsonUtility.FromJson<BattleExperienceLevelConfig>(source.text);
            return loaded ?? new BattleExperienceLevelConfig();
        }
    }

    /// <summary>Battle-session-only level progression. No save data is written.</summary>
    public sealed class BattleExperienceProgression
    {
        private readonly BattleExperienceLevelConfig config;

        public BattleExperienceProgression(BattleExperienceLevelConfig config)
        {
            this.config = config ?? new BattleExperienceLevelConfig();
        }

        public int Level { get; private set; } = 1;
        public float CurrentExperience { get; private set; }
        public float RequiredExperience => config.RequiredForLevel(Level);

        public int Add(float amount)
        {
            if (amount <= 0f || float.IsNaN(amount) || float.IsInfinity(amount))
                return 0;

            CurrentExperience += amount;
            int levelsGained = 0;
            while (CurrentExperience >= RequiredExperience)
            {
                CurrentExperience -= RequiredExperience;
                Level++;
                levelsGained++;
            }

            return levelsGained;
        }
    }

    public enum SkillUpgradeRewardSource
    {
        ExperienceLevel = 0,
        BattleClear = 1
    }

    public readonly struct SkillUpgradeRewardRequest
    {
        public SkillUpgradeRewardRequest(
            SkillUpgradeRewardSource source,
            string idempotencyKey,
            string reason)
        {
            Source = source;
            IdempotencyKey = idempotencyKey;
            Reason = reason;
        }

        public SkillUpgradeRewardSource Source { get; }
        public string IdempotencyKey { get; }
        public string Reason { get; }
    }

    /// <summary>Separates reward entitlement from popup presentation.</summary>
    public sealed class BattleSkillUpgradeRewardQueue
    {
        private readonly Queue<SkillUpgradeRewardRequest> pending = new();
        private readonly HashSet<string> acceptedKeys = new(StringComparer.Ordinal);

        public int PendingCount => pending.Count;

        public bool TryEnqueue(SkillUpgradeRewardRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.IdempotencyKey)
                || !acceptedKeys.Add(request.IdempotencyKey))
                return false;

            pending.Enqueue(request);
            return true;
        }

        public bool TryPeek(out SkillUpgradeRewardRequest request)
        {
            if (pending.Count > 0)
            {
                request = pending.Peek();
                return true;
            }

            request = default;
            return false;
        }

        public bool TryComplete(string idempotencyKey)
        {
            if (pending.Count == 0
                || !string.Equals(
                    pending.Peek().IdempotencyKey,
                    idempotencyKey,
                    StringComparison.Ordinal))
                return false;

            pending.Dequeue();
            return true;
        }
    }
}
