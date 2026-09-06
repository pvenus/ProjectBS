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

        public const int AssaultGroupCount = 6;
        public const float UnitInterval = 0.14f;

        private static readonly float[] GroupStarts = { 0.45f, 1.25f, 2.05f, 2.85f, 3.65f, 4.45f };
        private static readonly int[] GroupCounts = { 5, 5, 5, 5, 4, 4 };
        private static readonly int[] GroupFastCounts = { 1, 1, 1, 1, 1, 1 };
        private static readonly float[] GroupCenterAngles = { 0f, 180f, 60f, 240f, 120f, 300f };

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
            for (int group = 0; group < GroupCounts.Length; group++)
            {
                int groupCount = GroupCounts[group];
                int fastStart = groupCount - GroupFastCounts[group];
                for (int slot = 0; slot < groupCount; slot++)
                {
                    // A compact arc reads as one assault group. Group centers alternate
                    // across the player, then rotate around the ring to build pressure
                    // without producing straight spawn lines or one flat mass.
                    float centeredSlot = slot - (groupCount - 1) * 0.5f;
                    float angle = GroupCenterAngles[group] + centeredSlot * 6f;
                    float radiusStep = slot == 0 ? 0f : ((slot + 1) / 2) * 0.35f * (slot % 2 == 0 ? 1f : -1f);
                    float radius = 11.5f + radiusStep;
                    Vector3 direction = Quaternion.Euler(0f, 0f, angle) * Vector3.right;
                    string key = slot >= fastStart ? FastKey : FodderKey;
                    reservations.Add(new LargeWaveReservation(
                        token,
                        key,
                        GroupStarts[group] + slot * UnitInterval,
                        direction * radius));
                    token++;
                }
            }

            if (reservations.Count != TotalCount || reservations[reservations.Count - 1].EmitTime > 4.9001f)
            {
                error = "Exact1 manifest count or terminal emit time is invalid.";
                return false;
            }

            result = reservations;
            return true;
        }
    }
}
