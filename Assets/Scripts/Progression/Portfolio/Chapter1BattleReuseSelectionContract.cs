using System;
using System.Collections.Generic;
using System.Linq;

namespace Progression.Portfolio
{
    public static class Chapter1BattleReuseSelectionContract
    {
        public const string Event27 = "event.act1.random_event.27.paper_armor_bandits";
        public const string Event30 = "event.act1.random_event.30.night_beacon_intruders";
        public const string Event31 = Chapter1Event31SelectionContract.Event31Id;
        public const string Event41 = "event.act1.random_event.41.yujin_broken_arrow_fletching";
        public const string Event46 = "event.act1.random_event.46.funeral_without_black_cloth";
        public const string Original02 = "event.act1.random_event.02.rain_seller";
        public const string Original10 = "event.act1.random_event.10.silent_jangseung";
        public const string Original13 = "event.act1.random_event.13.empty_bride_palanquin";
        public const string Original17 = "event.act1.random_event.17.three_bowls_on_mountain_path";
        public const string Original09 = Chapter1Event31SelectionContract.OriginalEvent09Id;
        public const string Original04 = Chapter1Event31SelectionContract.OriginalEvent04Id;

        private static readonly (string Left, string Right)[] pairs =
        {
            (Original02, Event27),
            (Original17, Event27),
            (Original10, Event30),
            (Original13, Event46)
            ,(Original09, Event31)
            ,(Original04, Event31)
            ,(Original10, Event41)
            ,(Event30, Event41)
        };

        public static bool Conflicts(
            IEnumerable<PortfolioEventDescriptor> selected,
            PortfolioEventDescriptor next)
        {
            if (next == null) return true;
            HashSet<string> ids = new(
                (selected ?? Array.Empty<PortfolioEventDescriptor>())
                    .Where(item => item != null).Select(item => item.EventId),
                StringComparer.Ordinal);
            foreach ((string left, string right) in pairs)
            {
                if ((string.Equals(next.EventId, left, StringComparison.Ordinal)
                        && ids.Contains(right))
                    || (string.Equals(next.EventId, right, StringComparison.Ordinal)
                        && ids.Contains(left)))
                    return true;
            }
            return false;
        }
    }
}
