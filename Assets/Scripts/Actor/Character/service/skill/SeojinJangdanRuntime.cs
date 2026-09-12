using System.Collections.Generic;
using UnityEngine;

namespace Character.Skill
{
    internal enum JangdanStage { None, DungArmed, GiArmed, SequenceCommitted }

    /// <summary>Single authoritative input-chain state for Seojin G1's jangdan skills.</summary>
    internal static class SeojinJangdanRuntime
    {
        internal const string RhythmId = "jangdan.seojin.g1.v1";
        internal const float Bpm = 120f;
        internal const float Beat = .5f;
        internal const float Subdivision = .125f;
        internal const float DungHoldMax = 1.5f;
        internal const float DungHoldRefresh = .125f;
        internal const float DungHoldBossMax = .375f;
        internal const float SequenceInputWindow = 1f;
        internal const float SequenceContactEnd = .25f;
        internal const float SequenceRecovery = .10f;
        internal const float SequenceSetDuration = SequenceContactEnd + SequenceRecovery;
        internal const string DungId = "skill.character.seojin.1.active_5.jangdan_dung";
        internal const string GiId = "skill.character.seojin.1.active_6.jangdan_gi";
        internal const string DeokId = "skill.character.seojin.1.active_7.jangdan_deok";
        internal const string SequenceId = "skill.character.seojin.1.active_8.deoreoreoreo";

        internal static int Level(EquipmentSkillRuntimeData runtime)
            => Mathf.Clamp(runtime != null ? runtime.resolvedLevel : 1, 1, 5);
        internal static float DungWindow(int level) => level >= 4 ? .875f : .75f;
        internal static float DeokWindow(int level) => level >= 4 ? .75f : .625f;
        internal static float GiRetentionBonus(int level) => level >= 5 ? .125f : 0f;
        internal static float QRetention(int level) => level >= 5 ? 1.125f : 1f;
        internal static float DungPull(int level) => level >= 3 ? 2.3f : 2f;
        internal static float KungStun(int level, bool boss) => level >= 5 ? (boss ? .20f : .65f) : (boss ? .15f : .50f);
        internal static float DeokStun(int level, bool paired, bool boss)
            => paired ? level >= 5 ? (boss ? .30f : 1.20f) : (boss ? .25f : 1f) : (boss ? .10f : .30f);
        internal static float QFinalStun(int level, bool boss)
            => level >= 5 ? (boss ? .20f : .75f) : (boss ? .15f : .60f);

        private sealed class State
        {
            internal JangdanStage stage;
            internal float expiresAt;
            internal float dungHoldExpiresAt;
            internal float dungHoldArmedAt;
            internal float lastPairCompletedAt = float.NegativeInfinity;
            internal int executionId;
            internal Vector2 point;
            internal Transform target;
            internal readonly List<CharacterManager> heldTargets = new();
            internal int sequenceNextStep;
            internal float sequenceExpiresAt;
            internal string sequenceToken;
            internal bool sequenceAssist;
        }

        private static readonly Dictionary<int, State> States = new();

        internal static bool IsJangdan(string id) => id == DungId || id == GiId || id == DeokId || id == SequenceId;

        internal static JangdanStage GetStage(Transform caster)
        {
            State state = Get(caster, false);
            if (state == null) return JangdanStage.None;
            if (state.stage != JangdanStage.None && Time.time > state.expiresAt)
                Clear(caster);
            return state.stage;
        }

        internal static bool IsCooldownLockedFollowup(Transform caster, string id)
        {
            JangdanStage stage = GetStage(caster);
            return id == DungId && stage == JangdanStage.DungArmed ||
                   id == DeokId && stage == JangdanStage.GiArmed;
        }

        internal static void Arm(Transform caster, JangdanStage stage, float duration,
            int executionId, Transform target, Vector2 point)
        {
            State state = Get(caster, true);
            state.stage = stage;
            state.expiresAt = Time.time + Mathf.Max(0f, duration);
            state.executionId = executionId;
            state.target = target;
            state.point = point;
            if (stage == JangdanStage.DungArmed)
            {
                state.dungHoldArmedAt = Time.time;
                state.dungHoldExpiresAt = Time.time + DungHoldMax;
            }
        }

        internal static void RegisterDungHoldTarget(Transform caster, CharacterManager target)
        {
            if (caster == null || target == null) return;
            State state = Get(caster, true);
            if (!state.heldTargets.Contains(target)) state.heldTargets.Add(target);
        }

