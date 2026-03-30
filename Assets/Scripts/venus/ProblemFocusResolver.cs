using UnityEngine;

public static class ProblemFocusResolver
{
    private const float OffensivePressureThreshold = 15f;

    public static ProblemFocus Resolve(DangerPlotVector danger)
    {
        return Resolve(danger, new RoleVector(0.33f, 0.33f, 0.33f));
    }

    public static ProblemFocus Resolve(DangerPlotVector danger, RoleVector roleVector)
    {
        DangerAxis dominantAxis = GetDominantAxis(danger);
        float dominantValue = GetAxisValue(danger, dominantAxis);

        if (dominantValue < OffensivePressureThreshold)
            return ProblemFocus.OffensivePressure;

        ProblemFocusScores scores = BuildScores(danger, roleVector);
        return GetBestProblemFocus(scores);
    }

    public static DangerAxis GetDominantAxis(DangerPlotVector danger)
    {
        if (danger.MaxValue <= 0.0001f)
            return DangerAxis.None;

        return danger.GetDominantAxis();
    }

    public static float GetAxisValue(DangerPlotVector danger, DangerAxis axis)
    {
        return danger.GetAxisValue(axis);
    }

    private static ProblemFocusScores BuildScores(DangerPlotVector danger, RoleVector roleVector)
    {
        ProblemFocusScores scores = new ProblemFocusScores
        {
            selfPreservation = CalculateSelfPreservationScore(danger, roleVector),
            allyRescue = CalculateAllyRescueScore(danger, roleVector),
            pressureMitigation = CalculatePressureMitigationScore(danger, roleVector),
            offensivePressure = CalculateOffensivePressureScore(danger, roleVector),
            opportunityCapture = CalculateOpportunityCaptureScore(danger, roleVector)
        };

        return scores;
    }

    private static float CalculateSelfPreservationScore(DangerPlotVector danger, RoleVector roleVector)
    {
        return danger.immediate + (roleVector.tank * 8f) + (danger.ambient * 0.15f);
    }

    private static float CalculateAllyRescueScore(DangerPlotVector danger, RoleVector roleVector)
    {
        return danger.linked + (roleVector.support * 8f) + (danger.ambient * 0.1f);
    }

    private static float CalculatePressureMitigationScore(DangerPlotVector danger, RoleVector roleVector)
    {
        return danger.ambient + (roleVector.tank * 6f) + (danger.immediate * 0.2f);
    }

    private static float CalculateOffensivePressureScore(DangerPlotVector danger, RoleVector roleVector)
    {
        float totalDanger = danger.Sum;
        float baseDrive = Mathf.Max(0f, 20f - totalDanger);
        return baseDrive + (roleVector.dps * 10f);
    }

    private static float CalculateOpportunityCaptureScore(DangerPlotVector danger, RoleVector roleVector)
    {
        float totalDanger = danger.Sum;
        float opportunityWindow = Mathf.Max(0f, 25f - totalDanger);
        return opportunityWindow + (roleVector.dps * 6f) + (roleVector.support * 2f);
    }

    private static ProblemFocus GetBestProblemFocus(ProblemFocusScores scores)
    {
        ProblemFocus bestFocus = ProblemFocus.None;
        float bestScore = float.NegativeInfinity;

        TrySetBest(ref bestFocus, ref bestScore, ProblemFocus.SelfPreservation, scores.selfPreservation);
        TrySetBest(ref bestFocus, ref bestScore, ProblemFocus.AllyRescue, scores.allyRescue);
        TrySetBest(ref bestFocus, ref bestScore, ProblemFocus.PressureMitigation, scores.pressureMitigation);
        TrySetBest(ref bestFocus, ref bestScore, ProblemFocus.OffensivePressure, scores.offensivePressure);
        TrySetBest(ref bestFocus, ref bestScore, ProblemFocus.OpportunityCapture, scores.opportunityCapture);

        return bestFocus;
    }

    private static void TrySetBest(ref ProblemFocus bestFocus, ref float bestScore, ProblemFocus candidateFocus, float candidateScore)
    {
        if (candidateScore <= bestScore)
            return;

        bestFocus = candidateFocus;
        bestScore = candidateScore;
    }

    private struct ProblemFocusScores
    {
        public float selfPreservation;
        public float allyRescue;
        public float pressureMitigation;
        public float offensivePressure;
        public float opportunityCapture;
    }
}