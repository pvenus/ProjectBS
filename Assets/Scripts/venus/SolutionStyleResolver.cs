using UnityEngine;

public static class SolutionStyleResolver
{
    public static SolutionStyle Resolve(ProblemFocus problemFocus, RoleVector roleVector)
    {
        switch (problemFocus)
        {
            case ProblemFocus.SelfPreservation:
                return ResolveSelfPreservation(roleVector);

            case ProblemFocus.AllyRescue:
                return ResolveAllyRescue(roleVector);

            case ProblemFocus.PressureMitigation:
                return ResolvePressureMitigation(roleVector);

            case ProblemFocus.OffensivePressure:
                return ResolveOffensivePressure(roleVector);

            case ProblemFocus.OpportunityCapture:
                return ResolveOpportunityCapture(roleVector);

            default:
                return SolutionStyle.None;
        }
    }

    private static SolutionStyle ResolveSelfPreservation(RoleVector roleVector)
    {
        if (roleVector.tank >= roleVector.support && roleVector.tank >= roleVector.dps)
            return SolutionStyle.DefensiveGuard;

        if (roleVector.support >= roleVector.tank && roleVector.support >= roleVector.dps)
            return SolutionStyle.HealingSupport;

        return SolutionStyle.ThreatElimination;
    }

    private static SolutionStyle ResolveAllyRescue(RoleVector roleVector)
    {
        if (roleVector.support >= roleVector.tank && roleVector.support >= roleVector.dps)
            return SolutionStyle.HealingSupport;

        if (roleVector.tank >= roleVector.support && roleVector.tank >= roleVector.dps)
            return SolutionStyle.AreaControl;

        return SolutionStyle.ThreatElimination;
    }

    private static SolutionStyle ResolvePressureMitigation(RoleVector roleVector)
    {
        if (roleVector.tank >= roleVector.support && roleVector.tank >= roleVector.dps)
            return SolutionStyle.AreaControl;

        if (roleVector.support >= roleVector.tank && roleVector.support >= roleVector.dps)
            return SolutionStyle.Stabilization;

        return SolutionStyle.DirectAttack;
    }

    private static SolutionStyle ResolveOffensivePressure(RoleVector roleVector)
    {
        if (roleVector.dps >= roleVector.tank && roleVector.dps >= roleVector.support)
            return SolutionStyle.OffensiveTempo;

        if (roleVector.tank >= roleVector.dps && roleVector.tank >= roleVector.support)
            return SolutionStyle.DirectAttack;

        return SolutionStyle.Stabilization;
    }

    private static SolutionStyle ResolveOpportunityCapture(RoleVector roleVector)
    {
        if (roleVector.dps >= roleVector.tank && roleVector.dps >= roleVector.support)
            return SolutionStyle.ThreatElimination;

        if (roleVector.support >= roleVector.tank && roleVector.support >= roleVector.dps)
            return SolutionStyle.OffensiveTempo;

        return SolutionStyle.AreaControl;
    }
}
