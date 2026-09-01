using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    public readonly struct LargeWaveReservation
    {
        public LargeWaveReservation(int token, string unitKey, float emitTime, Vector3 position)
        {
            Token = token;
            UnitKey = unitKey;
            EmitTime = emitTime;
            Position = position;
        }

        public int Token { get; }
        public string UnitKey { get; }
        public float EmitTime { get; }
        public Vector3 Position { get; }
    }

    public static class Episode1SoloLargeWaveManifest
    {
        public const string PolicyId = "seq.act1.chapter01.01.rescue_villagers.solo_large_wave.v2";
        public const string FodderKey = "spawn.swarm.fodder.melee";
        public const string FastKey = "spawn.swarm.fast.melee";
        public const int TotalCount = 28;
        public const int FodderCount = 22;
        public const int FastCount = 6;

        private static readonly float[] BatchStarts = { 0.45f, 0.80f, 1.15f, 1.50f };
        private static readonly int[] BatchCounts = { 8, 8, 6, 6 };
        private static readonly int[] BatchFastCounts = { 0, 2, 2, 2 };
        private static readonly float[] SectorAngles = { 0f, 90f, 135f, 315f, 180f, 45f, 270f, 0f };

        public static bool TryCreate(BattleLargeWavePolicySO policy, out IReadOnlyList<LargeWaveReservation> result, out string error)
        {
            result = null;
            error = null;
            if (policy == null || policy.PolicyId != PolicyId)
            {
                error = "Unsupported or missing exact1 large-wave policy.";
                return false;
            }

            if (policy.HardLivingCap != TotalCount || policy.SafetyRadius != 4.5f ||
                policy.MinimumSpawnRadius != 9f || policy.MaximumSpawnRadius != 14f)
            {
                error = "Exact1 policy bounds/cap do not match the locked contract.";
                return false;
            }

            var reservations = new List<LargeWaveReservation>(TotalCount);
            int token = 0;
            for (int batch = 0; batch < BatchCounts.Length; batch++)
            {
                int fastStart = BatchCounts[batch] - BatchFastCounts[batch];
                int half = BatchCounts[batch] / 2;
                for (int slot = 0; slot < BatchCounts[batch]; slot++)
                {
                    float angle = SectorAngles[batch * 2 + (slot < half ? 0 : 1)];
                    float radius = 9f + ((token * 37) % 51) * (5f / 50f);
                    Vector3 direction = Quaternion.Euler(0f, 0f, angle) * Vector3.right;
                    string key = slot >= fastStart ? FastKey : FodderKey;
                    reservations.Add(new LargeWaveReservation(
                        token,
                        key,
                        BatchStarts[batch] + (slot % half) * 0.05f,
                        direction * radius));
                    token++;
                }
            }

            if (reservations.Count != TotalCount || reservations[reservations.Count - 1].EmitTime > 1.6001f)
            {
                error = "Exact1 manifest count or terminal emit time is invalid.";
                return false;
            }

            result = reservations;
            return true;
        }
    }
}
