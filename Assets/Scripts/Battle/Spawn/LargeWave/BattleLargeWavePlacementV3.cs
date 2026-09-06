using System;
using System.Collections.Generic;
using Character;
using UnityEngine;

namespace Battle
{
    [Serializable]
    public sealed class BattleLargeWavePlacementV3
    {
        public string schemaVersion;
        public string profileId;
        public string battleId;
        public int partySize;
        public Vector2Json map;
        public BoundaryJson boundary;
        public int hardLivingCap;
        public List<SlotJson> slots = new();
        public List<FallbackJson> fallbackAnchors = new();
        public string sourceAuthority;
        public string version;
        public string digest;

        [Serializable] public sealed class Vector2Json { public float x; public float y; }
        [Serializable] public sealed class BoundaryJson { public string boundaryAssetId; public float movementInset; public float spawnInset; public float safeRadius; public float corridorWidth; }
        [Serializable] public sealed class SlotJson
        {
            public string slotId; public int waveIndex; public int batchIndex; public int slotIndex;
            public string unitKey; public float dueTime; public float radiusMin; public float radiusMax;
            public float preferredAngle; public float angleDeviation; public string fallbackAnchorId;
        }
        [Serializable] public sealed class FallbackJson { public string anchorId; public float x; public float y; public string boundaryAssetId; }

        public const string ResourcePath = "battle/act1/chapter01/battle.act1.chapter01.01.rescue_villagers.placement.v3";
        public const string Schema = "battle-large-wave-placement.v3";
        public const string BattleId = "battle.act1.chapter01.01.rescue_villagers";

        public static bool TryLoadAndResolve(ISpawnUnitResolver resolver,
            out BattleLargeWavePlacementV3 profile,
            out IReadOnlyList<LargeWaveReservation> reservations,
            out string error)
        {
            profile = null; reservations = null; error = null;
            TextAsset text = Resources.Load<TextAsset>(ResourcePath);
            if (text == null) { error = "placement v3 resource missing"; return false; }
            try { profile = JsonUtility.FromJson<BattleLargeWavePlacementV3>(text.text); }
            catch (Exception e) { error = $"placement v3 parse failed: {e.Message}"; return false; }
            if (profile == null || profile.schemaVersion != Schema || profile.battleId != BattleId ||
                profile.partySize != 1 || profile.map == null || profile.map.x != 32f || profile.map.y != 18f ||
                profile.boundary == null || profile.boundary.movementInset != .75f ||
                profile.boundary.spawnInset != .50f || profile.boundary.safeRadius != 4.5f ||
                profile.boundary.corridorWidth != 2.5f || profile.hardLivingCap != 28 ||
                profile.slots == null || profile.slots.Count != 28 ||
                profile.fallbackAnchors == null || profile.fallbackAnchors.Count != 28)
            { error = "placement v3 header/count/bounds mismatch"; return false; }

            var fallbackById = new Dictionary<string, FallbackJson>(StringComparer.Ordinal);
            foreach (FallbackJson fallback in profile.fallbackAnchors)
                if (fallback == null || string.IsNullOrEmpty(fallback.anchorId) || fallbackById.ContainsKey(fallback.anchorId))
                { error = "duplicate/missing fallback anchor"; return false; }
                else fallbackById.Add(fallback.anchorId, fallback);

            var rows = new List<LargeWaveReservation>(28);
            var usedIds = new HashSet<string>(StringComparer.Ordinal);
            var points = new List<Vector2>(28);
            int black = 0, chain = 0;
            float previousDue = -1f;
            foreach (SlotJson slot in profile.slots)
            {
                if (slot == null || !usedIds.Add(slot.slotId) || slot.dueTime < previousDue ||
                    resolver?.Resolve(new SpawnUnitRequest(slot.unitKey, SpawnUnitRole.Melee)) == null)
                { error = "slot identity/order/unit preflight failed"; return false; }
                previousDue = slot.dueTime;
                if (slot.unitKey == Episode1SoloLargeWaveManifest.FodderKey) black++;
                else if (slot.unitKey == Episode1SoloLargeWaveManifest.FastKey) chain++;
                else { error = "unsupported v3 unit key"; return false; }
                if (!TryResolvePoint(profile, slot, points, fallbackById, out Vector2 point))
                { error = $"no valid candidate for {slot.slotId}"; return false; }
                points.Add(point);
                rows.Add(new LargeWaveReservation(rows.Count, slot.unitKey, slot.dueTime, point));
            }
            if (black != 22 || chain != 6) { error = "v3 role totals mismatch"; return false; }
            reservations = rows;
            return true;
        }

        private static bool TryResolvePoint(BattleLargeWavePlacementV3 p, SlotJson slot,
            List<Vector2> reserved, Dictionary<string, FallbackJson> fallbacks, out Vector2 point)
        {
            float center = (slot.radiusMin + slot.radiusMax) * .5f;
            float[] radiusOffsets = { 0f, -.40f, .40f, -.80f, .80f };
            float[] angleOffsets = { 0f, 6f, -6f, 12f, -12f };
            foreach (float radiusOffset in radiusOffsets)
                foreach (float angleOffset in angleOffsets)
                {
                    float radius = Mathf.Clamp(center + radiusOffset, slot.radiusMin, slot.radiusMax);
                    if (IsValid(p, radius, slot.preferredAngle + angleOffset, slot, reserved, out point)) return true;
                }
            if (fallbacks.TryGetValue(slot.fallbackAnchorId, out FallbackJson fallback))
            {
                point = new Vector2(fallback.x, fallback.y);
                float radius = point.magnitude;
                float angle = Mathf.Atan2(point.y, point.x) * Mathf.Rad2Deg;
                if (fallback.boundaryAssetId == p.boundary.boundaryAssetId &&
                    IsValid(p, radius, angle, slot, reserved, out _)) return true;
            }
            point = default; return false;
        }

        private static bool IsValid(BattleLargeWavePlacementV3 p, float radius, float angle,
            SlotJson slot, List<Vector2> reserved, out Vector2 point)
        {
            point = Quaternion.Euler(0f, 0f, angle) * Vector2.right * radius;
            if (radius < slot.radiusMin || radius > slot.radiusMax ||
                Mathf.Abs(Mathf.DeltaAngle(slot.preferredAngle, angle)) > slot.angleDeviation ||
                Mathf.Abs(point.x) > p.map.x * .5f - p.boundary.movementInset - p.boundary.spawnInset ||
                Mathf.Abs(point.y) > p.map.y * .5f - p.boundary.movementInset - p.boundary.spawnInset ||
                point.magnitude < p.boundary.safeRadius) return false;
            for (int i = 0; i < reserved.Count; i++) if (Vector2.Distance(point, reserved[i]) < .65f) return false;
            Collider2D[] blockers = Physics2D.OverlapCircleAll(point, .40f);
            for (int i = 0; i < blockers.Length; i++)
                if (blockers[i] != null && !blockers[i].isTrigger && blockers[i].GetComponentInParent<CharacterManager>() == null)
                    return false;
            Vector2 safeEdge = point.normalized * p.boundary.safeRadius;
            RaycastHit2D[] corridor = Physics2D.CircleCastAll(point, p.boundary.corridorWidth * .5f,
                (safeEdge - point).normalized, Vector2.Distance(point, safeEdge));
            for (int i = 0; i < corridor.Length; i++)
                if (corridor[i].collider != null && !corridor[i].collider.isTrigger &&
                    corridor[i].collider.GetComponentInParent<CharacterManager>() == null) return false;
            return true;
        }
    }
}
