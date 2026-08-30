using System;

namespace Progression
{
    public sealed class RandomGrowthEventRouteBinding
    {
        private RandomGrowthEventRouteBinding(RandomGrowthEventIdentity identity)
        {
            Identity = identity;
            TransactionDomain = identity.PayloadKind == RandomGrowthPayloadKind.Safe
                ? "random-growth-safe" : identity.PayloadKind == RandomGrowthPayloadKind.Risk
                    ? "random-growth-risk" : "random-growth-decline";
        }

        public RandomGrowthEventIdentity Identity { get; }
        public string TransactionDomain { get; }
        public string StableRouteKey => TransactionDomain + "\n" + Identity.RouteId;

        public static bool TryCreate(string eventId, string nodeId, string reservationId,
            string choiceId, string resultId, RandomGrowthPayloadKind payloadKind,
            out RandomGrowthEventRouteBinding binding)
        {
            binding = null;
            if (!RandomGrowthEventIdentityCatalog.TryResolve(eventId, choiceId, payloadKind,
                    out RandomGrowthEventIdentity identity)
                || !string.Equals(identity.NodeId, nodeId, StringComparison.Ordinal)
                || !string.Equals(identity.ReservationId, reservationId, StringComparison.Ordinal)
                || !string.Equals(identity.ResultId, resultId, StringComparison.Ordinal))
                return false;
            binding = new RandomGrowthEventRouteBinding(identity);
            return true;
        }
    }
}
