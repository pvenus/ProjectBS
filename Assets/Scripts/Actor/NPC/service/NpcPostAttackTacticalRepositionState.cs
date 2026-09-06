using UnityEngine;

namespace Npc.Service
{
    /// <summary>Exact2 post-attack tactical choice. NpcPathing remains the sole mover.</summary>
    public sealed class NpcPostAttackTacticalRepositionState
    {
        public enum Pattern { Retreat, ForwardPressure, Orbit, Hold }

        public const float RefreshInterval = .20f;
        public const float TargetRefreshDistance = .35f;
        public const float SpawnLeashRadius = 6f;

        private Pattern lastPattern = Pattern.Hold;
        private Pattern secondLastPattern = Pattern.Hold;
        private Pattern pattern;
        private int orbitSlot;
        private float expiresAt;
        private float nextRefreshAt;
        private Vector2 targetSnapshot;
        private Vector2 destination;
        private bool active;

        public bool IsActive => active;
        public Pattern CurrentPattern => pattern;
        public Vector2 Destination => destination;
        public bool ShouldMove => active && pattern != Pattern.Hold;

        public static bool Supports(string equipmentId) =>
            equipmentId == "skill.character.black_cloth_raider.1.basic_attack.black_knife_cut" ||
            equipmentId == "skill.character.chain_dragger_raider.1.basic_attack.chain_swing";

        public void Enter(string equipmentId, int stableActorId, int attackSequence, Vector2 self,
            Vector2 target, float attackRange, Vector2 spawn, float time)
        {
            uint seed = unchecked((uint)stableActorId * 2654435761u) ^
                        unchecked((uint)(attackSequence + 1) * 2246822519u);
            pattern = SelectPattern(equipmentId, seed, lastPattern, secondLastPattern);
            orbitSlot = (int)((seed >> 16) & 3u);
            secondLastPattern = lastPattern;
            lastPattern = pattern;
            expiresAt = time + ResolveTimeout(equipmentId, pattern);
            nextRefreshAt = time + RefreshInterval;
            targetSnapshot = target;
            destination = ResolveDestination(equipmentId, pattern, orbitSlot, self, target, attackRange, spawn);
            active = true;
        }

        public bool ShouldRefresh(float time, Vector2 target) => active &&
            time >= nextRefreshAt &&
            (target - targetSnapshot).sqrMagnitude >= TargetRefreshDistance * TargetRefreshDistance;

        public void Refresh(string equipmentId, Vector2 self, Vector2 target, float attackRange,
            Vector2 spawn, float time)
        {
            if (!active) return;
            targetSnapshot = target;
            destination = ResolveDestination(equipmentId, pattern, orbitSlot, self, target, attackRange, spawn);
            nextRefreshAt = time + RefreshInterval;
        }

        public bool IsExpired(float time) => !active || time >= expiresAt;

        public void Exit()
        {
            active = false;
            destination = Vector2.zero;
        }

        public static Pattern SelectPattern(string equipmentId, uint seed,
            Pattern previous, Pattern twoBack)
        {
            bool chain = equipmentId ==
                "skill.character.chain_dragger_raider.1.basic_attack.chain_swing";
            float[] weights = chain
                ? new[] { 20f, 30f, 35f, 15f }
                : new[] { 30f, 20f, 40f, 10f };
            weights[(int)previous] *= .25f;
            weights[(int)twoBack] *= .60f;
            if (previous == twoBack) weights[(int)previous] = 0f;
            float total = weights[0] + weights[1] + weights[2] + weights[3];
            float roll = (seed & 0xffffu) / 65535f * total;
            for (int i = 0; i < weights.Length; i++)
            {
                if (roll <= weights[i]) return (Pattern)i;
                roll -= weights[i];
            }
            return Pattern.Hold;
        }

        public static Vector2 ResolveDestination(string equipmentId, Pattern selected, int orbitIndex,
            Vector2 self, Vector2 target, float attackRange, Vector2 spawn)
        {
            bool chain = equipmentId ==
                "skill.character.chain_dragger_raider.1.basic_attack.chain_swing";
            Vector2 away = self - target;
            if (away.sqrMagnitude <= .0001f) away = Vector2.right;
            away.Normalize();
            Vector2 candidate;
            switch (selected)
            {
                case Pattern.Retreat:
                    candidate = self + away * (chain ? .50f : .60f);
                    break;
                case Pattern.ForwardPressure:
                    candidate = self - away * (chain ? .25f : .30f);
                    break;
                case Pattern.Orbit:
                    Vector2[] axes = { -away, new(-away.y, away.x), away, new(away.y, -away.x) };
                    candidate = target + axes[Mathf.Abs(orbitIndex) % axes.Length] *
                        (chain ? Mathf.Clamp(attackRange, .90f, 1.20f)
                               : Mathf.Clamp(attackRange, .72f, .95f));
                    break;
                default:
                    candidate = self;
                    break;
            }

            Vector2 fromSpawn = candidate - spawn;
            if (fromSpawn.sqrMagnitude > SpawnLeashRadius * SpawnLeashRadius)
                candidate = spawn + fromSpawn.normalized * SpawnLeashRadius;
            return candidate;
        }

        public static float ResolveSpeedMultiplier(string equipmentId, Pattern selected)
        {
            bool chain = equipmentId ==
                "skill.character.chain_dragger_raider.1.basic_attack.chain_swing";
            return selected switch
            {
                Pattern.Retreat => chain ? 1.15f : 1.20f,
                Pattern.ForwardPressure => chain ? 1.25f : 1.35f,
                Pattern.Orbit => .90f,
                _ => 0f
            };
        }

        private static float ResolveTimeout(string equipmentId, Pattern selected)
        {
            bool chain = equipmentId ==
                "skill.character.chain_dragger_raider.1.basic_attack.chain_swing";
            if (selected == Pattern.Hold) return chain ? .35f : .25f;
            if (selected == Pattern.Orbit) return chain ? .82f : .72f;
            if (selected == Pattern.Retreat) return chain ? .58f : .50f;
            return chain ? .27f : .23f;
        }
    }
}
