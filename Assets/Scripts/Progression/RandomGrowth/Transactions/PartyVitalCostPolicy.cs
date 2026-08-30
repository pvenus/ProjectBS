using System;
using System.Collections.Generic;
using System.Linq;

namespace Progression
{
    public static class PartyVitalCostPolicy
    {
        public static PartyVitalCostPlan Evaluate(IEnumerable<PartyVitalSnapshot> source)
        {
            PartyVitalSnapshot[] roster = (source ?? Array.Empty<PartyVitalSnapshot>()).ToArray();
            if (roster.Length == 0
                || roster.Any(member => member == null || string.IsNullOrWhiteSpace(member.MemberId))
                || roster.Select(member => member.MemberId).Distinct(StringComparer.Ordinal).Count() != roster.Length)
            {
                return new PartyVitalCostPlan(false, "InvalidRoster", Array.Empty<PartyVitalMutation>());
            }

            if (roster.Any(member => !IsValidMax(member.MaxHp) || !IsValidCurrent(member.CurrentHp)))
            {
                return new PartyVitalCostPlan(false, "InvalidHpState", Array.Empty<PartyVitalMutation>());
            }

            if (roster.Any(member => !TryCalculateCost(member.MaxHp, out _)))
            {
                return new PartyVitalCostPlan(false, "InvalidHpState", Array.Empty<PartyVitalMutation>());
            }

            PartyVitalMutation[] mutations = roster.Select(member =>
            {
                TryCalculateCost(member.MaxHp, out int cost);
                return new PartyVitalMutation(member.MemberId, member.CurrentHp,
                    member.CurrentHp - cost, cost, member.MaxHp);
            }).ToArray();
            if (mutations.Any(item => item.After < 1))
            {
                return new PartyVitalCostPlan(false, "CannotPayAndSurvive", mutations);
            }
            return new PartyVitalCostPlan(true, string.Empty, mutations);
        }

        private static bool IsValidMax(float value) =>
            CanonicalFloatBits.IsFinite(value)
            && !CanonicalFloatBits.IsNegativeZero(value)
            && !CanonicalFloatBits.IsPositiveSubnormal(value)
            && value >= 1f;

        private static bool IsValidCurrent(float value) =>
            CanonicalFloatBits.IsFinite(value)
            && !CanonicalFloatBits.IsNegativeZero(value)
            && value > 0f;

        private static bool TryCalculateCost(float maxHp, out int cost)
        {
            cost = 0;
            double roundedCost = Math.Ceiling((double)maxHp * 0.10d);
            if (roundedCost < 1d || roundedCost > int.MaxValue) return false;
            cost = (int)roundedCost;
            return true;
        }
    }
}
