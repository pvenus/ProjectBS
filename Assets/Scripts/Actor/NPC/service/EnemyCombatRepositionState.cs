using UnityEngine;

namespace Npc.Service
{
    /// <summary>Pure enemy-only attack-gap steering. Movement remains owned by NpcPathing.</summary>
    public sealed class EnemyCombatRepositionState
    {
        public const float MinDwell = 0.45f;
        public const float MaxDwell = 0.75f;
        public const float RepathInterval = 0.20f;
        public const float LateralSpeedMultiplier = 0.70f;
        public const float RadialSpeedMultiplier = 0.55f;
        public const float MeleeBandMin = 0.72f;
        public const float MeleeBandMax = 0.95f;
        public const float MeleeRetreatLimit = 1.05f;
        public const float MinDesiredDelta = 0.04f;
        public const float DirectionHysteresisDegrees = 20f;
        public const float BlockedFlipDelay = 0.30f;
        public const float CrowdRadius = 0.65f;
        public const float MaxCrowdContribution = 0.35f;
        public const int MaxCrowdScan = 16;

        public readonly struct Result
        {
            public Result(bool shouldMove, Vector2 direction, float speedMultiplier)
            {
                ShouldMove = shouldMove;
                Direction = direction;
                SpeedMultiplier = speedMultiplier;
            }

            public bool ShouldMove { get; }
            public Vector2 Direction { get; }
            public float SpeedMultiplier { get; }
        }

        private int _side = 1;
        private float _dwellUntil;
        private float _nextRepath;
        private float _blockedSince = -1f;
        private Vector2 _cachedDirection;
        private bool _active;

        public bool IsActive => _active;

        public void Enter(int stableId, float time)
        {
            if (_active) return;
            _active = true;
            uint hash = unchecked((uint)stableId * 2654435761u);
            _side = (hash & 1u) == 0u ? -1 : 1;
            float t = ((hash >> 8) & 0xffffu) / 65535f;
            _dwellUntil = time + Mathf.Lerp(MinDwell, MaxDwell, t);
            _nextRepath = time;
            _blockedSince = -1f;
            _cachedDirection = Vector2.zero;
        }

        public void Exit()
        {
            _active = false;
            _blockedSince = -1f;
            _cachedDirection = Vector2.zero;
        }

        public Result EvaluateMelee(
            Vector2 self,
            Vector2 target,
            float attackRange,
            Vector2 crowdContribution,
            float time,
            bool blocked)
        {
            Vector2 fromTarget = self - target;
            float distance = fromTarget.magnitude;
            Vector2 radial = distance > 0.0001f ? fromTarget / distance : Vector2.right;

            UpdateBlockedState(time, blocked);
            if (time < _nextRepath && _cachedDirection.sqrMagnitude > 0.0001f)
                return new Result(true, _cachedDirection, LateralSpeedMultiplier);

            _nextRepath = time + RepathInterval;
            float range = Mathf.Max(0.05f, attackRange);
            Vector2 tangent = new Vector2(-radial.y, radial.x) * _side;
            Vector2 desired;
            float multiplier;

            if (distance > range * MeleeBandMax)
            {
                desired = -radial;
                multiplier = RadialSpeedMultiplier;
            }
            else if (distance < range * MeleeBandMin && distance < range * MeleeRetreatLimit)
            {
                desired = radial;
                multiplier = RadialSpeedMultiplier;
            }
            else
            {
                desired = tangent;
                multiplier = LateralSpeedMultiplier;
            }

            desired = ApplyCrowdAndHysteresis(desired, crowdContribution);
            _cachedDirection = desired;
            return new Result(desired.sqrMagnitude > 0.0001f, desired, multiplier);
        }

        public Result EvaluateRangedBand(
            Vector2 self,
            Vector2 target,
            Vector2 crowdContribution,
            float time,
            bool blocked)
        {
            Vector2 radial = self - target;
            radial = radial.sqrMagnitude > 0.0001f ? radial.normalized : Vector2.right;
            UpdateBlockedState(time, blocked);
            if (time >= _nextRepath || _cachedDirection.sqrMagnitude <= 0.0001f)
            {
                _nextRepath = time + RepathInterval;
                Vector2 tangent = new Vector2(-radial.y, radial.x) * _side;
                _cachedDirection = ApplyCrowdAndHysteresis(tangent, crowdContribution);
            }
            return new Result(_cachedDirection.sqrMagnitude > 0.0001f, _cachedDirection, LateralSpeedMultiplier);
        }

        private void UpdateBlockedState(float time, bool blocked)
        {
            if (!blocked)
            {
                _blockedSince = -1f;
                return;
            }
            if (_blockedSince < 0f) _blockedSince = time;
            if (time - _blockedSince >= BlockedFlipDelay && time >= _dwellUntil)
            {
                _side = -_side;
                _blockedSince = -1f;
                _dwellUntil = time + MinDwell;
                _nextRepath = 0f;
            }
        }

        private Vector2 ApplyCrowdAndHysteresis(Vector2 desired, Vector2 crowd)
        {
            crowd = Vector2.ClampMagnitude(crowd, MaxCrowdContribution);
            Vector2 next = desired + crowd;
            if (next.sqrMagnitude <= MinDesiredDelta * MinDesiredDelta) return Vector2.zero;
            next.Normalize();
            if (_cachedDirection.sqrMagnitude > 0.0001f &&
                Vector2.Angle(_cachedDirection, next) < DirectionHysteresisDegrees)
                return _cachedDirection;
            return next;
        }
    }
}
