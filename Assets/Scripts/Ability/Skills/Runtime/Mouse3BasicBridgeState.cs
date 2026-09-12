using System.Collections.Generic;
using UnityEngine;

namespace Skill
{
    /// <summary>Transient, non-save combat bridge from Thunder Command L5 to the next Basic step 1.</summary>
    public static class Mouse3BasicBridgeState
    {
        public readonly struct Bonus
        {
            public readonly float ForwardRatio;
            public readonly float RangeRatio;
            public readonly float InputGrace;
            public Bonus(float forwardRatio, float rangeRatio, float inputGrace)
            {
                ForwardRatio = Mathf.Max(0f, forwardRatio);
                RangeRatio = Mathf.Max(0f, rangeRatio);
                InputGrace = Mathf.Max(0f, inputGrace);
            }
            public bool Enabled => ForwardRatio > 0f || RangeRatio > 0f || InputGrace > 0f;
        }

        private static readonly Dictionary<int, Bonus> Pending = new();

        public static void Arm(GameObject caster, ResolvedMouse3SkillProfile profile)
        {
            if (caster == null || profile == null) return;
            Bonus bonus = new(profile.nextBasicForwardRatio, profile.nextBasicRangeRatio,
                profile.nextBasicInputGrace);
            if (!bonus.Enabled) return;
            Pending[caster.transform.root.GetInstanceID()] = bonus;
            Mouse3BasicBridgeCleanupMono cleanup = caster.transform.root.GetComponent<Mouse3BasicBridgeCleanupMono>() ??
                caster.transform.root.gameObject.AddComponent<Mouse3BasicBridgeCleanupMono>();
            cleanup.Bind(caster.transform.root);
        }

        public static bool TryConsume(Transform caster, out Bonus bonus)
        {
            bonus = default;
            if (caster == null) return false;
            int key = caster.root.GetInstanceID();
            if (!Pending.TryGetValue(key, out bonus)) return false;
            Pending.Remove(key);
            return bonus.Enabled;
        }

        public static void Clear(Transform caster)
        {
            if (caster != null) Pending.Remove(caster.root.GetInstanceID());
        }
    }

    [DisallowMultipleComponent]
    internal sealed class Mouse3BasicBridgeCleanupMono : MonoBehaviour
    {
        private Transform ownerRoot;
        internal void Bind(Transform root) => ownerRoot = root;
        private void OnDisable() => Mouse3BasicBridgeState.Clear(ownerRoot != null ? ownerRoot : transform);
        private void OnDestroy() => Mouse3BasicBridgeState.Clear(ownerRoot != null ? ownerRoot : transform);
    }
}
