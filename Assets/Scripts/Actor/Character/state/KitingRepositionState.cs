using System;
using System.Collections.Generic;
using Party;
using Stat;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Character.Skill
{
    public sealed class KitingRepositionState : ICharacterActionState
    {
        private const float ReadinessCadence = .10f;
        private const float ThreatCadence = .20f;
        private const float EntryDebounce = .15f;
        private const float DestinationLock = .45f;
        private const float MaxLeg = 1.20f;
        private const float InterLegHold = .20f;
        private const float ArriveRadius = .12f;

        private readonly Profile profile;
        private PartyMovementMono partyMovement;
        private Transform threat;
        private Vector2 destination;
        private Vector2 entryPosition;
        private float enteredAt;
        private float nextReadiness;
        private float nextThreat;
        private float destinationLockedUntil;
        private float holdUntil;
        private float lastProgressTime;
        private Vector2 lastProgressPosition;
        private int stuckFailures;
        private int orbitSign;
        private float orbitSignUntil;
        private float outwardUsed;
        private bool ownsMovement;
        private bool hasDestination;

        public bool IsFinished { get; private set; }

        public static bool TryCreate(CharacterActionContext context, out KitingRepositionState state)
        {
            state = null;
            if (!IsBattleScene()) return false;
            CharacterSO character = context?.CharacterManager?.RuntimeData?.characterSO;
            if (character == null || character.CharacterType != CharacterType.Player) return false;
            string id = character.CharacterId ?? string.Empty;
            Profile resolved = id.IndexOf("seojin", StringComparison.OrdinalIgnoreCase) >= 0
                ? Profile.Seojin
                : id.IndexOf("jihan", StringComparison.OrdinalIgnoreCase) >= 0
                    ? Profile.Jihan
                    : id.IndexOf("yujin", StringComparison.OrdinalIgnoreCase) >= 0
                        ? Profile.Yujin
                        : null;
            if (resolved == null) return false;
            state = new KitingRepositionState(resolved);
            return true;
        }

        private KitingRepositionState(Profile value) => profile = value;

        public void Enter(CharacterActionContext context)
        {
            IsFinished = false;
            enteredAt = Time.time;
            nextReadiness = enteredAt;
            nextThreat = enteredAt;
            holdUntil = enteredAt + EntryDebounce;
            entryPosition = context != null && context.OwnerTransform != null
                ? (Vector2)context.OwnerTransform.position : Vector2.zero;
            lastProgressPosition = entryPosition;
            lastProgressTime = enteredAt;
            orbitSign = ResolveStableSign(context);
            orbitSignUntil = enteredAt + 1.20f;
            partyMovement = context?.Owner != null
                ? context.Owner.GetComponent<PartyMovementMono>() ?? context.Owner.GetComponentInChildren<PartyMovementMono>()
                : null;
            ownsMovement = partyMovement == null || partyMovement.TryAcquireExternalMovement(this);
            if (!ownsMovement) Finish(context);
        }

        public void Tick(CharacterActionContext context, float deltaTime)
        {
            if (IsFinished || context == null || !ownsMovement) return;
            if (MustYieldMovement(context)) { Finish(context); return; }

            float now = Time.time;
            // Readiness is intentionally sampled every decision frame. The
            // cadence below throttles only threat/destination recomputation;
            // it must never delay the ready-skill handoff.
            OffensiveReadinessSnapshot readiness = Snapshot(context);
            if (readiness.hasUsableOffensive) { Finish(context); return; }
            if (now >= nextReadiness)
            {
                nextReadiness = now + ReadinessCadence;
                if (now >= nextThreat || !IsThreatValid(context, threat))
                {
                    threat = ResolveNearestThreat(context);
                    nextThreat = now + ThreatCadence;
                }
                if (now >= holdUntil && (!hasDestination || now >= destinationLockedUntil))
                {
                    hasDestination = TryChooseDestination(context, readiness, now, out destination);
                    destinationLockedUntil = now + DestinationLock;
                }
            }

            if (now - enteredAt >= MaxLeg)
            {
                context.MovementExecutionService?.StopMovement(context);
                if (holdUntil < now) holdUntil = now + InterLegHold;
                if (now >= holdUntil) { enteredAt = now; hasDestination = false; }
                return;
            }
            if (now < holdUntil || !hasDestination)
            {
                context.MovementExecutionService?.StopMovement(context);
                return;
            }

            float speed = context.CharacterManager != null
                ? context.CharacterManager.GetStatValue(StatType.MoveSpeed) : 0f;
            context.MovementExecutionService?.MoveTowardPoint(
                context, destination, speed, ArriveRadius);
            DetectStuck(context, now);
        }

        public void Exit(CharacterActionContext context)
        {
            context?.MovementExecutionService?.StopMovement(context);
            if (partyMovement != null) partyMovement.ReleaseExternalMovement(this);
            ownsMovement = false;
            hasDestination = false;
        }

        private void Finish(CharacterActionContext context)
        {
            if (IsFinished) return;
            Exit(context);
            IsFinished = true;
        }

        private bool MustYieldMovement(CharacterActionContext context)
        {
            if (context.Owner == null || !context.Owner.activeInHierarchy ||
                context.CharacterManager == null || !context.CharacterManager.IsTargetable ||
                !context.CharacterManager.CanMove || !context.CharacterManager.CanUseSkill)
                return true;
            if (partyMovement != null && partyMovement.HasRecentManualInput()) return true;
            if (context.AnimationMono != null && context.AnimationMono.IsPlayingAttack()) return true;
            return false;
        }

        private OffensiveReadinessSnapshot Snapshot(CharacterActionContext context)
        {
            bool validTarget = IsThreatValid(context, context.CurrentTarget);
            return context.SkillManager != null
                ? context.SkillManager.QueryOffensiveReadiness(
                    context.StateService == null || context.StateService.CanUseActiveSkill,
                    context.CharacterManager != null && context.CharacterManager.CanUseSkill,
                    validTarget)
                : default;
        }

        private bool TryChooseDestination(
            CharacterActionContext context,
            OffensiveReadinessSnapshot readiness,
            float now,
            out Vector2 result)
        {
            result = context.OwnerTransform.position;
            if (threat == null)
            {
                result = ClampToBounds(ResolveAllyCentroid(context), .60f);
                return Vector2.Distance(result, context.OwnerTransform.position) > ArriveRadius;
            }

            Vector2 self = context.OwnerTransform.position;
            Vector2 threatPos = threat.position;
            Vector2 away = (self - threatPos).normalized;
            if (away.sqrMagnitude < .001f) away = Vector2.right;
            Vector2 tangent = new Vector2(-away.y, away.x) * orbitSign;
            float distance = Vector2.Distance(self, threatPos);
            ResolveBand(readiness.resolvedRange, out float bandMin, out float bandMax);
            float step = readiness.remainingCooldownSeconds <= .35f ? .45f :
                readiness.remainingCooldownSeconds <= 1.25f ? .72f : 1.0f;
            float retreatCap = profile.family == Family.Yujin ? 2f : 1.5f;
            bool hardDanger = distance <= profile.dangerRadius;
            bool outsideMelee = profile.family == Family.Seojin && distance > bandMax + .25f;

            var directions = new List<Vector2>(5);
            if (outsideMelee)
            {
                directions.Add((-away + tangent * .70f).normalized);
                directions.Add((-away - tangent * .70f).normalized);
            }
            else if (hardDanger && outwardUsed < retreatCap)
            {
                directions.Add((away + tangent * .70f).normalized);
                directions.Add((away - tangent * .70f).normalized);
                directions.Add(tangent);
                directions.Add(-tangent);
            }
            else
            {
                directions.Add(tangent);
                directions.Add(-tangent);
                directions.Add((away + tangent * .70f).normalized);
                directions.Add((away - tangent * .70f).normalized);
            }

            float best = float.MinValue;
            for (int i = 0; i < directions.Count; i++)
            {
                Vector2 candidate = ClampToBounds(self + directions[i] * step, .60f);
                if (!IsCandidateValid(context, candidate)) continue;
                float candidateDistance = Vector2.Distance(candidate, threatPos);
                float danger = Mathf.Clamp01((candidateDistance - profile.dangerRadius) / 2f);
                float band = candidateDistance >= bandMin && candidateDistance <= bandMax
                    ? 1f : 1f - Mathf.Clamp01(Mathf.Min(Mathf.Abs(candidateDistance - bandMin), Mathf.Abs(candidateDistance - bandMax)) / 2f);
                float cohesion = ScoreCohesion(context, candidate);
                float score = danger * .40f + band * .25f + .15f + cohesion * .10f + .10f;
                if (score > best + .0001f)
                {
                    best = score;
                    result = candidate;
                }
            }
            if (best == float.MinValue) return false;
            if (Vector2.Distance(result, threatPos) > distance) outwardUsed += Vector2.Distance(self, result);
            return true;
        }

        private void ResolveBand(float range, out float min, out float max)
        {
            if (profile.family == Family.Seojin)
            {
                min = Mathf.Clamp(range * .75f, .45f, .85f);
                max = Mathf.Clamp(range * 1.05f, .45f, .85f);
            }
            else if (profile.family == Family.Jihan) { min = 1.40f; max = 2.20f; }
            else
            {
                min = Mathf.Clamp(range * .55f, 2.60f, 5.50f);
                max = Mathf.Clamp(range * .70f, 2.60f, 5.50f);
            }
        }

        private bool IsCandidateValid(CharacterActionContext context, Vector2 candidate)
        {
            if (Vector2.Distance(candidate, ResolveAllyCentroid(context)) > profile.cohesionHard) return false;
            Collider2D[] hits = Physics2D.OverlapCircleAll(candidate, ArriveRadius);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i] == null || hits[i].transform.root == context.OwnerTransform.root) continue;
                CharacterManager character = hits[i].GetComponentInParent<CharacterManager>();
                if (character == null) return false;
            }
            RaycastHit2D[] path = Physics2D.LinecastAll(context.OwnerTransform.position, candidate);
            for (int i = 0; i < path.Length; i++)
            {
                if (path[i].collider == null || path[i].collider.transform.root == context.OwnerTransform.root) continue;
                CharacterManager character = path[i].collider.GetComponentInParent<CharacterManager>();
                if (character == null || character.gameObject.layer != context.Owner.layer) return false;
            }
            return true;
        }

        private static Vector2 ClampToBounds(Vector2 value, float inset)
        {
            Camera camera = Camera.main;
            if (camera == null || !camera.orthographic) return value;
            float halfHeight = Mathf.Max(0f, camera.orthographicSize - inset - .40f);
            float halfWidth = Mathf.Max(0f, halfHeight * camera.aspect);
            Vector2 center = camera.transform.position;
            return new Vector2(
                Mathf.Clamp(value.x, center.x - halfWidth, center.x + halfWidth),
                Mathf.Clamp(value.y, center.y - halfHeight, center.y + halfHeight));
        }

        private Transform ResolveNearestThreat(CharacterActionContext context)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(context.OwnerTransform.position, 16f);
            CharacterManager best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < hits.Length; i++)
            {
                CharacterManager candidate = hits[i] != null ? hits[i].GetComponentInParent<CharacterManager>() : null;
                if (candidate == null || !candidate.IsTargetable || candidate.transform.root == context.OwnerTransform.root ||
                    candidate.gameObject.layer == context.Owner.layer) continue;
                float distance = ((Vector2)candidate.transform.position - (Vector2)context.OwnerTransform.position).sqrMagnitude;
                if (distance < bestDistance || (Mathf.Approximately(distance, bestDistance) &&
                    candidate.transform.root.GetInstanceID() < best.transform.root.GetInstanceID()))
                { best = candidate; bestDistance = distance; }
            }
            if (context.CurrentTarget != null && IsThreatValid(context, context.CurrentTarget))
            {
                CharacterManager current = context.CurrentTarget.GetComponentInParent<CharacterManager>();
                if (current != null && ((Vector2)current.transform.position - (Vector2)context.OwnerTransform.position).sqrMagnitude <= bestDistance)
                    best = current;
            }
            return best != null ? best.transform : null;
        }

        private static bool IsThreatValid(CharacterActionContext context, Transform value)
        {
            CharacterManager manager = value != null ? value.GetComponentInParent<CharacterManager>() : null;
            return manager != null && manager.IsTargetable && context?.OwnerTransform != null &&
                   manager.transform.root != context.OwnerTransform.root && manager.gameObject.layer != context.Owner.layer;
        }

        private static Vector2 ResolveAllyCentroid(CharacterActionContext context)
        {
            IReadOnlyList<CharacterManager> members = PartyManager.Instance != null ? PartyManager.Instance.Members : null;
            Vector2 sum = Vector2.zero; int count = 0;
            if (members != null)
            {
                for (int i = 0; i < members.Count; i++)
                {
                    if (members[i] == null || !members[i].IsTargetable) continue;
                    sum += (Vector2)members[i].transform.position; count++;
                }
            }
            return count > 0 ? sum / count : (Vector2)context.OwnerTransform.position;
        }

        private float ScoreCohesion(CharacterActionContext context, Vector2 candidate)
        {
            float d = Vector2.Distance(candidate, ResolveAllyCentroid(context));
            return d <= profile.cohesionSoft ? 1f : 1f - Mathf.Clamp01((d - profile.cohesionSoft) /
                Mathf.Max(.01f, profile.cohesionHard - profile.cohesionSoft));
        }

        private void DetectStuck(CharacterActionContext context, float now)
        {
            Vector2 current = context.OwnerTransform.position;
            if (now - lastProgressTime < .30f) return;
            if (Vector2.Distance(current, lastProgressPosition) < .08f)
            {
                stuckFailures++;
                hasDestination = false;
                if (stuckFailures >= 2) holdUntil = now + .40f;
            }
            else stuckFailures = 0;
            lastProgressPosition = current;
            lastProgressTime = now;
        }

        private static int ResolveStableSign(CharacterActionContext context) =>
            context?.OwnerTransform != null && (context.OwnerTransform.root.GetInstanceID() & 1) == 0 ? 1 : -1;

        private static bool IsBattleScene()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            return string.Equals(sceneName, "BattleScene", StringComparison.Ordinal) ||
                   sceneName.IndexOf("battle", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private enum Family { Seojin, Jihan, Yujin }
        private sealed class Profile
        {
            public readonly Family family; public readonly float dangerRadius;
            public readonly float cohesionSoft; public readonly float cohesionHard;
            private Profile(Family f, float d, float soft, float hard)
            { family = f; dangerRadius = d; cohesionSoft = soft; cohesionHard = hard; }
            public static readonly Profile Seojin = new(Family.Seojin, .55f, 3.5f, 5f);
            public static readonly Profile Jihan = new(Family.Jihan, 1f, 3f, 4.5f);
            public static readonly Profile Yujin = new(Family.Yujin, 1.6f, 3.5f, 5.5f);
        }
    }
}