        internal static void RefreshDungHold(Transform caster)
        {
            State state = Get(caster, false);
            if (state == null || Time.time >= state.dungHoldExpiresAt) return;
            for (int i = state.heldTargets.Count - 1; i >= 0; i--)
            {
                CharacterManager target = state.heldTargets[i];
                if (target == null || !target.isActiveAndEnabled)
                {
                    state.heldTargets.RemoveAt(i);
                    continue;
                }
                bool boss = target.RuntimeData?.characterSO != null &&
                    target.RuntimeData.characterSO.CharacterType == CharacterType.Boss;
                float holdEnd = boss
                    ? Mathf.Min(state.dungHoldExpiresAt, state.dungHoldArmedAt + DungHoldBossMax)
                    : state.dungHoldExpiresAt;
                float remaining = Mathf.Max(0f, holdEnd - Time.time);
                float duration = Mathf.Min(remaining,
                    boss ? DungHoldBossMax : DungHoldRefresh * 2f);
                if (duration > 0f)
                    target.SetStat(Stat.StatType.StunDuration,
                        Mathf.Max(target.GetStatValue(Stat.StatType.StunDuration), duration));
            }
        }

        internal static bool IsDungHoldActive(Transform caster)
        {
            State state = Get(caster, false);
            return state != null && Time.time < state.dungHoldExpiresAt;
        }

        internal static void CompleteDungHold(Transform caster)
        {
            State state = Get(caster, false);
            if (state == null) return;
            state.dungHoldExpiresAt = 0f;
            state.dungHoldArmedAt = 0f;
            state.heldTargets.Clear();
        }

        internal static void CompletePair(Transform caster)
        {
            State state = Get(caster, true);
            state.lastPairCompletedAt = Time.time;
            state.stage = JangdanStage.None;
            state.expiresAt = 0f;
        }

        internal static bool ConsumeSequenceAssist(Transform caster, float retention)
        {
            State state = Get(caster, false);
            bool active = state != null && Time.time - state.lastPairCompletedAt <= Mathf.Max(0f, retention);
            if (state != null) state.lastPairCompletedAt = float.NegativeInfinity;
            return active;
        }

        internal static bool TryGetSequenceStep(
            Transform caster,
            int executionId,
            float assistRetention,
            out int stepIndex,
            out string comboToken,
            out bool assist)
        {
            stepIndex = 0;
            comboToken = null;
            assist = false;
            if (caster == null) return false;

            State state = Get(caster, true);
            if (state.stage != JangdanStage.SequenceCommitted ||
                Time.time > state.sequenceExpiresAt)
            {
                state.stage = JangdanStage.SequenceCommitted;
                state.sequenceNextStep = 0;
                state.sequenceToken = $"jangdan-q-{caster.GetInstanceID()}-{executionId}";
                state.sequenceAssist = Time.time - state.lastPairCompletedAt <=
                    Mathf.Max(0f, assistRetention);
                state.lastPairCompletedAt = float.NegativeInfinity;
            }

            state.executionId = executionId;
            state.expiresAt = state.sequenceExpiresAt = Time.time + SequenceInputWindow;
            stepIndex = Mathf.Clamp(state.sequenceNextStep, 0, 3);
            comboToken = state.sequenceToken;
            assist = state.sequenceAssist;
            return true;
        }

        internal static void CommitSequenceStep(Transform caster, int completedStep)
        {
            State state = Get(caster, false);
            if (state == null || state.stage != JangdanStage.SequenceCommitted ||
                completedStep != state.sequenceNextStep)
                return;

            if (completedStep >= 3)
            {
                state.stage = JangdanStage.None;
                state.sequenceNextStep = 0;
                state.sequenceExpiresAt = 0f;
                state.expiresAt = 0f;
                state.sequenceToken = null;
                state.sequenceAssist = false;
                return;
            }

            state.sequenceNextStep = completedStep + 1;
            state.sequenceExpiresAt = state.expiresAt = Time.time + SequenceInputWindow;
        }

        internal static void CancelSequenceProgress(Transform caster)
        {
            State state = Get(caster, false);
            if (state == null || state.stage != JangdanStage.SequenceCommitted) return;
            state.stage = JangdanStage.None;
            state.sequenceNextStep = 0;
            state.sequenceExpiresAt = 0f;
            state.expiresAt = 0f;
            state.sequenceToken = null;
            state.sequenceAssist = false;
        }

        internal static void Clear(Transform caster)
        {
            if (caster == null) return;
            ProjectileEntity.DespawnVisualsFor(caster.gameObject, DungId);
            States.Remove(caster.GetInstanceID());
        }

        private static State Get(Transform caster, bool create)
        {
            if (caster == null) return null;
            int id = caster.GetInstanceID();
            if (!States.TryGetValue(id, out State state) && create)
            {
                state = new State();
                States.Add(id, state);
            }
            return state;
        }
    }
}
