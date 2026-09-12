using Character;
using Stat;
using UnityEngine;

namespace Skill
{
    [DisallowMultipleComponent]
    public sealed class Mouse3TimedMoveSpeedSlowMono : MonoBehaviour
    {
        private CharacterManager target;
        private float appliedPercent;
        private float expiresAt;

        public void Apply(float ratio, float duration)
        {
            target ??= GetComponent<CharacterManager>() ?? GetComponentInParent<CharacterManager>();
            if (target == null) return;
            float requested = -Mathf.Clamp01(ratio) * 100f;
            if (requested < appliedPercent)
            {
                target.AddStat(StatType.MoveSpeedPercent, requested - appliedPercent);
                appliedPercent = requested;
            }
            expiresAt = Mathf.Max(expiresAt, Time.time + Mathf.Max(0f, duration));
            enabled = true;
        }

        private void Update()
        {
            if (Time.time >= expiresAt) Restore();
        }

        private void OnDisable() => Restore();

        private void Restore()
        {
            if (target != null && !Mathf.Approximately(appliedPercent, 0f))
                target.AddStat(StatType.MoveSpeedPercent, -appliedPercent);
            appliedPercent = 0f;
            expiresAt = 0f;
            enabled = false;
        }
    }
}
