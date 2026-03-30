

using UnityEngine;

public static class DangerPerceptionResolver
{
    public static DangerPlotVector ResolveDelta(DangerEvent dangerEvent, DangerPerceptionProfile profile)
    {
        DangerPlotVector raw = dangerEvent.GetScaledDelta();

        if (profile == null)
            return raw;

        if (dangerEvent.IsRecoveryEvent())
            return profile.ApplyRecovery(raw);

        return profile.ApplyGain(raw);
    }

    public static void ApplyTo(AIVectorMono aiVector, DangerEvent dangerEvent, DangerPerceptionProfile profile)
    {
        if (aiVector == null)
            return;

        DangerPlotVector resolved = ResolveDelta(dangerEvent, profile);

        if (dangerEvent.IsRecoveryEvent())
            aiVector.ReduceDanger(resolved);
        else
            aiVector.AddDanger(resolved);
    }
}