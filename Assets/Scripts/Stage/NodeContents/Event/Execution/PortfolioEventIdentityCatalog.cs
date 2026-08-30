using System;
using System.Collections.Generic;
using System.Linq;

namespace Stage
{
    public sealed class PortfolioEventIdentity
    {
        public PortfolioEventIdentity(int number, string slug)
        {
            Number = number;
            EventId = $"event.act1.random_event.{number}.{slug}";
            SourcePopupId = $"node.act1.random_event.{number}.{slug}.intro";
            CanonicalImagePath =
                $"Assets/ImagesGenerated/Stage/popup_main/{SourcePopupId}.main.png";
        }

        public int Number { get; }
        public string EventId { get; }
        public string SourcePopupId { get; }
        public string NodeId => SourcePopupId;
        public string CanonicalImagePath { get; }
    }

    public static class PortfolioEventIdentityCatalog
    {
        private static readonly IReadOnlyList<PortfolioEventIdentity> entries =
            Array.AsReadOnly(new[]
            {
                new PortfolioEventIdentity(21, "breath_between_water_drops"),
                new PortfolioEventIdentity(22, "sleeping_hawk_watch"),
                new PortfolioEventIdentity(23, "temple_hundred_eight_steps"),
                new PortfolioEventIdentity(24, "herb_scent_empty_barracks"),
                new PortfolioEventIdentity(25, "hot_spring_beneath_ice"),
                new PortfolioEventIdentity(26, "sleepless_waystation"),
                new PortfolioEventIdentity(27, "paper_armor_bandits"),
                new PortfolioEventIdentity(28, "rockfall_scouts"),
                new PortfolioEventIdentity(29, "chain_bridge_tollkeepers"),
                new PortfolioEventIdentity(30, "night_beacon_intruders"),
                new PortfolioEventIdentity(31, "wounded_mountain_tiger_domain"),
                new PortfolioEventIdentity(32, "hidden_ledger_salt_cart"),
                new PortfolioEventIdentity(33, "false_mountain_rite_offering_box"),
                new PortfolioEventIdentity(34, "half_vein_map"),
                new PortfolioEventIdentity(34, "half_vein_map.followup.unstable_vein"),
                new PortfolioEventIdentity(35, "ownerless_wage_sack"),
                new PortfolioEventIdentity(36, "cracked_bronze_mirror"),
                new PortfolioEventIdentity(37, "nameless_long_sword_in_rain"),
                new PortfolioEventIdentity(38, "three_cups_of_moonlight"),
                new PortfolioEventIdentity(39, "self_knotting_rope"),
                new PortfolioEventIdentity(40, "jihan_empty_medicine_folio"),
                new PortfolioEventIdentity(41, "yujin_broken_arrow_fletching"),
                new PortfolioEventIdentity(42, "twice_ringing_mountain_echo"),
                new PortfolioEventIdentity(43, "reverse_growing_moss_marker"),
                new PortfolioEventIdentity(44, "buried_tax_stele"),
                new PortfolioEventIdentity(45, "false_wildfire_boundary_stones"),
                new PortfolioEventIdentity(46, "funeral_without_black_cloth")
            });

        public static IReadOnlyList<PortfolioEventIdentity> Entries => entries;

        public static bool TryResolve(string eventId, out PortfolioEventIdentity identity)
        {
            identity = entries.SingleOrDefault(item => string.Equals(
                item.EventId, eventId, StringComparison.Ordinal));
            return identity != null;
        }
    }
}
