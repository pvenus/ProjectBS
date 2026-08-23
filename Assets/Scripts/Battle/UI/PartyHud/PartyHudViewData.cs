using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.UI.PartyHud
{
    public enum PartyHudSkillState
    {
        Available,
        Unavailable,
        Passive
    }

    public sealed class PartyHudSkillSlotData
    {
        public Sprite Icon { get; }
        public float CooldownRemainingSeconds { get; }
        public float CooldownDurationSeconds { get; }
        public PartyHudSkillState State { get; }
        public bool IsBasicAttack { get; }

        public PartyHudSkillSlotData(
            Sprite icon,
            float cooldownRemainingSeconds,
            float cooldownDurationSeconds,
            PartyHudSkillState state,
            bool isBasicAttack = false)
        {
            Icon = icon;
            CooldownRemainingSeconds = Mathf.Max(0f, cooldownRemainingSeconds);
            CooldownDurationSeconds = Mathf.Max(0f, cooldownDurationSeconds);
            State = state;
            IsBasicAttack = isBasicAttack;
        }
    }

    public sealed class PartyHudMemberData
    {
        public string DisplayName { get; }
        public Sprite Portrait { get; }
        public float CurrentHp { get; }
        public float MaxHp { get; }
        public string StatusText { get; }
        public Color StatusColor { get; }
        public IReadOnlyList<PartyHudSkillSlotData> ActiveSkills { get; }
        public PartyHudSkillSlotData PassiveSkill { get; }

        public PartyHudMemberData(
            string displayName,
            Sprite portrait,
            float currentHp,
            float maxHp,
            string statusText,
            Color statusColor,
            IReadOnlyList<PartyHudSkillSlotData> activeSkills,
            PartyHudSkillSlotData passiveSkill)
        {
            DisplayName = displayName ?? string.Empty;
            Portrait = portrait;
            CurrentHp = Mathf.Max(0f, currentHp);
            MaxHp = Mathf.Max(0f, maxHp);
            StatusText = statusText ?? string.Empty;
            StatusColor = statusColor;
            ActiveSkills = activeSkills ?? Array.Empty<PartyHudSkillSlotData>();
            PassiveSkill = passiveSkill;
        }
    }

    public sealed class PartyHudViewData
    {
        public IReadOnlyList<PartyHudMemberData> Members { get; }

        public PartyHudViewData(IReadOnlyList<PartyHudMemberData> members)
        {
            Members = members ?? Array.Empty<PartyHudMemberData>();
        }
    }
}
